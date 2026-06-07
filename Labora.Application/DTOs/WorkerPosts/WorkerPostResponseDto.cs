namespace Labora.Application.DTOs.WorkerPosts;

public class WorkerPostResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedSalary { get; set; }
    public int ExperienceYears { get; set; }
    public string? Skills { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Status { get; set; }
    public Guid WorkerId { get; set; }
    public string WorkerFirstName { get; set; } = string.Empty;
    public string WorkerLastName { get; set; } = string.Empty;
    public string? WorkerAvatarUrl { get; set; }
    public string? WorkerPhone { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? SubCategoryId { get; set; }
    public string? SubCategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PortfolioImageDto> PortfolioImages { get; set; } = new();
    public int ViewCount { get; set; }

    public class PortfolioImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}