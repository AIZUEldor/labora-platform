using AutoMapper;
using Labora.Application.Common;
using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public JobService(IJobRepository jobRepository, IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<JobResponseDto> CreateAsync(JobRequestDto request, Guid employerId)
    {
        Job job = _mapper.Map<Job>(request);
        job.EmployerId = employerId;
        job.Deadline = request.Deadline.HasValue
            ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc)
            : null;

        Job createdJob = await _jobRepository.AddAsync(job);
        return _mapper.Map<JobResponseDto>(createdJob);
    }

    public async Task<JobResponseDto> GetByIdAsync(Guid id)
    {
        Job? job = await _jobRepository.GetByIdAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        return _mapper.Map<JobResponseDto>(job);
    }

    public async Task<PagedResult<JobResponseDto>> GetAllAsync(JobFilterDto filter)
    {
        (IEnumerable<Job> jobs, int totalCount) = await _jobRepository.GetFilteredJobsAsync(
            filter.Keyword,
            filter.City,
            filter.Country,
            filter.JobType,
            filter.MinSalary,
            filter.MaxSalary,
            filter.CategoryId,
            filter.PageNumber,
            filter.PageSize);

        return new PagedResult<JobResponseDto>
        {
            Items = _mapper.Map<IEnumerable<JobResponseDto>>(jobs),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<JobResponseDto>> GetByEmployerIdAsync(Guid employerId)
    {
        IEnumerable<Job> jobs = await _jobRepository.GetJobsByEmployerIdAsync(employerId);
        return _mapper.Map<IEnumerable<JobResponseDto>>(jobs);
    }

    public async Task<JobResponseDto> UpdateAsync(Guid id, JobRequestDto request, Guid employerId)
    {
        Job? job = await _jobRepository.GetByIdAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        if (job.EmployerId != employerId)
            throw new InvalidOperationException("Siz bu ishni tahrirlash huquqiga ega emassiz.");

        _mapper.Map(request, job);
        job.Deadline = request.Deadline.HasValue
            ? DateTime.SpecifyKind(request.Deadline.Value, DateTimeKind.Utc)
            : null;

        Job updatedJob = await _jobRepository.UpdateAsync(job);
        return _mapper.Map<JobResponseDto>(updatedJob);
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
}