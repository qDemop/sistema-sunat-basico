using ERP.WinForms.Forms;

namespace ERP.WinForms;

public class AppShell : ApplicationContext
{
    private readonly IShellFormFactory _formFactory;
    private Form? _currentForm;
    private bool _logoutRequested;

    public AppShell(IServiceProvider services)
        : this(new ServiceProviderFormFactory(services))
    {
    }

    internal AppShell(IShellFormFactory formFactory)
    {
        _formFactory = formFactory;
        ShowLogin();
    }

    private void ShowLogin()
    {
        _logoutRequested = false;
        var loginForm = _formFactory.CreateLoginForm();
        loginForm.LoginSucceeded += OnLoginSucceeded;
        loginForm.FormClosed += OnFormClosed;
        _currentForm = loginForm;
        _currentForm.Show();
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        if (_currentForm is LoginForm loginForm)
        {
            loginForm.LoginSucceeded -= OnLoginSucceeded;
            loginForm.FormClosed -= OnFormClosed;
            loginForm.Hide();
            loginForm.Dispose();
        }

        var mainForm = _formFactory.CreateMainForm();
        mainForm.LogoutRequested += OnLogoutRequested;
        mainForm.LogoutError += OnLogoutError;
        mainForm.FormClosed += OnFormClosed;
        _currentForm = mainForm;
        _currentForm.Show();
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        _logoutRequested = true;
        _currentForm?.Close();
    }

    private void OnLogoutError(object? sender, string e)
    {
        // Error is already displayed on the form; navigation proceeds regardless.
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (_logoutRequested && _currentForm is MainForm mainForm)
        {
            mainForm.LogoutRequested -= OnLogoutRequested;
            mainForm.LogoutError -= OnLogoutError;
            _currentForm.FormClosed -= OnFormClosed;
            _currentForm.Dispose();
            ShowLogin();
        }
        else
        {
            ExitThread();
        }
    }
}
