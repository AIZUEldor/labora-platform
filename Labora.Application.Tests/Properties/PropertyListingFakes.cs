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
        if (_listings.TryGetValue(id, out PropertyListing? listing))
            listing.IsDeleted = true;
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
        => throw new NotImplementedException();

    public Task<IEnumerable<(PropertyListing PropertyListing, string? CoverImageUrl)>> GetByOwnerIdAsync(Guid ownerId)
        => throw new NotImplementedException();

    public Task<IEnumerable<(Guid Id, double Latitude, double Longitude, decimal Price, PropertyType PropertyType)>> GetPublishedMarkersAsync()
        => throw new NotImplementedException();

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
