using Labora.Application.DTOs.WorkerPosts;
using Labora.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IWorkerPostService
{
    Task<WorkerPostResponseDto?> GetByIdAsync(Guid id);
    Task<List<WorkerPostResponseDto>> GetAllAsync(Guid? categoryId, Guid? subCategoryId, string? city);
    Task<List<WorkerPostResponseDto>> GetMyPostsAsync(Guid workerId);
    Task<WorkerPostResponseDto> CreateAsync(Guid workerId, CreateWorkerPostRequestDto dto);
    Task<WorkerPostResponseDto> UpdateAsync(Guid workerId, Guid postId, UpdateWorkerPostRequestDto dto);
    Task DeleteAsync(Guid workerId, Guid postId);
    Task<WorkerPortfolioImage> AddPortfolioImageAsync(Guid postId, Guid workerId, IFormFile file, string? caption);
    Task DeletePortfolioImageAsync(Guid postId, Guid workerId, Guid imageId);
    Task IncrementViewCountAsync(Guid id);
    Task ContactWorkerAsync(Guid postId, Guid employerId);
}