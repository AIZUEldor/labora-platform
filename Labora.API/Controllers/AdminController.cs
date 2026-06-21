using Labora.Application.DTOs.Admin;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        IEnumerable<AdminUserDto> users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserRequestDto request)
    {
        AdminUserDto user = await _adminService.BlockUserAsync(id, request.IsBlocked);
        return Ok(user);
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        IEnumerable<AdminJobDto> jobs = await _adminService.GetAllJobsAsync();
        return Ok(jobs);
    }

    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        await _adminService.DeleteJobAsync(id);
        return Ok(new { message = "Ish muvaffaqiyatli o'chirildi." });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        AdminStatsDto stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }
}