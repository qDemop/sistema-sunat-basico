using ERP.Application.Abstractions;
using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly ITokenRevocationRepository _revocationRepository;
    private readonly IAuditWriter _auditWriter;

    public LogoutCommandHandler(ITokenRevocationRepository revocationRepository, IAuditWriter auditWriter)
    {
        _revocationRepository = revocationRepository;
        _auditWriter = auditWriter;
    }

    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;

        await _revocationRepository.RevokeTokenAsync(
            request.Jti,
            request.UserId,
            request.ExpiresAt,
            correlationId,
            cancellationToken);

        await _auditWriter.WriteAsync(
            new AuditEventRecord(
                UsuarioId: request.UserId,
                RolActor: request.Rol,
                Modulo: "Authentication",
                Accion: "Logout",
                Entidad: "Token",
                EntidadId: request.Jti,
                Resultado: "Success",
                Datos: new Dictionary<string, object> { ["jti"] = request.Jti },
                CorrelationId: correlationId),
            cancellationToken);

        return new LogoutResponse(true, request.ExpiresAt, correlationId);
    }
}
