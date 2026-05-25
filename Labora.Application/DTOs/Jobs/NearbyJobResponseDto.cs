using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Jobs;

public class NearbyJobResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public JobType JobType { get; set; }
    public JobStatus Status { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid EmployerId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foydalanuvchidan masofasi
    public double DistanceKm { get; set; }
}