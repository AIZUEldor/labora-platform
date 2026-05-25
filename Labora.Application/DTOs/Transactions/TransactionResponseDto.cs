namespace Labora.Application.DTOs.Transactions;

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}