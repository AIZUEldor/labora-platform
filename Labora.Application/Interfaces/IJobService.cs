using Labora.Application.DTOs.Jobs;

namespace Labora.Application.Interfaces;

public interface IJobService
{
    Task<JobResponseDto> CreateAsync(JobRequestDto request, Guid employerId);
    Task<JobResponseDto> GetByIdAsync(Guid id);
    Task<IEnumerable<JobResponseDto>> GetAllAsync();
    Task<IEnumerable<JobResponseDto>> GetByEmployerIdAsync(Guid employerId);
    Task<JobResponseDto> UpdateAsync(Guid id, JobRequestDto request, Guid employerId);
    Task DeleteAsync(Guid id, Guid employerId);
}