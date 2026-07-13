using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using MediatR;

namespace ERP.Application.Features.Payroll.Queries;

public sealed record GetPayrollByPeriodQuery(string Periodo) : IRequest<PayrollPeriodSnapshot?>;
public sealed class GetPayrollByPeriodQueryHandler(IPayrollRepository repository) : IRequestHandler<GetPayrollByPeriodQuery, PayrollPeriodSnapshot?>
{ public Task<PayrollPeriodSnapshot?> Handle(GetPayrollByPeriodQuery request, CancellationToken ct) => repository.GetByPeriodAsync(request.Periodo, ct); }

public sealed record ListPayrollPeriodsQuery(string? Estado) : IRequest<IReadOnlyList<PayrollPeriodSnapshot>>;
public sealed class ListPayrollPeriodsQueryHandler(IPayrollRepository repository) : IRequestHandler<ListPayrollPeriodsQuery, IReadOnlyList<PayrollPeriodSnapshot>>
{ public Task<IReadOnlyList<PayrollPeriodSnapshot>> Handle(ListPayrollPeriodsQuery request, CancellationToken ct) => repository.ListPeriodsAsync(request.Estado, ct); }

public sealed record GetPayrollDashboardQuery(string Periodo) : IRequest<PayrollDashboardSnapshot>;
public sealed class GetPayrollDashboardQueryHandler(IPayrollRepository repository) : IRequestHandler<GetPayrollDashboardQuery, PayrollDashboardSnapshot>
{ public Task<PayrollDashboardSnapshot> Handle(GetPayrollDashboardQuery request, CancellationToken ct) => repository.GetDashboardAsync(request.Periodo, ct); }
