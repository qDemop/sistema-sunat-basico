using ERP.Application.Features.Authentication;
using ERP.WinForms.Forms;
using ERP.WinForms.Services;
using ERP.WinForms.Theming;

namespace ERP.WinForms.Tests;

public class AppShellTests
{
    [Fact]
    public void Constructor_creates_login_form()
    {
        var factory = new FakeFormFactory();
        using var shell = new AppShell(factory);

        Assert.Single(factory.CreatedLoginForms);
        Assert.Empty(factory.CreatedMainForms);
    }

    [Fact]
    public void Login_success_disposes_login_form_and_creates_main_form()
    {
        var factory = new FakeFormFactory();
        using var shell = new AppShell(factory);
        var loginForm = factory.CreatedLoginForms[0];

        loginForm.RaiseLoginSucceeded();

        Assert.Single(factory.CreatedMainForms);
        Assert.True(loginForm.IsDisposed);
        Assert.False(factory.CreatedMainForms[0].IsDisposed);
    }

    [Fact]
    public void Logout_disposes_main_form_and_creates_fresh_login_form()
    {
        var factory = new FakeFormFactory();
        using var shell = new AppShell(factory);
        var firstLogin = factory.CreatedLoginForms[0];
        firstLogin.RaiseLoginSucceeded();
        var mainForm = factory.CreatedMainForms[0];

        mainForm.RaiseLogoutRequested();
        var secondLogin = factory.CreatedLoginForms[1];

        Assert.True(mainForm.IsDisposed);
        Assert.True(firstLogin.IsDisposed);
        Assert.False(secondLogin.IsDisposed);
        Assert.Equal(2, factory.CreatedLoginForms.Count);
        Assert.Single(factory.CreatedMainForms);
    }

    private sealed class FakeFormFactory : IShellFormFactory
    {
        public List<FakeLoginForm> CreatedLoginForms { get; } = new();
        public List<FakeMainForm> CreatedMainForms { get; } = new();

        public LoginForm CreateLoginForm()
        {
            var form = new FakeLoginForm();
            CreatedLoginForms.Add(form);
            return form;
        }

        public MainForm CreateMainForm()
        {
            var form = new FakeMainForm();
            CreatedMainForms.Add(form);
            return form;
        }
    }

    private sealed class FakeLoginForm : LoginForm
    {
        public FakeLoginForm()
            : base(
                new StubAuthClient(),
                new StubSessionContext(),
                new StubCorrelationContext(),
                new ThemeManager())
        {
        }

        public void RaiseLoginSucceeded() => OnLoginSucceeded();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    private sealed class FakeMainForm : MainForm
    {
        public FakeMainForm()
            : base(
                new StubSessionContext(),
                new StubCorrelationContext(),
                new ThemeManager())
        {
        }

        public void RaiseLogoutRequested() => OnLogoutRequested();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    private sealed class StubAuthClient : IApiAuthClient
    {
        public Task<LoginResult> LoginAsync(
            string username,
            string password,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<LogoutResult> LogoutAsync(
            string token,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LogoutResult(true, correlationId));
        }
    }

    private sealed class StubSessionContext : ISessionContext
    {
        public string? Token => null;
        public UserSession? User => null;
        public IReadOnlyList<string> Modules => Array.Empty<string>();
        public bool IsAuthenticated => false;
        public string? LastCorrelationId => null;

        public void SetSession(
            string token,
            UserSession user,
            IReadOnlyList<string> modules,
            string correlationId)
        {
        }

        public Task<LogoutCompletionResult> LogoutAsync(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LogoutCompletionResult(true));
        }

        public void Clear()
        {
        }
    }

    private sealed class StubCorrelationContext : ICorrelationContext
    {
        public string NewCorrelationId() => "stub-corr";
    }
}
