namespace Labora.Application.DTOs.Transactions;

public class TransactionRequestDto
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}