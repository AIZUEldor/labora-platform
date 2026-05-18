using Labora.Application.DTOs.Auth;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            AuthResponseDto response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            return BadRequest(new { message = invalidOperationException.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            AuthResponseDto response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            return BadRequest(new { message = invalidOperationException.Message });
        }
    }
}