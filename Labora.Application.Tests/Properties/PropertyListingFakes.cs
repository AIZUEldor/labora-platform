using System.Linq.Expressions;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;

namespace Labora.Application.Tests.Properties;

internal sealed class FakePropertyListingRepository : IPropertyListingRepository
{
    private readonly Dictionary<Guid, PropertyListing> _listings = new();

    public int UpdateAsyncCallCount { get; private set; }
    public int GetByIdWithImagesAsyncCallCount { get; private set; }
    public int DeleteAsyncCallCount { get; private set; }

    public void SeedListing(PropertyListing listing) => _listings[listing.Id] = listing;

    public Task<PropertyListing?> GetByIdAsync(Guid id)
    {
        if (!_listings.TryGetValue(id, out PropertyListing? listing) || listing.IsDeleted)
            return Task.FromResult<PropertyListing?>(null);
        return Task.FromResult<PropertyListing?>(Clone(listing, includeImages: false));
    }

    public Task<IEnumerable<PropertyListing>> GetAllAsync() => throw new NotImplementedException();

    public Task<IEnumerable<PropertyListing>> FindAsync(Expression<Func<PropertyListing, bool>> predicate)
        => throw new NotImplementedException();

    public Task<PropertyListing> AddAsync(PropertyListing entity)
    {
        _listings[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<PropertyListing> UpdateAsync(PropertyListing entity)
    {
        UpdateAsyncCallCount++;
        // Mirrors GenericRepository.UpdateAsync's one observable side effect that matters for
        // these tests, without needing a real DbContext/change tracker.
        entity.UpdatedAt = DateTime.UtcNow;
        _listings[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        DeleteAsyncCallCount++;
        // Mirrors GenericRepository.DeleteAsync's real behavior: soft-delete only, PropertyImage
        // rows are never touched.
        if (_listings.TryGetValue(id, out PropertyListing? listing))
        {
            listing.IsDeleted = true;
            listing.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_listings.ContainsKey(id));

    public Task<PropertyListing?> GetByIdWithImagesAsync(Guid id)
    {
        GetByIdWithImagesAsyncCallCount++;
        if (!_listings.TryGetValue(id, out PropertyListing? listing) || listing.IsDeleted)
            return Task.FromResult<PropertyListing?>(null);

        // Mirrors the real (now-tracked) repository query: same graph, same reference identity for
        // the seeded PropertyImage instances, so tests can assert the exact objects are unchanged.
        return Task.FromResult<PropertyListing?>(listing);
    }

    public Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetAllPublishedAsync()
    {
        // Mirrors the real query's filter: !IsDeleted && Status == Published.
        IEnumerable<(PropertyListing, string?)> result = _listings.Values
            .Where(l => !l.IsDeleted && l.Status == PropertyListingStatus.Published)
            .Select(l => (Clone(l, includeImages: false), ComputeCoverImageUrl(l)));
        return Task.FromResult(result);
    }

    public Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetByOwnerIdAsync(Guid ownerId)
    {
        // Mirrors the real query's filter: !IsDeleted && OwnerId == ownerId, no Status filter.
        IEnumerable<(PropertyListing, string?)> result = _listings.Values
            .Where(l => !l.IsDeleted && l.OwnerId == ownerId)
            .Select(l => (Clone(l, includeImages: false), ComputeCoverImageUrl(l)));
        return Task.FromResult(result);
    }

    public Task<IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)>> GetPublishedMarkersAsync()
    {
        // Mirrors the real query's filter: !IsDeleted && Status == Published.
        IEnumerable<(Guid, double, double, decimal, PropertyType)> result = _listings.Values
            .Where(l => !l.IsDeleted && l.Status == PropertyListingStatus.Published)
            .Select(l => (l.Id, l.Latitude, l.Longitude, l.Price, l.PropertyType));
        return Task.FromResult(result);
    }

    private static string? ComputeCoverImageUrl(PropertyListing listing)
        => listing.Images
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();

    private static PropertyListing Clone(PropertyListing source, bool includeImages)
    {
        return new PropertyListing
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            PropertyType = source.PropertyType,
            RoomCount = source.RoomCount,
            AreaSquareMeters = source.AreaSquareMeters,
            FloorNumber = source.FloorNumber,
            TotalFloors = source.TotalFloors,
            RenovationStatus = source.RenovationStatus,
            Price = source.Price,
            RentalPeriod = source.RentalPeriod,
            Address = source.Address,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            ContactPhoneNumber = source.ContactPhoneNumber,
            Status = source.Status,
            OwnerId = source.OwnerId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            IsDeleted = source.IsDeleted,
            Images = includeImages ? source.Images : new List<PropertyImage>()
        };
    }
}
