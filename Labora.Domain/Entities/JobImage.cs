using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class JobImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }

    // Foreign key
    public Guid JobId { get; set; }

    // Navigation property
    public Job Job { get; set; } = null!;
}
