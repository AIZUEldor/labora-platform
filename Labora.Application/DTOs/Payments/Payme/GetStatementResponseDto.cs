using System.Text.Json.Serialization;

namespace Labora.Application.DTOs.Payments.Payme;

public class GetStatementResponseDto
{
    /// <summary>
    /// Confirmed as "transactions" (plural) by the literal example response on the official
    /// GetStatement documentation page - not "transaction" (singular, which is a different,
    /// per-item field on <see cref="PaymeStatementTransactionDto"/>).
    /// </summary>
    [JsonPropertyName("transactions")]
    public List<PaymeStatementTransactionDto> Transactions { get; set; } = new();
}
