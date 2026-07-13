using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ERP.Application.Abstractions;

namespace ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentDate, SystemCurrentDate>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);
        return services;
    }
}
