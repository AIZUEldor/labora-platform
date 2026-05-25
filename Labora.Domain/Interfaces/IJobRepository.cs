using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IJobRepository : IGenericRepository<Job>
{
    Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId);
    Task<IEnumerable<Job>> GetJobsByTypeAsync(JobType jobType);
    Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Job>> GetJobsByLocationAsync(string city, string country);
    Task<IEnumerable<Job>> SearchJobsAsync(string keyword);
    Task<(IEnumerable<Job> Jobs, int TotalCount)> GetFilteredJobsAsync(
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
}