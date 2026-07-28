using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Properties;

public class PropertyMarkerDto
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Price { get; set; }
    public PropertyType PropertyType { get; set; }
    public RentalPeriod RentalPeriod { get; set; }
}
