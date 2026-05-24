using AutoMapper;
using Labora.Application.DTOs.Users;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;

namespace Labora.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        return _mapper.Map<UserProfileResponseDto>(user);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        _mapper.Map(request, user);

        User updatedUser = await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserProfileResponseDto>(updatedUser);
    }
}