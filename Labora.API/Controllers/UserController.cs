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

    public UserController(IUserService userService)
    {
        _userService = userService;
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
}