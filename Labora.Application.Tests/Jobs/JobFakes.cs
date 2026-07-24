using System.Linq.Expressions;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Application.Interfaces;

namespace Labora.Application.Tests.Jobs;

internal sealed class FakeJobRepository : IJobRepository
{
    private readonly Dictionary<Guid, Job> _jobs = new();
    private readonly Dictionary<Guid, JobImage> _images = new();

    public int AddImageAsyncCallCount { get; private set; }
    public int DeleteImageAsyncCallCount { get; private set; }
    public int GetByIdWithImagesAsyncCallCount { get; private set; }
    public int GetFilteredJobsAsyncCallCount { get; private set; }
    public int GetJobsByEmployerIdAsyncCallCount { get; private set; }
    public Func<int, Exception?>? FaultOnAddImageAsync { get; set; }

    public void SeedJob(Job job) => _jobs[job.Id] = job;

    public void SeedImage(Job job, JobImage image)
    {
        _images[image.Id] = image;
        job.Images.Add(image);
    }

    public Task<Job?> GetByIdAsync(Guid id)
    {
        // Real GenericRepository.GetByIdAsync never Includes Images, so an unpopulated navigation
        // must come back empty here too - matching that lets tests catch a caller that wrongly
        // assumes this path returns images (it must use GetByIdWithImagesAsync instead).
        if (!_jobs.TryGetValue(id, out Job? job) || job.IsDeleted)
            return Task.FromResult<Job?>(null);
        return Task.FromResult<Job?>(Clone(job, includeImages: false));
    }

    public Task<IEnumerable<Job>> GetAllAsync() => throw new NotImplementedException();

    public Task<IEnumerable<Job>> FindAsync(Expression<Func<Job, bool>> predicate) => throw new NotImplementedException();

    public Task<Job> AddAsync(Job entity)
    {
        _jobs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Job> UpdateAsync(Job entity)
    {
        _jobs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        if (_jobs.TryGetValue(id, out Job? job))
            job.IsDeleted = true;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_jobs.ContainsKey(id));

    public Task<IEnumerable<(Job Job, string? CoverImageUrl)>> GetJobsByEmployerIdAsync(Guid employerId)
    {
        GetJobsByEmployerIdAsyncCallCount++;
        // No Include(j => j.Images) in the real query - clone without images so callers can't
        // accidentally observe image data this query never actually loads; CoverImageUrl is
        // computed separately, mirroring the real repository's projected-scalar approach.
        IEnumerable<(Job Job, string? CoverImageUrl)> result = _jobs.Values
            .Where(j => j.EmployerId == employerId && !j.IsDeleted)
            .Select(j => (Clone(j, includeImages: false), ComputeCoverImageUrl(j)));
        return Task.FromResult(result);
    }

    public Task<IEnumerable<Job>> GetJobsByTypeAsync(JobType jobType) => throw new NotImplementedException();
    public Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId) => throw new NotImplementedException();
    public Task<IEnumerable<Job>> GetJobsByLocationAsync(string city, string country) => throw new NotImplementedException();
    public Task<IEnumerable<Job>> SearchJobsAsync(string keyword) => throw new NotImplementedException();

    public Task<(IEnumerable<(Job Job, string? CoverImageUrl)> Jobs, int TotalCount)> GetFilteredJobsAsync(
        string? keyword,
        string? city,
        string? country,
        JobType? jobType,
        decimal? minSalary,
        decimal? maxSalary,
        Guid? categoryId,
        int pageNumber,
        int pageSize)
    {
        GetFilteredJobsAsyncCallCount++;
        // No Include(j => j.Images) in the real query - clone without images, same reasoning as
        // GetJobsByEmployerIdAsync above.
        List<(Job Job, string? CoverImageUrl)> jobs = _jobs.Values
            .Where(j => !j.IsDeleted)
            .Select(j => (Clone(j, includeImages: false), ComputeCoverImageUrl(j)))
            .ToList();
        return Task.FromResult<(IEnumerable<(Job Job, string? CoverImageUrl)> Jobs, int TotalCount)>((jobs, jobs.Count));
    }

    private static string? ComputeCoverImageUrl(Job job)
        => job.Images
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();

    public Task<IEnumerable<Job>> GetNearbyJobsAsync(double latitude, double longitude, double radiusKm)
        => throw new NotImplementedException();

    public Task<IEnumerable<Job>> GetRecommendedJobsAsync(
        double? latitude, double? longitude, IEnumerable<UserPreference> preferences)
        => throw new NotImplementedException();

    public Task<IEnumerable<Job>> GetAllActiveWithLocationAsync() => throw new NotImplementedException();

    public Task<Job?> GetByIdWithImagesAsync(Guid id)
    {
        GetByIdWithImagesAsyncCallCount++;
        if (!_jobs.TryGetValue(id, out Job? job) || job.IsDeleted)
            return Task.FromResult<Job?>(null);

        // Mirrors the real repository's ordered + filtered Include so tests observe the same
        // deterministic ordering contract the production query is expected to produce.
        return Task.FromResult<Job?>(Clone(job, includeImages: true));
    }

    private static Job Clone(Job source, bool includeImages)
    {
        return new Job
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            Salary = source.Salary,
            JobType = source.JobType,
            Status = source.Status,
            CategoryName = source.CategoryName,
            SubCategoryName = source.SubCategoryName,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            City = source.City,
            Country = source.Country,
            RequiredSkills = source.RequiredSkills,
            ExperienceYears = source.ExperienceYears,
            Deadline = source.Deadline,
            EmployerId = source.EmployerId,
            CategoryId = source.CategoryId,
            SubCategoryId = source.SubCategoryId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            IsDeleted = source.IsDeleted,
            Images = includeImages
                ? source.Images.Where(i => !i.IsDeleted).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToList()
                : new List<JobImage>()
        };
    }

    public Task<JobImage> AddImageAsync(Guid jobId, string imageUrl, string? caption)
    {
        AddImageAsyncCallCount++;
        Exception? fault = FaultOnAddImageAsync?.Invoke(AddImageAsyncCallCount);
        if (fault != null)
            throw fault;

        JobImage image = new JobImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            Caption = caption,
            JobId = jobId,
            CreatedAt = DateTime.UtcNow
        };
        _images[image.Id] = image;

        if (_jobs.TryGetValue(jobId, out Job? job))
            job.Images.Add(image);

        return Task.FromResult(image);
    }

    public Task DeleteImageAsync(Guid imageId)
    {
        DeleteImageAsyncCallCount++;
        if (_images.Remove(imageId, out JobImage? image))
        {
            if (_jobs.TryGetValue(image.JobId, out Job? job))
            {
                JobImage? toRemove = job.Images.FirstOrDefault(i => i.Id == imageId);
                if (toRemove != null)
                    job.Images.Remove(toRemove);
            }
        }
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<IEnumerable<User>> GetAllAsync() => throw new NotImplementedException();
    public Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate) => throw new NotImplementedException();
    public Task<User> AddAsync(User entity) => throw new NotImplementedException();
    public Task<User> UpdateAsync(User entity) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(Guid id) => throw new NotImplementedException();
    public Task<User?> GetByPhoneNumberAsync(string phoneNumber) => throw new NotImplementedException();
    public Task<bool> PhoneNumberExistsAsync(string phoneNumber) => throw new NotImplementedException();
    public Task<IEnumerable<User>> GetWorkerUsersAsync() => throw new NotImplementedException();
    public Task<User?> GetByIdForUpdateAsync(Guid id) => throw new NotImplementedException();
}

internal sealed class FakePushNotificationService : IPushNotificationService
{
    public Task RegisterTokenAsync(Guid userId, string token, string? deviceType) => Task.CompletedTask;
    public Task RemoveTokenAsync(Guid userId) => Task.CompletedTask;
    public Task SendToUserAsync(Guid userId, string title, string body, object? data = null) => Task.CompletedTask;
    public Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, object? data = null) => Task.CompletedTask;
}
