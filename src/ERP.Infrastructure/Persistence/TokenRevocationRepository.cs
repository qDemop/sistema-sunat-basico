using Dapper;
using ERP.Application.Abstractions;

namespace ERP.Infrastructure.Persistence;

public sealed class TokenRevocationRepository : ITokenRevocationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TokenRevocationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM "identity".token_revocation
                WHERE jti = @Jti
                  AND expira_en > now()
            );
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Jti = jti }, cancellationToken: cancellationToken));
    }

    public async Task RevokeTokenAsync(
        string jti,
        long userId,
        DateTime expiraEn,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO "identity".token_revocation
                (jti, id_usuario, expira_en, motivo, correlation_id)
            VALUES
                (@Jti, @UserId, @ExpiraEn, 'Logout', @CorrelationId)
            ON CONFLICT (jti) DO NOTHING;
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Jti = jti, UserId = userId, ExpiraEn = expiraEn, CorrelationId = correlationId },
            cancellationToken: cancellationToken));
    }
}
