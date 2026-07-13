using ERP.Application.Features.Authentication;

namespace ERP.WinForms.Services;

public sealed record LoginResult(
    bool IsSuccess,
    string? Token = null,
    UserSession? User = null,
    IReadOnlyList<string>? Modules = null,
    string? CorrelationId = null,
    string? ErrorMessage = null,
    bool IsLocked = false,
    DateTime? LockoutUntil = null)
{
    public static LoginResult Success(
        string token,
        UserSession user,
        IReadOnlyList<string> modules,
        string correlationId)
    {
        return new LoginResult(
            IsSuccess: true,
            Token: token,
            User: user,
            Modules: modules,
            CorrelationId: correlationId);
    }

    public static LoginResult Failed(string errorMessage)
    {
        return new LoginResult(
            IsSuccess: false,
            ErrorMessage: errorMessage);
    }

    public static LoginResult Locked(DateTime lockoutUntil)
    {
        return new LoginResult(
            IsSuccess: false,
            IsLocked: true,
            LockoutUntil: lockoutUntil,
            ErrorMessage: $"Account locked until {lockoutUntil:O}.");
    }
}
