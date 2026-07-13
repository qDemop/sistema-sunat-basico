using ERP.Application.Abstractions;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Infrastructure.Configuration;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Persistence.Payroll;
using ERP.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<ITokenRevocationRepository, TokenRevocationRepository>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IPayrollCatalogRepository, PayrollRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        return services;
    }
}
