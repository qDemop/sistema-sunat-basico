namespace ERP.WinForms.Services;

public sealed record LogoutCompletionResult(
    bool RemoteRevocationSucceeded,
    string? ErrorMessage = null);
