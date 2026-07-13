using AutoMapper;
using Labora.Application.DTOs.Users;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
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

    public async Task<string> UploadCvAsync(Guid userId, IFormFile file)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cv");
        Directory.CreateDirectory(uploadsFolder);

        string fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        using FileStream stream = new(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        string url = $"/uploads/cv/{fileName}";
        user.CvUrl = url;
        await _userRepository.UpdateAsync(user);

        return url;
    }

    public async Task<string> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        string fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        using FileStream stream = new(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        string url = $"/uploads/avatars/{fileName}";
        user.ProfileImageUrl = url;
        await _userRepository.UpdateAsync(user);

        return url;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        bool isValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValid)
            throw new InvalidOperationException("Joriy parol noto'g'ri.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User? user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (user is null)
            throw new InvalidOperationException("Bu telefon raqam ro'yxatdan o'tmagan.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAccountAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}