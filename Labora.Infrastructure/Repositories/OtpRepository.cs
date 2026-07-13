using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Labora.Infrastructure.Repositories;

public class OtpRepository : GenericRepository<OtpVerification>, IOtpRepository
{
    private readonly LaboaDbContext _context;

    public OtpRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<OtpVerification?> GetLatestByPhoneAndPurposeAsync(string phoneNumber, OtpPurpose purpose)
    {
        return await _context.OtpVerifications
            .Where(o => o.PhoneNumber == phoneNumber && o.Purpose == purpose && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<OtpVerification?> GetByOperationTokenHashAsync(string operationTokenHash)
    {
        return await _context.OtpVerifications
            .FirstOrDefaultAsync(o => o.OperationTokenHash == operationTokenHash && !o.IsDeleted);
    }

    public async Task<int> GetSendCountSinceAsync(string phoneNumber, OtpPurpose purpose, DateTime sinceUtc)
    {
        // Conservative upper-bound, not an exact rolling-window count: SendCount is cumulative
        // per row, so a resent row's full lifetime count is included whenever its LastSentAt
        // falls in the window, even if some of those sends happened before sinceUtc. Overcounts
        // only, never undercounts, which is the safe direction for a rate limit.
        return await _context.OtpVerifications
            .Where(o => o.PhoneNumber == phoneNumber && o.Purpose == purpose && o.LastSentAt >= sinceUtc && !o.IsDeleted)
            .SumAsync(o => o.SendCount);
    }

    public override async Task<OtpVerification> AddAsync(OtpVerification entity)
    {
        try
        {
            return await base.AddAsync(entity);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new OtpConflictException(
                "An active OTP verification already exists for this phone number and purpose.", ex);
        }
    }

    public override async Task<OtpVerification> UpdateAsync(OtpVerification entity)
    {
        try
        {
            return await base.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new OtpConcurrencyException(
                "The OTP verification row was modified concurrently.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new OtpConflictException(
                "A conflicting OTP verification already exists for this phone number and purpose.", ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
