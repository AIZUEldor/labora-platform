using Labora.Domain.Common;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Labora.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly LaboaDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(LaboaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        T? entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }
}