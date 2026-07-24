using System.Linq.Expressions;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Tests.Payme;

/// <summary>
/// Minimal in-memory IPaymentOrderRepository double for PaymeMerchantService business-logic tests -
/// no mocking library is used elsewhere in this test project, so a small hand-written fake keeps that
/// convention rather than introducing a new dependency.
/// </summary>
internal class FakePaymentOrderRepository : IPaymentOrderRepository
{
    private readonly Dictionary<Guid, PaymentOrder> _orders;

    public FakePaymentOrderRepository(params PaymentOrder[] seed)
    {
        _orders = seed.ToDictionary(o => o.Id);
    }

    public Task<PaymentOrder?> GetByIdAsync(Guid id) =>
        Task.FromResult(_orders.TryGetValue(id, out PaymentOrder? order) && !order.IsDeleted ? order : null);

    /// <summary>
    /// Returns an independent copy, matching real EF AsNoTracking() semantics: mutating the returned
    /// instance (as callers on a retry path do) must never be visible to a subsequent fetch unless an
    /// UpdateAsync explicitly persisted it - a shared reference here would silently mask exactly the
    /// identity-map staleness bug this "for update" method exists to prevent.
    /// </summary>
    public Task<PaymentOrder?> GetByIdForUpdateAsync(Guid id)
    {
        bool found = _orders.TryGetValue(id, out PaymentOrder? order) && !order.IsDeleted;
        return Task.FromResult(found ? Clone(order!) : null);
    }

    private static PaymentOrder Clone(PaymentOrder source) => new()
    {
        Id = source.Id,
        Amount = source.Amount,
        Provider = source.Provider,
        Status = source.Status,
        ProviderOrderId = source.ProviderOrderId,
        PaidAt = source.PaidAt,
        ExpiresAt = source.ExpiresAt,
        UserId = source.UserId,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
        IsDeleted = source.IsDeleted
    };

    public Task<IEnumerable<PaymentOrder>> GetByUserIdAsync(Guid userId) =>
        Task.FromResult(_orders.Values.Where(o => o.UserId == userId && !o.IsDeleted));

    public Task<PaymentOrder?> GetByProviderOrderIdAsync(string providerOrderId) =>
        Task.FromResult(_orders.Values.FirstOrDefault(o => o.ProviderOrderId == providerOrderId && !o.IsDeleted));

    public Task<IEnumerable<PaymentOrder>> GetAllAsync() =>
        Task.FromResult(_orders.Values.Where(o => !o.IsDeleted));

    public Task<IEnumerable<PaymentOrder>> FindAsync(Expression<Func<PaymentOrder, bool>> predicate) =>
        Task.FromResult(_orders.Values.AsQueryable().Where(predicate).AsEnumerable());

    public Task<PaymentOrder> AddAsync(PaymentOrder entity)
    {
        _orders[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<PaymentOrder> UpdateAsync(PaymentOrder entity)
    {
        _orders[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        if (_orders.TryGetValue(id, out PaymentOrder? order))
        {
            order.IsDeleted = true;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id) =>
        Task.FromResult(_orders.TryGetValue(id, out PaymentOrder? order) && !order.IsDeleted);

    /// <summary>Direct dictionary read for test assertions, bypassing the IsDeleted filter used elsewhere.</summary>
    public PaymentOrder? GetRaw(Guid id) => _orders.TryGetValue(id, out PaymentOrder? order) ? order : null;
}

/// <summary>In-memory IPaymeTransactionRepository double, with optional fault injection on AddAsync to
/// simulate a unique-index/xmin conflict on a specific call for concurrency/retry tests.</summary>
internal class FakePaymeTransactionRepository : IPaymeTransactionRepository
{
    private readonly Dictionary<string, PaymeTransaction> _byPaymeTransactionId = new();
    private readonly Dictionary<Guid, PaymeTransaction> _byPaymentOrderId = new();

    public int AddAsyncCallCount { get; private set; }
    public int UpdateAsyncCallCount { get; private set; }

    /// <summary>Given the 1-based AddAsync call number, optionally return an exception to throw instead
    /// of completing normally.</summary>
    public Func<int, Exception?>? FaultOnAddAsync { get; set; }

    /// <summary>Given the 1-based UpdateAsync call number, optionally return an exception to throw
    /// instead of completing normally.</summary>
    public Func<int, Exception?>? FaultOnUpdateAsync { get; set; }

    /// <summary>Inserts a row directly, bypassing AddAsync (and any configured fault/call counting) -
    /// used to simulate a concurrent writer's row becoming visible mid-test.</summary>
    public void SeedDirectly(PaymeTransaction transaction)
    {
        if (transaction.Id == Guid.Empty)
        {
            transaction.Id = Guid.NewGuid();
        }

        _byPaymeTransactionId[transaction.PaymeTransactionId] = transaction;
        _byPaymentOrderId[transaction.PaymentOrderId] = transaction;
    }

    public Task<PaymeTransaction?> GetByPaymeTransactionIdAsync(string paymeTransactionId) =>
        Task.FromResult(_byPaymeTransactionId.TryGetValue(paymeTransactionId, out PaymeTransaction? t) ? t : null);

    /// <summary>
    /// Returns an independent copy, matching real EF AsNoTracking() semantics: mutating the returned
    /// instance (as callers on a retry path do) must never be visible to a subsequent fetch unless an
    /// UpdateAsync explicitly persisted it - a shared reference here would silently mask exactly the
    /// identity-map staleness bug this "for update" method exists to prevent.
    /// </summary>
    public Task<PaymeTransaction?> GetByPaymeTransactionIdForUpdateAsync(string paymeTransactionId)
    {
        bool found = _byPaymeTransactionId.TryGetValue(paymeTransactionId, out PaymeTransaction? transaction);
        return Task.FromResult(found ? Clone(transaction!) : null);
    }

    private static PaymeTransaction Clone(PaymeTransaction source) => new()
    {
        Id = source.Id,
        PaymentOrderId = source.PaymentOrderId,
        PaymeTransactionId = source.PaymeTransactionId,
        AccountReference = source.AccountReference,
        PaymeTransactionTime = source.PaymeTransactionTime,
        RequestedAmountTiyin = source.RequestedAmountTiyin,
        MerchantCreateTime = source.MerchantCreateTime,
        MerchantPerformTime = source.MerchantPerformTime,
        MerchantCancelTime = source.MerchantCancelTime,
        InternalStatus = source.InternalStatus,
        PaymeStateCode = source.PaymeStateCode,
        CancelReason = source.CancelReason,
        Version = source.Version,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
        IsDeleted = source.IsDeleted
    };

    public Task<PaymeTransaction?> GetByPaymentOrderIdAsync(Guid paymentOrderId) =>
        Task.FromResult(_byPaymentOrderId.TryGetValue(paymentOrderId, out PaymeTransaction? t) ? t : null);

    public Task<IReadOnlyList<PaymeTransaction>> GetStatementAsync(long fromTimestamp, long toTimestamp) =>
        Task.FromResult<IReadOnlyList<PaymeTransaction>>(_byPaymeTransactionId.Values
            .Where(t => t.PaymeTransactionTime >= fromTimestamp && t.PaymeTransactionTime <= toTimestamp)
            .OrderBy(t => t.PaymeTransactionTime)
            .ToList());

    public Task<IEnumerable<PaymeTransaction>> GetAllAsync() =>
        Task.FromResult(_byPaymeTransactionId.Values.AsEnumerable());

    public Task<IEnumerable<PaymeTransaction>> FindAsync(Expression<Func<PaymeTransaction, bool>> predicate) =>
        Task.FromResult(_byPaymeTransactionId.Values.AsQueryable().Where(predicate).AsEnumerable());

    public Task<PaymeTransaction?> GetByIdAsync(Guid id) =>
        Task.FromResult(_byPaymeTransactionId.Values.FirstOrDefault(t => t.Id == id));

    public Task<bool> ExistsAsync(Guid id) =>
        Task.FromResult(_byPaymeTransactionId.Values.Any(t => t.Id == id));

    public Task DeleteAsync(Guid id) => Task.CompletedTask;

    public Task<PaymeTransaction> AddAsync(PaymeTransaction entity)
    {
        AddAsyncCallCount++;

        Exception? fault = FaultOnAddAsync?.Invoke(AddAsyncCallCount);
        if (fault is not null)
        {
            throw fault;
        }

        SeedDirectly(entity);
        return Task.FromResult(entity);
    }

    public Task<PaymeTransaction> UpdateAsync(PaymeTransaction entity)
    {
        UpdateAsyncCallCount++;

        Exception? fault = FaultOnUpdateAsync?.Invoke(UpdateAsyncCallCount);
        if (fault is not null)
        {
            throw fault;
        }

        SeedDirectly(entity);
        return Task.FromResult(entity);
    }
}

/// <summary>Minimal in-memory IUserRepository double, matching FakePaymentOrderRepository's shape.</summary>
internal class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users;

    public int UpdateAsyncCallCount { get; private set; }

    public FakeUserRepository(params User[] seed)
    {
        _users = seed.ToDictionary(u => u.Id);
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(_users.TryGetValue(id, out User? user) && !user.IsDeleted ? user : null);

    /// <summary>
    /// Returns an independent copy, matching real EF AsNoTracking() semantics: mutating the returned
    /// instance (as callers on a retry path do) must never be visible to a subsequent fetch unless an
    /// UpdateAsync explicitly persisted it - a shared reference here would silently mask exactly the
    /// identity-map staleness bug this "for update" method exists to prevent.
    /// </summary>
    public Task<User?> GetByIdForUpdateAsync(Guid id)
    {
        bool found = _users.TryGetValue(id, out User? user) && !user.IsDeleted;
        return Task.FromResult(found ? Clone(user!) : null);
    }

    private static User Clone(User source) => new()
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Age = source.Age,
        PhoneNumber = source.PhoneNumber,
        PasswordHash = source.PasswordHash,
        Role = source.Role,
        ProfileImageUrl = source.ProfileImageUrl,
        Latitude = source.Latitude,
        Longitude = source.Longitude,
        City = source.City,
        Country = source.Country,
        Balance = source.Balance,
        IsVerified = source.IsVerified,
        IsBlocked = source.IsBlocked,
        CvUrl = source.CvUrl,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
        IsDeleted = source.IsDeleted
    };

    public Task<User?> GetByPhoneNumberAsync(string phoneNumber) =>
        Task.FromResult(_users.Values.FirstOrDefault(u => u.PhoneNumber == phoneNumber && !u.IsDeleted));

    public Task<bool> PhoneNumberExistsAsync(string phoneNumber) =>
        Task.FromResult(_users.Values.Any(u => u.PhoneNumber == phoneNumber));

    public Task<IEnumerable<User>> GetWorkerUsersAsync() =>
        Task.FromResult(_users.Values.Where(u => u.Role == Labora.Domain.Enums.UserRole.Worker && !u.IsDeleted));

    public Task<IEnumerable<User>> GetAllAsync() =>
        Task.FromResult(_users.Values.Where(u => !u.IsDeleted));

    public Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate) =>
        Task.FromResult(_users.Values.AsQueryable().Where(predicate).AsEnumerable());

    public Task<User> AddAsync(User entity)
    {
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<User> UpdateAsync(User entity)
    {
        UpdateAsyncCallCount++;
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        if (_users.TryGetValue(id, out User? user))
        {
            user.IsDeleted = true;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id) =>
        Task.FromResult(_users.TryGetValue(id, out User? user) && !user.IsDeleted);

    /// <summary>Direct dictionary read for test assertions, bypassing the IsDeleted filter used elsewhere.</summary>
    public User? GetRaw(Guid id) => _users.TryGetValue(id, out User? user) ? user : null;
}

/// <summary>
/// No-op IUnitOfWork double: invokes the delegate directly with no real transaction. Sufficient for
/// these tests because PaymeMerchantService's own write ordering (PaymeTransaction created before
/// PaymentOrder is touched) already guarantees no partial PaymentOrder update on a validation failure
/// that happens before any write. True DB-level rollback (a write succeeding then a later write in the
/// same delegate failing) needs a real Postgres integration test, not unit-testable with this fake.
/// </summary>
internal class PassThroughUnitOfWork : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<Task> operation) => await operation();

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation) => await operation();
}
