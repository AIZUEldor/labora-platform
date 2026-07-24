using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CancelTransactionRequestDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Exact reason code enumeration was not confirmed by fetched documentation.</summary>
    [JsonPropertyName("reason")]
    public int Reason { get; set; }
}
