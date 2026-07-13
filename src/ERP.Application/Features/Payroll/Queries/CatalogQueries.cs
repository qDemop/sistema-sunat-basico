using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using MediatR;

namespace ERP.Application.Features.Payroll.Queries;

public sealed record GetDepartamentoQuery(long Id) : IRequest<DepartamentoSnapshot?>;
public sealed class GetDepartamentoQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<GetDepartamentoQuery, DepartamentoSnapshot?>
{ public Task<DepartamentoSnapshot?> Handle(GetDepartamentoQuery request, CancellationToken ct) => repository.GetDepartmentAsync(request.Id, ct); }
public sealed record ListDepartamentosQuery(bool? Active) : IRequest<IReadOnlyList<DepartamentoSnapshot>>;
public sealed class ListDepartamentosQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<ListDepartamentosQuery, IReadOnlyList<DepartamentoSnapshot>>
{ public Task<IReadOnlyList<DepartamentoSnapshot>> Handle(ListDepartamentosQuery request, CancellationToken ct) => repository.ListDepartmentsAsync(request.Active, ct); }
public sealed record GetEmpleadoQuery(long Id) : IRequest<EmpleadoSnapshot?>;
public sealed class GetEmpleadoQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<GetEmpleadoQuery, EmpleadoSnapshot?>
{ public Task<EmpleadoSnapshot?> Handle(GetEmpleadoQuery request, CancellationToken ct) => repository.GetEmployeeAsync(request.Id, ct); }
public sealed record ListEmpleadosQuery(long? DepartmentId, string? Search, bool? Active) : IRequest<IReadOnlyList<EmpleadoSnapshot>>;
public sealed class ListEmpleadosQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<ListEmpleadosQuery, IReadOnlyList<EmpleadoSnapshot>>
{ public Task<IReadOnlyList<EmpleadoSnapshot>> Handle(ListEmpleadosQuery request, CancellationToken ct) => repository.ListEmployeesAsync(request.DepartmentId, request.Search, request.Active, ct); }
public sealed record GetHorasExtraQuery(long Id) : IRequest<HorasExtraSnapshot?>;
public sealed class GetHorasExtraQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<GetHorasExtraQuery, HorasExtraSnapshot?>
{ public Task<HorasExtraSnapshot?> Handle(GetHorasExtraQuery request, CancellationToken ct) => repository.GetOvertimeAsync(request.Id, ct); }
public sealed record ListHorasExtraQuery(long? EmployeeId, string? Period, string? State) : IRequest<IReadOnlyList<HorasExtraSnapshot>>;
public sealed class ListHorasExtraQueryHandler(IPayrollCatalogRepository repository) : IRequestHandler<ListHorasExtraQuery, IReadOnlyList<HorasExtraSnapshot>>
{ public Task<IReadOnlyList<HorasExtraSnapshot>> Handle(ListHorasExtraQuery request, CancellationToken ct) => repository.ListOvertimeAsync(request.EmployeeId, request.Period, request.State, ct); }
