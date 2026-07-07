using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed record LoginCommand(string Username, string Password, string? CorrelationId = null) : IRequest<LoginResponse>;
