using Labora.Application.DTOs.WorkerPosts;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class WorkerPostService : IWorkerPostService
{
    private const int MaxPortfolioImages = 5;
    private const string PortfolioImagesSubFolder = "portfolio";

    private readonly IWorkerPostRepository _workerPostRepository;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;

    public WorkerPostService(
        IWorkerPostRepository workerPostRepository,
        INotificationService notificationService,
        IFileStorageService fileStorageService)
    {
        _workerPostRepository = workerPostRepository;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
    }

    public async Task<WorkerPostResponseDto?> GetByIdAsync(Guid id)
    {
        return await _workerPostRepository.GetByIdAsync(id);
    }

    public async Task<List<WorkerPostResponseDto>> GetAllAsync(Guid? categoryId, Guid? subCategoryId, string? city)
    {
        return await _workerPostRepository.GetAllAsync(categoryId, subCategoryId, city);
    }

    public async Task<List<WorkerPostResponseDto>> GetMyPostsAsync(Guid workerId)
    {
        return await _workerPostRepository.GetByWorkerIdAsync(workerId);
    }

    public async Task<WorkerPostResponseDto> CreateAsync(Guid workerId, CreateWorkerPostRequestDto dto)
    {
        WorkerPost workerPost = new WorkerPost
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            ExpectedSalary = dto.ExpectedSalary,
            ExperienceYears = dto.ExperienceYears,
            Skills = dto.Skills,
            City = dto.City,
            Country = dto.Country,
            Status = WorkerPostStatus.Active,
            WorkerId = workerId,
            CategoryId = dto.CategoryId,
            SubCategoryId = dto.SubCategoryId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        WorkerPost created = await _workerPostRepository.CreateAsync(workerPost);

        WorkerPostResponseDto? result = await _workerPostRepository.GetByIdAsync(created.Id);
        return result!;
    }

    public async Task<WorkerPostResponseDto> UpdateAsync(Guid workerId, Guid postId, UpdateWorkerPostRequestDto dto)
    {
        WorkerPostResponseDto? existing = await _workerPostRepository.GetByIdAsync(postId);
        if (existing == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");

        if (existing.WorkerId != workerId)
            throw new UnauthorizedAccessException("Bu e'lon sizga tegishli emas.");

        WorkerPost workerPost = new WorkerPost
        {
            Id = postId,
            Title = dto.Title,
            Description = dto.Description,
            ExpectedSalary = dto.ExpectedSalary,
            ExperienceYears = dto.ExperienceYears,
            Skills = dto.Skills,
            City = dto.City,
            Country = dto.Country,
            Status = (WorkerPostStatus)dto.Status,
            WorkerId = workerId,
            CategoryId = dto.CategoryId,
            SubCategoryId = dto.SubCategoryId,
        };

        await _workerPostRepository.UpdateAsync(workerPost);

        WorkerPostResponseDto? result = await _workerPostRepository.GetByIdAsync(postId);
        return result!;
    }

    public async Task DeleteAsync(Guid workerId, Guid postId)
    {
        WorkerPostResponseDto? existing = await _workerPostRepository.GetByIdAsync(postId);
        if (existing == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");

        if (existing.WorkerId != workerId)
            throw new UnauthorizedAccessException("Bu e'lon sizga tegishli emas.");

        await _workerPostRepository.DeleteAsync(postId);
    }

    public async Task<WorkerPortfolioImage> AddPortfolioImageAsync(Guid postId, Guid workerId, IFormFile file, string? caption)
    {
        WorkerPostResponseDto? post = await _workerPostRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");
        if (post.WorkerId != workerId)
            throw new UnauthorizedAccessException("Bu e'lon sizga tegishli emas.");
        if (post.PortfolioImages.Count >= MaxPortfolioImages)
            throw new ArgumentException($"Maksimal {MaxPortfolioImages} ta rasm yuklash mumkin.");

        ImageUploadValidator.Validate(file);

        string imageUrl = await _fileStorageService.SaveAsync(file, PortfolioImagesSubFolder);

        try
        {
            return await _workerPostRepository.AddPortfolioImageAsync(postId, imageUrl, caption);
        }
        catch
        {
            // The physical file was already written before this DB write was attempted. If the DB
            // write fails, the file would otherwise be orphaned with no matching row - remove it so
            // a failed upload never leaves storage inconsistent with the database.
            _fileStorageService.Delete(imageUrl);
            throw;
        }
    }

    public async Task DeletePortfolioImageAsync(Guid postId, Guid workerId, Guid imageId)
    {
        WorkerPostResponseDto? post = await _workerPostRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");
        if (post.WorkerId != workerId)
            throw new UnauthorizedAccessException("Bu e'lon sizga tegishli emas.");

        WorkerPostResponseDto.PortfolioImageDto? image = post.PortfolioImages.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new KeyNotFoundException("Rasm ushbu e'longa tegishli emas.");

        await _workerPostRepository.DeletePortfolioImageAsync(imageId);

        // Physical-file removal only happens after the DB row is confirmed gone, and must never turn
        // this already-successful delete into a failed request - a missing/inaccessible file at this
        // point is not an error (IFileStorageService.Delete guarantees it never throws for that).
        _fileStorageService.Delete(image.ImageUrl);
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await _workerPostRepository.IncrementViewCountAsync(id);
    }

    public async Task ContactWorkerAsync(Guid postId, Guid employerId)
    {
        WorkerPostResponseDto? post = await _workerPostRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");

        await _notificationService.CreateAsync(
            post.WorkerId,
            "Yangi taklif",
            $"Ish beruvchi siz bilan bog'lanmoqchi.",
            Labora.Domain.Enums.NotificationType.NewJobRecommended,
            employerId
        );
    }
}