using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId);
    Task<decimal> GetUserBalanceAsync(Guid userId);
}