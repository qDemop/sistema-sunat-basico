namespace ERP.Application.Abstractions;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(long userId, string nombre, string rol, string jti);
}
