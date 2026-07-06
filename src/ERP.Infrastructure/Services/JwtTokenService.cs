using ERP.Application.Abstractions;

namespace ERP.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) GenerateToken(long userId, string nombre, string rol, string jti)
    {
        throw new NotImplementedException(
            "JWT token generation is not implemented. " +
            "This is a Sprint 0 bootstrap stub. " +
            "Sprint 1 AUTH-T02 will implement token signing with System.IdentityModel.Tokens.Jwt.");
    }
}
