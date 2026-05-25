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

    public async Task<IEnumerable<Job>> GetJobsByEmployerIdAsync(Guid employerId)
    {
        return await _context.Jobs
            .Where(j => j.EmployerId == employerId && !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
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

    public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetFilteredJobsAsync(
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

        IEnumerable<Job> jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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