using Labora.Application.DTOs.Applications;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IJobRepository _jobRepository;

    public JobApplicationService(
        IJobApplicationRepository jobApplicationRepository,
        IJobRepository jobRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _jobRepository = jobRepository;
    }

    public async Task<ApplicationResponseDto> ApplyAsync(ApplicationRequestDto request, Guid workerId)
    {
        Job? job = await _jobRepository.GetByIdAsync(request.JobId);

        if (job is null)
            throw new InvalidOperationException($"Id={request.JobId} bo'lgan ish topilmadi.");

        if (job.Status != JobStatus.Active)
            throw new InvalidOperationException("Bu ish endi faol emas.");

        JobApplication jobApplication = new JobApplication
        {
            JobId = request.JobId,
            WorkerId = workerId,
            CoverLetter = request.CoverLetter,
            Status = ApplicationStatus.Pending
        };

        JobApplication createdApplication = await _jobApplicationRepository.AddAsync(jobApplication);
        return MapToResponseDto(createdApplication);
    }

    public async Task<IEnumerable<ApplicationResponseDto>> GetByWorkerIdAsync(Guid workerId)
    {
        IEnumerable<JobApplication> applications = await _jobApplicationRepository.GetApplicationsByWorkerIdAsync(workerId);
        return applications.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<ApplicationResponseDto>> GetByJobIdAsync(Guid jobId)
    {
        IEnumerable<JobApplication> applications = await _jobApplicationRepository.GetApplicationsByJobIdAsync(jobId);
        return applications.Select(MapToResponseDto);
    }

    public async Task<ApplicationResponseDto> UpdateStatusAsync(Guid id, string status, Guid employerId)
    {
        JobApplication? jobApplication = await _jobApplicationRepository.GetByIdAsync(id);

        if (jobApplication is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ariza topilmadi.");

        if (jobApplication.Job.EmployerId != employerId)
            throw new InvalidOperationException("Siz bu arizani o'zgartirish huquqiga ega emassiz.");

        if (!Enum.TryParse<ApplicationStatus>(status, true, out ApplicationStatus newStatus))
            throw new InvalidOperationException($"Noto'g'ri status: {status}");

        jobApplication.Status = newStatus;

        JobApplication updatedApplication = await _jobApplicationRepository.UpdateAsync(jobApplication);
        return MapToResponseDto(updatedApplication);
    }

    public async Task CancelAsync(Guid id, Guid workerId)
    {
        JobApplication? jobApplication = await _jobApplicationRepository.GetByIdAsync(id);

        if (jobApplication is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ariza topilmadi.");

        if (jobApplication.WorkerId != workerId)
            throw new InvalidOperationException("Siz bu arizani bekor qilish huquqiga ega emassiz.");

        if (jobApplication.Status != ApplicationStatus.Pending)
            throw new InvalidOperationException("Faqat kutilayotgan arizalarni bekor qilish mumkin.");

        jobApplication.Status = ApplicationStatus.Cancelled;
        await _jobApplicationRepository.UpdateAsync(jobApplication);
    }

    private static ApplicationResponseDto MapToResponseDto(JobApplication jobApplication)
    {
        return new ApplicationResponseDto
        {
            Id = jobApplication.Id,
            JobId = jobApplication.JobId,
            WorkerId = jobApplication.WorkerId,
            CoverLetter = jobApplication.CoverLetter,
            Status = jobApplication.Status,
            CreatedAt = jobApplication.CreatedAt
        };
    }
}