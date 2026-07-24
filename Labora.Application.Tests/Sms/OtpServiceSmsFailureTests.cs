using Labora.Application.Options;
using Labora.Application.Services;
using Labora.Application.Tests.Payme;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace Labora.Application.Tests.Sms;

public class OtpServiceSmsFailureTests
{
    private static OtpService CreateService(FakeOtpRepository repository, ThrowingSmsSender smsSender)
    {
        IOptions<OtpOptions> options = Microsoft.Extensions.Options.Options.Create(new OtpOptions
        {
            MaxSendsPerWindow = 5,
            RateLimitWindowMinutes = 60,
            OperationTokenExpiryMinutes = 15,
        });

        return new OtpService(
            repository,
            new FakeOtpSecurityService(),
            smsSender,
            new FakePhoneNumberNormalizer(),
            new PassThroughUnitOfWork(),
            options,
            new FakeOtpAbuseGuard(),
            new FakeOtpRequestContextProvider());
    }

    [Fact]
    public async Task StartOtpAsync_SmsSenderThrowsSmsProviderUnavailableException_VerificationBecomesFailedAndExceptionPropagates()
    {
        FakeOtpRepository repository = new();
        ThrowingSmsSender smsSender = new(new SmsProviderUnavailableException("SMS sending is currently disabled."));
        OtpService service = CreateService(repository, smsSender);

        await Assert.ThrowsAsync<SmsProviderUnavailableException>(
            () => service.StartOtpAsync("+998901234567", OtpPurpose.Login));

        OtpVerification persisted = Assert.Single(repository.Rows);
        Assert.Equal(OtpStatus.Failed, persisted.Status);
    }

    [Fact]
    public async Task StartOtpAsync_SmsSenderThrows_FailedStateIsPersistedViaRepositoryUpdate()
    {
        FakeOtpRepository repository = new();
        ThrowingSmsSender smsSender = new(new SmsProviderUnavailableException("SMS sending is currently disabled."));
        OtpService service = CreateService(repository, smsSender);

        try
        {
            await service.StartOtpAsync("+998901234567", OtpPurpose.Login);
        }
        catch (SmsProviderUnavailableException)
        {
            // expected - asserting persisted state below, not the exception itself here.
        }

        OtpVerification persisted = Assert.Single(repository.Rows);
        Assert.Equal(OtpStatus.Failed, persisted.Status);
    }
}
