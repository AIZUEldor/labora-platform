namespace Labora.Application.DTOs.Otp;

public class VerifyOtpResponseDto
{
    public bool IsVerified { get; set; }
    public string? OperationToken { get; set; }
    public DateTime? OperationTokenExpiresAt { get; set; }
}
