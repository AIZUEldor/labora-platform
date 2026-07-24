using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PerformTransactionResponseDto
{
    [JsonPropertyName("transaction")]
    public string Transaction { get; set; } = string.Empty;

    [JsonPropertyName("perform_time")]
    public long PerformTime { get; set; }

    [JsonPropertyName("state")]
    public int State { get; set; }
}
