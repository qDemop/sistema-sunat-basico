using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ERP.IntegrationTests.Authentication;

public class AuthLoginFlowInMemoryTests : IClassFixture<InMemoryAuthApiFixture>
{
    private readonly InMemoryAuthApiFixture _fixture;

    public AuthLoginFlowInMemoryTests(InMemoryAuthApiFixture fixture)
    {
        _fixture = fixture;
        _fixture.AuthRepository.AddUser("adminmem", "Admin123!", "Memory Admin", "Administrador Sistema");
    }

    [Fact]
    public async Task Third_invalid_attempt_returns_423_with_lockout_expiry()
    {
        var client = _fixture.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "adminmem", password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var lockedResponse = await client.PostAsJsonAsync("/api/auth/login", new { username = "adminmem", password = "wrong" });

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);
        Assert.True(lockedResponse.Headers.Contains("X-Correlation-ID"));
        var headerCorrelationId = Assert.Single(lockedResponse.Headers.GetValues("X-Correlation-ID"));

        var body = await lockedResponse.Content.ReadFromJsonAsync<ErrorResponseBody>();
        Assert.NotNull(body);
        Assert.Equal("AUTH_ACCOUNT_LOCKED", body.Code);
        Assert.Equal(headerCorrelationId, body.CorrelationId);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.TryGetValue("bloqueadoHasta", out var rawLockout));
        var lockout = rawLockout is JsonElement json
            ? json.GetDateTime()
            : Assert.IsType<DateTime>(rawLockout);
        Assert.True(lockout > DateTime.UtcNow.AddMinutes(14));
        Assert.True(lockout <= DateTime.UtcNow.AddMinutes(16));

        var audit = _fixture.AuditWriter.Events.Last();
        Assert.Equal("LoginFailure", audit.Accion);
        Assert.Equal("Failure", audit.Resultado);
        Assert.True(audit.Datos.ContainsKey("lockoutTriggered"));
        Assert.True((bool)audit.Datos["lockoutTriggered"]);
        Assert.Equal(headerCorrelationId, audit.CorrelationId);
    }

    [Fact]
    public async Task Valid_credentials_200_and_correlation_echoed_in_header_and_body()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-mem-123");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "adminmem", password = "Admin123!" });

        response.EnsureSuccessStatusCode();
        var headerCorrelationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        Assert.NotNull(body);
        Assert.Equal("corr-mem-123", headerCorrelationId);
        Assert.Equal("corr-mem-123", body.CorrelationId);
    }

    private sealed class LoginResponseBody
    {
        public string Token { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class ErrorResponseBody
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
