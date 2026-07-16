using Labora.Domain.Entities;
using Labora.Domain.Enums;

namespace Labora.Domain.Interfaces;

public interface IOtpBlockRepository : IGenericRepository<OtpBlock>
{
    Task<OtpBlock?> GetActiveAsync(OtpBlockType blockType, string scopeKey, DateTime nowUtc);
    Task<OtpBlock?> GetByScopeAsync(OtpBlockType blockType, string scopeKey);
}
