using Labora.Application.DTOs.Categories;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CategoryRequestDto request)
    {
        CategoryResponseDto response = await _categoryService.CreateAsync(request);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<CategoryResponseDto> categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories()
    {
        IEnumerable<CategoryResponseDto> categories = await _categoryService.GetRootCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        CategoryResponseDto category = await _categoryService.GetByIdAsync(id);
        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequestDto request)
    {
        CategoryResponseDto response = await _categoryService.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}