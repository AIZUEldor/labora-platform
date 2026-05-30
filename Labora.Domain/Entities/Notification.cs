using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public Guid? ReferenceId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}