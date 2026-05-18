using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly LaboaDbContext _context;

    public CategoryRepository(LaboaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetMainCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesByJobTypeAsync(JobType jobType)
    {
        return await _context.Categories
            .Where(c => c.JobType == jobType && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}