using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IOtpAbuseEventRepository : IGenericRepository<OtpAbuseEvent>
{
    Task<int> CountByPhoneAsync(string phoneNumber, OtpAbuseEventType eventType, DateTime sinceUtc);
    Task<int> CountByIpAsync(string ipHash, OtpAbuseEventType eventType, DateTime sinceUtc);
    Task<int> CountByDeviceAsync(string deviceHash, OtpAbuseEventType eventType, DateTime sinceUtc);
    Task<int> CountDistinctPhonesByIpAsync(string ipHash, OtpAbuseEventType eventType, DateTime sinceUtc);
    Task<int> CountDistinctPhonesByDeviceAsync(string deviceHash, OtpAbuseEventType eventType, DateTime sinceUtc);
}
