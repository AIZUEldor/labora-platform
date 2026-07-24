using AutoMapper;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Labora.Application.Tests.WorkerPosts;
using Labora.Domain.Entities;

namespace Labora.Application.Tests.Jobs;

public class JobImageTests
{
    private static (JobService Service, FakeJobRepository Repository, FakeFileStorageService Storage) CreateService()
    {
        FakeJobRepository repository = new();
        FakeFileStorageService storage = new();
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        JobService service = new(repository, new FakeUserRepository(), new FakePushNotificationService(), mapper, storage);
        return (service, repository, storage);
    }

    private static Job SeedJob(FakeJobRepository repository, Guid employerId, int imageCount = 0)
    {
        Job job = new()
        {
            Id = Guid.NewGuid(),
            Title = "Santexnik kerak",
            Description = "Tajribali santexnik",
            City = "Tashkent",
            Country = "O'zbekiston",
            EmployerId = employerId
        };
        repository.SeedJob(job);

        for (int i = 0; i < imageCount; i++)
        {
            repository.SeedImage(job, new JobImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = $"/uploads/job-images/existing-{i}.jpg",
                JobId = job.Id
            });
        }

        return job;
    }

    private static FakeFormFile ValidImageFile()
        => new(new byte[1024], "photo.jpg", "image/jpeg");

    [Fact]
    public async Task AddImageAsync_ValidRequest_SavesFileAndPersistsImage()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);

        JobImage result = await service.AddImageAsync(job.Id, employerId, ValidImageFile(), "Ish joyi");

        Assert.Equal(1, storage.SaveAsyncCallCount);
        Assert.Equal(1, repository.AddImageAsyncCallCount);
        Assert.Equal(storage.SavedUrls[0], result.ImageUrl);
    }

    [Fact]
    public async Task AddImageAsync_SixthImage_ThrowsAndDoesNotSaveFile()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId, imageCount: 5);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddImageAsync(job.Id, employerId, ValidImageFile(), null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
        Assert.Equal(0, repository.AddImageAsyncCallCount);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    public async Task AddImageAsync_InvalidContentType_ThrowsAndDoesNotSaveFile(string contentType)
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        FakeFormFile file = new(new byte[1024], "file.dat", contentType);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddImageAsync(job.Id, employerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddImageAsync_EmptyFile_ThrowsAndDoesNotSaveFile()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        FakeFormFile file = new(Array.Empty<byte>(), "photo.jpg", "image/jpeg");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddImageAsync(job.Id, employerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddImageAsync_OversizedFile_ThrowsAndDoesNotSaveFile()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        FakeFormFile file = new(new byte[(5 * 1024 * 1024) + 1], "photo.jpg", "image/jpeg");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddImageAsync(job.Id, employerId, file, null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddImageAsync_NonOwner_ThrowsUnauthorized()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        Job job = SeedJob(repository, ownerId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AddImageAsync(job.Id, attackerId, ValidImageFile(), null));

        Assert.Equal(0, storage.SaveAsyncCallCount);
    }

    [Fact]
    public async Task AddImageAsync_DbPersistenceFails_DeletesTheJustSavedPhysicalFile()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        repository.FaultOnAddImageAsync = _ => new InvalidOperationException("simulated DB failure");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddImageAsync(job.Id, employerId, ValidImageFile(), null));

        Assert.Equal(1, storage.SaveAsyncCallCount);
        Assert.Single(storage.DeletedUrls);
        Assert.Equal(storage.SavedUrls[0], storage.DeletedUrls[0]);
    }

    [Fact]
    public async Task DeleteImageAsync_ValidRequest_DeletesDbRowThenPhysicalFile()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        JobImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/job-images/a.jpg", JobId = job.Id };
        repository.SeedImage(job, image);

        await service.DeleteImageAsync(job.Id, employerId, image.Id);

        Assert.Equal(1, repository.DeleteImageAsyncCallCount);
        Assert.Single(storage.DeletedUrls);
        Assert.Equal(image.ImageUrl, storage.DeletedUrls[0]);
    }

    [Fact]
    public async Task DeleteImageAsync_NonOwner_ThrowsUnauthorized_AndDoesNotDelete()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid ownerId = Guid.NewGuid();
        Guid attackerId = Guid.NewGuid();
        Job job = SeedJob(repository, ownerId);
        JobImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/job-images/a.jpg", JobId = job.Id };
        repository.SeedImage(job, image);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteImageAsync(job.Id, attackerId, image.Id));

        Assert.Equal(0, repository.DeleteImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task DeleteImageAsync_ImageBelongsToAnotherJob_ThrowsAndDoesNotDelete()
    {
        (JobService service, FakeJobRepository repository, FakeFileStorageService storage) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job jobA = SeedJob(repository, employerId);
        Job jobB = SeedJob(repository, employerId);
        JobImage imageOnB = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/job-images/b.jpg", JobId = jobB.Id };
        repository.SeedImage(jobB, imageOnB);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteImageAsync(jobA.Id, employerId, imageOnB.Id));

        Assert.Equal(0, repository.DeleteImageAsyncCallCount);
        Assert.Empty(storage.DeletedUrls);
    }
}
