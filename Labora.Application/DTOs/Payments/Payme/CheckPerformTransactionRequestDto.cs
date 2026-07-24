using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class CheckPerformTransactionRequestDto
{
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("account")]
    public PaymeAccountDto Account { get; set; } = new();
}
