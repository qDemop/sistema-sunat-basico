using ERP.Application.Security;
using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;

        var user = new UserSession(request.UserId, request.Nombre, request.Rol);
        var modules = RoleModuleMapping.GetVisibleModules(request.Rol);

        return Task.FromResult(new CurrentUserResponse(user, modules, correlationId));
    }
}
