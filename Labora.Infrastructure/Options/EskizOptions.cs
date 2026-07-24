namespace Labora.Infrastructure.Options;

public class EskizOptions
{
    public const string SectionName = "Eskiz";

    // Defaults to false so a missing/absent "Eskiz" config section (the current production state -
    // the Eskiz contract is not signed yet) starts up safely with a no-op sender instead of crashing
    // on missing credentials. Email/Password/SenderName are only required when this is true - see
    // the conditional .Validate(...) calls in DependencyInjection.AddInfrastructure, which replace
    // the [Required] attributes this class used to carry.
    public bool Enabled { get; set; } = false;

    public string BaseUrl { get; set; } = "https://notify.eskiz.uz";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;
}
