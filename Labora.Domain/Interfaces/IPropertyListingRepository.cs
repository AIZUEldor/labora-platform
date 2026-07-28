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

    // "My listings" - unlike GetAllPublishedAsync, this is not filtered by Status: the owner sees
    // both Published and Archived listings, same "my X" convention as IJobRepository's employer
    // list. Same cover-image-only projection, no full Images Include.
    Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetByOwnerIdAsync(Guid ownerId);

    // Lightweight marker projection for the map screen - scalars only, no entity materialized and
    // no Application-layer DTO referenced here (Labora.Domain cannot depend on Labora.Application).
    Task<IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)>> GetPublishedMarkersAsync();

    // Mirrors IJobRepository.AddImageAsync/DeleteImageAsync, adapted for PropertyImage's extra
    // StorageKey/SortOrder columns and this entity's soft-delete convention (JobImage is hard-deleted).
    Task<PropertyImage> AddImageAsync(Guid propertyListingId, string imageUrl, string storageKey, int sortOrder);
    Task DeleteImageAsync(Guid imageId);

    // Bulk SortOrder rewrite for the reorder endpoint - one round trip for the whole ordered set.
    Task ReorderImagesAsync(IReadOnlyList<(Guid ImageId, int SortOrder)> orderedImages);
}
