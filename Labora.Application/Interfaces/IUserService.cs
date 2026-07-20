using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;
using Labora.Application.DTOs.Users;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponseDto> GetProfileAsync(Guid userId);
    Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<string> UploadCvAsync(Guid userId, IFormFile file);
    Task<string> UploadAvatarAsync(Guid userId, IFormFile file);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task DeleteAccountAsync(Guid userId);

    /// <summary>
    /// Re-verifies the authenticated user's own CurrentPassword (same check ChangePasswordAsync already
    /// uses), normalizes NewPhoneNumber, rejects a no-op change to the same number, pre-checks phone
    /// uniqueness, then starts an OtpPurpose.ChangePhoneNumber flow bound to the authenticated user's own
    /// Id with the OTP sent to the NEW number only. No JWT is issued here.
    /// </summary>
    Task<StartOtpResponseDto> ChangePhoneStartAsync(Guid userId, ChangePhoneStartRequestDto request);

    /// <summary>
    /// Before delegating to IOtpService.ResendOtpAsync, verifies the identified verification exists,
    /// has Purpose == OtpPurpose.ChangePhoneNumber, and has UserId equal to the authenticated userId -
    /// any other verification's Id (missing, wrong purpose, or owned by someone else) is rejected with
    /// the same generic not-found error IOtpService itself already uses, so no distinction leaks.
    /// </summary>
    Task<ResendOtpResponseDto> ChangePhoneResendAsync(Guid userId, ChangePhoneResendRequestDto request);

    /// <summary>
    /// Same ownership pre-check as ChangePhoneResendAsync before delegating to IOtpService.VerifyOtpAsync.
    /// </summary>
    Task<VerifyOtpResponseDto> ChangePhoneVerifyAsync(Guid userId, ChangePhoneVerifyRequestDto request);

    /// <summary>
    /// Consumes the operation token for OtpPurpose.ChangePhoneNumber and requires consumeResult.UserId to
    /// equal the authenticated userId - any mismatch (including null) is treated as an invalid operation
    /// token, so one authenticated user can never consume another user's phone-change token. The new
    /// phone number is taken only from the trusted consumed verification's own PhoneNumber, never from
    /// the request. Re-checks uniqueness immediately before updating, then returns a fresh JWT reflecting
    /// the new phone number.
    /// </summary>
    Task<AuthResponseDto> ChangePhoneCompleteAsync(Guid userId, ChangePhoneCompleteRequestDto request);
}