using Labora.Application.DTOs.Users;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponseDto> GetProfileAsync(Guid userId);
    Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<string> UploadCvAsync(Guid userId, IFormFile file);
    Task<string> UploadAvatarAsync(Guid userId, IFormFile file);
}