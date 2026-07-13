using Dapper;
using System.Diagnostics;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence.Payroll;
using ERP.IntegrationTests.Fixtures;
using Xunit.Abstractions;

namespace ERP.IntegrationTests.Payroll;

[Collection("PostgreSql")]
public sealed class PayrollCalculationPersistenceTests(PostgreSqlFixture fixture, ITestOutputHelper output)
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

    [SkippableFact]
    public async Task Canonical_lifecycle_finalizes_once_with_balanced_draft_entry_and_cancellation_is_terminal()
    {
        fixture.SkipIfNotAvailable();
        await using var connection = await fixture.CreateConnectionAsync();
        await connection.ExecuteAsync("DELETE FROM payroll.empleado WHERE dni='12345672'; DELETE FROM payroll.departamento WHERE nombre='Lifecycle Test'; DELETE FROM \"identity\".usuario WHERE username='lifecycleactor';");
        var actorId = await connection.ExecuteScalarAsync<long>("INSERT INTO \"identity\".usuario(username,password_hash,nombre_completo,id_rol,activo) SELECT 'lifecycleactor','hash','Lifecycle Actor',id_rol,true FROM \"identity\".rol WHERE nombre='Administrador RRHH' RETURNING id_usuario;");
        var departmentId = await connection.ExecuteScalarAsync<long>("INSERT INTO payroll.departamento(nombre,activo) VALUES('Lifecycle Test',true) RETURNING id_departamento;");
        var typeId = await connection.ExecuteScalarAsync<long>("SELECT id_tipo FROM payroll.tipo_descuento WHERE nombre='AFP';");
        await connection.ExecuteAsync("INSERT INTO payroll.empleado(id_departamento,id_tipo_descuento,dni,nombres,apellidos,cargo,salario_base,fecha_nacimiento,fecha_ingreso,banco,numero_cuenta,activo) VALUES(@DepartmentId,@TypeId,'12345672','Rosa','Diaz','Analista',2400,'1990-01-01','2020-01-01','Banco','12345678901235',true);", new { DepartmentId = departmentId, TypeId = typeId });
        await connection.ExecuteAsync("INSERT INTO accounting.periodo_contable(codigo,fecha_inicio,fecha_fin,estado) VALUES ('2026-08','2026-08-01','2026-08-31','Open'), ('2026-09','2026-09-01','2026-09-30','Open') ON CONFLICT (codigo) DO UPDATE SET estado='Open';");
        var repository = new PayrollRepository(fixture.CreateConnectionFactory());

        await repository.CalculateAsync(new PayrollOperationContext("2026-08", actorId, "lifecycle-calc"));
        await repository.FinalizeAsync(new PayrollOperationContext("2026-08", actorId, "lifecycle-finalize"));
        var finalized = await repository.GetByPeriodAsync("2026-08");

        Assert.NotNull(finalized);
        Assert.Equal(ERP.Domain.Payroll.PeriodoPlanillaEstado.Finalized, finalized.Estado);
        Assert.NotNull(finalized.AsientoDraftId);
        Assert.Equal(0m, await connection.ExecuteScalarAsync<decimal>("SELECT sum(debe) - sum(haber) FROM accounting.detalle_asiento WHERE id_asiento_contable=@Id", new { Id = finalized.AsientoDraftId }));
        var recoveredFinalization = await repository.FinalizeAsync(new PayrollOperationContext("2026-08", actorId, "lifecycle-repeat"));
        Assert.Equal(finalized.Id, recoveredFinalization.Id);
        Assert.Equal(finalized.AsientoDraftId, recoveredFinalization.AsientoDraftId);

        await repository.CalculateAsync(new PayrollOperationContext("2026-09", actorId, "cancel-calc"));
        await repository.CancelAsync(new PayrollOperationContext("2026-09", actorId, "cancel"));
        var cancelled = await repository.GetByPeriodAsync("2026-09");
        Assert.NotNull(cancelled);
        Assert.Equal(ERP.Domain.Payroll.PeriodoPlanillaEstado.Cancelled, cancelled.Estado);
        Assert.Null(cancelled.AsientoDraftId);
        var recoveredCancellation = await repository.CancelAsync(new PayrollOperationContext("2026-09", actorId, "cancel-repeat"));
        Assert.Equal(cancelled.Id, recoveredCancellation.Id);
        Assert.Equal(ERP.Domain.Payroll.PeriodoPlanillaEstado.Cancelled, recoveredCancellation.Estado);
    }

    [SkippableFact]
    public async Task PAY_NFR_001_calculates_100_employees_within_30_seconds()
    {
        fixture.SkipIfNotAvailable();
        await using var connection = await fixture.CreateConnectionAsync();
        await connection.ExecuteAsync("""
            DELETE FROM payroll.empleado WHERE dni BETWEEN '99000000' AND '99000099';
            DELETE FROM payroll.departamento WHERE nombre = 'PAY-NFR-001 Performance';
            DELETE FROM "identity".usuario WHERE username = 'paynfractor';
            UPDATE payroll.empleado SET activo = FALSE WHERE dni NOT BETWEEN '99000000' AND '99000099';
            """);

        var actorId = await connection.ExecuteScalarAsync<long>("""
            INSERT INTO "identity".usuario(username, password_hash, nombre_completo, id_rol, activo)
            SELECT 'paynfractor', 'hash', 'Performance Actor', id_rol, TRUE
            FROM "identity".rol
            WHERE nombre = 'Administrador RRHH'
            RETURNING id_usuario;
            """);
        var departmentId = await connection.ExecuteScalarAsync<long>("INSERT INTO payroll.departamento(nombre, activo) VALUES ('PAY-NFR-001 Performance', TRUE) RETURNING id_departamento;");
        var typeId = await connection.ExecuteScalarAsync<long>("SELECT id_tipo FROM payroll.tipo_descuento WHERE nombre = 'AFP';");
        await connection.ExecuteAsync("""
            INSERT INTO payroll.empleado(
                id_departamento, id_tipo_descuento, dni, nombres, apellidos, cargo,
                salario_base, fecha_nacimiento, fecha_ingreso, banco, numero_cuenta, activo)
            SELECT @DepartmentId, @TypeId, lpad((99000000 + value)::text, 8, '0'),
                   'Performance', 'Employee', 'Analista', 2400, DATE '1990-01-01',
                   DATE '2020-01-01', 'Banco', lpad((12345678901234 + value)::text, 14, '0'), TRUE
            FROM generate_series(0, 99) AS value;
            """, new { DepartmentId = departmentId, TypeId = typeId });
        Assert.Equal(100, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM payroll.empleado WHERE dni BETWEEN '99000000' AND '99000099';"));

        var repository = new PayrollRepository(fixture.CreateConnectionFactory());
        var stopwatch = Stopwatch.StartNew();
        await repository.CalculateAsync(new PayrollOperationContext("2026-12", actorId, "pay-nfr-001"));
        stopwatch.Stop();

        var persisted = await repository.GetByPeriodAsync("2026-12");
        Assert.NotNull(persisted);
        Assert.Equal(100, persisted.Resultados.Count);
        output.WriteLine($"PAY-NFR-001 operation=sp_calcular_planilla employees=100 duration={stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        Assert.True(stopwatch.Elapsed <= TimeSpan.FromSeconds(30),
            $"PAY-NFR-001 operation sp_calcular_planilla for 100 employees took {stopwatch.Elapsed.TotalSeconds:F3}s; limit is 30s.");
    }
}
