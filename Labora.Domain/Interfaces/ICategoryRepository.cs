using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IEnumerable<Category>> GetMainCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId);
    Task<IEnumerable<Category>> GetCategoriesByJobTypeAsync(JobType jobType);
}