 using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
    Task<IEnumerable<User>> GetWorkerUsersAsync();

    /// <summary>
    /// Untracked fetch by id, for callers that intend to mutate and UpdateAsync the result inside a
    /// retry loop - mirrors IPaymentOrderRepository.GetByIdForUpdateAsync/IPaymeTransactionRepository.
    /// GetByPaymeTransactionIdForUpdateAsync: a tracked read would hit EF Core's identity map on retry
    /// and return an already-tracked, possibly stale instance from a failed prior attempt instead of a
    /// genuinely fresh row.
    /// </summary>
    Task<User?> GetByIdForUpdateAsync(Guid id);
}