namespace Labora.Application.DTOs.Auth;

public class RegisterVerifyRequestDto
{
    public Guid VerificationId { get; set; }
    public string Code { get; set; } = string.Empty;
}
