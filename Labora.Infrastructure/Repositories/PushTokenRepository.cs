using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class PushTokenRepository : GenericRepository<PushToken>, IPushTokenRepository
{
    private readonly LaboaDbContext _context;

    public PushTokenRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PushToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PushTokens
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<PushToken?> GetByTokenAsync(string token)
    {
        return await _context.PushTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsDeleted);
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        List<PushToken> tokens = await _context.PushTokens
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .ToListAsync();

        foreach (PushToken token in tokens)
        {
            token.IsDeleted = true;
            token.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}