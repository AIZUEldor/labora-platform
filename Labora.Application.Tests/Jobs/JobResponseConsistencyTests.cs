using AutoMapper;
using Labora.Application.Common;
using Labora.Application.DTOs.Jobs;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Labora.Application.Tests.WorkerPosts;
using Labora.Domain.Entities;

namespace Labora.Application.Tests.Jobs;

public class JobResponseConsistencyTests
{
    private static (JobService Service, FakeJobRepository Repository) CreateService()
    {
        FakeJobRepository repository = new();
        FakeFileStorageService storage = new();
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        JobService service = new(repository, new FakeUserRepository(), new FakePushNotificationService(), mapper, storage);
        return (service, repository);
    }

    private static Job SeedJob(FakeJobRepository repository, Guid employerId)
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
        return job;
    }

    [Fact]
    public async Task GetByIdAsync_JobHasImages_ReturnsThemInResponse()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        JobImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/job-images/a.jpg", JobId = job.Id, CreatedAt = DateTime.UtcNow };
        repository.SeedImage(job, image);

        JobResponseDto result = await service.GetByIdAsync(job.Id);

        Assert.Single(result.Images);
        Assert.Equal(image.ImageUrl, result.Images[0].ImageUrl);
        Assert.Equal(image.Id, result.Images[0].Id);
        Assert.Equal(image.ImageUrl, result.CoverImageUrl);
    }

    [Fact]
    public async Task GetByIdAsync_JobHasNoImages_CoverImageUrlIsNullAndGalleryIsEmpty()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);

        JobResponseDto result = await service.GetByIdAsync(job.Id);

        Assert.Null(result.CoverImageUrl);
        Assert.Empty(result.Images);
    }

    [Fact]
    public async Task GetByIdAsync_ImagesOutOfInsertionOrder_ReturnsThemOrderedByCreatedAtThenId()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);

        DateTime baseTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid earlyTieId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid lateTieId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        JobImage newest = new() { Id = Guid.NewGuid(), ImageUrl = "/newest.jpg", JobId = job.Id, CreatedAt = baseTime.AddMinutes(10) };
        JobImage tieLate = new() { Id = lateTieId, ImageUrl = "/tie-late.jpg", JobId = job.Id, CreatedAt = baseTime };
        JobImage oldest = new() { Id = Guid.NewGuid(), ImageUrl = "/oldest.jpg", JobId = job.Id, CreatedAt = baseTime.AddMinutes(-10) };
        JobImage tieEarly = new() { Id = earlyTieId, ImageUrl = "/tie-early.jpg", JobId = job.Id, CreatedAt = baseTime };

        // Seeded deliberately out of order, including two images sharing the exact same CreatedAt
        // (tieEarly/tieLate) to prove the Id fallback breaks the tie deterministically.
        repository.SeedImage(job, newest);
        repository.SeedImage(job, tieLate);
        repository.SeedImage(job, oldest);
        repository.SeedImage(job, tieEarly);

        JobResponseDto result = await service.GetByIdAsync(job.Id);

        Assert.Equal(
            new[] { oldest.Id, tieEarly.Id, tieLate.Id, newest.Id },
            result.Images.Select(i => i.Id));
        Assert.Equal(oldest.ImageUrl, result.CoverImageUrl);
    }

    [Fact]
    public async Task GetAllAsync_JobsHaveImages_ListResponseReturnsCoverImageUrlOnly_AndMakesNoPerJobRepositoryCall()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job jobA = SeedJob(repository, employerId);
        Job jobB = SeedJob(repository, employerId);
        JobImage imageA = new() { Id = Guid.NewGuid(), ImageUrl = "/a.jpg", JobId = jobA.Id };
        JobImage imageB = new() { Id = Guid.NewGuid(), ImageUrl = "/b.jpg", JobId = jobB.Id };
        repository.SeedImage(jobA, imageA);
        repository.SeedImage(jobB, imageB);

        PagedResult<JobResponseDto> result = await service.GetAllAsync(new JobFilterDto());

        Assert.Equal(2, result.Items.Count());
        Assert.All(result.Items, item => Assert.Empty(item.Images));
        Assert.Equal(imageA.ImageUrl, result.Items.Single(i => i.Id == jobA.Id).CoverImageUrl);
        Assert.Equal(imageB.ImageUrl, result.Items.Single(i => i.Id == jobB.Id).CoverImageUrl);
        Assert.Equal(1, repository.GetFilteredJobsAsyncCallCount);
        Assert.Equal(0, repository.GetByIdWithImagesAsyncCallCount);
    }

    [Fact]
    public async Task GetAllAsync_JobWithMultipleImages_CoverImageUrlIsFirstByCreatedAtThenId()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);

        DateTime baseTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        JobImage newer = new() { Id = Guid.NewGuid(), ImageUrl = "/newer.jpg", JobId = job.Id, CreatedAt = baseTime.AddMinutes(10) };
        JobImage older = new() { Id = Guid.NewGuid(), ImageUrl = "/older.jpg", JobId = job.Id, CreatedAt = baseTime };

        // Seeded newest-first to prove the list-path projection (not just the detail Include)
        // honors the CreatedAt-then-Id ordering rule when picking the cover image.
        repository.SeedImage(job, newer);
        repository.SeedImage(job, older);

        PagedResult<JobResponseDto> result = await service.GetAllAsync(new JobFilterDto());

        Assert.Equal(older.ImageUrl, result.Items.Single().CoverImageUrl);
    }

    [Fact]
    public async Task GetAllAsync_JobWithNoImages_CoverImageUrlIsNull()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        SeedJob(repository, employerId);

        PagedResult<JobResponseDto> result = await service.GetAllAsync(new JobFilterDto());

        Assert.Null(result.Items.Single().CoverImageUrl);
    }

    [Fact]
    public async Task GetByEmployerIdAsync_JobsHaveImages_ListResponseReturnsCoverImageUrlOnly_AndMakesNoPerJobRepositoryCall()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job jobA = SeedJob(repository, employerId);
        Job jobB = SeedJob(repository, employerId);
        JobImage imageA = new() { Id = Guid.NewGuid(), ImageUrl = "/a.jpg", JobId = jobA.Id };
        JobImage imageB = new() { Id = Guid.NewGuid(), ImageUrl = "/b.jpg", JobId = jobB.Id };
        repository.SeedImage(jobA, imageA);
        repository.SeedImage(jobB, imageB);

        IEnumerable<JobResponseDto> result = await service.GetByEmployerIdAsync(employerId);

        Assert.Equal(2, result.Count());
        Assert.All(result, item => Assert.Empty(item.Images));
        Assert.Equal(imageA.ImageUrl, result.Single(i => i.Id == jobA.Id).CoverImageUrl);
        Assert.Equal(imageB.ImageUrl, result.Single(i => i.Id == jobB.Id).CoverImageUrl);
        Assert.Equal(1, repository.GetJobsByEmployerIdAsyncCallCount);
        Assert.Equal(0, repository.GetByIdWithImagesAsyncCallCount);
    }

    [Fact]
    public async Task UpdateAsync_JobHasExistingImages_ResponsePreservesThem()
    {
        (JobService service, FakeJobRepository repository) = CreateService();
        Guid employerId = Guid.NewGuid();
        Job job = SeedJob(repository, employerId);
        JobImage image = new() { Id = Guid.NewGuid(), ImageUrl = "/uploads/job-images/existing.jpg", JobId = job.Id };
        repository.SeedImage(job, image);

        JobRequestDto request = new()
        {
            Title = "Yangilangan sarlavha",
            Description = "Yangilangan tavsif",
            Salary = 5_000_000,
            City = "Tashkent",
            Country = "O'zbekiston"
        };

        JobResponseDto result = await service.UpdateAsync(job.Id, request, employerId);

        Assert.Single(result.Images);
        Assert.Equal(image.ImageUrl, result.Images[0].ImageUrl);
        Assert.Equal(image.ImageUrl, result.CoverImageUrl);
        Assert.Equal("Yangilangan sarlavha", result.Title);
    }
}
