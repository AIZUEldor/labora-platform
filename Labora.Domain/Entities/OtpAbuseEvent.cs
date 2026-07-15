using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class OtpAbuseEvent : BaseEntity
{
    public OtpAbuseEventType EventType { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IpHash { get; set; }
    public string? DeviceHash { get; set; }
    public OtpPurpose? Purpose { get; set; }
    public Guid? VerificationId { get; set; }
}
