using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Jobs;

public class JobRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public JobType JobType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategoryName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? RequiredSkills { get; set; }
    public int? ExperienceYears { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
}