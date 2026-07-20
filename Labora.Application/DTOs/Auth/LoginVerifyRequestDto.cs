namespace Labora.Application.DTOs.Auth;

public class LoginVerifyRequestDto
{
    public Guid VerificationId { get; set; }
    public string Code { get; set; } = string.Empty;
}
