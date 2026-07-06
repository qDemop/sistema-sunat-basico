namespace ERP.Application.Features.Authentication;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    UserSession User,
    IReadOnlyList<string> Modules,
    string CorrelationId);

public sealed record UserSession(long Id, string Nombre, string Rol);
