using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IPaymentOrderRepository : IGenericRepository<PaymentOrder>
{
    Task<IEnumerable<PaymentOrder>> GetByUserIdAsync(Guid userId);
    Task<PaymentOrder?> GetByProviderOrderIdAsync(string providerOrderId);

    /// <summary>
    /// Untracked fetch by id, for callers that intend to mutate and UpdateAsync the result inside a
    /// retry loop - mirrors IOtpRepository.GetByIdForUpdateAsync: a tracked read would hit EF Core's
    /// identity map on retry and return an already-tracked, possibly stale instance from a failed
    /// prior attempt instead of a genuinely fresh row.
    /// </summary>
    Task<PaymentOrder?> GetByIdForUpdateAsync(Guid id);
}
