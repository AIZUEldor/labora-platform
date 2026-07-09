using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class PaymentOrder : BaseEntity
{
    public decimal Amount { get; set; }
    public PaymentProvider Provider { get; set; }
    public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.Pending;
    public string? ProviderOrderId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Foreign key
    public Guid UserId { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
