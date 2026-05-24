using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobResponseDto> CreateAsync(JobRequestDto request, Guid employerId)
    {
        Job job = new Job
        {
            Title = request.Title,
            Description = request.Description,
            Salary = request.Salary,
            JobType = request.JobType,
            CategoryName = request.CategoryName,
            SubCategoryName = request.SubCategoryName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            City = request.City,
            Country = request.Country,
            RequiredSkills = request.RequiredSkills,
            ExperienceYears = request.ExperienceYears,
            Deadline = request.Deadline.HasValue
    ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc)
    : null,
            CategoryId = request.CategoryId,
            EmployerId = employerId
        };

        Job createdJob = await _jobRepository.AddAsync(job);
        return MapToResponseDto(createdJob);
    }

    public async Task<JobResponseDto> GetByIdAsync(Guid id)
    {
        Job? job = await _jobRepository.GetByIdAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        return MapToResponseDto(job);
    }

    public async Task<IEnumerable<JobResponseDto>> GetAllAsync()
    {
        IEnumerable<Job> jobs = await _jobRepository.GetAllAsync();
        return jobs.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<JobResponseDto>> GetByEmployerIdAsync(Guid employerId)
    {
        IEnumerable<Job> jobs = await _jobRepository.GetJobsByEmployerIdAsync(employerId);
        return jobs.Select(MapToResponseDto);
    }

    public async Task<JobResponseDto> UpdateAsync(Guid id, JobRequestDto request, Guid employerId)
    {
        Job? job = await _jobRepository.GetByIdAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        if (job.EmployerId != employerId)
            throw new InvalidOperationException("Siz bu ishni tahrirlash huquqiga ega emassiz.");

        job.Title = request.Title;
        job.Description = request.Description;
        job.Salary = request.Salary;
        job.JobType = request.JobType;
        job.CategoryName = request.CategoryName;
        job.SubCategoryName = request.SubCategoryName;
        job.Latitude = request.Latitude;
        job.Longitude = request.Longitude;
        job.City = request.City;
        job.Country = request.Country;
        job.RequiredSkills = request.RequiredSkills;
        job.ExperienceYears = request.ExperienceYears;
        job.Deadline = request.Deadline.HasValue
    ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc)
    : null;
        job.CategoryId = request.CategoryId;

        Job updatedJob = await _jobRepository.UpdateAsync(job);
        return MapToResponseDto(updatedJob);
    }

    public async Task DeleteAsync(Guid id, Guid employerId)
    {
        Job? job = await _jobRepository.GetByIdAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        if (job.EmployerId != employerId)
            throw new InvalidOperationException("Siz bu ishni o'chirish huquqiga ega emassiz.");

        await _jobRepository.DeleteAsync(id);
    }

    private static JobResponseDto MapToResponseDto(Job job)
    {
        return new JobResponseDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Salary = job.Salary,
            JobType = job.JobType,
            Status = job.Status,
            CategoryName = job.CategoryName,
            SubCategoryName = job.SubCategoryName,
            Latitude = job.Latitude,
            Longitude = job.Longitude,
            City = job.City,
            Country = job.Country,
            RequiredSkills = job.RequiredSkills,
            ExperienceYears = job.ExperienceYears,
            Deadline = job.Deadline,
            EmployerId = job.EmployerId,
            CategoryId = job.CategoryId,
            CreatedAt = job.CreatedAt
        };
    }
}