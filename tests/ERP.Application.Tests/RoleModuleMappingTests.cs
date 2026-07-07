using System.Linq;
using ERP.Application.Security;
using Xunit;

namespace ERP.Application.Tests;

public class RoleModuleMappingTests
{
    [Theory]
    [InlineData("Administrador RRHH", "Authentication", "Payroll")]
    [InlineData("Contador", "Authentication", "AccountingSUNAT", "GeneralLedger")]
    [InlineData("Gerente Financiero", "Authentication", "GeneralLedger", "Reports")]
    [InlineData("Administrador Sistema", "Authentication", "Payroll", "AccountingSUNAT", "GeneralLedger", "Reports", "Administration")]
    public void GetVisibleModules_ReturnsExpectedModules(string role, params string[] expectedModules)
    {
        var modules = RoleModuleMapping.GetVisibleModules(role);

        Assert.Equal(expectedModules, modules);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UnknownRole")]
    [InlineData("admin")]
    public void GetVisibleModules_ReturnsEmptyForUnknownRole(string role)
    {
        var modules = RoleModuleMapping.GetVisibleModules(role);

        Assert.Empty(modules);
    }

    [Theory]
    [InlineData("administrador rrhh")]
    [InlineData("ADMINISTRADOR RRHH")]
    [InlineData("Contador")]
    [InlineData("CONTADOR")]
    public void GetVisibleModules_IsCaseInsensitive(string role)
    {
        var modules = RoleModuleMapping.GetVisibleModules(role);

        Assert.NotEmpty(modules);
    }

    [Fact]
    public void IsKnownRole_ReturnsTrueForCanonicalRoles()
    {
        foreach (var role in RoleModuleMapping.GetKnownRoles())
        {
            Assert.True(RoleModuleMapping.IsKnownRole(role));
        }
    }

    [Fact]
    public void IsKnownRole_ReturnsFalseForUnknownRole()
    {
        Assert.False(RoleModuleMapping.IsKnownRole("Unknown"));
    }

    [Fact]
    public void GetKnownRoles_ReturnsFourRoles()
    {
        var roles = RoleModuleMapping.GetKnownRoles();

        Assert.Equal(4, roles.Count);
    }

    [Fact]
    public void AllModules_ContainsSixModules()
    {
        Assert.Equal(6, RoleModuleMapping.AllModules.Count);
    }
}
