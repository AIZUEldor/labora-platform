using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JobRequestDto request)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        JobResponseDto response = await _jobService.CreateAsync(request, employerId);
        return Ok(response);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<JobResponseDto> jobs = await _jobService.GetAllAsync();
        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        JobResponseDto job = await _jobService.GetByIdAsync(id);
        return Ok(job);
    }

    [HttpGet("my-jobs")]
    public async Task<IActionResult> GetMyJobs()
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<JobResponseDto> jobs = await _jobService.GetByEmployerIdAsync(employerId);
        return Ok(jobs);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JobRequestDto request)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        JobResponseDto response = await _jobService.UpdateAsync(id, request, employerId);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _jobService.DeleteAsync(id, employerId);
        return NoContent();
    }
}