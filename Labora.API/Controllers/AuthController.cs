using FluentValidation;
using Labora.Application.DTOs.Auth;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        FluentValidation.Results.ValidationResult validationResult =
            await _registerValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

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
        FluentValidation.Results.ValidationResult validationResult =
            await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

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