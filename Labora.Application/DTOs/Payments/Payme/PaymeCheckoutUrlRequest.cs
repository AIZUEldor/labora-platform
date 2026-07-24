namespace Labora.Application.DTOs.Payments.Payme;

public class PaymeCheckoutUrlRequest
{
    public string MerchantId { get; set; } = string.Empty;
    public long AmountInTiyin { get; set; }
    public string AccountFieldName { get; set; } = string.Empty;
    public string AccountValue { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? ReturnUrl { get; set; }
    public int? RedirectTimeoutMilliseconds { get; set; }
    public string? CurrencyCode { get; set; }
}
