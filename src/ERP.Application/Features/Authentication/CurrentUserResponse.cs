namespace ERP.Application.Features.Authentication;

public sealed record CurrentUserResponse(
    UserSession User,
    IReadOnlyList<string> Modules,
    string CorrelationId);
