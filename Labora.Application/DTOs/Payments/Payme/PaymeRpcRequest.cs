using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PaymeRpcRequest<TParams>
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public TParams? Params { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }
}
