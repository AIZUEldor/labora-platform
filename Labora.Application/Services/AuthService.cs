using Labora.Application.DTOs.Auth;
using Labora.Application.DTOs.Otp;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Labora.Domain.Exceptions;
using Labora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Labora.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        IOtpService otpService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        bool phoneExists = await _userRepository.PhoneNumberExistsAsync(request.PhoneNumber);
        if (phoneExists)
        {
            throw new InvalidOperationException("Bu telefon raqam allaqachon ro'yxatdan o'tgan.");
        }

        User newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Age = request.Age,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await _userRepository.AddAsync(newUser);

        string token = GenerateJwtToken(newUser);

        return new AuthResponseDto
        {
            UserId = newUser.Id,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            PhoneNumber = newUser.PhoneNumber,
            Role = newUser.Role,
            Token = token,
            TokenExpiration = DateTime.UtcNow.AddDays(7)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        User? user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (user is null)
        {
            throw new InvalidOperationException("Telefon raqam yoki parol noto'g'ri.");
        }

        bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Telefon raqam yoki parol noto'g'ri.");
        }

        string token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            Token = token,
            TokenExpiration = DateTime.UtcNow.AddDays(7)
        };
    }

    /// <summary>
    /// Registered/unregistered phone numbers deliberately take symmetric paths here: both return a
    /// StartOtpResponseDto of the same shape, the only difference being whether a real OtpVerification
    /// row (and a real SMS) exists behind it. Do not add a branch that returns a different response
    /// shape, status code, or exception type based on user existence - that reintroduces the phone
    /// enumeration this decoy path exists to prevent.
    ///
    /// The real StartOtpAsync call can also throw for reasons tied to durable per-phone state that
    /// only exists for a phone with prior OTP activity (an active flow already in progress, an abuse
    /// block, or a rate limit) - state a never-persisted decoy can never trigger. Left unhandled, that
    /// asymmetry (success for unregistered numbers vs. an error for registered-with-history numbers)
    /// is itself an enumeration channel, so these three specific, state-dependent exceptions are
    /// caught and folded into the same decoy response. Genuine infrastructure failures (SmsSendException,
    /// or anything else) are deliberately NOT caught here - they must keep propagating and failing
    /// closed rather than being masked as a fake success.
    /// </summary>
    public async Task<StartOtpResponseDto> ForgotPasswordStartAsync(ForgotPasswordStartRequestDto request)
    {
        User? user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);

        if (user is null)
        {
            return await _otpService.PrepareDecoyStartAsync(request.PhoneNumber, OtpPurpose.ForgotPassword);
        }

        try
        {
            return await _otpService.StartOtpAsync(request.PhoneNumber, OtpPurpose.ForgotPassword, user.Id);
        }
        catch (Exception ex) when (ex is OtpConflictException or OtpBlockedException or OtpSendRateLimitedException)
        {
            return await _otpService.PrepareDecoyStartAsync(request.PhoneNumber, OtpPurpose.ForgotPassword);
        }
    }

    public async Task<ResendOtpResponseDto> ForgotPasswordResendAsync(ForgotPasswordResendRequestDto request)
    {
        return await _otpService.ResendOtpAsync(request.VerificationId);
    }

    public async Task<VerifyOtpResponseDto> ForgotPasswordVerifyAsync(ForgotPasswordVerifyRequestDto request)
    {
        return await _otpService.VerifyOtpAsync(request.VerificationId, request.Code);
    }

    /// <summary>
    /// The password is updated only after ConsumeOtpAsync returns successfully (it throws for any
    /// invalid/expired/wrong-purpose/already-consumed token, so a failed consume never reaches the
    /// UserRepository call below), and only for the user identified by the consumed verification's own
    /// UserId - request never carries a phone number, so there is no client-supplied identifier this
    /// method could accidentally trust instead.
    /// </summary>
    public async Task<ForgotPasswordCompleteResponseDto> ForgotPasswordCompleteAsync(ForgotPasswordCompleteRequestDto request)
    {
        OtpConsumeResultDto consumeResult = await _otpService.ConsumeOtpAsync(
            request.VerificationId,
            OtpPurpose.ForgotPassword,
            request.OperationToken);

        if (consumeResult.UserId is null)
        {
            throw new InvalidOperationException("Ushbu OTP tasdiqlash bilan hech qanday foydalanuvchi bog'lanmagan.");
        }

        User? user = await _userRepository.GetByIdAsync(consumeResult.UserId.Value);
        if (user is null)
        {
            throw new InvalidOperationException("Foydalanuvchi topilmadi.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return new ForgotPasswordCompleteResponseDto { Success = true };
    }

    private string GenerateJwtToken(User user)
    {
        string jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
