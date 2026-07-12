using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Tests.Payroll;

public class PayrollContractsTests
{
    [Fact]
    public async Task repository_contract_expresses_payroll_use_cases_without_storage_terms()
    {
        IPayrollRepository repository = new FakePayrollRepository();

        var periodContext = new PayrollOperationContext("2026-07", 7, "corr-1");
        var overtimeContext = new OvertimeOperationContext(1, 7, "corr-1");
        await repository.CalculateAsync(periodContext);
        await repository.FinalizeAsync(periodContext);
        await repository.CancelAsync(periodContext);
        await repository.ApproveOvertimeAsync(overtimeContext);
        await repository.CancelOvertimeAsync(overtimeContext);

        var fake = (FakePayrollRepository)repository;
        Assert.Equal(periodContext, fake.CalculateContext);
        Assert.Equal(periodContext, fake.FinalizeContext);
        Assert.Equal(periodContext, fake.CancelContext);
        Assert.Equal(overtimeContext, fake.ApproveOvertimeContext);
        Assert.Equal(overtimeContext, fake.CancelOvertimeContext);
        Assert.Null(await repository.GetByPeriodAsync("2026-07"));
    }

    private sealed class FakePayrollRepository : IPayrollRepository
    {
        public PayrollOperationContext? CalculateContext { get; private set; }
        public PayrollOperationContext? FinalizeContext { get; private set; }
        public PayrollOperationContext? CancelContext { get; private set; }
        public OvertimeOperationContext? ApproveOvertimeContext { get; private set; }
        public OvertimeOperationContext? CancelOvertimeContext { get; private set; }
        public Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { CalculateContext = context; return Task.CompletedTask; }
        public Task<PayrollPeriodSnapshot> FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { FinalizeContext = context; return Task.FromResult(Snapshot(context.Periodo, ERP.Domain.Payroll.PeriodoPlanillaEstado.Finalized, 99)); }
        public Task<PayrollPeriodSnapshot> CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { CancelContext = context; return Task.FromResult(Snapshot(context.Periodo, ERP.Domain.Payroll.PeriodoPlanillaEstado.Cancelled, null)); }
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) { ApproveOvertimeContext = context; return Task.CompletedTask; }
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) { CancelOvertimeContext = context; return Task.CompletedTask; }
        public Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult<PayrollPeriodSnapshot?>(null);
        public Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PayrollPeriodSnapshot>>([]);
        public Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult(new PayrollDashboardSnapshot(periodo, 0, 0, 0, "SinCalcular", 0, 0, 0));
        private static PayrollPeriodSnapshot Snapshot(string periodo, ERP.Domain.Payroll.PeriodoPlanillaEstado state, long? asientoDraftId) => new(1, periodo, state, 0, 0, 0, 0, 0) { AsientoDraftId = asientoDraftId };
    }
}
