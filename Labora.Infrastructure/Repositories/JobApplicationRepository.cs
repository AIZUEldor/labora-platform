using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
{
    private readonly LaboaDbContext _context;

    public JobApplicationRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(Guid jobId)
    {
        return await _context.JobApplications
            .Where(a => a.JobId == jobId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetApplicationsByWorkerIdAsync(Guid workerId)
    {
        return await _context.JobApplications
            .Where(a => a.WorkerId == workerId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<JobApplication?> GetApplicationByJobAndWorkerAsync(Guid jobId, Guid workerId)
    {
        return await _context.JobApplications
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.WorkerId == workerId && !a.IsDeleted);
    }

    public async Task<bool> HasWorkerAppliedAsync(Guid jobId, Guid workerId)
    {
        return await _context.JobApplications
            .AnyAsync(a => a.JobId == jobId && a.WorkerId == workerId && !a.IsDeleted);
    }

    public async Task UpdateStatusAsync(Guid applicationId, ApplicationStatus status)
    {
        JobApplication? application = await _context.JobApplications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted);

        if (application is not null)
        {
            application.Status = status;
            application.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}