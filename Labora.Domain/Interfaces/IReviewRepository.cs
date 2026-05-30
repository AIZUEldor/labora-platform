using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByRevieweeIdAsync(Guid revieweeId);
    Task<IEnumerable<Review>> GetReviewsByReviewerIdAsync(Guid reviewerId);
    Task<Review?> GetReviewByJobApplicationIdAsync(Guid jobApplicationId);
    Task<bool> HasReviewedAsync(Guid reviewerId, Guid jobApplicationId);
    Task<double> GetAverageRatingAsync(Guid revieweeId);
}