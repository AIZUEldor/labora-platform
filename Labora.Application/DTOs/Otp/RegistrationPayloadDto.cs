using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Otp;

public class RegistrationPayloadDto
{
    public int Version { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
