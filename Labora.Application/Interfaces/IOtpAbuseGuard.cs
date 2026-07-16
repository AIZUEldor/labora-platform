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

    Task RecordSmsSentAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? verificationId,
        OtpRequestContext requestContext);
}
