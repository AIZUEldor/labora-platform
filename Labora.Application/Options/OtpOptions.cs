using System.ComponentModel.DataAnnotations;

namespace Labora.Application.Options;

public class OtpOptions
{
    public const string SectionName = "Otp";

    [Range(1, 60)]
    public int CodeExpiryMinutes { get; set; } = 5;

    [Range(1, 120)]
    public int OperationTokenExpiryMinutes { get; set; }

    [Range(1, 3600)]
    public int ResendCooldownSeconds { get; set; } = 60;

    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 5;

    [Range(1, 20)]
    public int MaxSendsPerWindow { get; set; }

    [Range(1, 1440)]
    public int RateLimitWindowMinutes { get; set; }

    [Range(1, 3600)]
    public int StaleIssuingSeconds { get; set; } = 120;
}
