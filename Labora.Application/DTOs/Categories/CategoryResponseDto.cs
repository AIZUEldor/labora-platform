using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Categories;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public JobType JobType { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryResponseDto> SubCategories { get; set; } = new List<CategoryResponseDto>();
    public DateTime CreatedAt { get; set; }
}