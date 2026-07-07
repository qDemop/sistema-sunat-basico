using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed record LogoutCommand(
    string Jti,
    long UserId,
    string Rol,
    DateTime ExpiresAt,
    string? CorrelationId = null) : IRequest<LogoutResponse>;
