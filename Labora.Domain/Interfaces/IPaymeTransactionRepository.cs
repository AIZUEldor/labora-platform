using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IPaymeTransactionRepository : IGenericRepository<PaymeTransaction>
{
    Task<PaymeTransaction?> GetByPaymeTransactionIdAsync(string paymeTransactionId);

    /// <summary>
    /// Untracked fetch by Payme transaction id, for callers that intend to mutate and UpdateAsync the
    /// result inside a retry loop - mirrors IOtpRepository.GetByIdForUpdateAsync: a tracked read would
    /// hit EF Core's identity map on retry and return an already-tracked, possibly stale instance from
    /// a failed prior attempt instead of a genuinely fresh row (and a fresh xmin baseline).
    /// </summary>
    Task<PaymeTransaction?> GetByPaymeTransactionIdForUpdateAsync(string paymeTransactionId);

    Task<PaymeTransaction?> GetByPaymentOrderIdAsync(Guid paymentOrderId);

    /// <summary>Inclusive range on PaymeTransactionTime, ordered ascending, per confirmed GetStatement behavior.</summary>
    Task<IReadOnlyList<PaymeTransaction>> GetStatementAsync(long fromTimestamp, long toTimestamp);
}
