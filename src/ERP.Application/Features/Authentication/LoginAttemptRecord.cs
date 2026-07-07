namespace ERP.Application.Features.Authentication;

public sealed record LoginAttemptRecord(
    long? UsuarioId,
    string Username,
    bool Exitoso,
    string? IpOrigen,
    string CorrelationId);
