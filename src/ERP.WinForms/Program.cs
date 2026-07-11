using ERP.Application;
using ERP.Infrastructure;
using ERP.WinForms.Forms;
using ERP.WinForms.Services;
using ERP.WinForms.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ERP.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(context.Configuration);
                services.AddHttpClient("ERP.API", client =>
                {
                    client.BaseAddress = new Uri(
                        context.Configuration["Api:BaseUrl"] ?? "http://localhost:5000");
                });

                services.AddSingleton<ThemeManager>();
                services.AddSingleton<ICorrelationContext, CorrelationContext>();
                services.AddSingleton<IApiAuthClient, ApiAuthClient>();
                services.AddSingleton<ISessionContext, SessionContext>();
                services.AddSingleton<IShellFormFactory, ServiceProviderFormFactory>();
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();
            })
            .Build();

        System.Windows.Forms.Application.Run(new AppShell(host.Services));
    }
}
