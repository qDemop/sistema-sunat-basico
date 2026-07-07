using System;
using System.Collections.Generic;

namespace ERP.Application.Features.Authentication;

public sealed record AuditEventRecord(
    long? UsuarioId,
    string? RolActor,
    string Modulo,
    string Accion,
    string Entidad,
    string? EntidadId,
    string Resultado,
    IReadOnlyDictionary<string, object> Datos,
    string CorrelationId)
{
    public DateTime FechaEvento { get; init; } = DateTime.UtcNow;
}
