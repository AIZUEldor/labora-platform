using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CheckTransactionResponseDto
{
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    [JsonPropertyName("perform_time")]
    public long PerformTime { get; set; }

    [JsonPropertyName("cancel_time")]
    public long CancelTime { get; set; }

    [JsonPropertyName("transaction")]
    public string Transaction { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public int State { get; set; }

    /// <summary>Exact reason code enumeration was not confirmed by fetched documentation.</summary>
    [JsonPropertyName("reason")]
    public int? Reason { get; set; }
}
