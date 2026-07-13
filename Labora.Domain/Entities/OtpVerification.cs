using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class OtpVerification : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public OtpPurpose Purpose { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public OtpStatus Status { get; set; } = OtpStatus.Issuing;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public int AttemptCount { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;
    public int SendCount { get; set; } = 0;
    public DateTime LastSentAt { get; set; }
    public string? OperationTokenHash { get; set; }
    public DateTime? OperationTokenExpiresAt { get; set; }
    public string? RegistrationPayload { get; set; }
    public uint Version { get; set; }

    // Navigation property
    public User? User { get; set; }
}
