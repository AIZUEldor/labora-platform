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

    [Range(1, 3600)]
    public int SmsMinIntervalSeconds { get; set; } = 60;

    [Range(1, 20)]
    public int MaxSmsPerHour { get; set; } = 5;

    [Range(1, 100)]
    public int MaxSmsPerDay { get; set; } = 10;

    [Range(1, 720)]
    public int FirstBlockDurationHours { get; set; } = 6;

    [Range(1, 720)]
    public int EscalatedBlockDurationHours { get; set; } = 24;

    [Range(1, 50)]
    public int CrossNumberDistinctPhoneLimit { get; set; } = 5;

    [Range(1, 1440)]
    public int CrossNumberDistinctPhoneWindowMinutes { get; set; } = 10;

    [Range(1, 100)]
    public int CrossNumberStartVolumeLimit { get; set; } = 10;

    [Range(1, 1440)]
    public int CrossNumberStartVolumeWindowMinutes { get; set; } = 60;

    [Range(1, 730)]
    public int ViolationDecayDays { get; set; } = 90;
}
