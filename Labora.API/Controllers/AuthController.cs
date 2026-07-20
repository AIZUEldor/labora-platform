using FluentValidation;
using FluentValidation.Results;
using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;
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
    private readonly IValidator<ForgotPasswordStartRequestDto> _forgotPasswordStartValidator;
    private readonly IValidator<ForgotPasswordResendRequestDto> _forgotPasswordResendValidator;
    private readonly IValidator<ForgotPasswordVerifyRequestDto> _forgotPasswordVerifyValidator;
    private readonly IValidator<ForgotPasswordCompleteRequestDto> _forgotPasswordCompleteValidator;
    private readonly IValidator<RegisterStartRequestDto> _registerStartValidator;
    private readonly IValidator<RegisterResendRequestDto> _registerResendValidator;
    private readonly IValidator<RegisterVerifyRequestDto> _registerVerifyValidator;
    private readonly IValidator<RegisterCompleteRequestDto> _registerCompleteValidator;
    private readonly IValidator<LoginResendRequestDto> _loginResendValidator;
    private readonly IValidator<LoginVerifyRequestDto> _loginVerifyValidator;
    private readonly IValidator<LoginCompleteRequestDto> _loginCompleteValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ForgotPasswordStartRequestDto> forgotPasswordStartValidator,
        IValidator<ForgotPasswordResendRequestDto> forgotPasswordResendValidator,
        IValidator<ForgotPasswordVerifyRequestDto> forgotPasswordVerifyValidator,
        IValidator<ForgotPasswordCompleteRequestDto> forgotPasswordCompleteValidator,
        IValidator<RegisterStartRequestDto> registerStartValidator,
        IValidator<RegisterResendRequestDto> registerResendValidator,
        IValidator<RegisterVerifyRequestDto> registerVerifyValidator,
        IValidator<RegisterCompleteRequestDto> registerCompleteValidator,
        IValidator<LoginResendRequestDto> loginResendValidator,
        IValidator<LoginVerifyRequestDto> loginVerifyValidator,
        IValidator<LoginCompleteRequestDto> loginCompleteValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordStartValidator = forgotPasswordStartValidator;
        _forgotPasswordResendValidator = forgotPasswordResendValidator;
        _forgotPasswordVerifyValidator = forgotPasswordVerifyValidator;
        _forgotPasswordCompleteValidator = forgotPasswordCompleteValidator;
        _registerStartValidator = registerStartValidator;
        _registerResendValidator = registerResendValidator;
        _registerVerifyValidator = registerVerifyValidator;
        _registerCompleteValidator = registerCompleteValidator;
        _loginResendValidator = loginResendValidator;
        _loginVerifyValidator = loginVerifyValidator;
        _loginCompleteValidator = loginCompleteValidator;
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

    [HttpPost("forgot-password/start")]
    public async Task<IActionResult> ForgotPasswordStart([FromBody] ForgotPasswordStartRequestDto request)
    {
        ValidationResult validationResult = await _forgotPasswordStartValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        StartOtpResponseDto response = await _authService.ForgotPasswordStartAsync(request);
        return Ok(response);
    }

    [HttpPost("forgot-password/resend")]
    public async Task<IActionResult> ForgotPasswordResend([FromBody] ForgotPasswordResendRequestDto request)
    {
        ValidationResult validationResult = await _forgotPasswordResendValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        ResendOtpResponseDto response = await _authService.ForgotPasswordResendAsync(request);
        return Ok(response);
    }

    [HttpPost("forgot-password/verify")]
    public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyRequestDto request)
    {
        ValidationResult validationResult = await _forgotPasswordVerifyValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        VerifyOtpResponseDto response = await _authService.ForgotPasswordVerifyAsync(request);
        return Ok(response);
    }

    [HttpPost("forgot-password/complete")]
    public async Task<IActionResult> ForgotPasswordComplete([FromBody] ForgotPasswordCompleteRequestDto request)
    {
        ValidationResult validationResult = await _forgotPasswordCompleteValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        ForgotPasswordCompleteResponseDto response = await _authService.ForgotPasswordCompleteAsync(request);
        return Ok(response);
    }

    [HttpPost("register/start")]
    public async Task<IActionResult> RegisterStart([FromBody] RegisterStartRequestDto request)
    {
        ValidationResult validationResult = await _registerStartValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        StartOtpResponseDto response = await _authService.RegisterStartAsync(request);
        return Ok(response);
    }

    [HttpPost("register/resend")]
    public async Task<IActionResult> RegisterResend([FromBody] RegisterResendRequestDto request)
    {
        ValidationResult validationResult = await _registerResendValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        ResendOtpResponseDto response = await _authService.RegisterResendAsync(request);
        return Ok(response);
    }

    [HttpPost("register/verify")]
    public async Task<IActionResult> RegisterVerify([FromBody] RegisterVerifyRequestDto request)
    {
        ValidationResult validationResult = await _registerVerifyValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        VerifyOtpResponseDto response = await _authService.RegisterVerifyAsync(request);
        return Ok(response);
    }

    [HttpPost("register/complete")]
    public async Task<IActionResult> RegisterComplete([FromBody] RegisterCompleteRequestDto request)
    {
        ValidationResult validationResult = await _registerCompleteValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        AuthResponseDto response = await _authService.RegisterCompleteAsync(request);
        return Ok(response);
    }

    [HttpPost("login/start")]
    public async Task<IActionResult> LoginStart([FromBody] LoginRequestDto request)
    {
        ValidationResult validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        StartOtpResponseDto response = await _authService.LoginStartAsync(request);
        return Ok(response);
    }

    [HttpPost("login/resend")]
    public async Task<IActionResult> LoginResend([FromBody] LoginResendRequestDto request)
    {
        ValidationResult validationResult = await _loginResendValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        ResendOtpResponseDto response = await _authService.LoginResendAsync(request);
        return Ok(response);
    }

    [HttpPost("login/verify")]
    public async Task<IActionResult> LoginVerify([FromBody] LoginVerifyRequestDto request)
    {
        ValidationResult validationResult = await _loginVerifyValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        VerifyOtpResponseDto response = await _authService.LoginVerifyAsync(request);
        return Ok(response);
    }

    [HttpPost("login/complete")]
    public async Task<IActionResult> LoginComplete([FromBody] LoginCompleteRequestDto request)
    {
        ValidationResult validationResult = await _loginCompleteValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        AuthResponseDto response = await _authService.LoginCompleteAsync(request);
        return Ok(response);
    }
}