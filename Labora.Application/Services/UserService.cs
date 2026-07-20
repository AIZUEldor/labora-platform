using AutoMapper;
using Labora.Application.Common;
using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;
using Labora.Application.DTOs.Users;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Labora.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IPhoneNumberNormalizer phoneNumberNormalizer,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _phoneNumberNormalizer = phoneNumberNormalizer;
        _jwtTokenService = jwtTokenService;
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

    /// <summary>
    /// Re-verifies CurrentPassword against the authenticated user's own record (same check
    /// ChangePasswordAsync uses) before ever touching IOtpService, so a wrong password never triggers
    /// SMS. Compares both phone numbers through the same normalizer so a no-op "change" to the user's
    /// own current number (regardless of how it was originally stored) is rejected before an OTP is
    /// started. Pre-checks uniqueness, then binds the OTP to the authenticated user's own Id -
    /// registrationPayload stays null, there is nothing new to carry. No JWT is issued here.
    /// </summary>
    public async Task<StartOtpResponseDto> ChangePhoneStartAsync(Guid userId, ChangePhoneStartRequestDto request)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        bool isPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidOperationException("Joriy parol noto'g'ri.");

        string normalizedNewPhone = _phoneNumberNormalizer.Normalize(request.NewPhoneNumber);
        string normalizedCurrentPhone = _phoneNumberNormalizer.Normalize(user.PhoneNumber);

        if (string.Equals(normalizedNewPhone, normalizedCurrentPhone, StringComparison.Ordinal))
            throw new InvalidOperationException("Yangi telefon raqam joriy raqam bilan bir xil.");

        bool phoneExists = await _userRepository.PhoneNumberExistsAsync(normalizedNewPhone);
        if (phoneExists)
            throw new InvalidOperationException("Bu telefon raqam allaqachon ro'yxatdan o'tgan.");

        return await _otpService.StartOtpAsync(normalizedNewPhone, OtpPurpose.ChangePhoneNumber, userId);
    }

    public async Task<ResendOtpResponseDto> ChangePhoneResendAsync(Guid userId, ChangePhoneResendRequestDto request)
    {
        await _otpService.EnsureVerificationOwnershipAsync(request.VerificationId, OtpPurpose.ChangePhoneNumber, userId);
        return await _otpService.ResendOtpAsync(request.VerificationId);
    }

    public async Task<VerifyOtpResponseDto> ChangePhoneVerifyAsync(Guid userId, ChangePhoneVerifyRequestDto request)
    {
        await _otpService.EnsureVerificationOwnershipAsync(request.VerificationId, OtpPurpose.ChangePhoneNumber, userId);
        return await _otpService.VerifyOtpAsync(request.VerificationId, request.Code);
    }

    /// <summary>
    /// consumeResult.UserId must equal the authenticated userId - unlike Login/ForgotPassword/
    /// Registration (all anonymous flows where consumeResult.UserId is the sole identity source), this
    /// flow has both an authenticated caller and a consumeResult.UserId, so without this check an
    /// authenticated user who somehow obtained another user's VerificationId+OperationToken could change
    /// that other user's phone number. Null and mismatch are treated identically (both "invalid operation
    /// token") so neither case is distinguishable. The new phone number comes only from
    /// consumeResult.PhoneNumber - request never carries a phone number. Uniqueness is re-checked
    /// immediately before UpdateAsync, and the returned JWT reflects the updated phone number.
    /// </summary>
    public async Task<AuthResponseDto> ChangePhoneCompleteAsync(Guid userId, ChangePhoneCompleteRequestDto request)
    {
        OtpConsumeResultDto consumeResult = await _otpService.ConsumeOtpAsync(
            request.VerificationId,
            OtpPurpose.ChangePhoneNumber,
            request.OperationToken);

        if (consumeResult.UserId is null || consumeResult.UserId.Value != userId)
            throw new InvalidOperationException("Operatsiya tokeni yaroqsiz.");

        User? user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException($"Id={userId} bo'lgan foydalanuvchi topilmadi.");

        string normalizedNewPhone = _phoneNumberNormalizer.Normalize(consumeResult.PhoneNumber);

        bool phoneExists = await _userRepository.PhoneNumberExistsAsync(normalizedNewPhone);
        if (phoneExists)
            throw new InvalidOperationException("Bu telefon raqam allaqachon ro'yxatdan o'tgan.");

        user.PhoneNumber = normalizedNewPhone;
        User updatedUser = await _userRepository.UpdateAsync(user);

        JwtTokenResult jwtResult = _jwtTokenService.GenerateToken(updatedUser);

        return new AuthResponseDto
        {
            UserId = updatedUser.Id,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            PhoneNumber = updatedUser.PhoneNumber,
            Role = updatedUser.Role,
            Token = jwtResult.Token,
            TokenExpiration = jwtResult.ExpiresAtUtc
        };
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