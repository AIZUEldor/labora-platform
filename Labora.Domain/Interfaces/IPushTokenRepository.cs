using Labora.Domain.Entities;

namespace Labora.Domain.Interfaces;

public interface IPushTokenRepository : IGenericRepository<PushToken>
{
    Task<IEnumerable<PushToken>> GetByUserIdAsync(Guid userId);
    Task<PushToken?> GetByTokenAsync(string token);
    Task DeleteByUserIdAsync(Guid userId);
}