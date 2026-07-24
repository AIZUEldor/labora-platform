using Labora.Domain.Entities;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Labora.Infrastructure.Repositories;

public class PaymeTransactionRepository : GenericRepository<PaymeTransaction>, IPaymeTransactionRepository
{
    private readonly LaboaDbContext _context;

    public PaymeTransactionRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PaymeTransaction?> GetByPaymeTransactionIdAsync(string paymeTransactionId)
    {
        return await _context.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeTransactionId && !t.IsDeleted);
    }

    public async Task<PaymeTransaction?> GetByPaymeTransactionIdForUpdateAsync(string paymeTransactionId)
    {
        return await _context.PaymeTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.PaymeTransactionId == paymeTransactionId && !t.IsDeleted);
    }

    public async Task<PaymeTransaction?> GetByPaymentOrderIdAsync(Guid paymentOrderId)
    {
        return await _context.PaymeTransactions
            .FirstOrDefaultAsync(t => t.PaymentOrderId == paymentOrderId && !t.IsDeleted);
    }

    public async Task<IReadOnlyList<PaymeTransaction>> GetStatementAsync(long fromTimestamp, long toTimestamp)
    {
        return await _context.PaymeTransactions
            .Where(t => t.PaymeTransactionTime >= fromTimestamp && t.PaymeTransactionTime <= toTimestamp && !t.IsDeleted)
            .OrderBy(t => t.PaymeTransactionTime)
            .ToListAsync();
    }

    public override async Task<PaymeTransaction> AddAsync(PaymeTransaction entity)
    {
        try
        {
            return await base.AddAsync(entity);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Detach the failed entity: a rolled-back transaction does not undo EF Core's in-memory
            // tracking, so without this it would remain tracked as Added and be swept into whichever
            // SaveChangesAsync a caller-level retry issues next on this same DbContext - either
            // failing that unrelated attempt for the wrong reason or silently inserting this
            // abandoned row alongside it.
            DetachFailedEntity(entity);
            throw new PaymeConflictException(
                "A Payme transaction already exists for this transaction id or payment order.", ex);
        }
    }

    public override async Task<PaymeTransaction> UpdateAsync(PaymeTransaction entity)
    {
        try
        {
            return await base.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            DetachFailedEntity(entity);
            throw new PaymeConcurrencyException(
                "The Payme transaction row was modified concurrently.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            DetachFailedEntity(entity);
            throw new PaymeConflictException(
                "A conflicting Payme transaction already exists for this transaction id or payment order.", ex);
        }
    }

    private void DetachFailedEntity(PaymeTransaction entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
