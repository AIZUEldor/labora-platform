using Labora.Application.DTOs.Notifications;
using Labora.Application.DTOs.WorkerPosts;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Tests.WorkerPosts;

internal sealed class FakeWorkerPostRepository : IWorkerPostRepository
{
    private readonly Dictionary<Guid, WorkerPost> _posts = new();
    private readonly Dictionary<Guid, WorkerPortfolioImage> _images = new();

    public int AddPortfolioImageAsyncCallCount { get; private set; }
    public int DeletePortfolioImageAsyncCallCount { get; private set; }
    public Func<int, Exception?>? FaultOnAddPortfolioImageAsync { get; set; }

    public void SeedPost(WorkerPost post) => _posts[post.Id] = post;

    public void SeedImage(WorkerPost post, WorkerPortfolioImage image)
    {
        _images[image.Id] = image;
        post.PortfolioImages.Add(image);
    }

    public Task<WorkerPostResponseDto?> GetByIdAsync(Guid id)
    {
        if (!_posts.TryGetValue(id, out WorkerPost? post) || post.IsDeleted)
            return Task.FromResult<WorkerPostResponseDto?>(null);

        return Task.FromResult<WorkerPostResponseDto?>(MapToDto(post));
    }

    public Task<List<WorkerPostResponseDto>> GetAllAsync(Guid? categoryId, Guid? subCategoryId, string? city)
        => Task.FromResult(_posts.Values.Where(p => !p.IsDeleted).Select(MapToDto).ToList());

    public Task<List<WorkerPostResponseDto>> GetByWorkerIdAsync(Guid workerId)
        => Task.FromResult(_posts.Values.Where(p => p.WorkerId == workerId && !p.IsDeleted).Select(MapToDto).ToList());

    public Task<WorkerPost> CreateAsync(WorkerPost workerPost)
    {
        _posts[workerPost.Id] = workerPost;
        return Task.FromResult(workerPost);
    }

    public Task<WorkerPost> UpdateAsync(WorkerPost workerPost)
    {
        _posts[workerPost.Id] = workerPost;
        return Task.FromResult(workerPost);
    }

    public Task DeleteAsync(Guid id)
    {
        if (_posts.TryGetValue(id, out WorkerPost? post))
            post.IsDeleted = true;
        return Task.CompletedTask;
    }

    public Task<WorkerPortfolioImage> AddPortfolioImageAsync(Guid postId, string imageUrl, string? caption)
    {
        AddPortfolioImageAsyncCallCount++;
        Exception? fault = FaultOnAddPortfolioImageAsync?.Invoke(AddPortfolioImageAsyncCallCount);
        if (fault != null)
            throw fault;

        WorkerPortfolioImage image = new WorkerPortfolioImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            Caption = caption,
            WorkerPostId = postId,
            CreatedAt = DateTime.UtcNow
        };
        _images[image.Id] = image;

        if (_posts.TryGetValue(postId, out WorkerPost? post))
            post.PortfolioImages.Add(image);

        return Task.FromResult(image);
    }

    public Task DeletePortfolioImageAsync(Guid imageId)
    {
        DeletePortfolioImageAsyncCallCount++;
        if (_images.Remove(imageId, out WorkerPortfolioImage? image))
        {
            if (_posts.TryGetValue(image.WorkerPostId, out WorkerPost? post))
            {
                WorkerPortfolioImage? toRemove = post.PortfolioImages.FirstOrDefault(i => i.Id == imageId);
                if (toRemove != null)
                    post.PortfolioImages.Remove(toRemove);
            }
        }
        return Task.CompletedTask;
    }

    public Task IncrementViewCountAsync(Guid id)
    {
        if (_posts.TryGetValue(id, out WorkerPost? post))
            post.ViewCount++;
        return Task.CompletedTask;
    }

    private static WorkerPostResponseDto MapToDto(WorkerPost w)
    {
        return new WorkerPostResponseDto
        {
            Id = w.Id,
            Title = w.Title,
            Description = w.Description,
            ExpectedSalary = w.ExpectedSalary,
            ExperienceYears = w.ExperienceYears,
            Skills = w.Skills,
            City = w.City,
            Country = w.Country,
            Status = (int)w.Status,
            WorkerId = w.WorkerId,
            WorkerFirstName = w.Worker?.FirstName ?? string.Empty,
            WorkerLastName = w.Worker?.LastName ?? string.Empty,
            WorkerAvatarUrl = w.Worker?.ProfileImageUrl,
            WorkerPhone = w.Worker?.PhoneNumber,
            CategoryId = w.CategoryId,
            CategoryName = w.Category?.Name,
            SubCategoryId = w.SubCategoryId,
            SubCategoryName = w.SubCategory?.Name,
            CreatedAt = w.CreatedAt,
            ViewCount = w.ViewCount,
            PortfolioImages = w.PortfolioImages.Select(p => new WorkerPostResponseDto.PortfolioImageDto
            {
                Id = p.Id,
                ImageUrl = p.ImageUrl,
                Caption = p.Caption
            }).ToList()
        };
    }
}

internal sealed class FakeFileStorageService : IFileStorageService
{
    public List<string> SavedUrls { get; } = new();
    public List<string> DeletedUrls { get; } = new();
    public int SaveAsyncCallCount { get; private set; }
    public Func<int, Exception?>? FaultOnSaveAsync { get; set; }

    public Task<string> SaveAsync(IFormFile file, string subFolder)
    {
        SaveAsyncCallCount++;
        Exception? fault = FaultOnSaveAsync?.Invoke(SaveAsyncCallCount);
        if (fault != null)
            throw fault;

        string url = $"/uploads/{subFolder}/{Guid.NewGuid()}.jpg";
        SavedUrls.Add(url);
        return Task.FromResult(url);
    }

    public void Delete(string relativeUrl)
    {
        DeletedUrls.Add(relativeUrl);
    }
}

internal sealed class FakeNotificationService : INotificationService
{
    public Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(Guid userId)
        => Task.FromResult(Enumerable.Empty<NotificationResponseDto>());

    public Task<int> GetUnreadCountAsync(Guid userId) => Task.FromResult(0);

    public Task MarkAsReadAsync(Guid notificationId) => Task.CompletedTask;

    public Task MarkAllAsReadAsync(Guid userId) => Task.CompletedTask;

    public Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? referenceId = null)
        => Task.CompletedTask;

    public Task SendJobRecommendationsAsync() => Task.CompletedTask;

    public Task SavePreferencesAsync(Guid userId, UserPreferenceRequestDto dto) => Task.CompletedTask;

    public Task<IEnumerable<UserPreferenceRequestDto>> GetPreferencesAsync(Guid userId)
        => Task.FromResult(Enumerable.Empty<UserPreferenceRequestDto>());
}

internal sealed class FakeFormFile : IFormFile
{
    private readonly byte[] _content;

    public FakeFormFile(byte[] content, string fileName, string contentType)
    {
        _content = content;
        FileName = fileName;
        ContentType = contentType;
        Name = "file";
    }

    public string ContentType { get; }
    public string ContentDisposition { get; } = string.Empty;
    public IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public long Length => _content.LongLength;
    public string Name { get; }
    public string FileName { get; }

    public void CopyTo(Stream target) => target.Write(_content, 0, _content.Length);

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        => target.WriteAsync(_content, 0, _content.Length, cancellationToken);

    public Stream OpenReadStream() => new MemoryStream(_content);
}
