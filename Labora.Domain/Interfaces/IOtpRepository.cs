using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IOtpRepository : IGenericRepository<OtpVerification>
{
    Task<OtpVerification?> GetLatestByPhoneAndPurposeAsync(string phoneNumber, OtpPurpose purpose);
    Task<OtpVerification?> GetByOperationTokenHashAsync(string operationTokenHash);

    /// <summary>
    /// Returns a conservative upper-bound estimate of sends for this phone+purpose since <paramref name="sinceUtc"/>,
    /// not an exact rolling-window count. <see cref="OtpVerification.SendCount"/> is cumulative per row (a resend
    /// reuses the same row rather than creating a new one), so a row whose most recent send falls inside the window
    /// but which was also sent earlier, outside the window, contributes its full lifetime count. The result can only
    /// overcount, never undercount, so it is safe to use as-is for rate limiting.
    /// </summary>
    Task<int> GetSendCountSinceAsync(string phoneNumber, OtpPurpose purpose, DateTime sinceUtc);
}
