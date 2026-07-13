using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class PlanillaTests
{
    [Fact]
    public void Create_persists_canonical_employee_result_and_net_equation()
    {
        var calculation = Planilla.Calculate(3000m, 2m, 3m, 10m);

        var payroll = Planilla.Create(50, 10, 20, 30, 3000m, calculation);

        Assert.Equal(50, payroll.Id);
        Assert.Equal(10, payroll.PeriodoPlanillaId);
        Assert.Equal(20, payroll.EmpleadoId);
        Assert.Equal(30, payroll.DepartamentoId);
        Assert.Equal(3000m, payroll.SalarioBaseAplicado);
        Assert.Equal(calculation.TotalBruto, payroll.TotalBruto);
        Assert.Equal(calculation.TotalDescuentos, payroll.TotalDescuentos);
        Assert.Equal(payroll.TotalBruto - payroll.TotalDescuentos, payroll.TotalNeto);
    }

    [Fact]
    public void Create_rejects_missing_identifiers_nonpositive_applied_salary_or_invalid_net()
    {
        var calculation = new PayrollCalculation(100m, 16.67m, 9.72m, 10m, 91m, 0m);

        Assert.Throws<DomainValidationException>(() => Planilla.Create(0, 1, 1, 1, 100m, calculation));
        Assert.Throws<DomainValidationException>(() => Planilla.Create(1, 1, 1, 1, 0m, calculation));
        Assert.Throws<DomainValidationException>(() => Planilla.Create(1, 1, 1, 1, 100m, calculation with { TotalNeto = 90.99m }));
    }
}
