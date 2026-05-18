using Labora.Domain.Common;

namespace Labora.Domain.Entities;

public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "deposit" | "withdrawal"
    public string? Description { get; set; }

    // Foreign key
    public Guid UserId { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}