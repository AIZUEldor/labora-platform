using AutoMapper;
using Labora.Application.DTOs.Categories;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto request)
    {
        Category category = _mapper.Map<Category>(request);
        Category createdCategory = await _categoryRepository.AddAsync(category);
        return _mapper.Map<CategoryResponseDto>(createdCategory);
    }

    public async Task<CategoryResponseDto> GetByIdAsync(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new InvalidOperationException($"Id={id} bo'lgan kategoriya topilmadi.");

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        IEnumerable<Category> categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetRootCategoriesAsync()
    {
        IEnumerable<Category> categories = await _categoryRepository.GetMainCategoriesAsync();
        return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
    }

    public async Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryRequestDto request)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new InvalidOperationException($"Id={id} bo'lgan kategoriya topilmadi.");

        _mapper.Map(request, category);
        Category updatedCategory = await _categoryRepository.UpdateAsync(category);
        return _mapper.Map<CategoryResponseDto>(updatedCategory);
    }

    public async Task DeleteAsync(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new InvalidOperationException($"Id={id} bo'lgan kategoriya topilmadi.");

        await _categoryRepository.DeleteAsync(id);
    }
}