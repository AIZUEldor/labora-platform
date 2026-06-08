using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class SavedJobRepository : ISavedJobRepository
{
    private readonly LaboaDbContext _context;

    public SavedJobRepository(LaboaDbContext context)
    {
        _context = context;
    }

    public async Task<List<SavedJob>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SavedJobs
            .Include(s => s.Job)
            .ThenInclude(j => j.Employer)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavedJob?> GetByUserAndJobAsync(Guid userId, Guid jobId)
    {
        return await _context.SavedJobs
            .FirstOrDefaultAsync(s => s.UserId == userId && s.JobId == jobId);
    }

    public async Task<SavedJob> AddAsync(SavedJob savedJob)
    {
        _context.SavedJobs.Add(savedJob);
        await _context.SaveChangesAsync();
        return savedJob;
    }

    public async Task DeleteAsync(SavedJob savedJob)
    {
        _context.SavedJobs.Remove(savedJob);
        await _context.SaveChangesAsync();
    }
}