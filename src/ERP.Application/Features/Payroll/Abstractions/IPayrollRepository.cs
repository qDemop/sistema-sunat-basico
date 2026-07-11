using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Features.Payroll.Abstractions;

public interface IPayrollRepository
{
    Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default);
    Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
    Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
    Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default);
}
