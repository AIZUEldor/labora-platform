using Labora.Application.Common;
using Labora.Domain.Entities;

namespace Labora.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(User user);
}
