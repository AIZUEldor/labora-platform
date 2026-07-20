namespace Labora.Application.DTOs.Auth;

public class LoginCompleteRequestDto
{
    public Guid VerificationId { get; set; }
    public string OperationToken { get; set; } = string.Empty;
}
