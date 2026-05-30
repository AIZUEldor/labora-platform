using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IUserPreferenceRepository : IGenericRepository<UserPreference>
{
    Task<IEnumerable<UserPreference>> GetByUserIdAsync(Guid userId);
    Task DeleteByUserIdAsync(Guid userId);
}