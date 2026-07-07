using Dapper;
using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;

namespace ERP.Infrastructure.Persistence;

public sealed class AuthenticationRepository : IAuthenticationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuthenticationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserAuthenticationData?> FindUserByUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                u.id_usuario AS Id,
                u.username AS Username,
                u.password_hash AS PasswordHash,
                u.nombre_completo AS NombreCompleto,
                r.nombre AS Rol,
                u.activo AS Activo,
                u.intentos_fallidos AS IntentosFallidos,
                u.bloqueado_hasta AS BloqueadoHasta
            FROM "identity".usuario u
            JOIN "identity".rol r ON r.id_rol = u.id_rol
            WHERE lower(u.username) = lower(@Username)
            LIMIT 1;
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserAuthenticationData>(
            new CommandDefinition(sql, new { Username = normalizedUsername }, cancellationToken: cancellationToken));
    }

    public async Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO "identity".login_attempt
                (id_usuario, username, exitoso, ip_origen, correlation_id)
            VALUES
                (@UsuarioId, @Username, @Exitoso, CAST(@IpOrigen AS inet), @CorrelationId);
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, record, cancellationToken: cancellationToken));
    }

    public async Task<AuthStateUpdateResult> RecordFailedLoginAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "identity".usuario
            SET intentos_fallidos = intentos_fallidos + 1,
                bloqueado_hasta = CASE
                    WHEN intentos_fallidos + 1 >= 3 THEN now() + interval '15 minutes'
                    ELSE bloqueado_hasta
                END
            WHERE id_usuario = @UserId
            RETURNING intentos_fallidos AS IntentosFallidos,
                      bloqueado_hasta AS BloqueadoHasta;
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<AuthStateRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return new AuthStateUpdateResult(
            row.IntentosFallidos,
            row.BloqueadoHasta,
            LockoutTriggered: row.IntentosFallidos >= 3);
    }

    public async Task RecordSuccessfulLoginAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE "identity".usuario
            SET intentos_fallidos = 0,
                bloqueado_hasta = NULL,
                ultimo_acceso = now()
            WHERE id_usuario = @UserId;
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    private sealed record AuthStateRow(int IntentosFallidos, DateTime? BloqueadoHasta);
}
