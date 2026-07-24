using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class PerformTransactionRequestDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
