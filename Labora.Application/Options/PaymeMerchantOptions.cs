using System.ComponentModel.DataAnnotations;

namespace Labora.Application.Options;

public class PaymeMerchantOptions
{
    public const string SectionName = "Payme";

    [Required(AllowEmptyStrings = false)]
    public string MerchantId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Login { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;
}
