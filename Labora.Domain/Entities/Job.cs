using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class Job : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public JobType JobType { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Active;
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategoryName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? RequiredSkills { get; set; }
    public int? ExperienceYears { get; set; }
    public DateTime? Deadline { get; set; }

    // Foreign key
    public Guid EmployerId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public Category? SubCategory { get; set; }

    // Navigation properties
    public User Employer { get; set; } = null!;
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public Category? Category { get; set; }
}