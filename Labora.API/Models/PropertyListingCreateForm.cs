using Labora.Domain.Enums;

namespace Labora.API.Models;

// [FromForm] binding target for POST api/PropertyListing - multipart-only concerns (IFormFile)
// live here so Application-layer contracts stay framework-independent.
public class PropertyListingCreateForm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public int RoomCount { get; set; }
    public decimal AreaSquareMeters { get; set; }
    public int? FloorNumber { get; set; }
    public int? TotalFloors { get; set; }
    public RenovationStatus RenovationStatus { get; set; }
    public decimal Price { get; set; }
    public RentalPeriod RentalPeriod { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public List<IFormFile> Images { get; set; } = new();
}
