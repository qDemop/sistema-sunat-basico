using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Dapper;
using ERP.Infrastructure.Services;
using ERP.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.IntegrationTests.Authentication;

public sealed class AuthTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlFixture _pgFixture;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly BCryptPasswordHasher _passwordHasher = new();

    public AuthTestFixture()
    {
        _pgFixture = new PostgreSqlFixture();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ERP.Infrastructure.Persistence.IDbConnectionFactory>(
                        _pgFixture.CreateConnectionFactory());
                });
            });
    }

    public bool IsAvailable => _pgFixture.IsAvailable;
    public string SkipReason => _pgFixture.SkipReason;
    public HttpClient Client => _factory.CreateClient();

    public void SkipIfNotAvailable()
    {
        Skip.IfNot(IsAvailable, SkipReason);
    }

    public async Task InitializeAsync()
    {
        await _pgFixture.InitializeAsync();

        if (!IsAvailable)
        {
            return;
        }

        await SeedTestUserAsync();
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _pgFixture.DisposeAsync();
    }

    public async Task<HttpResponseMessage> LoginAsync(string username, string password)
    {
        var client = Client;
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });
        return response;
    }

    public async Task<(string Token, string CorrelationId)> LoginAsAdminAsync()
    {
        var response = await LoginAsync("adminit", "Admin123!");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        return (body.Token, body.CorrelationId);
    }

    public async Task ResetTestUserAsync()
    {
        await SeedTestUserAsync();
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task SeedTestUserAsync()
    {
        await using var connection = await _pgFixture.CreateConnectionAsync();
        var hash = _passwordHasher.HashPassword("Admin123!");
        var roleId = await connection.ExecuteScalarAsync<long>("""
            SELECT id_rol
            FROM "identity".rol
            WHERE nombre = 'Administrador Sistema';
            """);
        await connection.ExecuteAsync("""
            INSERT INTO "identity".usuario (id_usuario, username, password_hash, nombre_completo, id_rol, activo)
            VALUES (900, 'adminit', @Hash, 'Integration Admin', @RoleId, TRUE)
            ON CONFLICT (username) DO UPDATE SET
                password_hash = EXCLUDED.password_hash,
                intentos_fallidos = 0,
                bloqueado_hasta = NULL,
                activo = TRUE;
            """, new { Hash = hash, RoleId = roleId });
    }

    private sealed class LoginResponseBody
    {
        public string Token { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }
}

[CollectionDefinition("AuthIntegration")]
public class AuthIntegrationCollection : ICollectionFixture<AuthTestFixture>
{
}
