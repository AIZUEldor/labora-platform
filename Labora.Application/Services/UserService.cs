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

        string[] parts = user.PasswordHash.Split(':');
        if (parts.Length != 2)
            throw new InvalidOperationException("Parol formati noto'g'ri.");

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);

        byte[] actualHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(request.CurrentPassword),
            salt,
            100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        bool isValid = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        if (!isValid)
            throw new InvalidOperationException("Joriy parol noto'g'ri.");

        byte[] newSalt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] newHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(request.NewPassword),
            newSalt,
            100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        user.PasswordHash = Convert.ToBase64String(newSalt) + ":" + Convert.ToBase64String(newHash);
        await _userRepository.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        User? user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (user is null)
            throw new InvalidOperationException("Bu telefon raqam ro'yxatdan o'tmagan.");

        byte[] newSalt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        byte[] newHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(request.NewPassword),
            newSalt,
            100000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        user.PasswordHash = Convert.ToBase64String(newSalt) + ":" + Convert.ToBase64String(newHash);
        await _userRepository.UpdateAsync(user);
    }
}