using System.ComponentModel.DataAnnotations;

namespace Labora.Infrastructure.Options;

public class EskizOptions
{
    public const string SectionName = "Eskiz";

    public string BaseUrl { get; set; } = "https://notify.eskiz.uz";

    [Required(AllowEmptyStrings = false)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string SenderName { get; set; } = string.Empty;
}
