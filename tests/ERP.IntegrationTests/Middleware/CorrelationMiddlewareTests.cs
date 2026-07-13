using System.Net;
using System.Net.Http.Json;
using ERP.IntegrationTests.Authentication;

namespace ERP.IntegrationTests.Middleware;

public class CorrelationMiddlewareTests : IClassFixture<InMemoryAuthApiFixture>
{
    private readonly InMemoryAuthApiFixture _fixture;

    public CorrelationMiddlewareTests(InMemoryAuthApiFixture fixture)
    {
        _fixture = fixture;
        _fixture.AuthRepository.AddUser("admincorr", "Admin123!", "Correlation Admin", "Administrador Sistema");
    }

    [Fact]
    public async Task without_X_Correlation_ID_generates_guid_and_echoes_in_header()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var correlationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task with_valid_X_Correlation_ID_echoes_in_header()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-123_abc");

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var correlationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.Equal("corr-123_abc", correlationId);
    }

    [Theory]
    [InlineData("invalid!!!")]
    [InlineData("this-is-a-very-long-correlation-id-that-exceeds-the-maximum-allowed-length-of-sixty-four")]
    public async Task with_invalid_X_Correlation_ID_generates_guid_and_echoes_in_header(string invalidCorrelationId)
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", invalidCorrelationId);

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var correlationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.NotEqual(invalidCorrelationId, correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task header_correlation_id_equals_body_correlation_id_on_error_response()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-login-123");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "admincorr", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var headerCorrelationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        var body = await response.Content.ReadFromJsonAsync<ErrorResponseBody>();
        Assert.NotNull(body);
        Assert.Equal("corr-login-123", headerCorrelationId);
        Assert.Equal("corr-login-123", body.CorrelationId);
    }

    [Fact]
    public async Task header_correlation_id_equals_body_correlation_id_on_success_response()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-success-456");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "admincorr", password = "Admin123!" });

        response.EnsureSuccessStatusCode();
        var headerCorrelationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        Assert.NotNull(body);
        Assert.Equal("corr-success-456", headerCorrelationId);
        Assert.Equal("corr-success-456", body.CorrelationId);
    }

    private sealed class ErrorResponseBody
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class LoginResponseBody
    {
        public string Token { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }
}
