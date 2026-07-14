namespace Labora.Application.DTOs.Otp;

public class ResendOtpResponseDto
{
    public Guid VerificationId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
