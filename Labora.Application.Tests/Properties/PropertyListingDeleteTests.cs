using AutoMapper;
using Labora.Application.DTOs.Properties;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Labora.Application.Tests.WorkerPosts;
using Labora.Application.Validators.Properties;
using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Application.Tests.Properties;

public class PropertyListingDeleteTests
{
    private static (PropertyListingService Service, FakePropertyListingRepository Repository) CreateService()
    {
        FakePropertyListingRepository repository = new();
        FakeFileStorageService storage = new();
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        PropertyListingService service = new(repository, mapper, storage, new PropertyListingRequestDtoValidator());
        return (service, repository);
    }

    private static PropertyListing SeedListing(FakePropertyListingRepository repository, Guid ownerId)
    {
        PropertyListing listing = new()
        {
            Id = Guid.NewGuid(),
            Title = "Listing to delete",
            Description = "Description",
            PropertyType = PropertyType.House,
            RoomCount = 3,
            AreaSquareMeters = 80,
            RenovationStatus = RenovationStatus.Good,
            Price = 500_000,
            RentalPeriod = RentalPeriod.Monthly,
            Address = "Some address",
            Latitude = 41.3,
            Longitude = 69.2,
            ContactPhoneNumber = "+998900000000",
            Status = PropertyListingStatus.Published,
            OwnerId = ownerId
        };
        listing.Images.Add(new PropertyImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = "/uploads/property-images/existing-0.jpg",
            StorageKey = "/uploads/property-images/existing-0.jpg",
            SortOrder = 0,
            PropertyListingId = listing.Id
        });
        repository.SeedListing(listing);
        return listing;
    }

    [Fact]
    public async Task DeleteAsync_Owner_SoftDeletesListing()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);

        await service.DeleteAsync(listing.Id, ownerId);

        Assert.Equal(1, repository.DeleteAsyncCallCount);
        Assert.True(listing.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ListingNotFound_ThrowsInvalidOperation()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteAsync(Guid.NewGuid(), ownerId));

        Assert.Equal(0, repository.DeleteAsyncCallCount);
    }

    [Fact]
    public async Task DeleteAsync_NonOwner_ThrowsInvalidOperation_AndDoesNotDelete()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteAsync(listing.Id, attackerId));

        Assert.Equal(0, repository.DeleteAsyncCallCount);
        Assert.False(listing.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_Owner_ExcludesListingFromPublishedListMarkersAndDetail()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);

        // Sanity check: visible everywhere before delete.
        Assert.Contains(await service.GetAllPublishedAsync(), r => r.Id == listing.Id);
        Assert.Contains(await service.GetPublishedMarkersAsync(), m => m.Id == listing.Id);
        Assert.Contains(await service.GetByOwnerIdAsync(ownerId), r => r.Id == listing.Id);
        await service.GetByIdAsync(listing.Id); // does not throw

        await service.DeleteAsync(listing.Id, ownerId);

        Assert.DoesNotContain(await service.GetAllPublishedAsync(), r => r.Id == listing.Id);
        Assert.DoesNotContain(await service.GetPublishedMarkersAsync(), m => m.Id == listing.Id);
        Assert.DoesNotContain(await service.GetByOwnerIdAsync(ownerId), r => r.Id == listing.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(listing.Id));
    }

    [Fact]
    public async Task DeleteAsync_Owner_RelatedImagesAreNeitherRemovedNorModified()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);
        PropertyImage originalImage = listing.Images.Single();
        string originalUrl = originalImage.ImageUrl;

        await service.DeleteAsync(listing.Id, ownerId);

        // The listing is soft-deleted, but its PropertyImage rows are left completely untouched -
        // same convention as JobImage on Job delete: no cascade, no IsDeleted flip, no data loss.
        PropertyImage stillPresent = Assert.Single(listing.Images);
        Assert.Same(originalImage, stillPresent);
        Assert.False(stillPresent.IsDeleted);
        Assert.Equal(originalUrl, stillPresent.ImageUrl);
    }
}
