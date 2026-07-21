namespace Labora.Application.Options;

public class OtpSecurityOptions
{
    public const string SectionName = "Otp";

    public string Pepper { get; set; } = string.Empty;
}
