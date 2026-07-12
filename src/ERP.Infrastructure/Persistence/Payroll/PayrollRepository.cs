using Dapper;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Payroll;

public sealed class PayrollRepository(IDbConnectionFactory connectionFactory) : IPayrollCatalogRepository
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
}
