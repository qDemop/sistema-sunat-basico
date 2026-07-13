using ERP.Application.Abstractions;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Infrastructure;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Persistence.Payroll;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Infrastructure.Tests;

public class AuthenticationPersistenceRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersAuthenticationRepository()
    {
        using var provider = CreateProvider();

        var service = provider.GetRequiredService<IAuthenticationRepository>();

        Assert.IsType<AuthenticationRepository>(service);
    }

    [Fact]
    public void AddInfrastructure_RegistersTokenRevocationRepository()
    {
        using var provider = CreateProvider();

        var service = provider.GetRequiredService<ITokenRevocationRepository>();

        Assert.IsType<TokenRevocationRepository>(service);
    }

    [Fact]
    public void AddInfrastructure_RegistersAuditWriter()
    {
        using var provider = CreateProvider();

        var service = provider.GetRequiredService<IAuditWriter>();

        Assert.IsType<AuditWriter>(service);
    }

    [Fact]
    public void AddInfrastructure_RegistersPayrollCatalogRepository()
    {
        using var provider = CreateProvider();

        var service = provider.GetRequiredService<IPayrollCatalogRepository>();

        Assert.IsType<PayrollRepository>(service);
    }

    private static ServiceProvider CreateProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=unused;Username=unused;Password=unused",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Key"] = "this-is-a-test-key-that-is-at-least-32-characters-long",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
