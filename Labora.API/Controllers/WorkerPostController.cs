using Labora.Application.DTOs.WorkerPosts;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkerPostController : ControllerBase
{
    private readonly IWorkerPostService _workerPostService;

    public WorkerPostController(IWorkerPostService workerPostService)
    {
        _workerPostService = workerPostService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? subCategoryId,
        [FromQuery] string? city)
    {
        List<WorkerPostResponseDto> result = await _workerPostService.GetAllAsync(categoryId, subCategoryId, city);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        WorkerPostResponseDto? result = await _workerPostService.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyPosts()
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        List<WorkerPostResponseDto> result = await _workerPostService.GetMyPostsAsync(workerId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateWorkerPostRequestDto dto)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        WorkerPostResponseDto result = await _workerPostService.CreateAsync(workerId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkerPostRequestDto dto)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        WorkerPostResponseDto result = await _workerPostService.UpdateAsync(workerId, id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _workerPostService.DeleteAsync(workerId, id);
        return NoContent();
    }

    [HttpPost("{id}/images")]
    [Authorize]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, [FromForm] string? caption)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        WorkerPortfolioImage result = await _workerPostService.AddPortfolioImageAsync(id, workerId, file, caption);
        return Ok(result);
    }

    [HttpDelete("{id}/images/{imageId}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _workerPostService.DeletePortfolioImageAsync(id, workerId, imageId);
        return NoContent();
    }

    [HttpPatch("{id}/view")]
    public async Task<IActionResult> IncrementView(Guid id)
    {
        await _workerPostService.IncrementViewCountAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/contact")]
    [Authorize]
    public async Task<IActionResult> ContactWorker(Guid id)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _workerPostService.ContactWorkerAsync(id, employerId);
        return NoContent();
    }
}