using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;

namespace Labora.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Starts an OTP-protected forgot-password flow for the given phone number. Returns the same
    /// StartOtpResponseDto shape whether or not the phone number belongs to a registered account -
    /// registered numbers get a real OTP via IOtpService.StartOtpAsync, unregistered numbers get a
    /// non-persisted decoy via IOtpService.PrepareDecoyStartAsync - so a caller cannot distinguish
    /// "registered" from "unregistered" from this response alone.
    /// </summary>
    Task<StartOtpResponseDto> ForgotPasswordStartAsync(ForgotPasswordStartRequestDto request);

    Task<ResendOtpResponseDto> ForgotPasswordResendAsync(ForgotPasswordResendRequestDto request);

    Task<VerifyOtpResponseDto> ForgotPasswordVerifyAsync(ForgotPasswordVerifyRequestDto request);

    /// <summary>
    /// Consumes the operation token for OtpPurpose.ForgotPassword and, only if that succeeds, updates
    /// the password of the user identified by the consumed verification's own UserId - never by any
    /// client-supplied phone number.
    /// </summary>
    Task<ForgotPasswordCompleteResponseDto> ForgotPasswordCompleteAsync(ForgotPasswordCompleteRequestDto request);
}