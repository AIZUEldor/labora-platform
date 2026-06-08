using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedJobController : ControllerBase
{
    private readonly ISavedJobService _savedJobService;

    public SavedJobController(ISavedJobService savedJobService)
    {
        _savedJobService = savedJobService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedJobs()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        List<Application.DTOs.Jobs.JobResponseDto> jobs = await _savedJobService.GetSavedJobsAsync(userId);
        return Ok(jobs);
    }

    [HttpPost("{jobId}")]
    public async Task<IActionResult> SaveJob(Guid jobId)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _savedJobService.SaveJobAsync(userId, jobId);
        return Ok(new { message = "Ish saqlandi." });
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> UnsaveJob(Guid jobId)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _savedJobService.UnsaveJobAsync(userId, jobId);
        return Ok(new { message = "Ish o'chirildi." });
    }

    [HttpGet("{jobId}/check")]
    public async Task<IActionResult> IsJobSaved(Guid jobId)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool isSaved = await _savedJobService.IsJobSavedAsync(userId, jobId);
        return Ok(new { isSaved });
    }
}