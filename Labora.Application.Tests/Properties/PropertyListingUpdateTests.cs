using AutoMapper;
using Labora.Application.DTOs.Properties;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Labora.Application.Tests.WorkerPosts;
using Labora.Application.Validators.Properties;
using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Application.Tests.Properties;

public class PropertyListingUpdateTests
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
            Title = "Original title",
            Description = "Original description",
            PropertyType = PropertyType.House,
            RoomCount = 3,
            AreaSquareMeters = 80,
            RenovationStatus = RenovationStatus.Good,
            Price = 500_000,
            RentalPeriod = RentalPeriod.Monthly,
            Address = "Original address",
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

    private static PropertyListingRequestDto ValidUpdateRequest() => new()
    {
        Title = "Updated title",
        Description = "Updated description",
        PropertyType = PropertyType.House,
        RoomCount = 4,
        AreaSquareMeters = 95,
        RenovationStatus = RenovationStatus.EuroRenovated,
        Price = 750_000,
        RentalPeriod = RentalPeriod.Daily,
        Address = "Updated address",
        Latitude = 41.31,
        Longitude = 69.21,
        ContactPhoneNumber = "+998911111111"
    };

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesEditableFields()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);
        PropertyListingRequestDto request = ValidUpdateRequest();

        PropertyListingResponseDto result = await service.UpdateAsync(listing.Id, request, ownerId);

        Assert.Equal(1, repository.UpdateAsyncCallCount);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.RoomCount, result.RoomCount);
        Assert.Equal(request.AreaSquareMeters, result.AreaSquareMeters);
        Assert.Equal(request.RenovationStatus, result.RenovationStatus);
        Assert.Equal(request.Price, result.Price);
        Assert.Equal(request.RentalPeriod, result.RentalPeriod);
        Assert.Equal(request.Address, result.Address);
        Assert.Equal(request.Latitude, result.Latitude);
        Assert.Equal(request.Longitude, result.Longitude);
        Assert.Equal(request.ContactPhoneNumber, result.ContactPhoneNumber);
    }

    [Fact]
    public async Task UpdateAsync_ListingNotFound_ThrowsInvalidOperation()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(Guid.NewGuid(), ValidUpdateRequest(), ownerId));

        Assert.Equal(0, repository.UpdateAsyncCallCount);
    }

    [Fact]
    public async Task UpdateAsync_NonOwner_ThrowsInvalidOperation_AndDoesNotUpdate()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(listing.Id, ValidUpdateRequest(), attackerId));

        Assert.Equal(0, repository.UpdateAsyncCallCount);
        Assert.Equal("Original title", listing.Title);
    }

    [Fact]
    public async Task UpdateAsync_Owner_ProtectedFieldsRemainUnchanged()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);
        Guid originalId = listing.Id;
        Guid originalOwnerId = listing.OwnerId;
        DateTime originalCreatedAt = listing.CreatedAt;
        PropertyListingStatus originalStatus = listing.Status;

        PropertyListingResponseDto result = await service.UpdateAsync(listing.Id, ValidUpdateRequest(), ownerId);

        Assert.Equal(originalId, result.Id);
        Assert.Equal(originalOwnerId, listing.OwnerId);
        Assert.Equal(originalCreatedAt, listing.CreatedAt);
        Assert.Equal(originalStatus, listing.Status);
        Assert.Equal(originalStatus, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_Owner_ImagesRemainUnchanged()
    {
        (PropertyListingService service, FakePropertyListingRepository repository) = CreateService();
        Guid ownerId = Guid.NewGuid();
        PropertyListing listing = SeedListing(repository, ownerId);
        PropertyImage originalImage = listing.Images.Single();

        PropertyListingResponseDto result = await service.UpdateAsync(listing.Id, ValidUpdateRequest(), ownerId);

        PropertyImage stillPresent = Assert.Single(listing.Images);
        Assert.Same(originalImage, stillPresent);
        Assert.Equal(originalImage.ImageUrl, stillPresent.ImageUrl);
        Assert.Equal(originalImage.SortOrder, stillPresent.SortOrder);
        Assert.Equal(originalImage.ImageUrl, result.CoverImageUrl);
    }
}
