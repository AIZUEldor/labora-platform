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

    Task<OtpConsumeResultDto> ConsumeOtpAsync(
        Guid verificationId,
        string? operationToken = null);
}
