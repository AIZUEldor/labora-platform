using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface ISavedJobRepository
{
    Task<List<SavedJob>> GetByUserIdAsync(Guid userId);
    Task<SavedJob?> GetByUserAndJobAsync(Guid userId, Guid jobId);
    Task<SavedJob> AddAsync(SavedJob savedJob);
    Task DeleteAsync(SavedJob savedJob);
}