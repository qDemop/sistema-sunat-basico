using ERP.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.WinForms;

public sealed class ServiceProviderFormFactory : IShellFormFactory
{
    private readonly IServiceProvider _services;

    public ServiceProviderFormFactory(IServiceProvider services)
    {
        _services = services;
    }

    public LoginForm CreateLoginForm()
    {
        return _services.GetRequiredService<LoginForm>();
    }

    public MainForm CreateMainForm()
    {
        return _services.GetRequiredService<MainForm>();
    }
}
