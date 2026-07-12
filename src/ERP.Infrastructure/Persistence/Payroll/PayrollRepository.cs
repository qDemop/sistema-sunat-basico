using Dapper;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence;
using Npgsql;

namespace ERP.Infrastructure.Persistence.Payroll;

public sealed class PayrollRepository(IDbConnectionFactory connectionFactory) : IPayrollCatalogRepository, IPayrollRepository
{
    public async Task<DepartamentoSnapshot> CreateDepartmentAsync(DepartamentoInput input, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO payroll.departamento (nombre, descripcion) VALUES (@Nombre, @Descripcion)
            RETURNING id_departamento AS Id, nombre AS Nombre, descripcion AS Descripcion, activo AS Activo;
            """;
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<DepartmentRow>(new CommandDefinition(sql, input, cancellationToken: cancellationToken));
        return new DepartamentoSnapshot(row.Id, new DepartamentoInput(row.Nombre, row.Descripcion), row.Activo, 0);
    }

    public async Task<DepartamentoSnapshot?> GetDepartmentAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<DepartmentRow>(new CommandDefinition("SELECT id_departamento AS Id, nombre AS Nombre, descripcion AS Descripcion, activo AS Activo, (SELECT count(*)::int FROM payroll.empleado WHERE id_departamento = d.id_departamento AND activo) AS EmpleadosActivos FROM payroll.departamento d WHERE id_departamento = @Id", new { Id = id }, cancellationToken: cancellationToken));
        return row is null ? null : DepartmentSnapshot(row);
    }

    public async Task<IReadOnlyList<DepartamentoSnapshot>> ListDepartmentsAsync(bool? active, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id_departamento AS Id, nombre AS Nombre, descripcion AS Descripcion, activo AS Activo, (SELECT count(*)::int FROM payroll.empleado WHERE id_departamento = d.id_departamento AND activo) AS EmpleadosActivos FROM payroll.departamento d WHERE (@Active IS NULL OR activo = @Active) ORDER BY nombre";
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<DepartmentRow>(new CommandDefinition(sql, new { Active = active }, cancellationToken: cancellationToken));
        return rows.Select(DepartmentSnapshot).ToArray();
    }

    public async Task<bool> UpdateDepartmentAsync(long id, DepartamentoInput input, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("UPDATE payroll.departamento SET nombre = @Nombre, descripcion = @Descripcion WHERE id_departamento = @Id", new { Id = id, input.Nombre, input.Descripcion }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<bool> SetDepartmentActiveAsync(long id, bool active, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("UPDATE payroll.departamento SET activo = @Active WHERE id_departamento = @Id AND activo IS DISTINCT FROM @Active", new { Id = id, Active = active }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<EmpleadoSnapshot> CreateEmployeeAsync(EmpleadoInput input, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO payroll.empleado (id_departamento, id_tipo_descuento, dni, nombres, apellidos, cargo, salario_base, fecha_nacimiento, fecha_ingreso, banco, numero_cuenta)
            SELECT @DepartamentoId, @TipoDescuentoId, @Dni, @Nombres, @Apellidos, @Cargo, @SalarioBase, @FechaNacimiento, @FechaIngreso, @Banco, @NumeroCuenta
            WHERE EXISTS (SELECT 1 FROM payroll.departamento WHERE id_departamento = @DepartamentoId AND activo)
            RETURNING id_empleado AS Id, activo AS Activo, fecha_creacion AS FechaCreacion;
            """;
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<EmployeeRow>(new CommandDefinition(sql, input, cancellationToken: cancellationToken));
        return await GetEmployeeAsync(row.Id, cancellationToken) ?? throw new InvalidOperationException("Created employee could not be read.");
    }

    public async Task<EmpleadoSnapshot?> GetEmployeeAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<EmployeeReadRow>(new CommandDefinition(EmployeeSelect + " WHERE e.id_empleado = @Id", new { Id = id }, cancellationToken: cancellationToken));
        return row is null ? null : EmployeeSnapshot(row);
    }

    public async Task<IReadOnlyList<EmpleadoSnapshot>> ListEmployeesAsync(long? departmentId, string? search, bool? active, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<EmployeeReadRow>(new CommandDefinition(EmployeeSelect + " WHERE (@DepartmentId IS NULL OR e.id_departamento = @DepartmentId) AND (@Active IS NULL OR e.activo = @Active) AND (@Search IS NULL OR concat_ws(' ', e.dni, e.nombres, e.apellidos, e.cargo) ILIKE '%' || @Search || '%') ORDER BY e.apellidos, e.nombres", new { DepartmentId = departmentId, Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(), Active = active }, cancellationToken: cancellationToken));
        return rows.Select(EmployeeSnapshot).ToArray();
    }

    public async Task<bool> UpdateEmployeeAsync(long id, EmpleadoInput input, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE payroll.empleado SET id_departamento=@DepartamentoId, id_tipo_descuento=@TipoDescuentoId, dni=@Dni, nombres=@Nombres, apellidos=@Apellidos, cargo=@Cargo, salario_base=@SalarioBase, fecha_nacimiento=@FechaNacimiento, fecha_ingreso=@FechaIngreso, banco=@Banco, numero_cuenta=@NumeroCuenta WHERE id_empleado=@Id AND EXISTS (SELECT 1 FROM payroll.departamento WHERE id_departamento=@DepartamentoId AND activo)";
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, input.DepartamentoId, input.TipoDescuentoId, input.Dni, input.Nombres, input.Apellidos, input.Cargo, input.SalarioBase, input.FechaNacimiento, input.FechaIngreso, input.Banco, input.NumeroCuenta }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<bool> SetEmployeeActiveAsync(long id, bool active, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("UPDATE payroll.empleado SET activo = @Active WHERE id_empleado = @Id AND activo IS DISTINCT FROM @Active", new { Id = id, Active = active }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<HorasExtraSnapshot> CreateOvertimeAsync(HorasExtraInput input, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO payroll.horas_extra (id_empleado, periodo, horas_primeras_dos, horas_posteriores)
            VALUES (@EmpleadoId, @Periodo, @HorasPrimerasDos, @HorasPosteriores)
            RETURNING id_horas_extra AS Id, estado AS Estado, fecha_registro AS FechaRegistro;
            """;
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<OvertimeRow>(new CommandDefinition(sql, input, cancellationToken: cancellationToken));
        return new HorasExtraSnapshot(row.Id, input with { Id = row.Id }, row.Estado, row.FechaRegistro, null, null);
    }

    public async Task<HorasExtraSnapshot?> GetOvertimeAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<OvertimeReadRow>(new CommandDefinition(OvertimeSelect + " WHERE id_horas_extra = @Id", new { Id = id }, cancellationToken: cancellationToken));
        return row is null ? null : OvertimeSnapshot(row);
    }

    public async Task<IReadOnlyList<HorasExtraSnapshot>> ListOvertimeAsync(long? employeeId, string? period, string? state, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<OvertimeReadRow>(new CommandDefinition(OvertimeSelect + " WHERE (@EmployeeId IS NULL OR id_empleado = @EmployeeId) AND (@Period IS NULL OR periodo = @Period) AND (@State IS NULL OR estado = @State) ORDER BY periodo DESC, id_horas_extra DESC", new { EmployeeId = employeeId, Period = string.IsNullOrWhiteSpace(period) ? null : period.Trim(), State = string.IsNullOrWhiteSpace(state) ? null : state.Trim() }, cancellationToken: cancellationToken));
        return rows.Select(OvertimeSnapshot).ToArray();
    }

    public async Task<bool> UpdateOvertimeAsync(long id, HorasExtraInput input, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("UPDATE payroll.horas_extra SET id_empleado=@EmpleadoId, periodo=@Periodo, horas_primeras_dos=@HorasPrimerasDos, horas_posteriores=@HorasPosteriores WHERE id_horas_extra=@Id AND estado='Draft' AND NOT EXISTS (SELECT 1 FROM payroll.periodo_planilla WHERE periodo=@Periodo)", new { Id = id, input.EmpleadoId, input.Periodo, input.HorasPrimerasDos, input.HorasPosteriores }, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<bool> PayrollPeriodExistsAsync(string period, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("SELECT EXISTS (SELECT 1 FROM payroll.periodo_planilla WHERE periodo = @Period);", new { Period = period }, cancellationToken: cancellationToken));
    }

    public Task<bool> ReactivateEmployeeAsync(long id, CancellationToken cancellationToken = default) => SetEmployeeActiveAsync(id, true, cancellationToken);

    public async Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("CALL payroll.sp_aprobar_horas_extra(@HorasExtraId, @ActorUserId, @CorrelationId);", context, cancellationToken: cancellationToken));
    }

    public async Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("CALL payroll.sp_cancelar_horas_extra(@HorasExtraId, @ActorUserId, @CorrelationId);", context, cancellationToken: cancellationToken));
    }

    public async Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition("CALL payroll.sp_calcular_planilla(@Periodo, @ActorUserId, @CorrelationId);", context, cancellationToken: cancellationToken));
        }
        catch (PostgresException exception)
        {
            var translated = PayrollCalculationErrorTranslator.Translate(exception);
            if (translated is not null) throw translated;
            throw;
        }
    }

    public async Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await ReadPayrollSnapshotAsync(connection, null, periodo, cancellationToken);
    }

    public async Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT pp.id_periodo_planilla AS Id, pp.periodo AS Periodo, pp.estado AS Estado, pp.total_bruto AS TotalBruto, pp.total_descuentos AS TotalDescuentos, pp.total_neto AS TotalNeto, pp.total_provision_gratificacion AS TotalProvisionGratificacion, pp.total_provision_cts AS TotalProvisionCts, pp.fecha_calculo AS FechaCalculo, pp.fecha_finalizacion AS FechaFinalizacion, pp.id_usuario_finalizador AS FinalizadoPorId, ac.id_asiento_contable AS AsientoDraftId FROM payroll.periodo_planilla pp LEFT JOIN accounting.asiento_contable ac ON ac.origen='Planilla' AND ac.id_origen=pp.id_periodo_planilla WHERE (@Estado IS NULL OR pp.estado=@Estado) ORDER BY pp.periodo DESC";
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PayrollPeriodRow>(new CommandDefinition(sql, new { Estado = string.IsNullOrWhiteSpace(estado) ? null : estado }, cancellationToken: cancellationToken));
        return rows.Select(row => PeriodSnapshot(row, [])).ToArray();
    }

    public async Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default)
    {
        const string sql = "WITH period_end AS (SELECT (to_date(@Periodo || '-01', 'YYYY-MM-DD') + INTERVAL '1 month - 1 day')::date AS value), eligible AS (SELECT e.id_empleado FROM payroll.empleado e JOIN payroll.tipo_descuento t ON t.id_tipo=e.id_tipo_descuento CROSS JOIN period_end pe WHERE e.activo AND t.activo AND (SELECT count(*) FROM payroll.config_descuento_previsional_version cdv WHERE cdv.id_tipo=e.id_tipo_descuento AND cdv.estado='Active' AND cdv.fecha_inicio <= pe.value AND (cdv.fecha_fin IS NULL OR cdv.fecha_fin >= pe.value)) = 1) SELECT (SELECT count(*)::int FROM payroll.empleado WHERE activo) AS EmpleadosActivos, (SELECT count(*)::int FROM eligible) AS EmpleadosElegibles, ((SELECT count(*)::int FROM payroll.empleado WHERE activo) - (SELECT count(*)::int FROM eligible)) AS DatosIncompletos, COALESCE(pp.estado, 'SinCalcular') AS PeriodoEstado, COALESCE(pp.total_bruto, 0) AS TotalBruto, COALESCE(pp.total_neto, 0) AS TotalNeto, COALESCE(pp.total_bruto + pp.total_provision_gratificacion + pp.total_provision_cts, 0) AS CostoPlanilla FROM (SELECT 1) x LEFT JOIN payroll.periodo_planilla pp ON pp.periodo=@Periodo";
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleAsync<PayrollDashboardRow>(new CommandDefinition(sql, new { Periodo = periodo }, cancellationToken: cancellationToken));
        return new PayrollDashboardSnapshot(periodo, row.EmpleadosActivos, row.EmpleadosElegibles, row.DatosIncompletos, row.PeriodoEstado, row.TotalBruto, row.TotalNeto, row.CostoPlanilla);
    }

    public Task<PayrollPeriodSnapshot> FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => ExecuteLifecycleProcedureAsync("CALL payroll.sp_finalizar_planilla(@Periodo, @ActorUserId, @CorrelationId);", context, ERP.Domain.Payroll.PeriodoPlanillaEstado.Finalized, cancellationToken);
    public Task<PayrollPeriodSnapshot> CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => ExecuteLifecycleProcedureAsync("CALL payroll.sp_cancelar_planilla(@Periodo, @ActorUserId, @CorrelationId);", context, ERP.Domain.Payroll.PeriodoPlanillaEstado.Cancelled, cancellationToken);

    private async Task<PayrollPeriodSnapshot> ExecuteLifecycleProcedureAsync(string sql, PayrollOperationContext context, ERP.Domain.Payroll.PeriodoPlanillaEstado expectedState, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();
            var existing = await ReadPayrollSnapshotAsync(connection, transaction, context.Periodo, cancellationToken);
            if (existing?.Estado == expectedState)
            {
                transaction.Commit();
                return existing;
            }
            if (existing is not null && existing.Estado != ERP.Domain.Payroll.PeriodoPlanillaEstado.Draft)
                throw new PayrollOperationException(PayrollOperationError.StateConflict);

            await connection.ExecuteAsync(new CommandDefinition(sql, context, transaction, cancellationToken: cancellationToken));
            var projection = await ReadPayrollSnapshotAsync(connection, transaction, context.Periodo, cancellationToken)
                ?? throw new PayrollOperationException(PayrollOperationError.NotFound);
            if (projection.Estado != expectedState) throw new PayrollOperationException(PayrollOperationError.StateConflict);
            transaction.Commit();
            return projection;
        }
        catch (PostgresException exception)
        {
            var translated = PayrollCalculationErrorTranslator.Translate(exception);
            if (translated is not null) throw translated;
            throw;
        }
    }

    private async Task ExecutePayrollProcedureAsync(string sql, PayrollOperationContext context, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(sql, context, cancellationToken: cancellationToken));
        }
        catch (PostgresException exception)
        {
            var translated = PayrollCalculationErrorTranslator.Translate(exception);
            if (translated is not null) throw translated;
            throw;
        }
    }
    private const string PayrollResultsSql = "SELECT p.id_empleado AS EmpleadoId, concat_ws(' ', e.nombres, e.apellidos) AS Nombre, p.id_departamento AS DepartamentoId, dep.nombre AS Departamento, p.salario_base_aplicado AS SalarioBase, d.horas_extra_total AS HorasExtraMonto, p.total_bruto AS TotalBruto, td.nombre AS TipoDescuento, d.id_config_descuento_version AS ConfigDescuentoVersionId, cdv.porcentaje AS ConfigDescuentoPorcentaje, d.afp AS Afp, d.onp AS Onp, d.descuentos_adicionales AS DescuentosAdicionales, p.total_descuentos AS TotalDescuentos, p.total_neto AS TotalNeto, d.provision_gratificacion AS ProvisionGratificacion, d.provision_cts AS ProvisionCts, p.total_bruto+d.provision_gratificacion+d.provision_cts AS CostoTotal FROM payroll.planilla p JOIN payroll.detalle_planilla d ON d.id_planilla=p.id_planilla JOIN payroll.empleado e ON e.id_empleado=p.id_empleado JOIN payroll.departamento dep ON dep.id_departamento=p.id_departamento JOIN payroll.tipo_descuento td ON td.id_tipo=e.id_tipo_descuento JOIN payroll.config_descuento_previsional_version cdv ON cdv.id_config_descuento_version=d.id_config_descuento_version WHERE p.id_periodo_planilla=@PeriodId ORDER BY p.id_empleado";
    private const string PayrollPeriodSql = "SELECT pp.id_periodo_planilla AS Id, pp.periodo AS Periodo, pp.estado AS Estado, pp.total_bruto AS TotalBruto, pp.total_descuentos AS TotalDescuentos, pp.total_neto AS TotalNeto, pp.total_provision_gratificacion AS TotalProvisionGratificacion, pp.total_provision_cts AS TotalProvisionCts, pp.fecha_calculo AS FechaCalculo, pp.fecha_finalizacion AS FechaFinalizacion, pp.id_usuario_finalizador AS FinalizadoPorId, ac.id_asiento_contable AS AsientoDraftId FROM payroll.periodo_planilla pp LEFT JOIN accounting.asiento_contable ac ON ac.origen='Planilla' AND ac.id_origen=pp.id_periodo_planilla WHERE pp.periodo=@Periodo";
    private static async Task<PayrollPeriodSnapshot?> ReadPayrollSnapshotAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction? transaction, string periodo, CancellationToken cancellationToken)
    {
        var row = await connection.QuerySingleOrDefaultAsync<PayrollPeriodRow>(new CommandDefinition(PayrollPeriodSql, new { Periodo = periodo }, transaction, cancellationToken: cancellationToken));
        if (row is null) return null;
        var results = await connection.QueryAsync<PayrollResultRow>(new CommandDefinition(PayrollResultsSql, new { PeriodId = row.Id }, transaction, cancellationToken: cancellationToken));
        return PeriodSnapshot(row, results);
    }
    private static PayrollPeriodSnapshot PeriodSnapshot(PayrollPeriodRow row, IEnumerable<PayrollResultRow> results) => new(row.Id, row.Periodo, Enum.Parse<ERP.Domain.Payroll.PeriodoPlanillaEstado>(row.Estado), row.TotalBruto, row.TotalDescuentos, row.TotalNeto, row.TotalProvisionGratificacion, row.TotalProvisionCts) { FechaCalculo = row.FechaCalculo, FechaFinalizacion = row.FechaFinalizacion, FinalizadoPorId = row.FinalizadoPorId, AsientoDraftId = row.AsientoDraftId, Resultados = results.Select(x => new PayrollEmployeeResultSnapshot(x.EmpleadoId, x.Nombre, x.DepartamentoId, x.Departamento, x.SalarioBase, x.HorasExtraMonto, x.TotalBruto, x.TipoDescuento, x.ConfigDescuentoVersionId, x.ConfigDescuentoPorcentaje, x.Afp, x.Onp, x.DescuentosAdicionales, x.TotalDescuentos, x.TotalNeto, x.ProvisionGratificacion, x.ProvisionCts, x.CostoTotal)).ToArray() };

    private const string EmployeeSelect = "SELECT e.id_empleado AS Id, e.id_departamento AS DepartamentoId, e.id_tipo_descuento AS TipoDescuentoId, e.dni AS Dni, e.nombres AS Nombres, e.apellidos AS Apellidos, e.cargo AS Cargo, e.salario_base AS SalarioBase, e.fecha_nacimiento AS FechaNacimiento, e.fecha_ingreso AS FechaIngreso, e.banco AS Banco, e.numero_cuenta AS NumeroCuenta, e.activo AS Activo, d.nombre AS Departamento, t.nombre AS TipoDescuento, e.fecha_creacion AS FechaCreacion FROM payroll.empleado e JOIN payroll.departamento d ON d.id_departamento=e.id_departamento JOIN payroll.tipo_descuento t ON t.id_tipo=e.id_tipo_descuento";
    private const string OvertimeSelect = "SELECT id_horas_extra AS Id, id_empleado AS EmpleadoId, periodo AS Periodo, horas_primeras_dos AS HorasPrimerasDos, horas_posteriores AS HorasPosteriores, estado AS Estado, fecha_registro AS FechaRegistro, fecha_aprobacion AS FechaAprobacion, id_usuario_aprobador AS AprobadoPorId FROM payroll.horas_extra";
    private static DepartamentoSnapshot DepartmentSnapshot(DepartmentRow row) => new(row.Id, new DepartamentoInput(row.Nombre, row.Descripcion), row.Activo, row.EmpleadosActivos);
    private static EmpleadoSnapshot EmployeeSnapshot(EmployeeReadRow row) => new(row.Id, new EmpleadoInput(row.DepartamentoId, row.TipoDescuentoId, row.Dni, row.Nombres, row.Apellidos, row.Cargo, row.SalarioBase, row.FechaNacimiento, row.FechaIngreso, row.Banco, row.NumeroCuenta), row.Activo, row.Departamento, row.TipoDescuento, row.FechaCreacion);
    private static HorasExtraSnapshot OvertimeSnapshot(OvertimeReadRow row) => new(row.Id, new HorasExtraInput(row.Id, row.EmpleadoId, row.Periodo, row.HorasPrimerasDos, row.HorasPosteriores), row.Estado, row.FechaRegistro, row.FechaAprobacion, row.AprobadoPorId);
    private sealed record DepartmentRow(long Id, string Nombre, string? Descripcion, bool Activo, int EmpleadosActivos = 0);
    private sealed record EmployeeRow(long Id, bool Activo, DateTimeOffset FechaCreacion);
    private sealed record OvertimeRow(long Id, string Estado, DateTimeOffset FechaRegistro);
    private sealed record EmployeeReadRow(long Id, long DepartamentoId, long TipoDescuentoId, string Dni, string Nombres, string Apellidos, string Cargo, decimal SalarioBase, DateOnly FechaNacimiento, DateOnly FechaIngreso, string Banco, string NumeroCuenta, bool Activo, string Departamento, string TipoDescuento, DateTimeOffset FechaCreacion);
    private sealed record OvertimeReadRow(long Id, long EmpleadoId, string Periodo, decimal HorasPrimerasDos, decimal HorasPosteriores, string Estado, DateTimeOffset FechaRegistro, DateTimeOffset? FechaAprobacion, long? AprobadoPorId);
    private sealed record PayrollPeriodRow(long Id, string Periodo, string Estado, decimal TotalBruto, decimal TotalDescuentos, decimal TotalNeto, decimal TotalProvisionGratificacion, decimal TotalProvisionCts, DateTimeOffset FechaCalculo, DateTimeOffset? FechaFinalizacion, long? FinalizadoPorId, long? AsientoDraftId);
    private sealed record PayrollResultRow(long EmpleadoId, string Nombre, long DepartamentoId, string Departamento, decimal SalarioBase, decimal HorasExtraMonto, decimal TotalBruto, string TipoDescuento, long ConfigDescuentoVersionId, decimal ConfigDescuentoPorcentaje, decimal Afp, decimal Onp, decimal DescuentosAdicionales, decimal TotalDescuentos, decimal TotalNeto, decimal ProvisionGratificacion, decimal ProvisionCts, decimal CostoTotal);
    private sealed record PayrollDashboardRow(int EmpleadosActivos, int EmpleadosElegibles, int DatosIncompletos, string PeriodoEstado, decimal TotalBruto, decimal TotalNeto, decimal CostoPlanilla);
}
