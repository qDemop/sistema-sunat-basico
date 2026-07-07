using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed record GetCurrentUserQuery(
    long UserId,
    string Nombre,
    string Rol,
    string? CorrelationId = null) : IRequest<CurrentUserResponse>;
