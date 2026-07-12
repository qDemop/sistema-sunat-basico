using ERP.WinForms.Services;
using ERP.WinForms.Theming;
using ERP.WinForms.Forms;
using ERP.WinForms.Presenters;

namespace ERP.WinForms;

public class MainForm : Form
{
    private readonly ISessionContext _sessionContext;
    private readonly ICorrelationContext _correlationContext;
    private readonly ThemeManager _themeManager;
    private readonly Func<PayrollWorkspaceForm>? _payrollWorkspaceFactory;
    private readonly Button _themeToggleButton;
    private readonly Button _logoutButton;
    private readonly Label _titleLabel;
    private readonly Label _userLabel;
    private readonly Label _statusLabel;
    private readonly FlowLayoutPanel _modulesPanel;

    public event EventHandler? LogoutRequested;
    public event EventHandler<string>? LogoutError;

    public MainForm(
        ISessionContext sessionContext,
        ICorrelationContext correlationContext,
        ThemeManager themeManager,
        Func<PayrollWorkspaceForm>? payrollWorkspaceFactory = null)
    {
        _sessionContext = sessionContext;
        _correlationContext = correlationContext;
        _themeManager = themeManager;
        _payrollWorkspaceFactory = payrollWorkspaceFactory;

        Text = "ERP - Sistema SUNAT Basico";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1024;
        Height = 768;
        MinimumSize = new Size(1024, 720);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(16, 12, 16, 12)
        };

        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font(Font.FontFamily, 14F, FontStyle.Bold),
            Location = new Point(16, 12),
            Text = "ERP - Sistema SUNAT Basico"
        };

        _userLabel = new Label
        {
            AutoSize = true,
            Location = new Point(17, 42),
            Text = GetUserStatusText()
        };

        _statusLabel = new Label
        {
            AutoSize = true,
            Location = new Point(17, 64),
            Text = string.Empty,
            Visible = false
        };

        _themeToggleButton = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Height = 36,
            Width = 144,
            Location = new Point(Width - 320, 18)
        };
        _themeToggleButton.Click += ThemeToggleButton_Click;

        _logoutButton = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Height = 36,
            Width = 120,
            Text = "Cerrar sesión",
            Location = new Point(Width - 160, 18)
        };
        _logoutButton.Click += LogoutButton_Click;

        headerPanel.Controls.Add(_titleLabel);
        headerPanel.Controls.Add(_userLabel);
        headerPanel.Controls.Add(_statusLabel);
        headerPanel.Controls.Add(_themeToggleButton);
        headerPanel.Controls.Add(_logoutButton);
        Controls.Add(headerPanel);

        _modulesPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32),
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        Controls.Add(_modulesPanel);

        BuildModuleCards();
        ApplyCurrentTheme();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_themeToggleButton is null || _logoutButton is null)
        {
            return;
        }

        _logoutButton.Location = new Point(ClientSize.Width - _logoutButton.Width - 16, 18);
        _themeToggleButton.Location = new Point(
            _logoutButton.Location.X - _themeToggleButton.Width - 8,
            18);
    }

    private async void LogoutButton_Click(object? sender, EventArgs e)
    {
        _logoutButton.Enabled = false;
        try
        {
            await LogoutCoreAsync();
        }
        finally
        {
            _logoutButton.Enabled = true;
        }
    }

    protected virtual void OnLogoutRequested()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private async Task LogoutCoreAsync()
    {
        try
        {
            var correlationId = _correlationContext.NewCorrelationId();
            var result = await _sessionContext.LogoutAsync(correlationId);

            if (!result.RemoteRevocationSucceeded && !string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ShowLogoutError(result.ErrorMessage);
                LogoutError?.Invoke(this, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            var message = $"Logout failed: {ex.Message}";
            ShowLogoutError(message);
            LogoutError?.Invoke(this, message);
        }
        finally
        {
            _sessionContext.Clear();
            OnLogoutRequested();
        }
    }

    private void ShowLogoutError(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.Visible = true;
        _statusLabel.ForeColor = _themeManager.CurrentPalette.Danger;
    }

    private void ThemeToggleButton_Click(object? sender, EventArgs e)
    {
        _themeManager.ToggleLightDark();
        ApplyCurrentTheme();
    }

    private void ApplyCurrentTheme()
    {
        _themeToggleButton.Text = _themeManager.CurrentMode == AppThemeMode.Dark
            ? "Tema: Oscuro"
            : "Tema: Claro";

        ThemeApplier.Apply(this, _themeManager.CurrentPalette);
        _userLabel.ForeColor = _themeManager.CurrentPalette.TextSecondary;
        if (_statusLabel.Visible)
        {
            _statusLabel.ForeColor = _themeManager.CurrentPalette.Danger;
        }
    }

    private void BuildModuleCards()
    {
        var modules = _sessionContext.Modules;
        if (modules.Count == 0)
        {
            _modulesPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Text = "No tiene módulos asignados.",
                Margin = new Padding(0, 8, 0, 8)
            });
            return;
        }

        foreach (var module in modules)
        {
            var button = new Button
            {
                Text = TranslateModuleName(module),
                Height = 56,
                Width = 280,
                Margin = new Padding(0, 8, 0, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            if (module == "Authentication")
            {
                button.Enabled = false;
            }
            else if (module == "Payroll")
            {
                button.Enabled = PayrollNavigationPolicy.CanOpen(_sessionContext.User?.Rol) && _payrollWorkspaceFactory is not null;
                button.Click += (_, _) => { using var workspace = _payrollWorkspaceFactory?.Invoke(); workspace?.ShowDialog(this); };
            }

            _modulesPanel.Controls.Add(button);
        }
    }

    private static string TranslateModuleName(string module) => module switch
    {
        "Authentication" => "Autenticación",
        "Payroll" => "Planillas",
        "AccountingSUNAT" => "Contabilidad SUNAT",
        "GeneralLedger" => "Libro mayor",
        "Reports" => "Reportes",
        "Administration" => "Administración",
        _ => module
    };

    private string GetUserStatusText()
    {
        var user = _sessionContext.User;
        if (user is null)
        {
            return "Sin sesión activa";
        }

        return $"{user.Nombre} - {user.Rol}";
    }
}
