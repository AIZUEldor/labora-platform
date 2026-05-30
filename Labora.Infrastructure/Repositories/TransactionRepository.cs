using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    private readonly LaboaDbContext _context;

    public TransactionRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetUserBalanceAsync(Guid userId)
    {
        decimal deposits = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == "deposit" && !t.IsDeleted)
            .SumAsync(t => t.Amount);

        decimal withdrawals = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == "withdrawal" && !t.IsDeleted)
            .SumAsync(t => t.Amount);

        return deposits - withdrawals;
    }
}