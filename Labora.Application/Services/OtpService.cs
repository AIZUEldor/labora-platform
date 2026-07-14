using Labora.Application.DTOs.Otp;
using Labora.Application.Interfaces;
using Labora.Application.Options;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Labora.Application.Services;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IOtpSecurityService _otpSecurityService;
    private readonly ISmsSender _smsSender;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<OtpOptions> _otpOptions;

    public OtpService(
        IOtpRepository otpRepository,
        IOtpSecurityService otpSecurityService,
        ISmsSender smsSender,
        IPhoneNumberNormalizer phoneNumberNormalizer,
        IUnitOfWork unitOfWork,
        IOptions<OtpOptions> otpOptions)
    {
        _otpRepository = otpRepository;
        _otpSecurityService = otpSecurityService;
        _smsSender = smsSender;
        _phoneNumberNormalizer = phoneNumberNormalizer;
        _unitOfWork = unitOfWork;
        _otpOptions = otpOptions;
    }

    public Task EnforceRateLimitAsync(string phoneNumber, OtpPurpose purpose)
    {
        throw new NotImplementedException();
    }

    public Task<StartOtpResponseDto> StartOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? userId = null,
        string? registrationPayload = null)
    {
        throw new NotImplementedException();
    }

    public Task<StartOtpResponseDto> PrepareDecoyStartAsync(
        string phoneNumber,
        OtpPurpose purpose)
    {
        throw new NotImplementedException();
    }

    public Task<ResendOtpResponseDto> ResendOtpAsync(Guid verificationId)
    {
        throw new NotImplementedException();
    }

    public Task<VerifyOtpResponseDto> VerifyOtpAsync(
        Guid verificationId,
        string code)
    {
        throw new NotImplementedException();
    }

    public Task<OtpConsumeResultDto> ConsumeOtpAsync(
        Guid verificationId,
        string? operationToken = null)
    {
        throw new NotImplementedException();
    }
}
