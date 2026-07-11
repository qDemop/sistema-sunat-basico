using System.Data;
using System.Net.Http;
using Dapper;
using Docker.DotNet;
using DotNet.Testcontainers.Builders;
using ERP.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ERP.IntegrationTests.Fixtures;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public bool IsAvailable { get; private set; }
    public string SkipReason { get; private set; } = string.Empty;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithDatabase("erp_test")
                .WithUsername("erp_test_user")
                .WithPassword("erp_test_password")
                .WithImage("postgres:16")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            IsAvailable = true;

            await ApplySchemaAsync();
            await ApplySeedsAsync();
        }
        catch (Exception ex) when (IsDockerUnavailableException(ex))
        {
            IsAvailable = false;
            SkipReason = $"Docker/PostgreSQL container is not available: {ex.Message}";
        }
    }

    internal static bool IsDockerUnavailableException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            // Typed Docker client / daemon failures.
            if (current is DockerApiException or HttpRequestException)
            {
                return true;
            }

            // Testcontainers' specific configuration parameter when it cannot locate a Docker endpoint.
            if (current.Message.Contains("DockerEndpointAuthConfig", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public async Task ResetDataAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            TRUNCATE TABLE "identity".login_attempt RESTART IDENTITY CASCADE;
            TRUNCATE TABLE "identity".token_revocation RESTART IDENTITY CASCADE;
            TRUNCATE TABLE "identity".usuario RESTART IDENTITY CASCADE;
            TRUNCATE TABLE audit.audit_log RESTART IDENTITY CASCADE;
            """);
        await ApplySeedsAsync();
    }

    public IDbConnectionFactory CreateConnectionFactory()
    {
        return new TestDbConnectionFactory(ConnectionString);
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    private async Task ApplySchemaAsync()
    {
        var sql = await File.ReadAllTextAsync("database/schema.sql");
        await using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql);
    }

    private async Task ApplySeedsAsync()
    {
        var sql = await File.ReadAllTextAsync("database/seeds.sql");
        await using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql);
    }

    private sealed class TestDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public TestDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}

public static class PostgreSqlFixtureExtensions
{
    public static void SkipIfNotAvailable(this PostgreSqlFixture fixture)
    {
        Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);
    }
}

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}
