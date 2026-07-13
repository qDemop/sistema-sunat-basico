using ERP.Application.Features.Authentication;
using ERP.WinForms.Services;

namespace ERP.WinForms.Tests.Services;

public class SessionContextTests
{
    private const string TestToken = "test-jwt-token";
    private const string TestCorrelationId = "corr-logout-123";

    [Fact]
    public async Task Logout_clears_token()
    {
        var authClient = new FakeApiAuthClient();
        var session = new SessionContext(authClient);
        session.SetSession(
            TestToken,
            new UserSession(1, "Admin", "Administrador Sistema"),
            new[] { "Authentication", "Payroll" },
            "corr-login");

        var result = await session.LogoutAsync(TestCorrelationId);

        Assert.True(result.RemoteRevocationSucceeded);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Token);
        Assert.Null(session.User);
        Assert.Empty(session.Modules);
        Assert.True(authClient.LogoutCalled);
        Assert.Equal(TestToken, authClient.LastToken);
        Assert.Equal(TestCorrelationId, authClient.LastCorrelationId);
    }

    [Fact]
    public async Task LogoutAsync_clears_session_when_remote_revocation_throws()
    {
        var authClient = new FakeApiAuthClient { ThrowOnLogout = true };
        var session = new SessionContext(authClient);
        session.SetSession(
            TestToken,
            new UserSession(1, "Admin", "Administrador Sistema"),
            new[] { "Authentication" },
            "corr-login");

        var result = await session.LogoutAsync(TestCorrelationId);

        Assert.False(result.RemoteRevocationSucceeded);
        Assert.NotNull(result.ErrorMessage);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Token);
        Assert.Null(session.User);
        Assert.Empty(session.Modules);
        Assert.True(authClient.LogoutCalled);
    }

    [Fact]
    public async Task LogoutAsync_clears_session_when_remote_revocation_returns_failure()
    {
        var authClient = new FakeApiAuthClient { LogoutResult = new LogoutResult(false, TestCorrelationId) };
        var session = new SessionContext(authClient);
        session.SetSession(
            TestToken,
            new UserSession(1, "Admin", "Administrador Sistema"),
            new[] { "Authentication" },
            "corr-login");

        var result = await session.LogoutAsync(TestCorrelationId);

        Assert.False(result.RemoteRevocationSucceeded);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Token);
        Assert.Empty(session.Modules);
    }

    [Fact]
    public void SetSession_populates_all_properties()
    {
        var session = new SessionContext(new FakeApiAuthClient());
        var user = new UserSession(2, "Contador", "Contador");

        session.SetSession(
            TestToken,
            user,
            new[] { "Authentication", "AccountingSUNAT", "GeneralLedger" },
            "corr-set");

        Assert.True(session.IsAuthenticated);
        Assert.Equal(TestToken, session.Token);
        Assert.Equal(user, session.User);
        Assert.Equal("Contador", session.User?.Rol);
        Assert.Equal(3, session.Modules.Count);
        Assert.Equal("corr-set", session.LastCorrelationId);
    }

    [Fact]
    public void Clear_without_logout_resets_session()
    {
        var session = new SessionContext(new FakeApiAuthClient());
        session.SetSession(
            TestToken,
            new UserSession(1, "Admin", "Administrador Sistema"),
            new[] { "Authentication" },
            "corr-set");

        session.Clear();

        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Token);
        Assert.Null(session.User);
        Assert.Empty(session.Modules);
    }

    private sealed class FakeApiAuthClient : IApiAuthClient
    {
        public bool ThrowOnLogout { get; set; }
        public LogoutResult LogoutResult { get; set; } = new(true, TestCorrelationId);
        public bool LogoutCalled { get; private set; }
        public string? LastToken { get; private set; }
        public string? LastCorrelationId { get; private set; }

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
            LogoutCalled = true;
            LastToken = token;
            LastCorrelationId = correlationId;

            if (ThrowOnLogout)
            {
                throw new HttpRequestException("Network error");
            }

            return Task.FromResult(LogoutResult);
        }
    }
}
