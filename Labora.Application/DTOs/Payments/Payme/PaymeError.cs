using System.Text.Json;
using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PaymeError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Official Payme documentation fetched for this task did not confirm whether "message" is a
    /// plain string or a per-language object (e.g. ru/uz/en keys); kept as a raw JsonElement so
    /// deserialization succeeds regardless of which shape the real API sends. Confirm the exact
    /// shape once sandbox/business-account documentation access is available.
    /// </summary>
    [JsonPropertyName("message")]
    public JsonElement? Message { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
