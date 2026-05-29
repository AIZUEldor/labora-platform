using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushNotificationsController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;

    public PushNotificationsController(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenDto dto)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _pushNotificationService.RegisterTokenAsync(userId, dto.Token, dto.DeviceType);
        return NoContent();
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveToken()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _pushNotificationService.RemoveTokenAsync(userId);
        return NoContent();
    }
}

public record RegisterTokenDto(string Token, string? DeviceType);