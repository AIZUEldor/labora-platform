using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class PropertyListing : BaseEntity
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
    public PropertyListingStatus Status { get; set; } = PropertyListingStatus.Published;

    // Foreign key
    public Guid OwnerId { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
