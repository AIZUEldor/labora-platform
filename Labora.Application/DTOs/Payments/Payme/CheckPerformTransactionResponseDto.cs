using System.Text.Json;
using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CheckPerformTransactionResponseDto
{
    [JsonPropertyName("allow")]
    public bool Allow { get; set; }

    /// <summary>
    /// "additional"/"detail" (fiscal receipt) sub-shapes were not fully confirmed field-by-field by
    /// official documentation fetched for this task - kept generic rather than modeled precisely.
    /// </summary>
    [JsonPropertyName("additional")]
    public Dictionary<string, JsonElement>? Additional { get; set; }

    [JsonPropertyName("detail")]
    public JsonElement? Detail { get; set; }
}
