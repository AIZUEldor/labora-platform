using Labora.Application.Common;
using Labora.Application.Interfaces;
using Labora.Application.Options;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Labora.Application.Services;

public class OtpAbuseGuard : IOtpAbuseGuard
{
    private const int MaxConcurrencyRetries = 3;

    private readonly IOtpAbuseEventRepository _otpAbuseEventRepository;
    private readonly IOtpBlockRepository _otpBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<OtpOptions> _otpOptions;

    public OtpAbuseGuard(
        IOtpAbuseEventRepository otpAbuseEventRepository,
        IOtpBlockRepository otpBlockRepository,
        IUnitOfWork unitOfWork,
        IOptions<OtpOptions> otpOptions)
    {
        _otpAbuseEventRepository = otpAbuseEventRepository;
        _otpBlockRepository = otpBlockRepository;
        _unitOfWork = unitOfWork;
        _otpOptions = otpOptions;
    }

    public async Task EnsureNotBlockedAsync(string phoneNumber, OtpRequestContext requestContext)
    {
        BlockVerdict? activeBlock = await FindActiveBlockAsync(phoneNumber, requestContext, DateTime.UtcNow);

        if (activeBlock is not null)
        {
            throw new OtpBlockedException(activeBlock.Value.BlockType, activeBlock.Value.BlockReason, activeBlock.Value.ExpiresAtUtc);
        }
    }

    public Task RecordStartAttemptAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext)
    {
        return RunWithRetryAsync(() => EvaluateAndRecordStartAttemptAsync(phoneNumber, purpose, verificationId, requestContext));
    }

    public Task RecordSmsSentAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext)
    {
        // Deliberately not wrapped in its own ExecuteInTransactionAsync call: EF Core cannot open a
        // second transaction on a DbContext that already has one in progress, so a self-contained
        // transaction here would make it impossible for a future caller to combine this insert with
        // the Issuing -> Pending status transition in one atomic Transaction B. A single AddAsync
        // enlists in whatever ambient transaction the caller has already started (or auto-commits
        // standalone if there is none), and there is no unique constraint or row-version on
        // OtpAbuseEvent for this insert to conflict on, so no retry is needed either.
        return _otpAbuseEventRepository.AddAsync(new OtpAbuseEvent
        {
            EventType = OtpAbuseEventType.SmsSent,
            PhoneNumber = phoneNumber,
            IpHash = requestContext.IpHash,
            DeviceHash = requestContext.DeviceHash,
            Purpose = purpose,
            VerificationId = verificationId,
        });
    }

    private async Task RunWithRetryAsync(Func<Task<BlockVerdict?>> transactionalOperation)
    {
        for (int attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            try
            {
                BlockVerdict? violation = await _unitOfWork.ExecuteInTransactionAsync(transactionalOperation);

                if (violation is not null)
                {
                    throw new OtpBlockedException(violation.Value.BlockType, violation.Value.BlockReason, violation.Value.ExpiresAtUtc);
                }

                return;
            }
            catch (Exception ex) when (attempt < MaxConcurrencyRetries && IsRetryableBlockConflict(ex))
            {
                // A concurrent writer touched the same OtpBlock scope inside this transaction attempt.
                // Postgres aborts the whole transaction after any failed statement, and UnitOfWork has
                // already rolled it back, so the only correct recovery is to retry the entire
                // evaluate+record+apply sequence from scratch in a brand-new transaction with fresh
                // reads - continuing inside the now-unusable transaction is not an option.
            }
        }
    }

    private async Task<BlockVerdict?> EvaluateAndRecordStartAttemptAsync(
        string phoneNumber, OtpPurpose purpose, Guid? verificationId, OtpRequestContext requestContext)
    {
        DateTime nowUtc = DateTime.UtcNow;
        OtpOptions options = _otpOptions.Value;

        // EnsureNotBlockedAsync is only an early, possibly-stale optimization for the caller;
        // this re-check inside the transaction is the actual correctness boundary. If any scope
        // is already actively blocked, stop here - do not insert the event or evaluate thresholds.
        BlockVerdict? activeBlock = await FindActiveBlockAsync(phoneNumber, requestContext, nowUtc);
        if (activeBlock is not null)
        {
            return activeBlock;
        }

        // Pre-SMS phone rate gate: counted from prior SmsSent events, not StartAttempt, so a
        // start that never resulted in an actual send does not consume the phone's SMS quota,
        // but a quota already exhausted by earlier sends still blocks this new start before
        // another SMS is attempted.
        bool phoneRateExceeded = await IsPhoneRateExceededAsync(phoneNumber, nowUtc, options);

        OtpBlockReason? ipReason = requestContext.IpHash is not null
            ? await EvaluateCrossNumberThresholdAsync(OtpBlockType.Ip, requestContext.IpHash, nowUtc, options)
            : null;

        OtpBlockReason? deviceReason = requestContext.DeviceHash is not null
            ? await EvaluateCrossNumberThresholdAsync(OtpBlockType.Device, requestContext.DeviceHash, nowUtc, options)
            : null;

        await _otpAbuseEventRepository.AddAsync(new OtpAbuseEvent
        {
            EventType = OtpAbuseEventType.StartAttempt,
            PhoneNumber = phoneNumber,
            IpHash = requestContext.IpHash,
            DeviceHash = requestContext.DeviceHash,
            Purpose = purpose,
            VerificationId = verificationId,
        });

        BlockVerdict? violation = null;

        if (phoneRateExceeded)
        {
            OtpBlock block = await ApplyViolationAsync(OtpBlockType.Phone, phoneNumber, OtpBlockReason.PhoneSendRateExceeded, nowUtc, options);
            violation ??= (block.BlockType, block.BlockReason, block.ExpiresAt);
        }

        if (ipReason is not null)
        {
            OtpBlock block = await ApplyViolationAsync(OtpBlockType.Ip, requestContext.IpHash!, ipReason.Value, nowUtc, options);
            violation ??= (block.BlockType, block.BlockReason, block.ExpiresAt);
        }

        if (deviceReason is not null)
        {
            OtpBlock block = await ApplyViolationAsync(OtpBlockType.Device, requestContext.DeviceHash!, deviceReason.Value, nowUtc, options);
            violation ??= (block.BlockType, block.BlockReason, block.ExpiresAt);
        }

        return violation;
    }

    private async Task<BlockVerdict?> FindActiveBlockAsync(string phoneNumber, OtpRequestContext requestContext, DateTime nowUtc)
    {
        OtpBlock? phoneBlock = await _otpBlockRepository.GetActiveAsync(OtpBlockType.Phone, phoneNumber, nowUtc);
        if (phoneBlock is not null)
        {
            return (phoneBlock.BlockType, phoneBlock.BlockReason, phoneBlock.ExpiresAt);
        }

        if (requestContext.IpHash is not null)
        {
            OtpBlock? ipBlock = await _otpBlockRepository.GetActiveAsync(OtpBlockType.Ip, requestContext.IpHash, nowUtc);
            if (ipBlock is not null)
            {
                return (ipBlock.BlockType, ipBlock.BlockReason, ipBlock.ExpiresAt);
            }
        }

        if (requestContext.DeviceHash is not null)
        {
            OtpBlock? deviceBlock = await _otpBlockRepository.GetActiveAsync(OtpBlockType.Device, requestContext.DeviceHash, nowUtc);
            if (deviceBlock is not null)
            {
                return (deviceBlock.BlockType, deviceBlock.BlockReason, deviceBlock.ExpiresAt);
            }
        }

        return null;
    }

    private async Task<bool> IsPhoneRateExceededAsync(string phoneNumber, DateTime nowUtc, OtpOptions options)
    {
        int recentCount = await _otpAbuseEventRepository.CountByPhoneAsync(
            phoneNumber, OtpAbuseEventType.SmsSent, nowUtc.AddSeconds(-options.SmsMinIntervalSeconds));
        if (recentCount >= 1)
        {
            return true;
        }

        int hourlyCount = await _otpAbuseEventRepository.CountByPhoneAsync(
            phoneNumber, OtpAbuseEventType.SmsSent, nowUtc.AddHours(-1));
        if (hourlyCount >= options.MaxSmsPerHour)
        {
            return true;
        }

        int dailyCount = await _otpAbuseEventRepository.CountByPhoneAsync(
            phoneNumber, OtpAbuseEventType.SmsSent, nowUtc.AddDays(-1));
        return dailyCount >= options.MaxSmsPerDay;
    }

    private async Task<OtpBlockReason?> EvaluateCrossNumberThresholdAsync(
        OtpBlockType scopeType, string scopeHash, DateTime nowUtc, OtpOptions options)
    {
        int distinctPhones = scopeType == OtpBlockType.Ip
            ? await _otpAbuseEventRepository.CountDistinctPhonesByIpAsync(
                scopeHash, OtpAbuseEventType.StartAttempt, nowUtc.AddMinutes(-options.CrossNumberDistinctPhoneWindowMinutes))
            : await _otpAbuseEventRepository.CountDistinctPhonesByDeviceAsync(
                scopeHash, OtpAbuseEventType.StartAttempt, nowUtc.AddMinutes(-options.CrossNumberDistinctPhoneWindowMinutes));

        if (distinctPhones >= options.CrossNumberDistinctPhoneLimit)
        {
            return OtpBlockReason.CrossNumberDistinctPhonesExceeded;
        }

        int totalStarts = scopeType == OtpBlockType.Ip
            ? await _otpAbuseEventRepository.CountByIpAsync(
                scopeHash, OtpAbuseEventType.StartAttempt, nowUtc.AddMinutes(-options.CrossNumberStartVolumeWindowMinutes))
            : await _otpAbuseEventRepository.CountByDeviceAsync(
                scopeHash, OtpAbuseEventType.StartAttempt, nowUtc.AddMinutes(-options.CrossNumberStartVolumeWindowMinutes));

        return totalStarts >= options.CrossNumberStartVolumeLimit
            ? OtpBlockReason.CrossNumberStartVolumeExceeded
            : null;
    }

    private async Task<OtpBlock> ApplyViolationAsync(
        OtpBlockType blockType, string scopeKey, OtpBlockReason reason, DateTime nowUtc, OtpOptions options)
    {
        OtpBlock? existing = await _otpBlockRepository.GetByScopeAsync(blockType, scopeKey);

        if (existing is null)
        {
            return await _otpBlockRepository.AddAsync(new OtpBlock
            {
                BlockType = blockType,
                ScopeKey = scopeKey,
                BlockReason = reason,
                ViolationCount = 1,
                ExpiresAt = nowUtc.AddHours(options.FirstBlockDurationHours),
                LastViolationAt = nowUtc,
            });
        }

        bool withinDecayWindow = existing.LastViolationAt is not null
            && existing.LastViolationAt.Value >= nowUtc.AddDays(-options.ViolationDecayDays);

        existing.BlockReason = reason;
        existing.ViolationCount = withinDecayWindow ? existing.ViolationCount + 1 : 1;
        existing.ExpiresAt = nowUtc.AddHours(withinDecayWindow ? options.EscalatedBlockDurationHours : options.FirstBlockDurationHours);
        existing.LastViolationAt = nowUtc;

        return await _otpBlockRepository.UpdateAsync(existing);
    }

    private static bool IsRetryableBlockConflict(Exception ex) =>
        ex is OtpBlockConflictException or OtpBlockConcurrencyException;

    private readonly record struct BlockVerdict(OtpBlockType BlockType, OtpBlockReason BlockReason, DateTime ExpiresAtUtc)
    {
        public static implicit operator BlockVerdict((OtpBlockType BlockType, OtpBlockReason BlockReason, DateTime ExpiresAt) tuple) =>
            new(tuple.BlockType, tuple.BlockReason, tuple.ExpiresAt);
    }
}
