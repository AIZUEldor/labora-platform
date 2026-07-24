using Labora.Application.Interfaces;
using Labora.Domain.Exceptions;

namespace Labora.Infrastructure.Services;

/// <summary>
/// Registered instead of EskizSmsSender when Eskiz:Enabled is false (see DependencyInjection.cs).
/// Performs zero I/O - the Eskiz contract is not signed yet, and no outbound request to the real
/// provider may occur while disabled, even with empty/placeholder credentials.
/// </summary>
public sealed class DisabledSmsSender : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message)
        => throw new SmsProviderUnavailableException("SMS sending is currently disabled.");
}
