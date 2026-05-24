using FluentValidation;
using Labora.Application.DTOs.Applications;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] ApplicationRequestDto request)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ApplicationResponseDto response = await _jobApplicationService.ApplyAsync(request, workerId);
        return Ok(response);
    }

    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<ApplicationResponseDto> applications = await _jobApplicationService.GetByWorkerIdAsync(workerId);
        return Ok(applications);
    }

    [HttpGet("job/{jobId:guid}")]
    public async Task<IActionResult> GetByJobId(Guid jobId)
    {
        IEnumerable<ApplicationResponseDto> applications = await _jobApplicationService.GetByJobIdAsync(jobId);
        return Ok(applications);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ApplicationResponseDto response = await _jobApplicationService.UpdateStatusAsync(id, status, employerId);
        return Ok(response);
    }

    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        Guid workerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _jobApplicationService.CancelAsync(id, workerId);
        return NoContent();
    }
}