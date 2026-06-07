using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class WorkerPortfolioImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }

    // Foreign key
    public Guid WorkerPostId { get; set; }

    // Navigation property
    public WorkerPost WorkerPost { get; set; } = null!;
}