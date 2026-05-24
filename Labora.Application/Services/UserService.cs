using Labora.Application.DTOs.Users;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        return MapToResponseDto(user);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Age = request.Age;
        user.ProfileImageUrl = request.ProfileImageUrl;
        user.Latitude = request.Latitude;
        user.Longitude = request.Longitude;
        user.City = request.City;
        user.Country = request.Country;

        User updatedUser = await _userRepository.UpdateAsync(user);
        return MapToResponseDto(updatedUser);
    }

    private static UserProfileResponseDto MapToResponseDto(User user)
    {
        return new UserProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            City = user.City,
            Country = user.Country,
            Balance = user.Balance,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }
}