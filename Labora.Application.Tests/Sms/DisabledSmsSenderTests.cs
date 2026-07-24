using Labora.Domain.Exceptions;
using Labora.Infrastructure.Services;

namespace Labora.Application.Tests.Sms;

public class DisabledSmsSenderTests
{
    [Fact]
    public async Task SendAsync_AlwaysThrowsSmsProviderUnavailableException()
    {
        DisabledSmsSender sender = new();

        SmsProviderUnavailableException exception = await Assert.ThrowsAsync<SmsProviderUnavailableException>(
            () => sender.SendAsync("+998901234567", "test message"));

        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
    }

    [Fact]
    public async Task SendAsync_MessageDoesNotExposeConfigurationOrCredentialDetails()
    {
        DisabledSmsSender sender = new();

        SmsProviderUnavailableException exception = await Assert.ThrowsAsync<SmsProviderUnavailableException>(
            () => sender.SendAsync("+998901234567", "test message"));

        Assert.DoesNotContain("Email", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("eskiz.uz", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
