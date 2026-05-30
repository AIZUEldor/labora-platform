using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(Guid jobId);
    Task<IEnumerable<JobApplication>> GetApplicationsByWorkerIdAsync(Guid workerId);
    Task<JobApplication?> GetApplicationByJobAndWorkerAsync(Guid jobId, Guid workerId);
    Task<bool> HasWorkerAppliedAsync(Guid jobId, Guid workerId);
    Task UpdateStatusAsync(Guid applicationId, ApplicationStatus status);
}