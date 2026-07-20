using FluentValidation;
using FluentValidation.Results;
using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;
using Labora.Application.DTOs.Users;
using Labora.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<ChangePhoneStartRequestDto> _changePhoneStartValidator;
    private readonly IValidator<ChangePhoneResendRequestDto> _changePhoneResendValidator;
    private readonly IValidator<ChangePhoneVerifyRequestDto> _changePhoneVerifyValidator;
    private readonly IValidator<ChangePhoneCompleteRequestDto> _changePhoneCompleteValidator;

    public UserController(
        IUserService userService,
        IValidator<ChangePhoneStartRequestDto> changePhoneStartValidator,
        IValidator<ChangePhoneResendRequestDto> changePhoneResendValidator,
        IValidator<ChangePhoneVerifyRequestDto> changePhoneVerifyValidator,
        IValidator<ChangePhoneCompleteRequestDto> changePhoneCompleteValidator)
    {
        _userService = userService;
        _changePhoneStartValidator = changePhoneStartValidator;
        _changePhoneResendValidator = changePhoneResendValidator;
        _changePhoneVerifyValidator = changePhoneVerifyValidator;
        _changePhoneCompleteValidator = changePhoneCompleteValidator;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserProfileResponseDto response = await _userService.GetProfileAsync(userId);
        return Ok(response);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserProfileResponseDto response = await _userService.UpdateProfileAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("upload-cv")]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        string url = await _userService.UploadCvAsync(userId, file);
        return Ok(new { url });
    }

    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        string url = await _userService.UploadAvatarAsync(userId, file);
        return Ok(new { url });
    }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Parol muvaffaqiyatli o'zgartirildi." });
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.DeleteAccountAsync(userId);

        return Ok(new { message = "Hisob muvaffaqiyatli o'chirildi." });
    }

    /// <summary>
    /// Disabled: this endpoint used to reset a password from an unverified phone number with no proof
    /// of ownership. It no longer calls IUserService.ForgotPasswordAsync at all - the vulnerable code
    /// path is unreachable from here - and never looks anything up by phone, so this response reveals
    /// nothing about whether request.PhoneNumber belongs to an account. Use the OTP-protected flow at
    /// AuthController's /api/auth/forgot-password/start instead.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        return StatusCode(StatusCodes.Status410Gone, new
        {
            statusCode = StatusCodes.Status410Gone,
            message = "Bu endpoint endi ishlamaydi. Parolni tiklash uchun OTP orqali parolni tiklash oqimidan (/api/auth/forgot-password/start) foydalaning."
        });
    }

    [HttpPost("change-phone/start")]
    public async Task<IActionResult> ChangePhoneStart([FromBody] ChangePhoneStartRequestDto request)
    {
        ValidationResult validationResult = await _changePhoneStartValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        StartOtpResponseDto response = await _userService.ChangePhoneStartAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("change-phone/resend")]
    public async Task<IActionResult> ChangePhoneResend([FromBody] ChangePhoneResendRequestDto request)
    {
        ValidationResult validationResult = await _changePhoneResendValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ResendOtpResponseDto response = await _userService.ChangePhoneResendAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("change-phone/verify")]
    public async Task<IActionResult> ChangePhoneVerify([FromBody] ChangePhoneVerifyRequestDto request)
    {
        ValidationResult validationResult = await _changePhoneVerifyValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        VerifyOtpResponseDto response = await _userService.ChangePhoneVerifyAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("change-phone/complete")]
    public async Task<IActionResult> ChangePhoneComplete([FromBody] ChangePhoneCompleteRequestDto request)
    {
        ValidationResult validationResult = await _changePhoneCompleteValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        AuthResponseDto response = await _userService.ChangePhoneCompleteAsync(userId, request);
        return Ok(response);
    }
}