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
            throw new OtpBlockConcurrencyException(
                "The OTP block row was modified concurrently.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new OtpBlockConflictException(
                "A conflicting OTP block already exists for this block type and scope.", ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
