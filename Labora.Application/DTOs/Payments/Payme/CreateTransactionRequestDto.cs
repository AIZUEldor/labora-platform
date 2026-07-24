using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CreateTransactionRequestDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Timestamp unit (seconds vs. milliseconds) was not confirmed by fetched documentation.</summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("account")]
    public PaymeAccountDto Account { get; set; } = new();
}
