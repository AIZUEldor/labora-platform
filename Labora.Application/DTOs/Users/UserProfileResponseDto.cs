using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Users;

public class UserProfileResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? ProfileImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal Balance { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}