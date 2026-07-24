using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CancelTransactionResponseDto
{
    [JsonPropertyName("transaction")]
    public string Transaction { get; set; } = string.Empty;

    [JsonPropertyName("cancel_time")]
    public long CancelTime { get; set; }

    [JsonPropertyName("state")]
    public int State { get; set; }
}
