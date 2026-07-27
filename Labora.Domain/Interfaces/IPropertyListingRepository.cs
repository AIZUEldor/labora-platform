using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IPropertyListingRepository : IGenericRepository<PropertyListing>
{
    // Mirrors IJobRepository.GetByIdWithImagesAsync: ordered + filtered Include, used for the
    // detail fetch so the mapped response reflects real persisted PropertyImage rows instead of
    // always showing an empty gallery/null cover image.
    Task<PropertyListing?> GetByIdWithImagesAsync(Guid id);

    // Mirrors IJobRepository.GetJobsByEmployerIdAsync: pairs each listing with a separately-
    // projected cover image URL instead of loading the full Images collection - one query, no
    // per-listing round trip, and no Images navigation touched on the returned entities.
    Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetAllPublishedAsync();

    // Lightweight marker projection for the map screen - scalars only, no entity materialized and
    // no Application-layer DTO referenced here (Labora.Domain cannot depend on Labora.Application).
    Task<IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)>> GetPublishedMarkersAsync();
}
