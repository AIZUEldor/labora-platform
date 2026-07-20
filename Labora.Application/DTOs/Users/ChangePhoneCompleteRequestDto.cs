namespace Labora.Application.DTOs.Users;

public class ChangePhoneCompleteRequestDto
{
    public Guid VerificationId { get; set; }
    public string OperationToken { get; set; } = string.Empty;
}
