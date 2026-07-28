using AutoMapper;
using Labora.Application.DTOs.Properties;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Labora.Application.Tests.WorkerPosts;
using Labora.Application.Validators.Properties;
using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Application.Tests.Properties;

public class PropertyListingImageTests
{
    private static (PropertyListingService Service, FakePropertyListingRepository Repository, FakeFileStorageService Storage) CreateService()
    {
        FakePropertyListingRepository repository = new();
        FakeFileStorageService storage = new();
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        PropertyListingService service = new(repository, mapper, storage, new PropertyListingRequestDtoValidator());
        return (service, repository, storage);
    }

    private static PropertyListing SeedListing(FakePropertyListingRepository repository, Guid ownerId, int imageCount = 1)
    {
        PropertyListing listing = new()
        {
            Id = Guid.NewGuid(),
            Title = "Listing",
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
        for (int i = 0; i < imageCount; i++)
        {
            listing.Images.Add(new PropertyImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = $"/uploads/property-images/existing-{i}.jpg",
                StorageKey = $"/uploads/property-images/existing-{i}.jpg",
                SortOrder = i,
                PropertyListingId = listing.Id
            });
        }
        repository.SeedListing(listing);
        return listing;
    }

    private static FakeFormFile ValidFile() => new(new byte[] { 1, 2, 3 }, "photo.jpg", "image/jpeg");

    [Fact]
    public async Task AddImageAsync_Owner_AppendsAfterHighestSortOrder()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, _) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2); // SortOrder 0, 1

        PropertyImageDto result = await service.AddImageAsync(listing.Id, ownerId, ValidFile());

        Assert.Equal(2, result.SortOrder);
        Assert.Equal(1, repository.AddImageAsyncCallCount);
        Assert.Equal(3, listing.Images.Count);
    }

    [Fact]
    public async Task AddImageAsync_AtMax_ThrowsAndDoesNotUploadOrPersist()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 10); // MaxPropertyImages

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddImageAsync(listing.Id, ownerId, ValidFile()));

        Assert.Equal(0, storage.SaveAsyncCallCount);
        Assert.Equal(0, repository.AddImageAsyncCallCount);
        Assert.Equal(10, listing.Images.Count);
    }

    [Fact]
    public async Task AddImageAsync_NonOwner_ThrowsAndDoesNotUploadOrPersist()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddImageAsync(listing.Id, attackerId, ValidFile()));

        Assert.Equal(0, storage.SaveAsyncCallCount);
        Assert.Equal(0, repository.AddImageAsyncCallCount);
    }

    [Fact]
    public async Task AddImageAsync_RepositoryFailure_DeletesTheJustSavedPhysicalFile()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);
        repository.FaultOnAddImageAsync = _ => new InvalidOperationException("simulated DB failure");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddImageAsync(listing.Id, ownerId, ValidFile()));

        Assert.Single(storage.SavedUrls);
        Assert.Equal(storage.SavedUrls, storage.DeletedUrls);
    }

    [Fact]
    public async Task DeleteImageAsync_Owner_SoftDeletesRowAndDeletesFile()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);
        PropertyImage imageToDelete = listing.Images.First();

        await service.DeleteImageAsync(listing.Id, ownerId, imageToDelete.Id);

        Assert.True(imageToDelete.IsDeleted);
        Assert.Contains(imageToDelete.ImageUrl, storage.DeletedUrls);
        Assert.Equal(1, repository.DeleteImageAsyncCallCount);
    }

    [Fact]
    public async Task DeleteImageAsync_LastRemainingImage_ThrowsAndDoesNotDelete()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 1);
        PropertyImage onlyImage = listing.Images.Single();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.DeleteImageAsync(listing.Id, ownerId, onlyImage.Id));

        Assert.False(onlyImage.IsDeleted);
        Assert.Equal(0, repository.DeleteImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task DeleteImageAsync_ImageNotOnProperty_ThrowsAndDoesNotDelete()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteImageAsync(listing.Id, ownerId, Guid.NewGuid()));

        Assert.Equal(0, repository.DeleteImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task DeleteImageAsync_NonOwner_ThrowsAndDoesNotDelete()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);
        PropertyImage image = listing.Images.First();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteImageAsync(listing.Id, attackerId, image.Id));

        Assert.False(image.IsDeleted);
        Assert.Equal(0, repository.DeleteImageAsyncCallCount);
    }

    [Fact]
    public async Task ReorderImagesAsync_Owner_NormalizesSortOrderToRequestedSequence()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, _) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 3); // SortOrder 0,1,2
        List<Guid> reversedOrder = listing.Images.OrderByDescending(i => i.SortOrder).Select(i => i.Id).ToList();

        IEnumerable<PropertyImageDto> result = await service.ReorderImagesAsync(listing.Id, ownerId, reversedOrder);

        List<PropertyImageDto> dtos = result.ToList();
        Assert.Equal(reversedOrder, dtos.Select(d => d.Id).ToList());
        Assert.Equal(new[] { 0, 1, 2 }, dtos.Select(d => d.SortOrder).ToList());
        // Index 0 of the requested order is now the cover image.
        PropertyImage newCover = listing.Images.Single(i => i.Id == reversedOrder[0]);
        Assert.Equal(0, newCover.SortOrder);
    }

    [Fact]
    public async Task ReorderImagesAsync_DuplicateIds_ThrowsAndDoesNotReorder()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, _) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);
        Guid imageId = listing.Images.First().Id;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReorderImagesAsync(listing.Id, ownerId, new List<Guid> { imageId, imageId }));

        Assert.Equal(0, repository.ReorderImagesAsyncCallCount);
    }

    [Fact]
    public async Task ReorderImagesAsync_MissingOrForeignId_ThrowsAndDoesNotReorder()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, _) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);
        List<Guid> partialList = new() { listing.Images.First().Id }; // missing the second active image

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReorderImagesAsync(listing.Id, ownerId, partialList));

        List<Guid> foreignIdList = new() { listing.Images.First().Id, Guid.NewGuid() };
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ReorderImagesAsync(listing.Id, ownerId, foreignIdList));

        Assert.Equal(0, repository.ReorderImagesAsyncCallCount);
    }

    [Fact]
    public async Task ReorderImagesAsync_NonOwner_ThrowsAndDoesNotReorder()
    {
        (PropertyListingService service, FakePropertyListingRepository repository, _) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId, imageCount: 2);
        List<Guid> order = listing.Images.Select(i => i.Id).ToList();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReorderImagesAsync(listing.Id, attackerId, order));

        Assert.Equal(0, repository.ReorderImagesAsyncCallCount);
    }
}
