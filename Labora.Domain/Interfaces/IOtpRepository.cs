using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IOtpRepository : IGenericRepository<OtpVerification>
{
    Task<OtpVerification?> GetLatestByPhoneAndPurposeAsync(string phoneNumber, OtpPurpose purpose);
    Task<OtpVerification?> GetByOperationTokenHashAsync(string operationTokenHash);

    /// <summary>
    /// Fetches by Id without tracking, for callers that intend to mutate and UpdateAsync the result
    /// - most importantly, callers that may retry after a conflict. A tracked GetByIdAsync would hit
    /// EF Core's identity map on a retry within the same DbContext scope and return the same
    /// already-tracked (and possibly already-Modified, already-stale) instance from the failed prior
    /// attempt instead of a genuinely fresh row, silently defeating the retry's fresh-read guarantee
    /// (including a fresh xmin for the optimistic concurrency check). GenericRepository.UpdateAsync
    /// already handles a detached entity correctly: it attaches it and uses its current Version as
    /// the concurrency check's original value.
    /// </summary>
    Task<OtpVerification?> GetByIdForUpdateAsync(Guid id);

    /// <summary>
    /// Returns a conservative upper-bound estimate of sends for this phone+purpose since <paramref name="sinceUtc"/>,
    /// not an exact rolling-window count. <see cref="OtpVerification.SendCount"/> is cumulative per row (a resend
    /// reuses the same row rather than creating a new one), so a row whose most recent send falls inside the window
    /// but which was also sent earlier, outside the window, contributes its full lifetime count. The result can only
    /// overcount, never undercount, so it is safe to use as-is for rate limiting.
    /// </summary>
    Task<int> GetSendCountSinceAsync(string phoneNumber, OtpPurpose purpose, DateTime sinceUtc);
}
