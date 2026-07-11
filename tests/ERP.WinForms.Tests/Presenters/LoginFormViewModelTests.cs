using ERP.Application.Features.Authentication;
using ERP.WinForms.Presenters;
using ERP.WinForms.Services;

namespace ERP.WinForms.Tests.Presenters;

public class LoginFormViewModelTests
{
    private const string TestUsername = "admin";
    private const string TestPassword = "Admin123!";
    private const string TestToken = "test-jwt-token";
    private const string TestCorrelationId = "corr-login-123";

    [Fact]
    public async Task Submit_valid_publishes_NavigateToDashboard()
    {
        var view = new FakeLoginFormView(TestUsername, TestPassword);
        var authClient = new FakeApiAuthClient();
        var sessionContext = new FakeSessionContext();
        var correlation = new FakeCorrelationContext(TestCorrelationId);
        var presenter = new LoginFormPresenter(view, authClient, sessionContext, correlation);
        var navigated = false;
        presenter.NavigateToDashboard += (_, _) => navigated = true;

        await presenter.SubmitAsync();

        Assert.True(navigated);
        Assert.True(sessionContext.IsAuthenticated);
        Assert.Equal(TestToken, sessionContext.Token);
        Assert.Equal(TestUsername, sessionContext.User?.Nombre);
        Assert.Equal("Administrador Sistema", sessionContext.User?.Rol);
        Assert.Equal(TestCorrelationId, authClient.LastCorrelationId);
    }

    [Fact]
    public async Task Submit_invalid_401_shows_generic_error()
    {
        var view = new FakeLoginFormView(TestUsername, "wrong");
        var authClient = new FakeApiAuthClient { FailWith401 = true };
        var sessionContext = new FakeSessionContext();
        var correlation = new FakeCorrelationContext(TestCorrelationId);
        var presenter = new LoginFormPresenter(view, authClient, sessionContext, correlation);
        var navigated = false;
        presenter.NavigateToDashboard += (_, _) => navigated = true;

        await presenter.SubmitAsync();

        Assert.False(navigated);
        Assert.False(sessionContext.IsAuthenticated);
        Assert.Equal("Invalid credentials.", view.LastError);
    }

    [Fact]
    public async Task Submit_empty_username_shows_validation_error()
    {
        var view = new FakeLoginFormView("", TestPassword);
        var authClient = new FakeApiAuthClient();
        var sessionContext = new FakeSessionContext();
        var correlation = new FakeCorrelationContext(TestCorrelationId);
        var presenter = new LoginFormPresenter(view, authClient, sessionContext, correlation);

        await presenter.SubmitAsync();

        Assert.False(sessionContext.IsAuthenticated);
        Assert.Empty(authClient.LoginCalls);
        Assert.NotNull(view.LastError);
        Assert.Contains("required", view.LastError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Submit_locked_423_shows_lockout_message()
    {
        var lockout = DateTime.UtcNow.AddMinutes(15);
        var view = new FakeLoginFormView(TestUsername, TestPassword);
        var authClient = new FakeApiAuthClient { FailWith423 = true, LockoutUntil = lockout };
        var sessionContext = new FakeSessionContext();
        var correlation = new FakeCorrelationContext(TestCorrelationId);
        var presenter = new LoginFormPresenter(view, authClient, sessionContext, correlation);

        await presenter.SubmitAsync();

        Assert.False(sessionContext.IsAuthenticated);
        Assert.NotNull(view.LastError);
        Assert.Contains("locked", view.LastError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(lockout.ToString("O"), view.LastError);
    }

    [Fact]
    public async Task Submit_unexpected_exception_shows_safe_error_and_remains_on_login()
    {
        var view = new FakeLoginFormView(TestUsername, TestPassword);
        var authClient = new FakeApiAuthClient { ThrowOnLogin = true };
        var sessionContext = new FakeSessionContext();
        var correlation = new FakeCorrelationContext(TestCorrelationId);
        var presenter = new LoginFormPresenter(view, authClient, sessionContext, correlation);
        var navigated = false;
        presenter.NavigateToDashboard += (_, _) => navigated = true;

        await presenter.SubmitAsync();

        Assert.False(navigated);
        Assert.False(sessionContext.IsAuthenticated);
        Assert.NotNull(view.LastError);
        Assert.Contains("unexpected error", view.LastError, StringComparison.OrdinalIgnoreCase);
        Assert.False(view.Busy);
    }

    private sealed class FakeLoginFormView : ILoginFormView
    {
        public FakeLoginFormView(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
        public string? LastError { get; private set; }
        public bool Busy { get; private set; }

        public void ShowError(string message)
        {
            LastError = message;
        }

        public void ClearError()
        {
            LastError = null;
        }

        public void SetBusy(bool busy)
        {
            Busy = busy;
        }
    }

    private sealed class FakeApiAuthClient : IApiAuthClient
    {
        public bool FailWith401 { get; set; }
        public bool FailWith423 { get; set; }
        public bool ThrowOnLogin { get; set; }
        public DateTime? LockoutUntil { get; set; }
        public string? LastCorrelationId { get; private set; }
        public List<(string username, string password, string correlationId)> LoginCalls { get; } = new();

        public Task<LoginResult> LoginAsync(
            string username,
            string password,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            LastCorrelationId = correlationId;
            LoginCalls.Add((username, password, correlationId));

            if (ThrowOnLogin)
            {
                throw new InvalidOperationException("Simulated unexpected failure");
            }

            if (FailWith401)
            {
                return Task.FromResult(LoginResult.Failed("Invalid credentials."));
            }

            if (FailWith423)
            {
                return Task.FromResult(LoginResult.Locked(LockoutUntil!.Value));
            }

            var user = new UserSession(1, username, "Administrador Sistema");
            return Task.FromResult(LoginResult.Success(
                TestToken,
                user,
                new[] { "Authentication", "Payroll" },
                correlationId));
        }

        public Task<LogoutResult> LogoutAsync(
            string token,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LogoutResult(true, correlationId));
        }
    }

    private sealed class FakeSessionContext : ISessionContext
    {
        public string? Token { get; private set; }
        public UserSession? User { get; private set; }
        public IReadOnlyList<string> Modules { get; private set; } = Array.Empty<string>();
        public bool IsAuthenticated { get; private set; }
        public string? LastCorrelationId { get; private set; }
        public int LogoutCallCount { get; private set; }

        public void SetSession(
            string token,
            UserSession user,
            IReadOnlyList<string> modules,
            string correlationId)
        {
            Token = token;
            User = user;
            Modules = modules;
            IsAuthenticated = true;
            LastCorrelationId = correlationId;
        }

        public Task<LogoutCompletionResult> LogoutAsync(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            LogoutCallCount++;
            Clear();
            return Task.FromResult(new LogoutCompletionResult(true));
        }

        public void Clear()
        {
            Token = null;
            User = null;
            Modules = Array.Empty<string>();
            IsAuthenticated = false;
            LastCorrelationId = null;
        }
    }

    private sealed class FakeCorrelationContext : ICorrelationContext
    {
        private readonly string _correlationId;

        public FakeCorrelationContext(string correlationId)
        {
            _correlationId = correlationId;
        }

        public string NewCorrelationId() => _correlationId;
    }
}
