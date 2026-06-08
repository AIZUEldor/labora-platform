using Labora.Application.DTOs.Jobs;

namespace Labora.Application.Interfaces;

public interface ISavedJobService
{
    Task<List<JobResponseDto>> GetSavedJobsAsync(Guid userId);
    Task SaveJobAsync(Guid userId, Guid jobId);
    Task UnsaveJobAsync(Guid userId, Guid jobId);
    Task<bool> IsJobSavedAsync(Guid userId, Guid jobId);
}