using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Commands;
using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Tests.Payroll;

public sealed class CatalogCommandHandlerTests
{
    [Fact]
    public async Task Create_employee_delegates_validated_input_and_returns_persisted_employee()
    {
        var repository = new FakeCatalogRepository();
        var handler = new CreateEmpleadoCommandHandler(repository);
        var input = EmployeeInput();

        var result = await handler.Handle(new CreateEmpleadoCommand(input), CancellationToken.None);

        Assert.Equal(input, repository.CreatedEmployee);
        Assert.Equal(42, result.Id);
        Assert.True(result.Activo);
    }

    [Fact]
    public async Task Reactivate_employee_returns_not_found_when_repository_does_not_change_a_row()
    {
        var repository = new FakeCatalogRepository { ReactivateEmployeeResult = false };
        var handler = new ReactivateEmpleadoCommandHandler(repository);

        var result = await handler.Handle(new ReactivateEmpleadoCommand(42), CancellationToken.None);

        Assert.False(result.Found);
        Assert.False(result.Changed);
    }

    [Fact]
    public async Task Approve_overtime_checks_that_no_payroll_period_exists_before_calling_the_operation()
    {
        var repository = new FakeCatalogRepository { Overtime = Overtime("2026-07"), PayrollExists = true };
        var handler = new ApproveHorasExtraCommandHandler(repository);

        var result = await handler.Handle(new ApproveHorasExtraCommand(9, 7, "corr-1"), CancellationToken.None);

        Assert.Equal(CatalogCommandStatus.Conflict, result.Status);
        Assert.False(repository.ApproveCalled);
    }

    [Fact]
    public async Task Approve_overtime_uses_actor_and_correlation_when_no_payroll_exists()
    {
        var repository = new FakeCatalogRepository { Overtime = Overtime("2026-07"), PayrollExists = false };
        var handler = new ApproveHorasExtraCommandHandler(repository);

        var result = await handler.Handle(new ApproveHorasExtraCommand(9, 7, "corr-1"), CancellationToken.None);

        Assert.Equal(CatalogCommandStatus.Success, result.Status);
        Assert.Equal(new OvertimeOperationContext(9, 7, "corr-1"), repository.ApproveContext);
    }

    [Fact]
    public async Task Cancel_overtime_checks_the_period_before_calling_the_canonical_operation()
    {
        var repository = new FakeCatalogRepository { Overtime = Overtime("2026-07"), PayrollExists = true };
        var handler = new CancelHorasExtraCommandHandler(repository);

        var result = await handler.Handle(new CancelHorasExtraCommand(9, 7, "corr-1"), CancellationToken.None);

        Assert.Equal(CatalogCommandStatus.Conflict, result.Status);
        Assert.False(repository.CancelCalled);
    }

    [Fact]
    public async Task Create_overtime_rejects_a_period_that_already_has_payroll()
    {
        var handler = new CreateHorasExtraCommandHandler(new FakeCatalogRepository { PayrollExists = true });

        await Assert.ThrowsAsync<CatalogConflictException>(() => handler.Handle(new CreateHorasExtraCommand(Overtime("2026-07")), CancellationToken.None));
    }

    [Fact]
    public async Task Update_overtime_returns_conflict_before_mutation_when_payroll_exists()
    {
        var repository = new FakeCatalogRepository { PayrollExists = true };
        var result = await new UpdateHorasExtraCommandHandler(repository).Handle(new UpdateHorasExtraCommand(9, Overtime("2026-07")), CancellationToken.None);

        Assert.Equal(CatalogCommandStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Deactivate_employee_delegates_logical_lifecycle_change()
    {
        var repository = new FakeCatalogRepository { SetEmployeeActiveResult = true, Employee = new EmpleadoSnapshot(42, EmployeeInput(), true, "Recursos Humanos", "AFP", DateTimeOffset.UtcNow) };
        var result = await new SetEmpleadoActiveCommandHandler(repository).Handle(new SetEmpleadoActiveCommand(42, false), CancellationToken.None);

        Assert.Equal(CatalogCommandStatus.Success, result.Status);
        Assert.False(repository.LastEmployeeActiveValue);
    }

    private static EmpleadoInput EmployeeInput() => new(1, 2, "12345678", "Ana", "Perez", "Analista", 2500m,
        new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "Banco", "12345678901234");

    private static HorasExtraInput Overtime(string period) => new(9, 2, period, 2m, 1m);

    private sealed class FakeCatalogRepository : IPayrollCatalogRepository
    {
        public EmpleadoInput? CreatedEmployee { get; private set; }
        public EmpleadoSnapshot? Employee { get; init; }
        public HorasExtraInput? Overtime { get; init; }
        public bool PayrollExists { get; init; }
        public bool ReactivateEmployeeResult { get; init; } = true;
        public bool SetEmployeeActiveResult { get; init; }
        public bool LastEmployeeActiveValue { get; private set; }
        public bool ApproveCalled { get; private set; }
        public bool CancelCalled { get; private set; }
        public OvertimeOperationContext? ApproveContext { get; private set; }

        public Task<DepartamentoSnapshot> CreateDepartmentAsync(DepartamentoInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DepartamentoSnapshot(1, input, true, 0));
        public Task<DepartamentoSnapshot?> GetDepartmentAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<DepartamentoSnapshot?>(null);
        public Task<IReadOnlyList<DepartamentoSnapshot>> ListDepartmentsAsync(bool? active, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DepartamentoSnapshot>>([]);
        public Task<bool> UpdateDepartmentAsync(long id, DepartamentoInput input, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> SetDepartmentActiveAsync(long id, bool active, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<EmpleadoSnapshot> CreateEmployeeAsync(EmpleadoInput input, CancellationToken cancellationToken = default)
        {
            CreatedEmployee = input;
            return Task.FromResult(new EmpleadoSnapshot(42, input, true, "Recursos Humanos", "AFP", DateTimeOffset.UtcNow));
        }

        public Task<bool> ReactivateEmployeeAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(ReactivateEmployeeResult);
        public Task<EmpleadoSnapshot?> GetEmployeeAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(Employee?.Id == id ? Employee : null);
        public Task<IReadOnlyList<EmpleadoSnapshot>> ListEmployeesAsync(long? departmentId, string? search, bool? active, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmpleadoSnapshot>>([]);
        public Task<bool> UpdateEmployeeAsync(long id, EmpleadoInput input, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> SetEmployeeActiveAsync(long id, bool active, CancellationToken cancellationToken = default) { LastEmployeeActiveValue = active; return Task.FromResult(SetEmployeeActiveResult || (active && ReactivateEmployeeResult)); }
        public Task<HorasExtraSnapshot> CreateOvertimeAsync(HorasExtraInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(new HorasExtraSnapshot(input.Id, input, "Draft", DateTimeOffset.UtcNow, null, null));
        public Task<HorasExtraSnapshot?> GetOvertimeAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(Overtime is null ? null : new HorasExtraSnapshot(Overtime.Id, Overtime, "Draft", DateTimeOffset.UtcNow, null, null));
        public Task<IReadOnlyList<HorasExtraSnapshot>> ListOvertimeAsync(long? employeeId, string? period, string? state, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<HorasExtraSnapshot>>([]);
        public Task<bool> UpdateOvertimeAsync(long id, HorasExtraInput input, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> PayrollPeriodExistsAsync(string period, CancellationToken cancellationToken = default) => Task.FromResult(PayrollExists);
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) { ApproveCalled = true; ApproveContext = context; return Task.CompletedTask; }
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) { CancelCalled = true; return Task.CompletedTask; }
    }
}
