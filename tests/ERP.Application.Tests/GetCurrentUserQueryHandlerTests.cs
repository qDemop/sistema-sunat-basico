using System;
using System.Linq;
using ERP.Application.Features.Authentication;
using ERP.Application.Security;
using Xunit;

namespace ERP.Application.Tests;

public class GetCurrentUserQueryHandlerTests
{
    private const long TestUserId = 5;
    private const string TestNombre = "Maria Lopez";
    private const string TestRol = "Contador";

    [Fact]
    public async Task Handle_ReturnsUserAndModules()
    {
        var handler = new GetCurrentUserQueryHandler();
        var query = new GetCurrentUserQuery(TestUserId, TestNombre, TestRol);

        var response = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(TestUserId, response.User.Id);
        Assert.Equal(TestNombre, response.User.Nombre);
        Assert.Equal(TestRol, response.User.Rol);
        Assert.Equal(RoleModuleMapping.GetVisibleModules(TestRol), response.Modules);
    }

    [Theory]
    [InlineData("Administrador RRHH", "Authentication", "Payroll")]
    [InlineData("Contador", "Authentication", "AccountingSUNAT", "GeneralLedger")]
    [InlineData("Gerente Financiero", "Authentication", "GeneralLedger", "Reports")]
    [InlineData("Administrador Sistema", "Authentication", "Payroll", "AccountingSUNAT", "GeneralLedger", "Reports", "Administration")]
    public async Task Handle_ReturnsModulesForEachRole(string role, params string[] expectedModules)
    {
        var handler = new GetCurrentUserQueryHandler();
        var query = new GetCurrentUserQuery(TestUserId, TestNombre, role);

        var response = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(expectedModules, response.Modules);
    }

    [Fact]
    public async Task Handle_UnknownRole_ReturnsEmptyModules()
    {
        var handler = new GetCurrentUserQueryHandler();
        var query = new GetCurrentUserQuery(TestUserId, TestNombre, "UnknownRole");

        var response = await handler.Handle(query, CancellationToken.None);

        Assert.Empty(response.Modules);
    }

    [Fact]
    public async Task Handle_PreservesCorrelationId()
    {
        const string correlationId = "corr-me-123";
        var handler = new GetCurrentUserQueryHandler();
        var query = new GetCurrentUserQuery(TestUserId, TestNombre, TestRol, correlationId);

        var response = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(correlationId, response.CorrelationId);
    }

    [Fact]
    public async Task Handle_MissingCorrelationId_GeneratesOne()
    {
        var handler = new GetCurrentUserQueryHandler();
        var query = new GetCurrentUserQuery(TestUserId, TestNombre, TestRol, null);

        var response = await handler.Handle(query, CancellationToken.None);

        Assert.NotEmpty(response.CorrelationId);
        Assert.Equal(32, response.CorrelationId.Length);
        Assert.True(response.CorrelationId.All(c => char.IsLetterOrDigit(c)));
    }
}
