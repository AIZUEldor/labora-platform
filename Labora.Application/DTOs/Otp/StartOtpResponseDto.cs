namespace Labora.Application.DTOs.Otp;

public class StartOtpResponseDto
{
    public Guid VerificationId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int MaxAttempts { get; set; }
}
