using Labora.Application.DTOs.Auth;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
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

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
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
