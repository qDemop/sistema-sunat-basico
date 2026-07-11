using ERP.Application.Features.Authentication;

namespace ERP.WinForms.Services;

public sealed class SessionContext : ISessionContext
{
    private readonly IApiAuthClient _authClient;

    public string? Token { get; private set; }
    public UserSession? User { get; private set; }
    public IReadOnlyList<string> Modules { get; private set; } = Array.Empty<string>();
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && User is not null;
    public string? LastCorrelationId { get; private set; }

    public SessionContext(IApiAuthClient authClient)
    {
        _authClient = authClient;
    }

    public void SetSession(
        string token,
        UserSession user,
        IReadOnlyList<string> modules,
        string correlationId)
    {
        Token = token;
        User = user;
        Modules = modules ?? Array.Empty<string>();
        LastCorrelationId = correlationId;
    }

    public async Task<LogoutCompletionResult> LogoutAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var token = Token;
        var revocationSucceeded = false;
        string? errorMessage = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                var result = await _authClient.LogoutAsync(token, correlationId, cancellationToken);
                revocationSucceeded = result.IsSuccess;
                if (!revocationSucceeded)
                {
                    errorMessage = "Server logout was not confirmed. Local session has been cleared.";
                }
            }
            catch (Exception ex)
            {
                revocationSucceeded = false;
                errorMessage = $"Unable to contact server during logout. Local session has been cleared. ({ex.Message})";
            }
        }

        Clear();
        return new LogoutCompletionResult(revocationSucceeded, errorMessage);
    }

    public void Clear()
    {
        Token = null;
        User = null;
        Modules = Array.Empty<string>();
        LastCorrelationId = null;
    }
}
