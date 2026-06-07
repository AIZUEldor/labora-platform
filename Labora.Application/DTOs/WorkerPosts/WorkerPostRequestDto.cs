namespace Labora.Application.DTOs.WorkerPosts;

public class CreateWorkerPostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public int ExperienceYears { get; set; }
    public string? Skills { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
}

public class UpdateWorkerPostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public int ExperienceYears { get; set; }
    public string? Skills { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public int Status { get; set; }
}