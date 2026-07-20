using Labora.Application.Common;
using Labora.Application.Interfaces;
using Labora.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Labora.Application.Services;

/// <summary>
/// Placed in Application, not Infrastructure: this project's existing convention already places
/// pure crypto/configuration-driven services with no database/HTTP/file I/O in Labora.Application.Services
/// (PasswordHasher, OtpSecurityService, IdentifierHasher, UzbekistanPhoneNumberNormalizer all live here,
/// none in Infrastructure) - JWT signing is exactly this kind of service, so it follows the same placement.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult GenerateToken(User user)
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

        DateTime expiresAtUtc = DateTime.UtcNow.AddDays(7);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtTokenResult(tokenString, expiresAtUtc);
    }
}
