using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CreateTransactionResponseDto
{
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    [JsonPropertyName("transaction")]
    public string Transaction { get; set; } = string.Empty;

    /// <summary>Exact enum values for Payme's transaction state were not confirmed by fetched documentation.</summary>
    [JsonPropertyName("state")]
    public int State { get; set; }

    [JsonPropertyName("receivers")]
    public List<PaymeReceiverDto>? Receivers { get; set; }
}
