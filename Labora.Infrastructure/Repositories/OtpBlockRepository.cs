using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Labora.Infrastructure.Repositories;

public class OtpBlockRepository : GenericRepository<OtpBlock>, IOtpBlockRepository
{
    private readonly LaboaDbContext _context;

    public OtpBlockRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<OtpBlock?> GetActiveAsync(OtpBlockType blockType, string scopeKey, DateTime nowUtc)
    {
        return await _context.OtpBlocks
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BlockType == blockType && b.ScopeKey == scopeKey && b.ExpiresAt > nowUtc && !b.IsDeleted);
    }

    public async Task<OtpBlock?> GetByScopeAsync(OtpBlockType blockType, string scopeKey)
    {
        return await _context.OtpBlocks
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BlockType == blockType && b.ScopeKey == scopeKey && !b.IsDeleted);
    }

    public override async Task<OtpBlock> AddAsync(OtpBlock entity)
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
            throw new OtpBlockConflictException(
                "An OTP block already exists for this block type and scope.", ex);
        }
    }

    public override async Task<OtpBlock> UpdateAsync(OtpBlock entity)
    {
        try
        {
            return await base.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            DetachFailedEntity(entity);
            throw new OtpBlockConcurrencyException(
                "The OTP block row was modified concurrently.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            DetachFailedEntity(entity);
            throw new OtpBlockConflictException(
                "A conflicting OTP block already exists for this block type and scope.", ex);
        }
    }

    private void DetachFailedEntity(OtpBlock entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
