using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    public Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Login business logic is not implemented. " +
            "This is a CQRS skeleton stub for Sprint 0 bootstrap. " +
            "Sprint 1 AUTH-T02 will implement credential validation, lockout, JWT issue, and audit.");
    }
}
