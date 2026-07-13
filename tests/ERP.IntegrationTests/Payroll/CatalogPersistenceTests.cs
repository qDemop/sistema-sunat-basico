using Dapper;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence.Payroll;
using ERP.IntegrationTests.Fixtures;

namespace ERP.IntegrationTests.Payroll;

[Collection("PostgreSql")]
public sealed class CatalogPersistenceTests(PostgreSqlFixture fixture)
{
    [SkippableFact]
    public async Task Create_department_persists_and_duplicate_name_is_rejected_by_canonical_unique_constraint()
    {
        fixture.SkipIfNotAvailable();
        await using var connection = await fixture.CreateConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM payroll.departamento WHERE nombre = 'IT Catalog Department';");
        var repository = new PayrollRepository(fixture.CreateConnectionFactory());

        var created = await repository.CreateDepartmentAsync(new DepartamentoInput("IT Catalog Department", "integration"));
        var persisted = await connection.QuerySingleAsync<(long Id, string Name, bool Active)>("SELECT id_departamento AS Id, nombre AS Name, activo AS Active FROM payroll.departamento WHERE id_departamento = @Id", new { created.Id });

        Assert.Equal("IT Catalog Department", persisted.Name);
        Assert.True(persisted.Active);
        await Assert.ThrowsAsync<Npgsql.PostgresException>(() => repository.CreateDepartmentAsync(new DepartamentoInput("IT Catalog Department", null)));
    }
}
