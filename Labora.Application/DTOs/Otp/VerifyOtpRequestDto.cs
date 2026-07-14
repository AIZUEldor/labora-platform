namespace Labora.Application.DTOs.Otp;

public class VerifyOtpRequestDto
{
    public Guid VerificationId { get; set; }
    public string Code { get; set; } = string.Empty;
}
