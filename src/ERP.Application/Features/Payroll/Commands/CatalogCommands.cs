using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using MediatR;

namespace ERP.Application.Features.Payroll.Commands;

public sealed record CreateDepartamentoCommand(DepartamentoInput Input) : IRequest<DepartamentoSnapshot>;
public sealed class CreateDepartamentoCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<CreateDepartamentoCommand, DepartamentoSnapshot>
{
    public Task<DepartamentoSnapshot> Handle(CreateDepartamentoCommand request, CancellationToken cancellationToken) => repository.CreateDepartmentAsync(request.Input, cancellationToken);
}
public sealed record UpdateDepartamentoCommand(long Id, DepartamentoInput Input) : IRequest<CatalogCommandResult>;
public sealed class UpdateDepartamentoCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<UpdateDepartamentoCommand, CatalogCommandResult>
{ public async Task<CatalogCommandResult> Handle(UpdateDepartamentoCommand request, CancellationToken ct) => await repository.UpdateDepartmentAsync(request.Id, request.Input, ct) ? CatalogCommandResult.Success() : CatalogCommandResult.NotFound(); }
public sealed record SetDepartamentoActiveCommand(long Id, bool Active) : IRequest<CatalogCommandResult>;
public sealed class SetDepartamentoActiveCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<SetDepartamentoActiveCommand, CatalogCommandResult>
{ public async Task<CatalogCommandResult> Handle(SetDepartamentoActiveCommand request, CancellationToken ct) { var department = await repository.GetDepartmentAsync(request.Id, ct); if (department is null) return CatalogCommandResult.NotFound(); if (department.Activo == request.Active) return CatalogCommandResult.Conflict(); return await repository.SetDepartmentActiveAsync(request.Id, request.Active, ct) ? CatalogCommandResult.Success() : CatalogCommandResult.Conflict(); } }

public sealed record CreateEmpleadoCommand(EmpleadoInput Input) : IRequest<EmpleadoSnapshot>;
public sealed class CreateEmpleadoCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<CreateEmpleadoCommand, EmpleadoSnapshot>
{
    public Task<EmpleadoSnapshot> Handle(CreateEmpleadoCommand request, CancellationToken cancellationToken) => repository.CreateEmployeeAsync(request.Input, cancellationToken);
}
public sealed record UpdateEmpleadoCommand(long Id, EmpleadoInput Input) : IRequest<CatalogCommandResult>;
public sealed class UpdateEmpleadoCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<UpdateEmpleadoCommand, CatalogCommandResult>
{ public async Task<CatalogCommandResult> Handle(UpdateEmpleadoCommand request, CancellationToken ct) => await repository.UpdateEmployeeAsync(request.Id, request.Input, ct) ? CatalogCommandResult.Success() : CatalogCommandResult.NotFound(); }
public sealed record SetEmpleadoActiveCommand(long Id, bool Active) : IRequest<CatalogCommandResult>;
public sealed class SetEmpleadoActiveCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<SetEmpleadoActiveCommand, CatalogCommandResult>
{ public async Task<CatalogCommandResult> Handle(SetEmpleadoActiveCommand request, CancellationToken ct) { var employee = await repository.GetEmployeeAsync(request.Id, ct); if (employee is null) return CatalogCommandResult.NotFound(); if (employee.Activo == request.Active) return CatalogCommandResult.Conflict(); return await repository.SetEmployeeActiveAsync(request.Id, request.Active, ct) ? CatalogCommandResult.Success() : CatalogCommandResult.Conflict(); } }

public sealed record ReactivateEmpleadoCommand(long Id) : IRequest<CatalogCommandResult>;
public sealed class ReactivateEmpleadoCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<ReactivateEmpleadoCommand, CatalogCommandResult>
{
    public Task<CatalogCommandResult> Handle(ReactivateEmpleadoCommand request, CancellationToken cancellationToken) =>
        new SetEmpleadoActiveCommandHandler(repository).Handle(new SetEmpleadoActiveCommand(request.Id, true), cancellationToken);
}

public sealed record CreateHorasExtraCommand(HorasExtraInput Input) : IRequest<HorasExtraSnapshot>;
public sealed class CreateHorasExtraCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<CreateHorasExtraCommand, HorasExtraSnapshot>
{
    public async Task<HorasExtraSnapshot> Handle(CreateHorasExtraCommand request, CancellationToken cancellationToken)
    {
        if (await repository.PayrollPeriodExistsAsync(request.Input.Periodo, cancellationToken)) throw new CatalogConflictException("Overtime cannot be registered after payroll exists.");
        return await repository.CreateOvertimeAsync(request.Input, cancellationToken);
    }
}
public sealed record UpdateHorasExtraCommand(long Id, HorasExtraInput Input) : IRequest<CatalogCommandResult>;
public sealed class UpdateHorasExtraCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<UpdateHorasExtraCommand, CatalogCommandResult>
{ public async Task<CatalogCommandResult> Handle(UpdateHorasExtraCommand request, CancellationToken ct) { if (await repository.PayrollPeriodExistsAsync(request.Input.Periodo, ct)) return CatalogCommandResult.Conflict(); return await repository.UpdateOvertimeAsync(request.Id, request.Input with { Id = request.Id }, ct) ? CatalogCommandResult.Success() : CatalogCommandResult.NotFound(); } }

public sealed record ApproveHorasExtraCommand(long Id, long ActorUserId, string CorrelationId) : IRequest<CatalogCommandResult>;
public sealed class ApproveHorasExtraCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<ApproveHorasExtraCommand, CatalogCommandResult>
{
    public async Task<CatalogCommandResult> Handle(ApproveHorasExtraCommand request, CancellationToken cancellationToken)
    {
        var overtime = await repository.GetOvertimeAsync(request.Id, cancellationToken);
        if (overtime is null) return CatalogCommandResult.NotFound();
        if (await repository.PayrollPeriodExistsAsync(overtime.Input.Periodo, cancellationToken)) return CatalogCommandResult.Conflict();
        await repository.ApproveOvertimeAsync(new OvertimeOperationContext(request.Id, request.ActorUserId, request.CorrelationId), cancellationToken);
        return CatalogCommandResult.Success();
    }
}

public sealed record CancelHorasExtraCommand(long Id, long ActorUserId, string CorrelationId) : IRequest<CatalogCommandResult>;
public sealed class CancelHorasExtraCommandHandler(IPayrollCatalogRepository repository) : IRequestHandler<CancelHorasExtraCommand, CatalogCommandResult>
{
    public async Task<CatalogCommandResult> Handle(CancelHorasExtraCommand request, CancellationToken cancellationToken)
    {
        var overtime = await repository.GetOvertimeAsync(request.Id, cancellationToken);
        if (overtime is null) return CatalogCommandResult.NotFound();
        if (await repository.PayrollPeriodExistsAsync(overtime.Input.Periodo, cancellationToken)) return CatalogCommandResult.Conflict();
        await repository.CancelOvertimeAsync(new OvertimeOperationContext(request.Id, request.ActorUserId, request.CorrelationId), cancellationToken);
        return CatalogCommandResult.Success();
    }
}
