using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Otp;

public class OtpConsumeResultDto
{
    public Guid VerificationId { get; set; }
    public OtpPurpose Purpose { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? RegistrationPayload { get; set; }
}
