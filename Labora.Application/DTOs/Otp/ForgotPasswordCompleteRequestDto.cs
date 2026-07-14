namespace Labora.Application.DTOs.Otp;

public class ForgotPasswordCompleteRequestDto
{
    public Guid VerificationId { get; set; }
    public string OperationToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
