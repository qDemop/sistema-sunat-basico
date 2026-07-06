using ERP.Application;
using ERP.Infrastructure;
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
                services.AddSingleton<MainForm>();
            })
            .Build();

        System.Windows.Forms.Application.Run(host.Services.GetRequiredService<MainForm>());
    }
}
