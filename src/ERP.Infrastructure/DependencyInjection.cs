using ERP.Application.Abstractions;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, NpgsqlDbConnectionFactory>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
