using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class PushToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? DeviceType { get; set; } // ios | android

    // Navigation
    public User User { get; set; } = null!;
}