namespace Labora.Application.Options;

public class IdentifierHashOptions
{
    public const string SectionName = "OtpIdentity";

    public string Pepper { get; set; } = string.Empty;
}
