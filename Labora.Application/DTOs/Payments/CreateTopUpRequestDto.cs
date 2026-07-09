using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Payments;

public class CreateTopUpRequestDto
{
    public decimal Amount { get; set; }
    public PaymentProvider Provider { get; set; }
}
