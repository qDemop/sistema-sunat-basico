using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ERP.Application.Features.Authentication;
using ERP.WinForms.Services;

namespace ERP.WinForms.Tests.Services;

public class ApiAuthClientTests
{
    private const string TestCorrelationId = "corr-api-123";

    [Fact]
    public async Task Authorized_request_adds_X_Correlation_ID()
    {
        var handler = new CaptureHandler();
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        var request = Assert.Single(handler.Requests);
        Assert.True(request.Headers.Contains("X-Correlation-ID"));
        var correlationId = Assert.Single(request.Headers.GetValues("X-Correlation-ID"));
        Assert.Equal(TestCorrelationId, correlationId);
    }

    [Fact]
    public async Task LoginAsync_parses_success_response()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                token = "jwt-123",
                expiresAt = DateTime.UtcNow.AddHours(1),
                user = new { id = 1, nombre = "Admin", rol = "Administrador Sistema" },
                modules = new[] { "Authentication", "Payroll" },
                correlationId = TestCorrelationId
            })
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-123", result.Token);
        Assert.Equal("Admin", result.User?.Nombre);
        Assert.Equal("Administrador Sistema", result.User?.Rol);
        Assert.Equal(2, result.Modules?.Count);
        Assert.Equal(TestCorrelationId, result.CorrelationId);

        var request = Assert.Single(handler.Requests);
        Assert.False(request.Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task LoginAsync_parses_401()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = JsonContent.Create(new
            {
                status = 401,
                code = "AUTH_INVALID_CREDENTIALS",
                message = "Invalid credentials.",
                correlationId = TestCorrelationId
            })
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "wrong", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid credentials.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_parses_423()
    {
        var lockout = DateTime.UtcNow.AddMinutes(15);
        var handler = new CaptureHandler(new HttpResponseMessage((HttpStatusCode)423)
        {
            Content = JsonContent.Create(new
            {
                status = 423,
                code = "AUTH_ACCOUNT_LOCKED",
                message = "Account locked.",
                correlationId = TestCorrelationId,
                data = new Dictionary<string, object> { ["bloqueadoHasta"] = lockout }
            })
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsLocked);
        Assert.Equal(lockout, result.LockoutUntil);
    }

    [Fact]
    public async Task LogoutAsync_adds_bearer_token_and_correlation_id()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { revoked = true, correlationId = TestCorrelationId })
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        await client.LogoutAsync("jwt-123", TestCorrelationId);

        var request = Assert.Single(handler.Requests);
        Assert.True(request.Headers.Contains("X-Correlation-ID"));
        Assert.Equal(TestCorrelationId, Assert.Single(request.Headers.GetValues("X-Correlation-ID")));
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("jwt-123", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task LoginAsync_network_failure_returns_safe_error()
    {
        var handler = new ThrowingHandler(new HttpRequestException("No connection"));
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Unable to connect", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_timeout_returns_safe_error()
    {
        var handler = new ThrowingHandler(new TaskCanceledException("Timeout"));
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("timed out", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_server_5xx_returns_safe_error()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("<html>error</html>")
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_malformed_json_returns_safe_error()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{not valid json}")
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_missing_token_in_success_response_returns_safe_error()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                user = new { id = 1, nombre = "Admin", rol = "Administrador Sistema" },
                modules = Array.Empty<string>(),
                correlationId = TestCorrelationId
            })
        });
        var factory = new StubHttpClientFactory(handler);
        var client = new ApiAuthClient(factory, "ERP.API");

        var result = await client.LoginAsync("admin", "Admin123!", TestCorrelationId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public CaptureHandler(HttpResponseMessage? response = null)
        {
            _response = response ?? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    token = "jwt-123",
                    expiresAt = DateTime.UtcNow.AddHours(1),
                    user = new { id = 1, nombre = "Admin", rol = "Administrador Sistema" },
                    modules = Array.Empty<string>(),
                    correlationId = "default"
                })
            };
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_response);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(_exception);
        }
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler) { BaseAddress = new Uri("http://localhost:5000") };
        }
    }
}
