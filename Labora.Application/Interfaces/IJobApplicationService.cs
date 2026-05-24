using Labora.Application.DTOs.Applications;

namespace Labora.Application.Interfaces;

public interface IJobApplicationService
{
    Task<ApplicationResponseDto> ApplyAsync(ApplicationRequestDto request, Guid workerId);
    Task<IEnumerable<ApplicationResponseDto>> GetByWorkerIdAsync(Guid workerId);
    Task<IEnumerable<ApplicationResponseDto>> GetByJobIdAsync(Guid jobId);
    Task<ApplicationResponseDto> UpdateStatusAsync(Guid id, string status, Guid employerId);
    Task CancelAsync(Guid id, Guid workerId);
}