namespace Labora.Application.DTOs.Auth;

public class ForgotPasswordVerifyRequestDto
{
    public Guid VerificationId { get; set; }
    public string Code { get; set; } = string.Empty;
}
