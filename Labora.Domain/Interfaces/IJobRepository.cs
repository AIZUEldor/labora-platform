using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IJobRepository : IGenericRepository<Job>
{
    // These two power the list-shaped endpoints (search/browse and "my jobs"). They pair each Job
    // with a separately-projected cover image URL instead of loading the full Images collection -
    // one query, no per-job round trip, and no Images navigation touched on the returned entities.
    Task<IEnumerable<(Job Job, string? CoverImageUrl)>> GetJobsByEmployerIdAsync(Guid employerId);
    Task<IEnumerable<Job>> GetJobsByTypeAsync(JobType jobType);
    Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Job>> GetJobsByLocationAsync(string city, string country);
    Task<IEnumerable<Job>> SearchJobsAsync(string keyword);
    Task<(IEnumerable<(Job Job, string? CoverImageUrl)> Jobs, int TotalCount)> GetFilteredJobsAsync(
        string? keyword,
        string? city,
        string? country,
        JobType? jobType,
        decimal? minSalary,
        decimal? maxSalary,
        Guid? categoryId,
        int pageNumber,
        int pageSize);
    Task<IEnumerable<Job>> GetNearbyJobsAsync(
        double latitude,
        double longitude,
        double radiusKm);

    Task<IEnumerable<Job>> GetRecommendedJobsAsync(
    double? latitude,
    double? longitude,
    IEnumerable<UserPreference> preferences);

    Task<IEnumerable<Job>> GetAllActiveWithLocationAsync();

    Task<Job?> GetByIdWithImagesAsync(Guid id);
    Task<JobImage> AddImageAsync(Guid jobId, string imageUrl, string? caption);
    Task DeleteImageAsync(Guid imageId);
}