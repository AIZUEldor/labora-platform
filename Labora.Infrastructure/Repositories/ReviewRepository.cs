using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    private readonly LaboaDbContext _context;

    public ReviewRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetReviewsByRevieweeIdAsync(Guid revieweeId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RevieweeId == revieweeId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByReviewerIdAsync(Guid reviewerId)
    {
        return await _context.Reviews
            .Where(r => r.ReviewerId == reviewerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByJobApplicationIdAsync(Guid jobApplicationId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.JobApplicationId == jobApplicationId && !r.IsDeleted);
    }

    public async Task<bool> HasReviewedAsync(Guid reviewerId, Guid jobApplicationId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ReviewerId == reviewerId &&
                          r.JobApplicationId == jobApplicationId &&
                          !r.IsDeleted);
    }

    public async Task<double> GetAverageRatingAsync(Guid revieweeId)
    {
        IEnumerable<Review> reviews = await _context.Reviews
            .Where(r => r.RevieweeId == revieweeId && !r.IsDeleted)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return reviews.Average(r => r.OverallRating);
    }
}