using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Categories;

public class CategoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public JobType JobType { get; set; }
    public Guid? ParentCategoryId { get; set; }
}