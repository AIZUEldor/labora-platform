using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Jobs;

public class JobResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public JobType JobType { get; set; }
    public JobStatus Status { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategoryName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? RequiredSkills { get; set; }
    public int? ExperienceYears { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid EmployerId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Cheap thumbnail for list/card responses: the first image by CreatedAt then Id. Always set
    // (detail, update, and list responses alike). Never persisted - derived at read time.
    public string? CoverImageUrl { get; set; }

    // Full gallery. Only populated by detail/update responses (whichever service fetch actually
    // loaded Images); list/search/my-jobs responses deliberately leave this empty - see JobService.
    public List<JobImageDto> Images { get; set; } = new();
}