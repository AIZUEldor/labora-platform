using Labora.Application.DTOs.WorkerPosts;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class WorkerPostService : IWorkerPostService
{
    private readonly IWorkerPostRepository _workerPostRepository;
    private readonly INotificationService _notificationService;

    public WorkerPostService(
        IWorkerPostRepository workerPostRepository,
        INotificationService notificationService)
    {
        _workerPostRepository = workerPostRepository;
        _notificationService = notificationService;
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

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "portfolio");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string imageUrl = $"/uploads/portfolio/{fileName}";
        return await _workerPostRepository.AddPortfolioImageAsync(postId, imageUrl, caption);
    }

    public async Task DeletePortfolioImageAsync(Guid postId, Guid workerId, Guid imageId)
    {
        WorkerPostResponseDto? post = await _workerPostRepository.GetByIdAsync(postId);
        if (post == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");
        if (post.WorkerId != workerId)
            throw new UnauthorizedAccessException("Bu e'lon sizga tegishli emas.");

        await _workerPostRepository.DeletePortfolioImageAsync(imageId);
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