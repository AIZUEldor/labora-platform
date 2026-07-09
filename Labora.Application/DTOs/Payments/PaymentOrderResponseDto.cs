using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Payments;

public class PaymentOrderResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentProvider Provider { get; set; }
    public PaymentOrderStatus Status { get; set; }
    public string? ProviderOrderId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
