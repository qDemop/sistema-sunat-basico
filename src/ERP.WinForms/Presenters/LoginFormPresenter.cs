using ERP.WinForms.Services;

namespace ERP.WinForms.Presenters;

public sealed class LoginFormPresenter
{
    private readonly ILoginFormView _view;
    private readonly IApiAuthClient _authClient;
    private readonly ISessionContext _sessionContext;
    private readonly ICorrelationContext _correlationContext;

    public event EventHandler? NavigateToDashboard;

    public LoginFormPresenter(
        ILoginFormView view,
        IApiAuthClient authClient,
        ISessionContext sessionContext,
        ICorrelationContext correlationContext)
    {
        _view = view;
        _authClient = authClient;
        _sessionContext = sessionContext;
        _correlationContext = correlationContext;
    }

    public async Task SubmitAsync()
    {
        _view.ClearError();

        var username = _view.Username.Trim();
        var password = _view.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _view.ShowError("Username and password are required.");
            return;
        }

        _view.SetBusy(true);
        try
        {
            var correlationId = _correlationContext.NewCorrelationId();
            var result = await _authClient.LoginAsync(username, password, correlationId);

            if (result.IsSuccess && result.Token is not null && result.User is not null)
            {
                _sessionContext.SetSession(
                    result.Token,
                    result.User,
                    result.Modules ?? Array.Empty<string>(),
                    result.CorrelationId ?? correlationId);

                NavigateToDashboard?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _view.ShowError(result.ErrorMessage ?? "Invalid credentials.");
            }
        }
        catch (Exception ex)
        {
            _view.ShowError($"An unexpected error occurred. Please try again. ({ex.Message})");
        }
        finally
        {
            _view.SetBusy(false);
        }
    }
}
