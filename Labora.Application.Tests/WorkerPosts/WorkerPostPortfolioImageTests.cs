using Labora.Application.Services;
using Labora.Domain.Entities;

namespace Labora.Application.Tests.WorkerPosts;

public class WorkerPostPortfolioImageTests
{
    private static (WorkerPostService Service, FakeWorkerPostRepository Repository, FakeFileStorageService Storage) CreateService()
    {
        FakeWorkerPostRepository repository = new();
        FakeFileStorageService storage = new();
        WorkerPostService service = new(repository, new FakeNotificationService(), storage);
        return (service, repository, storage);
    }

    private static WorkerPost SeedPost(FakeWorkerPostRepository repository, Guid workerId, int imageCount = 0)
    {
        WorkerPost post = new()
        {
            Id = Guid.NewGuid(),
            Title = "Usta",
            Description = "Tajribali usta",
            City = "Tashkent",
            Country = "O'zbekiston",
            WorkerId = workerId
        };
        repository.SeedPost(post);

        for (int i = 0; i < imageCount; i++)
        {
            repository.SeedImage(post, new WorkerPortfolioImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = $"/uploads/portfolio/existing-{i}.jpg",
                WorkerPostId = post.Id
            });
        }

        return post;
    }

    private static FakeFormFile ValidImageFile()
        => new(new byte[1024], "photo.jpg", "image/jpeg");

    [Fact]
    public async Task AddPortfolioImageAsync_ValidRequest_SavesFileAndPersistsImage()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);

        WorkerPortfolioImage result = await service.AddPortfolioImageAsync(post.Id, workerId, ValidImageFile(), "Mening ishim");

        Assert.Equal(1, storage.SaveAsyncCallCount);
        Assert.Equal(1, repository.AddPortfolioImageAsyncCallCount);
        Assert.Equal(storage.SavedUrls[0], result.ImageUrl);
    }

    [Fact]
    public async Task AddPortfolioImageAsync_SixthImage_ThrowsAndDoesNotSaveFile()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId, imageCount: 5);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddPortfolioImageAsync(post.Id, workerId, ValidImageFile(), null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
        Assert.Equal(0, repository.AddPortfolioImageAsyncCallCount);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("text/plain")]
    public async Task AddPortfolioImageAsync_InvalidContentType_ThrowsAndDoesNotSaveFile(string contentType)
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);
        FakeFormFile file = new(new byte[1024], "file.dat", contentType);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddPortfolioImageAsync(post.Id, workerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddPortfolioImageAsync_OversizedFile_ThrowsAndDoesNotSaveFile()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);
        FakeFormFile file = new(new byte[(5 * 1024 * 1024) + 1], "photo.jpg", "image/jpeg");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddPortfolioImageAsync(post.Id, workerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddPortfolioImageAsync_EmptyFile_ThrowsAndDoesNotSaveFile()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);
        FakeFormFile file = new(Array.Empty<byte>(), "photo.jpg", "image/jpeg");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddPortfolioImageAsync(post.Id, workerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddPortfolioImageAsync_NonOwner_ThrowsUnauthorized()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, ownerId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AddPortfolioImageAsync(post.Id, attackerId, ValidImageFile(), null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddPortfolioImageAsync_DbPersistenceFails_DeletesTheJustSavedPhysicalFile()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);
        repository.FaultOnAddPortfolioImageAsync = _ => new InvalidOperationException("simulated DB failure");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddPortfolioImageAsync(post.Id, workerId, ValidImageFile(), null));

        Assert.Equal(1, storage.SaveAsyncCallCount);
        Assert.Single(storage.DeletedUrls);
        Assert.Equal(storage.SavedUrls[0], storage.DeletedUrls[0]);
    }

    [Fact]
    public async Task DeletePortfolioImageAsync_ValidRequest_DeletesDbRowThenPhysicalFile()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, workerId);
        WorkerPortfolioImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/portfolio/a.jpg", WorkerPostId = post.Id };
        repository.SeedImage(post, image);

        await service.DeletePortfolioImageAsync(post.Id, workerId, image.Id);

        Assert.Equal(1, repository.DeletePortfolioImageAsyncCallCount);
        Assert.Single(storage.DeletedUrls);
        Assert.Equal(image.ImageUrl, storage.DeletedUrls[0]);
    }

    [Fact]
    public async Task DeletePortfolioImageAsync_NonOwner_ThrowsUnauthorized_AndDoesNotDelete()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        WorkerPost post = SeedPost(repository, ownerId);
        WorkerPortfolioImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/portfolio/a.jpg", WorkerPostId = post.Id };
        repository.SeedImage(post, image);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeletePortfolioImageAsync(post.Id, attackerId, image.Id));

        Assert.Equal(0, repository.DeletePortfolioImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task DeletePortfolioImageAsync_ImageBelongsToAnotherWorkerPost_ThrowsAndDoesNotDelete()
    {
        (WorkerPostService service, FakeWorkerPostRepository repository, FakeFileStorageService storage) = CreateService();
        Guid workerId = Guid.NewGuid();
        WorkerPost postA = SeedPost(repository, workerId);
        WorkerPost postB = SeedPost(repository, workerId);
        WorkerPortfolioImage imageOnB = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/portfolio/b.jpg", WorkerPostId = postB.Id };
        repository.SeedImage(postB, imageOnB);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeletePortfolioImageAsync(postA.Id, workerId, imageOnB.Id));

        Assert.Equal(0, repository.DeletePortfolioImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }
}
