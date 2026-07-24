using Labora.Infrastructure.Options;

namespace Labora.API.Startup;

public static class EskizStartupDiagnostics
{
    public static void WarnIfDisabled(IConfiguration configuration, ILogger logger)
    {
        bool eskizEnabled = configuration.GetValue<bool>(
            $"{EskizOptions.SectionName}:{nameof(EskizOptions.Enabled)}");

        if (!eskizEnabled)
        {
            logger.LogWarning("Eskiz SMS provider is disabled. OTP SMS delivery is unavailable.");
        }
    }
}
