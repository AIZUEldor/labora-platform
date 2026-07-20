using Labora.Domain.Enums;

namespace Labora.Application.Common.Validation;

/// <summary>
/// Single source of truth for registration field limits and the public-registration role allowlist,
/// shared by RegisterRequestDtoValidator, RegisterStartRequestDtoValidator, and
/// AuthService.IsRegistrationPayloadValid so the three enforcement points can never drift apart.
/// </summary>
public static class RegistrationRules
{
    public const int MinNameLength = 2;
    public const int MaxNameLength = 50;
    public const int MinAge = 16;
    public const int MaxAge = 100;

    /// <summary>
    /// Roles a public, unauthenticated registration request may self-assign. UserRole.Admin is
    /// deliberately excluded - Admin accounts must never be reachable through anonymous registration.
    /// </summary>
    private static readonly IReadOnlyCollection<UserRole> AllowedPublicRoles = new[]
    {
        UserRole.Worker,
        UserRole.Employer
    };

    public static bool IsAllowedPublicRole(UserRole role) => AllowedPublicRoles.Contains(role);
}
