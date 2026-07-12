using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ERP.IntegrationTests.Fixtures;

namespace ERP.IntegrationTests.Authentication;

[Collection("AuthIntegration")]
public class AuthLoginFlowTests : IAsyncLifetime
{
    private readonly AuthTestFixture _fixture;

    public AuthLoginFlowTests(AuthTestFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _fixture.SkipIfNotAvailable();
        await _fixture.ResetTestUserAsync();
    }


    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [SkippableFact]
    public async Task Valid_credentials_200_token_modules()
    {
        var response = await _fixture.LoginAsync("adminit", "Admin123!");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));

        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.NotNull(body.User);
        Assert.Equal("Integration Admin", body.User.Nombre);
        Assert.Equal("Administrador Sistema", body.User.Rol);
        Assert.NotEmpty(body.Modules);
        Assert.Contains("Authentication", body.Modules);
        Assert.Contains("Payroll", body.Modules);
    }

    [SkippableFact]
    public async Task Invalid_credentials_return_401()
    {
        var response = await _fixture.LoginAsync("adminit", "incorrect-password");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [SkippableFact]
    public async Task Valid_login_allows_current_user_lookup()
    {
        var (token, _) = await _fixture.LoginAsAdminAsync();
        var client = _fixture.CreateAuthenticatedClient(token);

        var response = await client.GetAsync("/api/auth/me");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponseBody>();
        Assert.NotNull(body);
        Assert.NotNull(body.User);
        Assert.Equal(900, body.User.Id);
        Assert.Equal("Integration Admin", body.User.Nombre);
        Assert.Equal("Administrador Sistema", body.User.Rol);
    }

    [SkippableFact]
    public async Task Three_failures_lock_15_min_423()
    {
        for (var i = 0; i < 2; i++)
        {
            var response = await _fixture.LoginAsync("adminit", "wrong");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var lockedResponse = await _fixture.LoginAsync("adminit", "wrong");

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);
        Assert.True(lockedResponse.Headers.Contains("X-Correlation-ID"));
        var headerCorrelationId = Assert.Single(lockedResponse.Headers.GetValues("X-Correlation-ID"));

        var body = await lockedResponse.Content.ReadFromJsonAsync<ErrorResponseBody>();
        Assert.NotNull(body);
        Assert.Equal("AUTH_ACCOUNT_LOCKED", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.Message));
        Assert.Equal(headerCorrelationId, body.CorrelationId);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.TryGetValue("bloqueadoHasta", out var rawLockout));
        var lockout = rawLockout is JsonElement json
            ? json.GetDateTime()
            : Assert.IsType<DateTime>(rawLockout);
        Assert.True(lockout > DateTime.UtcNow.AddMinutes(14),
            $"Expected lockout expiry at least 14 minutes in the future, but was {lockout:O}.");
        Assert.True(lockout <= DateTime.UtcNow.AddMinutes(16),
            $"Expected lockout expiry at most 16 minutes in the future, but was {lockout:O}.");
    }

    [SkippableFact]
    public async Task Logout_revokes_jti_next_401()
    {
        var (token, _) = await _fixture.LoginAsAdminAsync();
        var client = _fixture.CreateAuthenticatedClient(token);

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    private sealed class LoginResponseBody
    {
        public string Token { get; set; } = string.Empty;
        public UserBody User { get; set; } = null!;
        public List<string> Modules { get; set; } = new();
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class UserBody
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    private sealed class CurrentUserResponseBody
    {
        public UserBody User { get; set; } = null!;
    }

    private sealed class ErrorResponseBody
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
