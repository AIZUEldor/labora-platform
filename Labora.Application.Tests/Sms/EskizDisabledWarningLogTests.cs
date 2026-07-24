using Labora.API.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Labora.Application.Tests.Sms;

public class EskizDisabledWarningLogTests
{
    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }

    private static IConfiguration BuildConfiguration(IDictionary<string, string?> eskizValues)
        => new ConfigurationBuilder().AddInMemoryCollection(eskizValues).Build();

    [Fact]
    public void WarnIfDisabled_EnabledMissing_LogsExactlyOneWarning()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>());
        CapturingLogger logger = new();

        EskizStartupDiagnostics.WarnIfDisabled(configuration, logger);

        (LogLevel _, string message) = Assert.Single(logger.Entries);
        Assert.Equal("Eskiz SMS provider is disabled. OTP SMS delivery is unavailable.", message);
    }

    [Fact]
    public void WarnIfDisabled_EnabledFalse_LogsExactlyOneWarning()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        });
        CapturingLogger logger = new();

        EskizStartupDiagnostics.WarnIfDisabled(configuration, logger);

        (LogLevel _, string message) = Assert.Single(logger.Entries);
        Assert.Equal("Eskiz SMS provider is disabled. OTP SMS delivery is unavailable.", message);
    }

    [Fact]
    public void WarnIfDisabled_WarningDoesNotExposeConfigurationOrCredentialDetails()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "false"
        });
        CapturingLogger logger = new();

        EskizStartupDiagnostics.WarnIfDisabled(configuration, logger);

        string combined = string.Join(Environment.NewLine, logger.Entries.Select(e => e.Message));
        Assert.DoesNotContain("Email", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("eskiz.uz", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WarnIfDisabled_EnabledTrue_DoesNotLogWarning()
    {
        IConfiguration configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Eskiz:Enabled"] = "true",
            ["Eskiz:Email"] = "ops@labora.uz",
            ["Eskiz:Password"] = "secret",
            ["Eskiz:SenderName"] = "Labora",
        });
        CapturingLogger logger = new();

        EskizStartupDiagnostics.WarnIfDisabled(configuration, logger);

        Assert.Empty(logger.Entries);
    }
}
