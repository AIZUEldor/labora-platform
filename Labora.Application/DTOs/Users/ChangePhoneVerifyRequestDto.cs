namespace Labora.Application.DTOs.Users;

public class ChangePhoneVerifyRequestDto
{
    public Guid VerificationId { get; set; }
    public string Code { get; set; } = string.Empty;
}
