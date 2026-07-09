using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IPaymentOrderRepository : IGenericRepository<PaymentOrder>
{
    Task<IEnumerable<PaymentOrder>> GetByUserIdAsync(Guid userId);
    Task<PaymentOrder?> GetByProviderOrderIdAsync(string providerOrderId);
}
