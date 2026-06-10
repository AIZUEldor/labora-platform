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
    private readonly IUserRepository _userRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;

    public JobService(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IPushNotificationService pushNotificationService,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _pushNotificationService = pushNotificationService;
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

        _ = NotifyNearbyWorkersAsync(createdJob);

        return _mapper.Map<JobResponseDto>(createdJob);
    }

    private async Task NotifyNearbyWorkersAsync(Job job)
    {
        try
        {
            if (job.Latitude == 0 && job.Longitude == 0) return;

            IEnumerable<User> workers = await _userRepository.GetWorkerUsersAsync();

            List<Guid> nearbyWorkerIds = workers
                .Where(w =>
                    w.Latitude.HasValue &&
                    w.Longitude.HasValue &&
                    CalculateDistance(job.Latitude, job.Longitude,
                        w.Latitude.Value, w.Longitude.Value) <= 5.0)
                .Select(w => w.Id)
                .ToList();

            if (!nearbyWorkerIds.Any()) return;

            await _pushNotificationService.SendToUsersAsync(
                nearbyWorkerIds,
                "Yangi ish e'loni",
                $"{job.Title} — {job.Salary:N0} so'm",
                new { referenceId = job.Id.ToString() }
            );
        }
        catch
        {
            // Push xatoligi job yaratishni to'xtatmasin
        }
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

    public async Task<IEnumerable<NearbyJobResponseDto>> GetNearbyJobsAsync(
        double latitude,
        double longitude,
        double radiusKm)
    {
        IEnumerable<Job> jobs = await _jobRepository.GetNearbyJobsAsync(latitude, longitude, radiusKm);

        return jobs.Select(job => new NearbyJobResponseDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Salary = job.Salary,
            JobType = job.JobType,
            Status = job.Status,
            CategoryName = job.CategoryName,
            Latitude = job.Latitude,
            Longitude = job.Longitude,
            City = job.City,
            Country = job.Country,
            EmployerId = job.EmployerId,
            CreatedAt = job.CreatedAt,
            DistanceKm = CalculateDistance(latitude, longitude, job.Latitude, job.Longitude)
        });
    }

    public async Task<IEnumerable<NearbyJobResponseDto>> GetAllActiveJobsAsync(double latitude, double longitude)
    {
        IEnumerable<Job> jobs = await _jobRepository.GetAllActiveWithLocationAsync();

        return jobs.Select(job => new NearbyJobResponseDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Salary = job.Salary,
            JobType = job.JobType,
            Status = job.Status,
            CategoryName = job.CategoryName,
            Latitude = job.Latitude,
            Longitude = job.Longitude,
            City = job.City,
            Country = job.Country,
            EmployerId = job.EmployerId,
            CreatedAt = job.CreatedAt,
            DistanceKm = CalculateDistance(latitude, longitude, job.Latitude, job.Longitude)
        });
    }

    private static double CalculateDistance(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        double earthRadiusKm = 6371.0;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
}