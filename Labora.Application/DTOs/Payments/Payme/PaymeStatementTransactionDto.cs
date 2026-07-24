using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

/// <summary>
/// Confirmed field-for-field by the literal example response on the official GetStatement
/// documentation page (developer.help.paycom.uz/metody-merchant-api/getstatement).
/// </summary>
public class PaymeStatementTransactionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("account")]
    public PaymeAccountDto Account { get; set; } = new();

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

    [JsonPropertyName("reason")]
    public int? Reason { get; set; }

    [JsonPropertyName("receivers")]
    public List<PaymeReceiverDto>? Receivers { get; set; }
}
