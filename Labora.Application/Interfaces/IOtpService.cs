using Labora.Application.DTOs.Otp;

namespace Labora.Application.Interfaces;

public interface IOtpService
{
    Task<StartOtpResponseDto> StartOtpAsync(StartOtpRequestDto request);
    Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpRequestDto request);
    Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request);
    Task<ForgotPasswordCompleteResponseDto> ForgotPasswordCompleteAsync(ForgotPasswordCompleteRequestDto request);
}
