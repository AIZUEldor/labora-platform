using AutoMapper;
using Labora.Application.Common;
using Labora.Application.DTOs.Jobs;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class JobService : IJobService
{
    private const int MaxJobImages = 5;
    private const string JobImagesSubFolder = "job-images";

    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public JobService(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IPushNotificationService pushNotificationService,
        IMapper mapper,
        IFileStorageService fileStorageService)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _pushNotificationService = pushNotificationService;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
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
        // Uses the images-inclusive fetch (not the plain GetByIdAsync) so the mapped response
        // actually reflects persisted JobImage rows instead of always showing an empty list.
        Job? job = await _jobRepository.GetByIdWithImagesAsync(id);

        if (job is null)
            throw new InvalidOperationException($"Id={id} bo'lgan ish topilmadi.");

        return _mapper.Map<JobResponseDto>(job);
    }

    public async Task<PagedResult<JobResponseDto>> GetAllAsync(JobFilterDto filter)
    {
        // Deliberately does not load the full Images collection: no list/search/card UI renders the
        // gallery today (only the single-job detail screen does, via GetByIdAsync) - only a cheap,
        // separately-projected CoverImageUrl is fetched, in the same single query as the jobs
        // themselves (see JobRepository.GetFilteredJobsAsync), avoiding both the join/query cost of
        // a full Include and any N+1 per-job lookup.
        (IEnumerable<(Job Job, string? CoverImageUrl)> jobs, int totalCount) = await _jobRepository.GetFilteredJobsAsync(
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
            Items = jobs.Select(MapToListItem),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<JobResponseDto>> GetByEmployerIdAsync(Guid employerId)
    {
        // Same reasoning as GetAllAsync: this is the employer's "my jobs" list, not a detail view.
        IEnumerable<(Job Job, string? CoverImageUrl)> jobs = await _jobRepository.GetJobsByEmployerIdAsync(employerId);
        return jobs.Select(MapToListItem);
    }

    // Single conversion point for both list-shaped endpoints: maps the entity's scalar fields via
    // the normal AutoMapper profile, then overwrites CoverImageUrl with the value the repository
    // already computed cheaply (the AutoMapper-derived one would otherwise be null here, since
    // Images was never loaded for this query shape - see the MappingProfile comment). Images is
    // left as AutoMapper's default empty list, matching "list responses never expose the gallery".
    private JobResponseDto MapToListItem((Job Job, string? CoverImageUrl) item)
    {
        JobResponseDto dto = _mapper.Map<JobResponseDto>(item.Job);
        dto.CoverImageUrl = item.CoverImageUrl;
        return dto;
    }

    public async Task<JobResponseDto> UpdateAsync(Guid id, JobRequestDto request, Guid employerId)
    {
        // Images-inclusive fetch: JobRequestDto never touches Images, so the collection loaded here
        // survives _mapper.Map(request, job) untouched and the returned response ends up showing the
        // job's real persisted images instead of always an empty list.
        Job? job = await _jobRepository.GetByIdWithImagesAsync(id);

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

    public async Task<JobImage> AddImageAsync(Guid jobId, Guid employerId, IFormFile file, string? caption)
    {
        Job? job = await _jobRepository.GetByIdWithImagesAsync(jobId);
        if (job is null)
            throw new KeyNotFoundException("Ish topilmadi.");
        if (job.EmployerId != employerId)
            throw new UnauthorizedAccessException("Bu ish sizga tegishli emas.");
        if (job.Images.Count >= MaxJobImages)
            throw new ArgumentException($"Maksimal {MaxJobImages} ta rasm yuklash mumkin.");

        ImageUploadValidator.Validate(file);

        string imageUrl = await _fileStorageService.SaveAsync(file, JobImagesSubFolder);

        try
        {
            return await _jobRepository.AddImageAsync(jobId, imageUrl, caption);
        }
        catch
        {
            // The physical file was already written before this DB write was attempted. If the DB
            // write fails, the file would otherwise be orphaned with no matching row - remove it so
            // a failed upload never leaves storage inconsistent with the database.
            _fileStorageService.Delete(imageUrl);
            throw;
        }
    }

    public async Task DeleteImageAsync(Guid jobId, Guid employerId, Guid imageId)
    {
        Job? job = await _jobRepository.GetByIdWithImagesAsync(jobId);
        if (job is null)
            throw new KeyNotFoundException("Ish topilmadi.");
        if (job.EmployerId != employerId)
            throw new UnauthorizedAccessException("Bu ish sizga tegishli emas.");

        JobImage? image = job.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new KeyNotFoundException("Rasm ushbu ishga tegishli emas.");

        await _jobRepository.DeleteImageAsync(imageId);

        // Physical-file removal only happens after the DB row is confirmed gone, and must never turn
        // this already-successful delete into a failed request - a missing/inaccessible file at this
        // point is not an error (IFileStorageService.Delete guarantees it never throws for that).
        _fileStorageService.Delete(image.ImageUrl);
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