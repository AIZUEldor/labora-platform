using Labora.Application.Common;
using Labora.Domain.Enums;

namespace Labora.Application.Interfaces;

public interface IOtpAbuseGuard
{
    Task EnsureNotBlockedAsync(string phoneNumber, OtpRequestContext requestContext);

    Task RecordStartAttemptAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext);

    /// <summary>
    /// Composable core of RecordStartAttemptAsync: performs the active-block re-check, phone-rate
    /// and cross-number threshold evaluation, the StartAttempt insert, and any block creation or
    /// escalation - identical business logic, but does not open its own transaction, does not retry,
    /// and does not throw OtpBlockedException. Must be called from within a transaction the caller
    /// already owns (via IUnitOfWork.ExecuteInTransactionAsync); the caller decides whether and when
    /// to react to a non-null verdict.
    /// </summary>
    Task<OtpBlockVerdict?> EvaluateStartAttemptInCurrentTransactionAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext);

    Task RecordSmsSentAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext);
}
