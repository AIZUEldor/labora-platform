using Labora.Application.DTOs.Categories;

namespace Labora.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponseDto> CreateAsync(CategoryRequestDto request);
    Task<CategoryResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
    Task<IEnumerable<CategoryResponseDto>> GetRootCategoriesAsync();
    Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryRequestDto request);
    Task DeleteAsync(Guid id);
}