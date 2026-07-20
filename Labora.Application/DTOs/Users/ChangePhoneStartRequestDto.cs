namespace Labora.Application.DTOs.Users;

public class ChangePhoneStartRequestDto
{
    public string NewPhoneNumber { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
}
