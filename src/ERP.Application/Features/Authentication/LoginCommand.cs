using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
