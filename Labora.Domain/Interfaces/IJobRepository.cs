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
}