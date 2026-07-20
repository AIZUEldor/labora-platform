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

    /// <summary>
    /// Pre-checks phone uniqueness, hashes the password, and starts an OTP-protected registration by
    /// handing IOtpService.StartOtpAsync a serialized RegistrationPayloadDto (never the raw request,
    /// never a plaintext password) as the OtpPurpose.Registration flow's server-trusted payload.
    /// </summary>
    Task<StartOtpResponseDto> RegisterStartAsync(RegisterStartRequestDto request);

    Task<ResendOtpResponseDto> RegisterResendAsync(RegisterResendRequestDto request);

    Task<VerifyOtpResponseDto> RegisterVerifyAsync(RegisterVerifyRequestDto request);

    /// <summary>
    /// Consumes the operation token for OtpPurpose.Registration and, only if that succeeds, creates the
    /// User exclusively from the RegistrationPayloadDto deserialized out of the consumed verification -
    /// request carries only VerificationId/OperationToken, so there is no client-supplied registration
    /// field this method could accidentally trust instead. Fails closed on a missing, malformed, or
    /// phone-inconsistent payload, and re-checks phone uniqueness after consume before creating the user.
    /// </summary>
    Task<AuthResponseDto> RegisterCompleteAsync(RegisterCompleteRequestDto request);
}