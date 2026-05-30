using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Jobs;

public class JobFilterDto
{
    public string? Keyword { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public JobType? JobType { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public Guid? CategoryId { get; set; }

    // Joylashuv filtri
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; } = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}