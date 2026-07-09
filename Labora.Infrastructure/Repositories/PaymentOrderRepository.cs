using Labora.Domain.Entities;
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
}
