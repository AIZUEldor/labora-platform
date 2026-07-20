using Labora.Application.Common.Validation;
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
using System.Text.Json;

namespace Labora.Application.Services;

public class AuthService : IAuthService
{
    private const int CurrentRegistrationPayloadVersion = 1;

    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _phoneNumberNormalizer = phoneNumberNormalizer;
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
        User user = await VerifyLoginCredentialsAsync(request.PhoneNumber, request.Password);

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
    /// Shared by LoginAsync and LoginStartAsync - unknown phone, wrong password, and a blocked user all
    /// throw the exact same exception/message, so none of the three can be distinguished from any other
    /// by a caller. A blocked user's correct password is deliberately treated identically to a wrong
    /// password so blocked-account state is never revealed.
    /// </summary>
    private async Task<User> VerifyLoginCredentialsAsync(string phoneNumber, string password)
    {
        User? user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        if (user is null)
        {
            throw new InvalidOperationException("Telefon raqam yoki parol noto'g'ri.");
        }

        bool isPasswordValid = _passwordHasher.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Telefon raqam yoki parol noto'g'ri.");
        }

        if (user.IsBlocked)
        {
            throw new InvalidOperationException("Telefon raqam yoki parol noto'g'ri.");
        }

        return user;
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

    /// <summary>
    /// Pre-checks phone uniqueness against the normalized phone before ever hashing a password or
    /// touching IOtpService, so no SMS is sent for a number that is already registered. The password is
    /// hashed here and only the hash - never the plaintext - is placed into RegistrationPayloadDto,
    /// which is then serialized and handed to StartOtpAsync as the OtpPurpose.Registration payload;
    /// nothing about the original RegisterStartRequestDto (including its plaintext Password) is ever
    /// persisted.
    /// </summary>
    public async Task<StartOtpResponseDto> RegisterStartAsync(RegisterStartRequestDto request)
    {
        string normalizedPhone = _phoneNumberNormalizer.Normalize(request.PhoneNumber);

        bool phoneExists = await _userRepository.PhoneNumberExistsAsync(normalizedPhone);
        if (phoneExists)
        {
            throw new InvalidOperationException("Bu telefon raqam allaqachon ro'yxatdan o'tgan.");
        }

        RegistrationPayloadDto payload = new RegistrationPayloadDto
        {
            Version = CurrentRegistrationPayloadVersion,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Age = request.Age,
            PhoneNumber = normalizedPhone,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        string serializedPayload = JsonSerializer.Serialize(payload);

        return await _otpService.StartOtpAsync(
            normalizedPhone,
            OtpPurpose.Registration,
            userId: null,
            registrationPayload: serializedPayload);
    }

    public async Task<ResendOtpResponseDto> RegisterResendAsync(RegisterResendRequestDto request)
    {
        return await _otpService.ResendOtpAsync(request.VerificationId);
    }

    public async Task<VerifyOtpResponseDto> RegisterVerifyAsync(RegisterVerifyRequestDto request)
    {
        return await _otpService.VerifyOtpAsync(request.VerificationId, request.Code);
    }

    /// <summary>
    /// The User is created only after ConsumeOtpAsync returns successfully (it throws for any
    /// invalid/expired/wrong-purpose/already-consumed token) and only from the RegistrationPayloadDto
    /// deserialized out of the consumed verification's own RegistrationPayload - request never carries
    /// registration fields, so there is nothing client-supplied this method could accidentally trust
    /// instead. Every failure mode here (missing payload, malformed payload, payload/consume-result
    /// phone mismatch) fails closed with a generic exception rather than falling back to any
    /// client-supplied data.
    /// </summary>
    public async Task<AuthResponseDto> RegisterCompleteAsync(RegisterCompleteRequestDto request)
    {
        OtpConsumeResultDto consumeResult = await _otpService.ConsumeOtpAsync(
            request.VerificationId,
            OtpPurpose.Registration,
            request.OperationToken);

        if (string.IsNullOrWhiteSpace(consumeResult.RegistrationPayload))
        {
            throw new InvalidOperationException("Ro'yxatdan o'tish ma'lumotlari topilmadi.");
        }

        RegistrationPayloadDto? payload;
        try
        {
            payload = JsonSerializer.Deserialize<RegistrationPayloadDto>(consumeResult.RegistrationPayload);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Ro'yxatdan o'tish ma'lumotlari yaroqsiz.");
        }

        if (payload is null || !IsRegistrationPayloadValid(payload))
        {
            throw new InvalidOperationException("Ro'yxatdan o'tish ma'lumotlari yaroqsiz.");
        }

        string normalizedPayloadPhone = _phoneNumberNormalizer.Normalize(payload.PhoneNumber);
        string normalizedConsumedPhone = _phoneNumberNormalizer.Normalize(consumeResult.PhoneNumber);

        if (!string.Equals(normalizedPayloadPhone, normalizedConsumedPhone, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Ro'yxatdan o'tish ma'lumotlari mos kelmadi.");
        }

        bool phoneExists = await _userRepository.PhoneNumberExistsAsync(normalizedConsumedPhone);
        if (phoneExists)
        {
            throw new InvalidOperationException("Bu telefon raqam allaqachon ro'yxatdan o'tgan.");
        }

        User newUser = new User
        {
            FirstName = payload.FirstName,
            LastName = payload.LastName,
            Age = payload.Age,
            PhoneNumber = normalizedConsumedPhone,
            PasswordHash = payload.PasswordHash,
            Role = payload.Role
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

    /// <summary>
    /// Fails closed on anything a tampered, truncated, or schema-drifted RegistrationPayload could
    /// produce via System.Text.Json's default lenient deserialization (silently-defaulted missing
    /// properties, an out-of-range Age, an unsupported Role) - every field actually written onto the
    /// created User is checked here, not just the two used for the phone-binding comparison. Version
    /// is checked first and explicitly against the single currently-supported value, so an old-shape
    /// payload (Version 0, e.g. from a JSON default) is rejected rather than silently accepted. Name
    /// length, age range, and role checks all defer to RegistrationRules - the same source of truth
    /// enforced by RegisterRequestDtoValidator/RegisterStartRequestDtoValidator - so this can never
    /// permit something the request-time validators would have rejected (including UserRole.Admin,
    /// which RegistrationRules.IsAllowedPublicRole excludes).
    /// </summary>
    private static bool IsRegistrationPayloadValid(RegistrationPayloadDto payload)
    {
        if (payload.Version != CurrentRegistrationPayloadVersion)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(payload.FirstName)
            || payload.FirstName.Length < RegistrationRules.MinNameLength
            || payload.FirstName.Length > RegistrationRules.MaxNameLength)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(payload.LastName)
            || payload.LastName.Length < RegistrationRules.MinNameLength
            || payload.LastName.Length > RegistrationRules.MaxNameLength)
        {
            return false;
        }

        if (payload.Age < RegistrationRules.MinAge || payload.Age > RegistrationRules.MaxAge)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(payload.PhoneNumber))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(payload.PasswordHash))
        {
            return false;
        }

        if (!RegistrationRules.IsAllowedPublicRole(payload.Role))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reuses VerifyLoginCredentialsAsync, which already rejects unknown phone, wrong password, and a
    /// blocked user with the same generic exception, so none of the three is distinguishable here either.
    /// Only on success is StartOtpAsync called, bound to the verified user's own Id (registrationPayload
    /// stays null - there is no new data to carry, the user already exists). No JWT is issued here.
    /// </summary>
    public async Task<StartOtpResponseDto> LoginStartAsync(LoginRequestDto request)
    {
        User user = await VerifyLoginCredentialsAsync(request.PhoneNumber, request.Password);

        return await _otpService.StartOtpAsync(request.PhoneNumber, OtpPurpose.Login, user.Id);
    }

    public async Task<ResendOtpResponseDto> LoginResendAsync(LoginResendRequestDto request)
    {
        return await _otpService.ResendOtpAsync(request.VerificationId);
    }

    public async Task<VerifyOtpResponseDto> LoginVerifyAsync(LoginVerifyRequestDto request)
    {
        return await _otpService.VerifyOtpAsync(request.VerificationId, request.Code);
    }

    /// <summary>
    /// The JWT is issued only after ConsumeOtpAsync returns successfully (it throws for any
    /// invalid/expired/wrong-purpose/already-consumed token), and only for the user identified by the
    /// consumed verification's own UserId - request never carries a phone number or password, so there
    /// is nothing client-supplied this method could accidentally trust instead. A missing or
    /// since-blocked user (e.g. blocked by an admin between Start and Complete) is rejected with the
    /// same generic error LoginAsync/LoginStartAsync already use.
    /// </summary>
    public async Task<AuthResponseDto> LoginCompleteAsync(LoginCompleteRequestDto request)
    {
        OtpConsumeResultDto consumeResult = await _otpService.ConsumeOtpAsync(
            request.VerificationId,
            OtpPurpose.Login,
            request.OperationToken);

        if (consumeResult.UserId is null)
        {
            throw new InvalidOperationException("Ushbu OTP tasdiqlash bilan hech qanday foydalanuvchi bog'lanmagan.");
        }

        User? user = await _userRepository.GetByIdAsync(consumeResult.UserId.Value);
        if (user is null || user.IsBlocked)
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
