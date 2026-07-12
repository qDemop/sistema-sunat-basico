using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Commands;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Application.Features.Payroll.Queries;
using ERP.Domain.Payroll;

namespace ERP.Application.Tests.Payroll;

public sealed class PayrollCalculationHandlersTests
{
    [Fact]
    public async Task Calculate_invokes_canonical_operation_then_returns_persisted_projection()
    {
        var repository = new CalculationRepository();
        var handler = new CalculatePayrollCommandHandler(repository);

        var result = await handler.Handle(new CalculatePayrollCommand("2026-07", 7, "calc-1"), CancellationToken.None);

        Assert.Equal(new PayrollOperationContext("2026-07", 7, "calc-1"), repository.Calculation);
        Assert.Equal("2026-07", result.Periodo);
        Assert.Single(result.Resultados);
        Assert.Equal(1800m, result.Resultados[0].TotalNeto);
    }

    [Fact]
    public async Task Dashboard_query_returns_persisted_operational_totals_for_requested_period()
    {
        var repository = new CalculationRepository();
        var handler = new GetPayrollDashboardQueryHandler(repository);

        var result = await handler.Handle(new GetPayrollDashboardQuery("2026-07"), CancellationToken.None);

        Assert.Equal("2026-07", result.Periodo);
        Assert.Equal(1, result.EmpleadosElegibles);
        Assert.Equal(2500m, result.CostoPlanilla);
    }

    private sealed class CalculationRepository : IPayrollRepository
    {
        public PayrollOperationContext? Calculation { get; private set; }
        public Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { Calculation = context; return Task.CompletedTask; }
        public Task FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult<PayrollPeriodSnapshot?>(Snapshot());
        public Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PayrollPeriodSnapshot>>([Snapshot()]);
        public Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult(new PayrollDashboardSnapshot(periodo, 1, 1, 0, "Draft", 2000m, 1800m, 2500m));

        private static PayrollPeriodSnapshot Snapshot() => new(1, "2026-07", PeriodoPlanillaEstado.Draft, 2000m, 200m, 1800m, 333.33m, 194.44m)
        {
            Resultados = [new PayrollEmployeeResultSnapshot(1, "Ana Perez", 1, "Operations", 2000m, 0m, 2000m, "AFP", 5, 10m, 200m, 0m, 0m, 200m, 1800m, 333.33m, 194.44m, 2527.77m)]
        };
    }
}
