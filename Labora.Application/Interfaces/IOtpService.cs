using Labora.Application.DTOs.Otp;
using Labora.Domain.Enums;

namespace Labora.Application.Interfaces;

public interface IOtpService
{
    Task EnforceRateLimitAsync(string phoneNumber, OtpPurpose purpose);

    Task<StartOtpResponseDto> StartOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? userId = null,
        string? registrationPayload = null);

    Task<StartOtpResponseDto> PrepareDecoyStartAsync(
        string phoneNumber,
        OtpPurpose purpose);

    Task<ResendOtpResponseDto> ResendOtpAsync(Guid verificationId);

    Task<VerifyOtpResponseDto> VerifyOtpAsync(
        Guid verificationId,
        string code);

    /// <summary>
    /// Validates and consumes the operation token produced by a prior successful VerifyOtpAsync call,
    /// transitioning the verification from Verified to Consumed exactly once. expectedPurpose must
    /// match the verification's own Purpose - a caller (e.g. a registration flow) uses this to assert
    /// it is consuming a token that was actually issued for that specific purpose, not one issued for
    /// a different flow (e.g. ForgotPassword) that happens to share a valid token/id pair.
    /// </summary>
    Task<OtpConsumeResultDto> ConsumeOtpAsync(
        Guid verificationId,
        OtpPurpose expectedPurpose,
        string? operationToken = null);
}
