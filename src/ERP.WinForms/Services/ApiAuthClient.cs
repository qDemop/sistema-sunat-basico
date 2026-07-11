using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ERP.Application.Features.Authentication;

namespace ERP.WinForms.Services;

public sealed class ApiAuthClient : IApiAuthClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiAuthClient(IHttpClientFactory httpClientFactory, string clientName = "ERP.API")
    {
        _httpClientFactory = httpClientFactory;
        _clientName = clientName;
    }

    public async Task<LoginResult> LoginAsync(
        string username,
        string password,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(correlationId);
            var request = new { Username = username, Password = password };

            using var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                request,
                JsonOptions,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var error = await ReadErrorAsync(response, cancellationToken);
                return LoginResult.Failed(error?.Message ?? "Invalid credentials.");
            }

            if ((int)response.StatusCode == 423)
            {
                var error = await ReadErrorAsync(response, cancellationToken);
                var lockout = ExtractLockoutUntil(error);
                return lockout is not null
                    ? LoginResult.Locked(lockout.Value)
                    : LoginResult.Failed(error?.Message ?? "Account is locked.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken);
                return LoginResult.Failed(error?.Message ?? "Login request failed.");
            }

            var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>(JsonOptions, cancellationToken);
            if (body is null || string.IsNullOrWhiteSpace(body.Token) || body.User is null)
            {
                return LoginResult.Failed("Invalid server response.");
            }

            var user = new UserSession(body.User.Id, body.User.Nombre, body.User.Rol);
            return LoginResult.Success(
                body.Token,
                user,
                (IReadOnlyList<string>?)body.Modules ?? Array.Empty<string>(),
                body.CorrelationId ?? correlationId);
        }
        catch (TaskCanceledException)
        {
            return LoginResult.Failed("The login request timed out. Please try again.");
        }
        catch (HttpRequestException)
        {
            return LoginResult.Failed("Unable to connect to the server. Please check your network and try again.");
        }
        catch (JsonException)
        {
            return LoginResult.Failed("Received an invalid response from the server.");
        }
        catch (InvalidOperationException)
        {
            return LoginResult.Failed("The login request could not be completed.");
        }
    }

    public async Task<LogoutResult> LogoutAsync(
        string token,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.PostAsync(
                "/api/auth/logout",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new LogoutResult(false, correlationId);
            }

            var body = await response.Content.ReadFromJsonAsync<LogoutResponseBody>(JsonOptions, cancellationToken);
            return new LogoutResult(body?.Revoked ?? true, body?.CorrelationId ?? correlationId);
        }
        catch (Exception)
        {
            return new LogoutResult(false, correlationId);
        }
    }

    private HttpClient CreateClient(string correlationId)
    {
        var client = _httpClientFactory.CreateClient(_clientName);
        client.DefaultRequestHeaders.Remove("X-Correlation-ID");
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
        return client;
    }

    private static async Task<ErrorBody?> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ErrorBody>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static DateTime? ExtractLockoutUntil(ErrorBody? error)
    {
        if (error?.Data is null)
        {
            return null;
        }

        if (error.Data.TryGetValue("bloqueadoHasta", out var raw))
        {
            return raw switch
            {
                DateTime dt => dt,
                JsonElement json => json.GetDateTime(),
                _ => null
            };
        }

        return null;
    }

    private sealed class LoginResponseBody
    {
        public string Token { get; set; } = string.Empty;
        public UserBody User { get; set; } = new();
        public List<string> Modules { get; set; } = new();
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class UserBody
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    private sealed class LogoutResponseBody
    {
        public bool Revoked { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

    private sealed class ErrorBody
    {
        public int Status { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public Dictionary<string, object>? Data { get; set; }
    }
}
