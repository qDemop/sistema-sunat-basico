using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Features.Payroll.Abstractions;

public interface IPayrollRepository
{
    Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task<PayrollPeriodSnapshot> FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task<PayrollPeriodSnapshot> CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
    Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
    Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default);
    Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default);
}
