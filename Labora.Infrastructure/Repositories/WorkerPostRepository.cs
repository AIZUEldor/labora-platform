using Labora.Application.DTOs.WorkerPosts;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class WorkerPostRepository : IWorkerPostRepository
{
    private readonly LaboaDbContext _context;

    public WorkerPostRepository(LaboaDbContext context)
    {
        _context = context;
    }

    public async Task<WorkerPostResponseDto?> GetByIdAsync(Guid id)
    {
        WorkerPost? w = await _context.WorkerPosts
            .Include(x => x.Worker)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.PortfolioImages)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        return w == null ? null : MapToDto(w);
    }

    public async Task<List<WorkerPostResponseDto>> GetAllAsync(Guid? categoryId, Guid? subCategoryId, string? city)
    {
        IQueryable<WorkerPost> query = _context.WorkerPosts
            .Include(x => x.Worker)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.PortfolioImages)
            .Where(x => !x.IsDeleted && x.Status == WorkerPostStatus.Active);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);

        if (subCategoryId.HasValue)
            query = query.Where(x => x.SubCategoryId == subCategoryId);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(x => x.City.ToLower().Contains(city.ToLower()));

        List<WorkerPost> posts = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return posts.Select(w => MapToDto(w)).ToList();
    }

    public async Task<List<WorkerPostResponseDto>> GetByWorkerIdAsync(Guid workerId)
    {
        List<WorkerPost> posts = await _context.WorkerPosts
            .Include(x => x.Worker)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.PortfolioImages)
            .Where(x => x.WorkerId == workerId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return posts.Select(w => MapToDto(w)).ToList();
    }

    public async Task<WorkerPost> CreateAsync(WorkerPost workerPost)
    {
        await _context.WorkerPosts.AddAsync(workerPost);
        await _context.SaveChangesAsync();
        return workerPost;
    }

    public async Task<WorkerPost> UpdateAsync(WorkerPost workerPost)
    {
        WorkerPost? existing = await _context.WorkerPosts.FindAsync(workerPost.Id);
        if (existing == null)
            throw new KeyNotFoundException("WorkerPost topilmadi.");

        existing.Title = workerPost.Title;
        existing.Description = workerPost.Description;
        existing.ExpectedSalary = workerPost.ExpectedSalary;
        existing.ExperienceYears = workerPost.ExperienceYears;
        existing.Skills = workerPost.Skills;
        existing.City = workerPost.City;
        existing.Country = workerPost.Country;
        existing.Status = workerPost.Status;
        existing.CategoryId = workerPost.CategoryId;
        existing.SubCategoryId = workerPost.SubCategoryId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        WorkerPost? workerPost = await _context.WorkerPosts.FindAsync(id);
        if (workerPost != null)
        {
            workerPost.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<WorkerPortfolioImage> AddPortfolioImageAsync(Guid postId, string imageUrl, string? caption)
    {
        WorkerPortfolioImage image = new WorkerPortfolioImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            Caption = caption,
            WorkerPostId = postId,
            CreatedAt = DateTime.UtcNow
        };
        await _context.WorkerPortfolioImages.AddAsync(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task DeletePortfolioImageAsync(Guid imageId)
    {
        WorkerPortfolioImage? image = await _context.WorkerPortfolioImages.FindAsync(imageId);
        if (image != null)
        {
            _context.WorkerPortfolioImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        WorkerPost? workerPost = await _context.WorkerPosts.FindAsync(id);
        if (workerPost != null)
        {
            workerPost.ViewCount++;
            await _context.SaveChangesAsync();
        }
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
            WorkerFirstName = w.Worker.FirstName,
            WorkerLastName = w.Worker.LastName,
            WorkerAvatarUrl = w.Worker.ProfileImageUrl,
            WorkerPhone = w.Worker.PhoneNumber,
            CategoryId = w.CategoryId,
            CategoryName = w.Category != null ? w.Category.Name : null,
            SubCategoryId = w.SubCategoryId,
            SubCategoryName = w.SubCategory != null ? w.SubCategory.Name : null,
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