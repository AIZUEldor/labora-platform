using AutoMapper;
using Labora.Application.DTOs.Payments;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class PaymentService : IPaymentService
{
    private const int ExpiryMinutes = 30;

    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IMapper _mapper;

    public PaymentService(IPaymentOrderRepository paymentOrderRepository, IMapper mapper)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _mapper = mapper;
    }

    public async Task<PaymentOrderResponseDto> CreateTopUpAsync(CreateTopUpRequestDto request, Guid userId)
    {
        PaymentOrder order = new PaymentOrder
        {
            Amount = request.Amount,
            Provider = request.Provider,
            Status = PaymentOrderStatus.Pending,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes)
        };

        PaymentOrder createdOrder = await _paymentOrderRepository.AddAsync(order);
        return _mapper.Map<PaymentOrderResponseDto>(createdOrder);
    }

    public async Task<PaymentOrderResponseDto> GetByIdAsync(Guid orderId, Guid userId)
    {
        PaymentOrder? order = await _paymentOrderRepository.GetByIdAsync(orderId);

        if (order is null)
            throw new InvalidOperationException($"Id={orderId} bo'lgan to'lov buyurtmasi topilmadi.");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException("Bu to'lov buyurtmasiga ruxsatingiz yo'q.");

        return _mapper.Map<PaymentOrderResponseDto>(order);
    }

    public async Task<IEnumerable<PaymentOrderResponseDto>> GetMyPaymentOrdersAsync(Guid userId)
    {
        IEnumerable<PaymentOrder> orders = await _paymentOrderRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PaymentOrderResponseDto>>(orders);
    }
}
