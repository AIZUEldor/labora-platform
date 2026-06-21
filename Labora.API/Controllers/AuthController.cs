using FluentValidation;
using FluentValidation.Results;
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
        ValidationResult validationResult = await _registerValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        AuthResponseDto response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        ValidationResult validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        AuthResponseDto response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("debug-hash")]
    public IActionResult DebugHash([FromBody] string password)
    {
        byte[] salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        string result = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        return Ok(new { hash = result });
    }
}