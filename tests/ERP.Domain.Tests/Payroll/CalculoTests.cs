using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class CalculoTests
{
    [Fact]
    public void bruto_provision_grat_cts_neto_tolerance_0_01()
    {
        var calculation = Planilla.Calculate(3000.00m, 2, 3, 10m);

        Assert.Equal(3081.88m, calculation.TotalBruto);
        Assert.Equal(513.65m, calculation.ProvisionGratificacion);
        Assert.Equal(299.63m, calculation.ProvisionCts);
        Assert.Equal(308.19m, calculation.TotalDescuentos);
        Assert.InRange(Math.Abs(calculation.TotalNeto - 2773.69m), 0m, 0.01m);
    }

    [Fact]
    public void provisions_are_not_cash_or_net_pay()
    {
        var calculation = Planilla.Calculate(2400.00m, 0, 0, 13m);

        Assert.Equal(2400.00m, calculation.TotalBruto);
        Assert.Equal(312.00m, calculation.TotalDescuentos);
        Assert.Equal(2088.00m, calculation.TotalNeto);
        Assert.Equal(400.00m, calculation.ProvisionGratificacion);
        Assert.Equal(233.33m, calculation.ProvisionCts);
    }
}
