using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Labora.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly LaboaDbContext _context;

    public UserRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
    {
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<IEnumerable<User>> GetWorkerUsersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == Labora.Domain.Enums.UserRole.Worker && !u.IsDeleted)
            .ToListAsync();
    }

    public override async Task<User> AddAsync(User entity)
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
            throw new InvalidOperationException(
                "Bu telefon raqam allaqachon ro'yxatdan o'tgan.", ex);
        }
    }

    private void DetachFailedEntity(User entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}