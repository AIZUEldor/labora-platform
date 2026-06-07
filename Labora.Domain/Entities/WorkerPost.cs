using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class WorkerPost : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public int ExperienceYears { get; set; }
    public string? Skills { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public WorkerPostStatus Status { get; set; } = WorkerPostStatus.Active;
    public int ViewCount { get; set; } = 0;

    // Foreign keys
    public Guid WorkerId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }

    // Navigation properties
    public User Worker { get; set; } = null!;
    public Category? Category { get; set; }
    public Category? SubCategory { get; set; }
    public ICollection<WorkerPortfolioImage> PortfolioImages { get; set; } = new List<WorkerPortfolioImage>();
}