using Labora.Application.Common;
using Labora.Application.DTOs.Otp;
using Labora.Application.Interfaces;
using Labora.Application.Options;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Labora.Application.Services;

public class OtpService : IOtpService
{
    private const int MaxWriteConflictRetries = 3;
    private const int MaxVerifyConcurrencyRetries = 3;
    private const int MaxConsumeConcurrencyRetries = 3;

    private readonly IOtpRepository _otpRepository;
    private readonly IOtpSecurityService _otpSecurityService;
    private readonly ISmsSender _smsSender;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<OtpOptions> _otpOptions;
    private readonly IOtpAbuseGuard _otpAbuseGuard;
    private readonly IOtpRequestContextProvider _otpRequestContextProvider;

    public OtpService(
        IOtpRepository otpRepository,
        IOtpSecurityService otpSecurityService,
        ISmsSender smsSender,
        IPhoneNumberNormalizer phoneNumberNormalizer,
        IUnitOfWork unitOfWork,
        IOptions<OtpOptions> otpOptions,
        IOtpAbuseGuard otpAbuseGuard,
        IOtpRequestContextProvider otpRequestContextProvider)
    {
        _otpRepository = otpRepository;
        _otpSecurityService = otpSecurityService;
        _smsSender = smsSender;
        _phoneNumberNormalizer = phoneNumberNormalizer;
        _unitOfWork = unitOfWork;
        _otpOptions = otpOptions;
        _otpAbuseGuard = otpAbuseGuard;
        _otpRequestContextProvider = otpRequestContextProvider;
    }

    public Task EnforceRateLimitAsync(string phoneNumber, OtpPurpose purpose)
    {
        throw new NotImplementedException();
    }

    public async Task<StartOtpResponseDto> StartOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? userId = null,
        string? registrationPayload = null)
    {
        string normalizedPhone = _phoneNumberNormalizer.Normalize(phoneNumber);
        OtpRequestContext requestContext = _otpRequestContextProvider.GetContext();
        OtpOptions options = _otpOptions.Value;

        (OtpVerification verification, string code) = await ResolveAndIssueWithRetryAsync(
            () => ResolveStartTargetAsync(normalizedPhone, purpose, userId, registrationPayload, options),
            requestContext,
            options);

        verification = await SendCodeAndAdvanceAsync(verification, code, requestContext);

        return new StartOtpResponseDto
        {
            VerificationId = verification.Id,
            ExpiresAt = verification.ExpiresAt,
            MaxAttempts = verification.MaxAttempts
        };
    }

    /// <summary>
    /// Produces a StartOtpResponseDto indistinguishable in shape from a real StartOtpAsync result,
    /// without persisting any row or sending any SMS - used by callers (e.g. forgot-password) that
    /// must not reveal via response shape whether phoneNumber belongs to a real account. phoneNumber
    /// and purpose are accepted only to match StartOtpAsync's signature; no real OTP flow exists for
    /// this VerificationId, so a Resend/Verify/Consume against it will simply behave as "not found."
    /// This is intentionally a response-shape decoy only - it does not attempt to mask the timing
    /// difference against a real Start (which performs real DB writes and an outbound SMS call).
    /// </summary>
    public Task<StartOtpResponseDto> PrepareDecoyStartAsync(
        string phoneNumber,
        OtpPurpose purpose)
    {
        OtpOptions options = _otpOptions.Value;

        return Task.FromResult(new StartOtpResponseDto
        {
            VerificationId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(options.CodeExpiryMinutes),
            MaxAttempts = options.MaxAttempts
        });
    }

    public async Task<ResendOtpResponseDto> ResendOtpAsync(Guid verificationId)
    {
        OtpRequestContext requestContext = _otpRequestContextProvider.GetContext();
        OtpOptions options = _otpOptions.Value;

        (OtpVerification verification, string code) = await ResolveAndIssueWithRetryAsync(
            () => ResolveResendTargetAsync(verificationId, options),
            requestContext,
            options);

        verification = await SendCodeAndAdvanceAsync(verification, code, requestContext);

        return new ResendOtpResponseDto
        {
            VerificationId = verification.Id,
            ExpiresAt = verification.ExpiresAt
        };
    }

    public async Task<VerifyOtpResponseDto> VerifyOtpAsync(
        Guid verificationId,
        string code)
    {
        for (int attempt = 1; attempt <= MaxVerifyConcurrencyRetries; attempt++)
        {
            try
            {
                return await TryVerifyAsync(verificationId, code);
            }
            catch (OtpConcurrencyException) when (attempt < MaxVerifyConcurrencyRetries)
            {
                // The row was modified concurrently (e.g. a racing verify/resend call); reload
                // fresh state and re-evaluate from scratch rather than acting on stale data.
            }
        }

        throw new InvalidOperationException("Unreachable: OTP verify retry loop exited without returning or throwing.");
    }

    public async Task<OtpConsumeResultDto> ConsumeOtpAsync(
        Guid verificationId,
        OtpPurpose expectedPurpose,
        string? operationToken = null)
    {
        if (string.IsNullOrWhiteSpace(operationToken))
        {
            throw new OtpInvalidOperationTokenException("An operation token is required to consume this OTP verification.");
        }

        for (int attempt = 1; attempt <= MaxConsumeConcurrencyRetries; attempt++)
        {
            try
            {
                return await TryConsumeAsync(verificationId, expectedPurpose, operationToken);
            }
            catch (OtpConcurrencyException) when (attempt < MaxConsumeConcurrencyRetries)
            {
                // A concurrent consumer touched this row between our read and write; the transaction
                // was rolled back, so retry the whole check-and-consume sequence from scratch in a
                // brand-new transaction with a fresh read rather than acting on stale data.
            }
        }

        throw new InvalidOperationException("Unreachable: OTP consume retry loop exited without returning or throwing.");
    }

    /// <summary>
    /// Transaction A, fully atomic: resolves the target row (new or reused), evaluates the abuse
    /// guard's active-block re-check + phone-rate/cross-number thresholds + StartAttempt insert +
    /// block escalation, and - only if not blocked - issues a fresh code and persists the row as
    /// Issuing, all inside one IUnitOfWork transaction. If the guard returns a verdict, the
    /// OtpVerification write is skipped entirely and the (already-committed, since nothing failed)
    /// transaction still records the StartAttempt/block evidence; OtpBlockedException is thrown only
    /// after that commit succeeds. Any DB conflict (OtpVerification unique/xmin, or OtpBlock
    /// unique/xmin surfacing from the guard's own writes) aborts the whole transaction, and the
    /// bounded retry below re-runs this entire delegate from scratch in a brand-new transaction with
    /// fresh reads. The retry never extends past this method: once it returns, SMS sending happens
    /// exactly once, outside any transaction.
    /// </summary>
    private async Task<(OtpVerification Verification, string Code)> ResolveAndIssueWithRetryAsync(
        Func<Task<(OtpVerification Candidate, bool IsNewRow)>> resolveTarget,
        OtpRequestContext requestContext,
        OtpOptions options)
    {
        for (int attempt = 1; attempt <= MaxWriteConflictRetries; attempt++)
        {
            try
            {
                (OtpBlockVerdict? verdict, OtpVerification? verification, string? code) = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    (OtpVerification candidate, bool isNewRow) = await resolveTarget();

                    // The active-block re-check, phone-rate/cross-number evaluation, and StartAttempt
                    // insert all happen here, atomically with the OtpVerification write below - this
                    // is what makes the guard's in-transaction re-check the true correctness boundary
                    // (a block or newly-tripped threshold prevents the Issuing row from ever being
                    // written) and guarantees the StartAttempt event's VerificationId always equals
                    // candidate.Id, since nothing commits until both agree on the same object.
                    OtpBlockVerdict? blockVerdict = await _otpAbuseGuard.EvaluateStartAttemptInCurrentTransactionAsync(
                        candidate.PhoneNumber, candidate.Purpose, candidate.Id, requestContext);

                    if (blockVerdict is not null)
                    {
                        return (blockVerdict, (OtpVerification?)null, (string?)null);
                    }

                    string issuedCode = PrepareIssuedCode(candidate, options);

                    OtpVerification saved = isNewRow
                        ? await _otpRepository.AddAsync(candidate)
                        : await _otpRepository.UpdateAsync(candidate);

                    return ((OtpBlockVerdict?)null, saved, issuedCode);
                });

                if (verdict is not null)
                {
                    throw new OtpBlockedException(verdict.BlockType, verdict.BlockReason, verdict.ExpiresAtUtc);
                }

                return (verification!, code!);
            }
            catch (Exception ex) when (attempt < MaxWriteConflictRetries && IsRetryableVerificationConflict(ex))
            {
                // A DB-level conflict aborted the whole transaction (Postgres poisons a transaction
                // after any failed statement), whether it came from the OtpVerification write or from
                // the guard's own OtpBlock write. UnitOfWork has already rolled it back, so the only
                // correct recovery is to retry this entire delegate from scratch in a brand-new
                // transaction with fresh reads - a genuine business conflict (e.g. resolveTarget
                // finding a non-recoverable active flow) simply reproduces the same exception, bounded
                // by MaxWriteConflictRetries, while a transient race resolves correctly.
            }
        }

        throw new InvalidOperationException(
            "Unreachable: OTP verification write-conflict retry loop exited without returning or throwing.");
    }

    /// <summary>
    /// In-memory field setup for a (re)issued code - no I/O, safe to call inside Transaction A right
    /// before the Add/Update. AttemptCount is deliberately left untouched here - it is never reset by
    /// issuing a new code, so an attacker cannot bypass MaxAttempts by repeatedly resending; only a
    /// fresh row (after the previous one reaches a terminal Consumed/Failed state) starts a new
    /// attempt budget.
    /// </summary>
    private string PrepareIssuedCode(OtpVerification verification, OtpOptions options)
    {
        string code = _otpSecurityService.GenerateOtpCode();
        DateTime nowUtc = DateTime.UtcNow;

        verification.CodeHash = _otpSecurityService.HashOtpCode(verification.Id, verification.Purpose, verification.PhoneNumber, code);
        verification.Status = OtpStatus.Issuing;
        verification.ExpiresAt = nowUtc.AddMinutes(options.CodeExpiryMinutes);
        verification.MaxAttempts = options.MaxAttempts;
        verification.SendCount += 1;
        verification.LastSentAt = nowUtc;

        return code;
    }

    private async Task<(OtpVerification Candidate, bool IsNewRow)> ResolveStartTargetAsync(
        string normalizedPhone, OtpPurpose purpose, Guid? userId, string? registrationPayload, OtpOptions options)
    {
        DateTime nowUtc = DateTime.UtcNow;

        int sendCount = await _otpRepository.GetSendCountSinceAsync(
            normalizedPhone, purpose, nowUtc.AddMinutes(-options.RateLimitWindowMinutes));
        if (sendCount >= options.MaxSendsPerWindow)
        {
            throw new OtpSendRateLimitedException(
                nowUtc.AddMinutes(options.RateLimitWindowMinutes), "Maximum OTP sends for this window have been reached.");
        }

        OtpVerification? existing = await _otpRepository.GetLatestByPhoneAndPurposeAsync(normalizedPhone, purpose);

        if (existing is not null && IsActiveFlow(existing))
        {
            if (!IsRecoverable(existing, nowUtc, options))
            {
                throw new OtpConflictException(
                    "An active OTP verification already exists for this phone number and purpose.");
            }

            return (existing, false);
        }

        OtpVerification verification = new()
        {
            PhoneNumber = normalizedPhone,
            UserId = userId,
            Purpose = purpose,
            RegistrationPayload = registrationPayload,
        };

        return (verification, true);
    }

    private async Task<(OtpVerification Candidate, bool IsNewRow)> ResolveResendTargetAsync(Guid verificationId, OtpOptions options)
    {
        OtpVerification? verification = await _otpRepository.GetByIdForUpdateAsync(verificationId);
        if (verification is null)
        {
            throw new KeyNotFoundException("OTP verification topilmadi.");
        }

        if (verification.Status is not (OtpStatus.Issuing or OtpStatus.Pending))
        {
            throw new OtpConflictException("This OTP verification is not in a resendable state.");
        }

        DateTime nowUtc = DateTime.UtcNow;

        DateTime cooldownEndsAt = verification.LastSentAt.AddSeconds(options.ResendCooldownSeconds);
        if (nowUtc < cooldownEndsAt)
        {
            throw new OtpSendRateLimitedException(cooldownEndsAt, "OTP resend cooldown has not elapsed yet.");
        }

        int sendCount = await _otpRepository.GetSendCountSinceAsync(
            verification.PhoneNumber, verification.Purpose, nowUtc.AddMinutes(-options.RateLimitWindowMinutes));
        if (sendCount >= options.MaxSendsPerWindow)
        {
            throw new OtpSendRateLimitedException(
                nowUtc.AddMinutes(options.RateLimitWindowMinutes), "Maximum OTP sends for this window have been reached.");
        }

        return (verification, false);
    }

    /// <summary>
    /// SMS + Transaction B. The SMS call happens exactly once and outside any transaction, per the
    /// approved Transaction A -> SMS -> Transaction B flow. On provider failure the row is marked
    /// Failed (single statement, no SmsSent event recorded) and the original exception propagates -
    /// no fail-open path exists. On success, the Issuing -> Pending transition and the SmsSent audit
    /// event are committed together atomically (RecordSmsSentAsync enlists in this same transaction
    /// rather than opening its own).
    /// </summary>
    private async Task<OtpVerification> SendCodeAndAdvanceAsync(
        OtpVerification verification, string code, OtpRequestContext requestContext)
    {
        try
        {
            await _smsSender.SendAsync(verification.PhoneNumber, BuildSmsMessage(code));
        }
        catch
        {
            verification.Status = OtpStatus.Failed;
            await _otpRepository.UpdateAsync(verification);
            throw;
        }

        verification.Status = OtpStatus.Pending;

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            OtpVerification updated = await _otpRepository.UpdateAsync(verification);
            await _otpAbuseGuard.RecordSmsSentAsync(verification.PhoneNumber, verification.Purpose, verification.Id, requestContext);
            return updated;
        });
    }

    private async Task<VerifyOtpResponseDto> TryVerifyAsync(Guid verificationId, string code)
    {
        OtpVerification? verification = await _otpRepository.GetByIdForUpdateAsync(verificationId);
        if (verification is null)
        {
            throw new KeyNotFoundException("OTP verification topilmadi.");
        }

        if (verification.Status != OtpStatus.Pending)
        {
            throw new OtpConflictException("This OTP verification is not awaiting a code.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        if (verification.ExpiresAt <= nowUtc)
        {
            verification.Status = OtpStatus.Failed;
            await _otpRepository.UpdateAsync(verification);
            throw new OtpExpiredException("This OTP code has expired.");
        }

        // Wrong-code attempts are intentionally out of the abuse guard's scope (it only tracks
        // StartAttempt/SmsSent) - no OtpAbuseGuard call happens anywhere in this method.
        verification.AttemptCount += 1;

        bool isCorrect = _otpSecurityService.VerifyOtpCode(
            verification.Id, verification.Purpose, verification.PhoneNumber, code, verification.CodeHash);

        if (!isCorrect)
        {
            if (verification.AttemptCount >= verification.MaxAttempts)
            {
                verification.Status = OtpStatus.Failed;
                await _otpRepository.UpdateAsync(verification);
                throw new OtpMaxAttemptsExceededException("Maximum OTP verification attempts exceeded.");
            }

            await _otpRepository.UpdateAsync(verification);
            return new VerifyOtpResponseDto { IsVerified = false };
        }

        string operationToken = _otpSecurityService.GenerateOperationToken();
        verification.Status = OtpStatus.Verified;
        verification.VerifiedAt = nowUtc;
        verification.OperationTokenHash = _otpSecurityService.HashOperationToken(verification.Id, operationToken);
        verification.OperationTokenExpiresAt = nowUtc.AddMinutes(_otpOptions.Value.OperationTokenExpiryMinutes);

        await _otpRepository.UpdateAsync(verification);

        return new VerifyOtpResponseDto
        {
            IsVerified = true,
            OperationToken = operationToken,
            OperationTokenExpiresAt = verification.OperationTokenExpiresAt
        };
    }

    /// <summary>
    /// Check-and-consume, fully atomic: the operation-token validation and the Verified -> Consumed
    /// transition happen inside one transaction, so a concurrent consumer either sees the row before
    /// this commits (and races on the xmin check, retried by the caller) or after (and correctly finds
    /// it already Consumed with its token hash cleared) - there is no window where two callers can both
    /// observe a valid, still-Verified row.
    /// </summary>
    private async Task<OtpConsumeResultDto> TryConsumeAsync(Guid verificationId, OtpPurpose expectedPurpose, string operationToken)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            OtpVerification? verification = await _otpRepository.GetByIdForUpdateAsync(verificationId);

            // Row not found, purpose mismatch, and token-hash mismatch are all deliberately
            // indistinguishable from each other here - a caller must not be able to learn which
            // specific reason caused the rejection.
            if (verification is null
                || verification.Purpose != expectedPurpose
                || string.IsNullOrEmpty(verification.OperationTokenHash)
                || !_otpSecurityService.VerifyOperationToken(verificationId, operationToken, verification.OperationTokenHash))
            {
                throw new OtpInvalidOperationTokenException("The operation token is invalid.");
            }

            DateTime nowUtc = DateTime.UtcNow;

            if (verification.OperationTokenExpiresAt is null || verification.OperationTokenExpiresAt <= nowUtc)
            {
                throw new OtpExpiredException("The operation token has expired.");
            }

            if (verification.Status != OtpStatus.Verified)
            {
                throw new OtpConflictException(
                    verification.Status == OtpStatus.Consumed
                        ? "This operation token has already been consumed."
                        : "This OTP verification is not in a consumable state.");
            }

            verification.Status = OtpStatus.Consumed;
            verification.ConsumedAt = nowUtc;
            verification.OperationTokenHash = null;
            verification.OperationTokenExpiresAt = null;

            OtpVerification updated = await _otpRepository.UpdateAsync(verification);

            return new OtpConsumeResultDto
            {
                VerificationId = updated.Id,
                Purpose = updated.Purpose,
                PhoneNumber = updated.PhoneNumber,
                UserId = updated.UserId,
                RegistrationPayload = updated.RegistrationPayload
            };
        });
    }

    private static bool IsActiveFlow(OtpVerification verification) =>
        verification.Status is OtpStatus.Issuing or OtpStatus.Pending or OtpStatus.Verified;

    /// <summary>
    /// An active-flow row can still be reused instead of hard-blocking a new Start when it is
    /// functionally dead: an Issuing row whose SMS send has been stuck past StaleIssuingSeconds, a
    /// Pending row whose code expired without ever being verified, or a Verified row whose operation
    /// token expired without ever being consumed. All other active states are a genuine conflict.
    /// </summary>
    private static bool IsRecoverable(OtpVerification verification, DateTime nowUtc, OtpOptions options) =>
        verification.Status switch
        {
            OtpStatus.Issuing => verification.LastSentAt <= nowUtc.AddSeconds(-options.StaleIssuingSeconds),
            OtpStatus.Pending => verification.ExpiresAt <= nowUtc,
            OtpStatus.Verified => verification.OperationTokenExpiresAt is not null && verification.OperationTokenExpiresAt <= nowUtc,
            _ => false
        };

    private static bool IsRetryableVerificationConflict(Exception ex) =>
        ex is OtpConflictException or OtpConcurrencyException or OtpBlockConflictException or OtpBlockConcurrencyException;

    private static string BuildSmsMessage(string code) => $"Tasdiqlash kodi: {code}. Kodni hech kimga bermang.";
}
