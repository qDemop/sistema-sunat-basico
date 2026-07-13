using Dapper;
using ERP.Domain.Authentication;
using ERP.Infrastructure.Persistence;
using ERP.IntegrationTests.Fixtures;
using Xunit;

namespace ERP.IntegrationTests.Authentication;

[Collection("PostgreSql")]
public class AuthenticationRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private AuthenticationRepository _repository = null!;

    public AuthenticationRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _repository = new AuthenticationRepository(_fixture.CreateConnectionFactory());
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_fixture.IsAvailable)
        {
            await _fixture.ResetDataAsync();
        }
    }

    [SkippableFact]
    public async Task GetByUsername_returns_Usuario_with_Rol()
    {
        _fixture.SkipIfNotAvailable();
        await using var connection = await _fixture.CreateConnectionAsync();
        var expectedRole = await connection.QuerySingleAsync<RoleRow>("""
            SELECT id_rol AS Id, nombre AS Nombre, nivel_acceso AS NivelAcceso
            FROM "identity".rol
            WHERE nombre = 'Administrador RRHH';
            """);
        await connection.ExecuteAsync("""
            INSERT INTO "identity".usuario (id_usuario, username, password_hash, nombre_completo, id_rol, activo, intentos_fallidos)
            VALUES (100, 'testuser', 'hash123', 'Test User', @RoleId, TRUE, 0);
            """, new { RoleId = expectedRole.Id });

        var usuario = await _repository.GetByUsernameWithRoleAsync("testuser");

        Assert.NotNull(usuario);
        Assert.Equal(100, usuario.Id);
        Assert.Equal("testuser", usuario.Username);
        Assert.Equal("hash123", usuario.PasswordHash);
        Assert.Equal("Test User", usuario.NombreCompleto);
        Assert.True(usuario.Activo);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.NotNull(usuario.Rol);
        Assert.Equal(expectedRole.Id, usuario.Rol.Id);
        Assert.Equal(expectedRole.Nombre, usuario.Rol.Nombre);
        Assert.Equal(expectedRole.NivelAcceso, usuario.Rol.NivelAcceso);
    }

    [SkippableFact]
    public async Task GetByUsername_returns_null_when_missing()
    {
        _fixture.SkipIfNotAvailable();
        var usuario = await _repository.GetByUsernameWithRoleAsync("nonexistent");

        Assert.Null(usuario);
    }

    private sealed record RoleRow(long Id, string Nombre, int NivelAcceso);
}
