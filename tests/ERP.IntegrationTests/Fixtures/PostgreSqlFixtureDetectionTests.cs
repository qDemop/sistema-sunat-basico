using System.Net;
using System.Net.Http;
using Docker.DotNet;

namespace ERP.IntegrationTests.Fixtures;

public class PostgreSqlFixtureDetectionTests
{
    [Fact]
    public void IsDockerUnavailableException_DockerApiException_returns_true()
    {
        var ex = new DockerApiException(HttpStatusCode.BadRequest, "Docker is not available");

        Assert.True(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_HttpRequestException_returns_true()
    {
        var ex = new HttpRequestException("No connection could be made because the target machine actively refused it.");

        Assert.True(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_DockerEndpointAuthConfig_message_returns_true()
    {
        var ex = new InvalidOperationException(
            "Docker is either not running or misconfigured. Please ensure that Docker is running and that the endpoint is properly configured. " +
            "(Parameter 'DockerEndpointAuthConfig')");

        Assert.True(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_DockerEndpointAuthConfig_inner_exception_returns_true()
    {
        var inner = new InvalidOperationException("(Parameter 'DockerEndpointAuthConfig')");
        var ex = new AggregateException(inner);

        Assert.True(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_schema_failure_returns_false()
    {
        var ex = new InvalidOperationException("schema.sql failed: relation 'usuario' already exists");

        Assert.False(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_seed_failure_returns_false()
    {
        var ex = new Npgsql.PostgresException("23505", "unique_violation", "duplicate key value violates unique constraint", "seeds.sql");

        Assert.False(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }

    [Fact]
    public void IsDockerUnavailableException_application_failure_returns_false()
    {
        var ex = new Exception("Unexpected application failure");

        Assert.False(PostgreSqlFixture.IsDockerUnavailableException(ex));
    }
}
