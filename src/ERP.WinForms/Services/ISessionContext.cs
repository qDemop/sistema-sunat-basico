using ERP.Application.Features.Authentication;

namespace ERP.WinForms.Services;

public interface ISessionContext
{
    string? Token { get; }
    UserSession? User { get; }
    IReadOnlyList<string> Modules { get; }
    bool IsAuthenticated { get; }
    string? LastCorrelationId { get; }

    void SetSession(
        string token,
        UserSession user,
        IReadOnlyList<string> modules,
        string correlationId);

    Task<LogoutCompletionResult> LogoutAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    void Clear();
}
