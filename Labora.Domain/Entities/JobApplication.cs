using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class JobApplication : BaseEntity
{
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    // Foreign keys
    public Guid JobId { get; set; }
    public Guid WorkerId { get; set; }

    // Navigation properties
    public Job Job { get; set; } = null!;
    public User Worker { get; set; } = null!;
}