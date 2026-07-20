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

    /// <summary>
    /// Validation only - no status change, no send, no verify-attempt increment, no token creation, no
    /// database write. Throws the same generic "verification not found" exception KeyNotFoundException
    /// used elsewhere in this service whenever the verification is missing, has a different Purpose, has
    /// no UserId, or has a UserId different from expectedUserId - all four cases are indistinguishable to
    /// the caller. Intended for an authenticated caller (e.g. UserController's Change Phone flow) to
    /// confirm a VerificationId it did not itself create actually belongs to it before calling
    /// ResendOtpAsync or VerifyOtpAsync, without exposing OTP persistence details outside this service.
    /// </summary>
    Task EnsureVerificationOwnershipAsync(
        Guid verificationId,
        OtpPurpose expectedPurpose,
        Guid expectedUserId);
}
