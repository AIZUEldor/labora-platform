using Labora.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Labora.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly LaboaDbContext _context;

    public UnitOfWork(LaboaDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return true;
        });
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            T result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
