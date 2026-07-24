using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PaymeReceiverDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }
}
