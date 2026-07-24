using Labora.Domain.Entities;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class PaymentOrderRepository : GenericRepository<PaymentOrder>, IPaymentOrderRepository
{
    private readonly LaboaDbContext _context;

    public PaymentOrderRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentOrder>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PaymentOrders
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaymentOrder?> GetByProviderOrderIdAsync(string providerOrderId)
    {
        return await _context.PaymentOrders
            .FirstOrDefaultAsync(p => p.ProviderOrderId == providerOrderId && !p.IsDeleted);
    }

    public async Task<PaymentOrder?> GetByIdForUpdateAsync(Guid id)
    {
        return await _context.PaymentOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public override async Task<PaymentOrder> UpdateAsync(PaymentOrder entity)
    {
        try
        {
            return await base.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Detach the failed entity: a rolled-back transaction does not undo EF Core's in-memory
            // tracking, so without this it would remain tracked and be swept into whichever
            // SaveChangesAsync a caller-level retry issues next on this same DbContext - either
            // failing that unrelated attempt for the wrong reason or silently persisting stale data.
            DetachFailedEntity(entity);
            throw new PaymeConcurrencyException(
                "The payment order row was modified concurrently.", ex);
        }
    }

    private void DetachFailedEntity(PaymentOrder entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }
}
