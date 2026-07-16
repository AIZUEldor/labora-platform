using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class OtpAbuseEventRepository : GenericRepository<OtpAbuseEvent>, IOtpAbuseEventRepository
{
    private readonly LaboaDbContext _context;

    public OtpAbuseEventRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<int> CountByPhoneAsync(string phoneNumber, OtpAbuseEventType eventType, DateTime sinceUtc)
    {
        return await _context.OtpAbuseEvents
            .AsNoTracking()
            .Where(e => e.PhoneNumber == phoneNumber && e.EventType == eventType && e.CreatedAt >= sinceUtc && !e.IsDeleted)
            .CountAsync();
    }

    public async Task<int> CountByIpAsync(string ipHash, OtpAbuseEventType eventType, DateTime sinceUtc)
    {
        return await _context.OtpAbuseEvents
            .AsNoTracking()
            .Where(e => e.IpHash == ipHash && e.EventType == eventType && e.CreatedAt >= sinceUtc && !e.IsDeleted)
            .CountAsync();
    }

    public async Task<int> CountByDeviceAsync(string deviceHash, OtpAbuseEventType eventType, DateTime sinceUtc)
    {
        return await _context.OtpAbuseEvents
            .AsNoTracking()
            .Where(e => e.DeviceHash == deviceHash && e.EventType == eventType && e.CreatedAt >= sinceUtc && !e.IsDeleted)
            .CountAsync();
    }

    public async Task<int> CountDistinctPhonesByIpAsync(string ipHash, OtpAbuseEventType eventType, DateTime sinceUtc)
    {
        return await _context.OtpAbuseEvents
            .AsNoTracking()
            .Where(e => e.IpHash == ipHash && e.EventType == eventType && e.CreatedAt >= sinceUtc && !e.IsDeleted)
            .Select(e => e.PhoneNumber)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> CountDistinctPhonesByDeviceAsync(string deviceHash, OtpAbuseEventType eventType, DateTime sinceUtc)
    {
        return await _context.OtpAbuseEvents
            .AsNoTracking()
            .Where(e => e.DeviceHash == deviceHash && e.EventType == eventType && e.CreatedAt >= sinceUtc && !e.IsDeleted)
            .Select(e => e.PhoneNumber)
            .Distinct()
            .CountAsync();
    }
}
