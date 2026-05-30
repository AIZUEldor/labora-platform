using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class UserPreferenceRepository : GenericRepository<UserPreference>, IUserPreferenceRepository
{
    private readonly LaboaDbContext _context;

    public UserPreferenceRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserPreference>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPreferences
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        List<UserPreference> preferences = await _context.UserPreferences
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        foreach (UserPreference preference in preferences)
        {
            preference.IsDeleted = true;
            preference.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}