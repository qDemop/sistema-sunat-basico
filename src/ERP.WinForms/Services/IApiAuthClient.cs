namespace ERP.WinForms.Services;

public interface IApiAuthClient
{
    Task<LoginResult> LoginAsync(
        string username,
        string password,
        string correlationId,
        CancellationToken cancellationToken = default);

    Task<LogoutResult> LogoutAsync(
        string token,
        string correlationId,
        CancellationToken cancellationToken = default);
}
