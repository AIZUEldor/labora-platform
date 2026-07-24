using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

/// <summary>
/// The "account" object Payme sends/expects. Payme does not mandate a field name for this object -
/// the merchant defines it. ALP Jobs' checkout URL design (see architecture doc) fixes this field to
/// "order_id", carrying PaymentOrder.Id as the merchant order reference.
/// </summary>
public class PaymeAccountDto
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; } = string.Empty;
}
