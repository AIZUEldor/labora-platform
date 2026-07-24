using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PaymeRpcResponse<TResult>
{
    [JsonPropertyName("result")]
    public TResult? Result { get; set; }

    [JsonPropertyName("error")]
    public PaymeError? Error { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }
}
