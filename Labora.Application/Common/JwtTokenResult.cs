namespace Labora.Application.Common;

/// <summary>
/// Token and expiration derived from a single DateTime.UtcNow call, so a caller building a response DTO
/// from both fields can never observe a sub-millisecond skew between the JWT's own exp claim and the
/// expiration timestamp reported alongside it (the failure mode of computing DateTime.UtcNow.AddDays(7)
/// twice independently).
/// </summary>
public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
