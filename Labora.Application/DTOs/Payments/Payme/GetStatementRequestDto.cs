using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class GetStatementRequestDto
{
    [JsonPropertyName("from")]
    public long From { get; set; }

    [JsonPropertyName("to")]
    public long To { get; set; }
}
