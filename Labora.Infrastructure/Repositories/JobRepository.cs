using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    private readonly LaboaDbContext _context;

    public JobRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<(Job Job, string? CoverImageUrl)>> GetJobsByEmployerIdAsync(Guid employerId)
    {
        // Single query: CoverImageUrl is a correlated-subquery scalar, not a loaded collection - no
        // Include(j => j.Images) here, so the full gallery is never fetched for this list endpoint.
        var rows = await _context.Jobs
            .Where(j => j.EmployerId == employerId && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new
            {
                Job = j,
                CoverImageUrl = j.Images
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.CreatedAt)
                    .ThenBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows.Select(r => (r.Job, r.CoverImageUrl));
    }

    public async Task<IEnumerable<Job>> GetJobsByTypeAsync(JobType jobType)
    {
        return await _context.Jobs
            .Where(j => j.JobType == jobType && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId)
    {
        return await _context.Jobs
            .Where(j => j.CategoryId == categoryId && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetJobsByLocationAsync(string city, string country)
    {
        return await _context.Jobs
            .Where(j => j.City == city && j.Country == country && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Job>> SearchJobsAsync(string keyword)
    {
        return await _context.Jobs
            .Where(j => !j.IsDeleted &&
                (j.Title.Contains(keyword) || j.Description.Contains(keyword)))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<(Job Job, string? CoverImageUrl)> Jobs, int TotalCount)> GetFilteredJobsAsync(
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
        IQueryable<Job> query = _context.Jobs.Where(j => !j.IsDeleted);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));

        if (!string.IsNullOrEmpty(city))
            query = query.Where(j => j.City == city);

        if (!string.IsNullOrEmpty(country))
            query = query.Where(j => j.Country == country);

        if (jobType.HasValue)
            query = query.Where(j => j.JobType == jobType.Value);

        if (minSalary.HasValue)
            query = query.Where(j => j.Salary >= minSalary.Value);

        if (maxSalary.HasValue)
            query = query.Where(j => j.Salary <= maxSalary.Value);

        if (categoryId.HasValue)
            query = query.Where(j => j.CategoryId == categoryId.Value);

        int totalCount = await query.CountAsync();

        // Same single-query cover-image projection as GetJobsByEmployerIdAsync above - pagination
        // and filtering happen exactly as before, only the final Select shape changed.
        var rows = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new
            {
                Job = j,
                CoverImageUrl = j.Images
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.CreatedAt)
                    .ThenBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        IEnumerable<(Job Job, string? CoverImageUrl)> jobs = rows.Select(r => (r.Job, r.CoverImageUrl));

        return (jobs, totalCount);
    }

    public async Task<IEnumerable<Job>> GetNearbyJobsAsync(
        double latitude,
        double longitude,
        double radiusKm)
    {
        // Haversine formula uchun taxminiy daraja/km nisbati
        double latDegreeKm = 111.0;
        double lonDegreeKm = 111.0 * Math.Cos(latitude * Math.PI / 180.0);

        double latDelta = radiusKm / latDegreeKm;
        double lonDelta = radiusKm / lonDegreeKm;

        // Bounding box — tez filter
        List<Job> candidateJobs = await _context.Jobs
            .Where(j => !j.IsDeleted &&
                j.Status == Domain.Enums.JobStatus.Active &&
                j.Latitude >= latitude - latDelta &&
                j.Latitude <= latitude + latDelta &&
                j.Longitude >= longitude - lonDelta &&
                j.Longitude <= longitude + lonDelta)
            .ToListAsync();

        // Aniq Haversine hisoblash
        return candidateJobs
            .Where(j => CalculateDistance(latitude, longitude, j.Latitude, j.Longitude) <= radiusKm)
            .OrderBy(j => CalculateDistance(latitude, longitude, j.Latitude, j.Longitude))
            .ToList();
    }

    public async Task<IEnumerable<Job>> GetRecommendedJobsAsync(
    double? latitude,
    double? longitude,
    IEnumerable<UserPreference> preferences)
    {
        IQueryable<Job> query = _context.Jobs
            .Where(j => !j.IsDeleted && j.Status == JobStatus.Active);

        // Kategoriya va JobType filtri
        List<Guid> categoryIds = preferences
            .Where(p => p.PreferredCategoryId.HasValue)
            .Select(p => p.PreferredCategoryId!.Value)
            .ToList();

        JobType? preferredJobType = preferences
            .FirstOrDefault(p => p.PreferredJobType.HasValue)?.PreferredJobType;

        double maxDistance = preferences
            .FirstOrDefault(p => p.MaxDistanceKm.HasValue)?.MaxDistanceKm ?? 20.0;

        if (categoryIds.Any())
            query = query.Where(j => j.CategoryId.HasValue && categoryIds.Contains(j.CategoryId.Value));

        if (preferredJobType.HasValue)
            query = query.Where(j => j.JobType == preferredJobType.Value);

        List<Job> jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        // Lokatsiya filtri
        if (latitude.HasValue && longitude.HasValue)
        {
            jobs = jobs
                .Where(j => CalculateDistance(
                    latitude.Value, longitude.Value,
                    j.Latitude, j.Longitude) <= maxDistance)
                .OrderBy(j => CalculateDistance(
                    latitude.Value, longitude.Value,
                    j.Latitude, j.Longitude))
                .ToList();
        }

        return jobs;
    }

    public async Task<IEnumerable<Job>> GetAllActiveWithLocationAsync()
    {
        return await _context.Jobs
            .Where(j => !j.IsDeleted &&
                        j.Status == Domain.Enums.JobStatus.Active &&
                        j.Latitude != 0 &&
                        j.Longitude != 0)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<Job?> GetByIdWithImagesAsync(Guid id)
    {
        // Ordered + filtered Include: images must come back in a deterministic order (CreatedAt,
        // then Id as a tie-breaker) rather than whatever order Postgres happens to return, and the
        // IsDeleted filter is a defensive match for this codebase's universal soft-delete convention
        // even though JobImage rows are currently hard-deleted (no code path sets IsDeleted on one).
        return await _context.Jobs
            .Include(j => j.Images
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.CreatedAt)
                .ThenBy(i => i.Id))
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
    }

    public async Task<JobImage> AddImageAsync(Guid jobId, string imageUrl, string? caption)
    {
        JobImage image = new JobImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            Caption = caption,
            JobId = jobId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.JobImages.AddAsync(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task DeleteImageAsync(Guid imageId)
    {
        JobImage? image = await _context.JobImages.FindAsync(imageId);
        if (image != null)
        {
            _context.JobImages.Remove(image);
            await _context.SaveChangesAsync();
        }
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