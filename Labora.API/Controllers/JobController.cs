using FluentValidation;
using FluentValidation.Results;
using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IValidator<JobRequestDto> _jobValidator;

    public JobController(IJobService jobService, IValidator<JobRequestDto> jobValidator)
    {
        _jobService = jobService;
        _jobValidator = jobValidator;
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Create([FromBody] JobRequestDto request)
    {
        ValidationResult validationResult = await _jobValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        JobResponseDto response = await _jobService.CreateAsync(request, employerId);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<JobResponseDto> jobs = await _jobService.GetAllAsync();
        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        JobResponseDto job = await _jobService.GetByIdAsync(id);
        return Ok(job);
    }

    [HttpGet("my-jobs")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMyJobs()
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        IEnumerable<JobResponseDto> jobs = await _jobService.GetByEmployerIdAsync(employerId);
        return Ok(jobs);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JobRequestDto request)
    {
        ValidationResult validationResult = await _jobValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        JobResponseDto response = await _jobService.UpdateAsync(id, request, employerId);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid employerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _jobService.DeleteAsync(id, employerId);
        return NoContent();
    }
}