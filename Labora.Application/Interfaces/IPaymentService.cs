using Labora.Application.DTOs.Payments;

namespace Labora.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentOrderResponseDto> CreateTopUpAsync(CreateTopUpRequestDto request, Guid userId);
    Task<PaymentOrderResponseDto> GetByIdAsync(Guid orderId, Guid userId);
    Task<IEnumerable<PaymentOrderResponseDto>> GetMyPaymentOrdersAsync(Guid userId);
}
