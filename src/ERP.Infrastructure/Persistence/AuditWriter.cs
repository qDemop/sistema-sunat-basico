using System.Text.Json;
using Dapper;
using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;

namespace ERP.Infrastructure.Persistence;

public sealed class AuditWriter : IAuditWriter
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditWriter(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task WriteAsync(AuditEventRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO audit.audit_log
                (id_usuario, rol_actor, modulo, accion, entidad, id_entidad, resultado, datos_json, fecha_evento, correlation_id)
            VALUES
                (@UsuarioId, @RolActor, @Modulo, @Accion, @Entidad, @EntidadId, @Resultado, CAST(@DatosJson AS jsonb), @FechaEvento, @CorrelationId);
            """;

        var parameters = new
        {
            record.UsuarioId,
            record.RolActor,
            record.Modulo,
            record.Accion,
            record.Entidad,
            record.EntidadId,
            record.Resultado,
            DatosJson = JsonSerializer.Serialize(record.Datos),
            record.FechaEvento,
            record.CorrelationId
        };

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }
}
