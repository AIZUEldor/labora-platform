using Labora.Application.DTOs.Admin;

namespace Labora.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto> BlockUserAsync(Guid userId, bool isBlocked);
    Task<IEnumerable<AdminJobDto>> GetAllJobsAsync();
    Task DeleteJobAsync(Guid jobId);
    Task<AdminStatsDto> GetStatsAsync();
}