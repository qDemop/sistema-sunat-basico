using Dapper;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence.Payroll;
using ERP.IntegrationTests.Fixtures;

namespace ERP.IntegrationTests.Payroll;

[Collection("PostgreSql")]
public sealed class PayrollCalculationPersistenceTests(PostgreSqlFixture fixture)
{
    [SkippableFact]
    public async Task Canonical_calculation_persists_net_invariant_and_effective_pension_version()
    {
        fixture.SkipIfNotAvailable();
        await using var connection = await fixture.CreateConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM payroll.empleado WHERE dni='12345671'; DELETE FROM payroll.departamento WHERE nombre='Calculation Test'; DELETE FROM \"identity\".usuario WHERE username='calcactor';");
        var actorId = await connection.ExecuteScalarAsync<long>("INSERT INTO \"identity\".usuario(username,password_hash,nombre_completo,id_rol,activo) SELECT 'calcactor','hash','Calculation Actor',id_rol,true FROM \"identity\".rol WHERE nombre='Administrador RRHH' RETURNING id_usuario;");
        var departmentId = await connection.ExecuteScalarAsync<long>("INSERT INTO payroll.departamento(nombre,activo) VALUES('Calculation Test',true) RETURNING id_departamento;");
        var typeId = await connection.ExecuteScalarAsync<long>("SELECT id_tipo FROM payroll.tipo_descuento WHERE nombre='AFP';");
        var employeeId = await connection.ExecuteScalarAsync<long>("INSERT INTO payroll.empleado(id_departamento,id_tipo_descuento,dni,nombres,apellidos,cargo,salario_base,fecha_nacimiento,fecha_ingreso,banco,numero_cuenta,activo) VALUES(@DepartmentId,@TypeId,'12345671','Maria','Lopez','Analista',2400,'1990-01-01','2020-01-01','Banco','12345678901234',true) RETURNING id_empleado;", new { DepartmentId = departmentId, TypeId = typeId });
        await connection.ExecuteAsync("INSERT INTO payroll.horas_extra(id_empleado,periodo,horas_primeras_dos,horas_posteriores,estado,fecha_aprobacion,id_usuario_aprobador) VALUES(@EmployeeId,'2026-07',2,1,'Approved',now(),@ActorId);", new { EmployeeId = employeeId, ActorId = actorId });
        var repository = new PayrollRepository(fixture.CreateConnectionFactory());

        await repository.CalculateAsync(new PayrollOperationContext("2026-07", actorId, "calc-pg-1"));
        var persisted = await repository.GetByPeriodAsync("2026-07");

        Assert.NotNull(persisted);
        var result = Assert.Single(persisted.Resultados);
        Assert.Equal(persisted.TotalBruto - persisted.TotalDescuentos, persisted.TotalNeto);
        Assert.Equal(result.TotalBruto - result.TotalDescuentos, result.TotalNeto);
        Assert.True(result.ConfigDescuentoVersionId > 0);
        Assert.Equal(10m, result.ConfigDescuentoPorcentaje);
        Assert.Equal(38.50m, result.HorasExtraMonto);
        Assert.Equal(2438.50m, result.TotalBruto);
        Assert.Equal(243.85m, result.Afp);
        Assert.Equal(2194.65m, result.TotalNeto);
        Assert.Equal(406.42m, result.ProvisionGratificacion);
        Assert.Equal(237.08m, result.ProvisionCts);

        var dashboard = await repository.GetDashboardAsync("2026-07");
        Assert.Equal(1, dashboard.EmpleadosActivos);
        Assert.Equal(1, dashboard.EmpleadosElegibles);
        Assert.Equal(0, dashboard.DatosIncompletos);

        var historicalDashboard = await repository.GetDashboardAsync("2025-12");
        Assert.Equal(0, historicalDashboard.EmpleadosElegibles);
        Assert.Equal(1, historicalDashboard.DatosIncompletos);

        var futureVersion = await connection.ExecuteScalarAsync<long>("INSERT INTO payroll.config_descuento_previsional_version(id_tipo,version,porcentaje,fecha_inicio,estado) VALUES(@TypeId,99,11.00,'2027-01-01','Draft') RETURNING id_config_descuento_version;", new { TypeId = typeId });
        Assert.True(futureVersion > 0);
        var effectiveVersion = await connection.ExecuteScalarAsync<long?>("SELECT payroll.fn_config_descuento_version_activa(@TypeId, DATE '2026-07-31');", new { TypeId = typeId });
        Assert.Equal(result.ConfigDescuentoVersionId, effectiveVersion);
        await Assert.ThrowsAsync<Npgsql.PostgresException>(() => connection.ExecuteAsync("INSERT INTO payroll.config_descuento_previsional_version(id_tipo,version,porcentaje,fecha_inicio,estado) VALUES(@TypeId,100,11.00,'2026-07-01','Active');", new { TypeId = typeId }));
    }
}
