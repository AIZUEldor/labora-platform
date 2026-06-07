using Labora.Application.DTOs.WorkerPosts;
using Labora.Domain.Entities;

namespace Labora.Application.Interfaces;

public interface IWorkerPostRepository
{
    Task<WorkerPostResponseDto?> GetByIdAsync(Guid id);
    Task<List<WorkerPostResponseDto>> GetAllAsync(Guid? categoryId, Guid? subCategoryId, string? city);
    Task<List<WorkerPostResponseDto>> GetByWorkerIdAsync(Guid workerId);
    Task<WorkerPost> CreateAsync(WorkerPost workerPost);
    Task<WorkerPost> UpdateAsync(WorkerPost workerPost);
    Task DeleteAsync(Guid id);
    Task<WorkerPortfolioImage> AddPortfolioImageAsync(Guid postId, string imageUrl, string? caption);
    Task DeletePortfolioImageAsync(Guid imageId);
    Task IncrementViewCountAsync(Guid id);
}