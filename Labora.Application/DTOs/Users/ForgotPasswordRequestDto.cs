namespace Labora.Application.DTOs.Users;

public class ForgotPasswordRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}