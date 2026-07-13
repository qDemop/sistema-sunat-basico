using ERP.WinForms.Presenters;

namespace ERP.WinForms.Tests.Presenters;

public sealed class PayrollNavigationPolicyTests
{
    [Theory]
    [InlineData("Administrador RRHH", true)]
    [InlineData("Administrador Sistema", true)]
    [InlineData("Contador", false)]
    public void Allows_only_payroll_roles_to_open_workspace(string role, bool expected)
    {
        Assert.Equal(expected, PayrollNavigationPolicy.CanOpen(role));
    }
}
