using MediatR;
using ERP.WinForms.Theming;

namespace ERP.WinForms;

public sealed class MainForm : Form
{
    private readonly ThemeManager _themeManager;
    private readonly Button _themeToggleButton;
    private readonly Label _titleLabel;
    private readonly Label _statusLabel;

    public MainForm(IMediator mediator, ThemeManager themeManager)
    {
        ArgumentNullException.ThrowIfNull(mediator);

        _themeManager = themeManager;
        Text = "ERP - Sistema SUNAT Basico";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1024;
        Height = 768;
        MinimumSize = new Size(1024, 720);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(16, 12, 16, 12)
        };

        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font(Font.FontFamily, 14F, FontStyle.Bold),
            Location = new Point(16, 12),
            Text = "ERP - Sistema SUNAT Basico"
        };

        _statusLabel = new Label
        {
            AutoSize = true,
            Location = new Point(17, 42),
            Text = "Base shell: theme foundation only"
        };

        _themeToggleButton = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Height = 36,
            Width = 144,
            Location = new Point(Width - 184, 18)
        };
        _themeToggleButton.Click += ThemeToggleButton_Click;

        headerPanel.Controls.Add(_titleLabel);
        headerPanel.Controls.Add(_statusLabel);
        headerPanel.Controls.Add(_themeToggleButton);
        Controls.Add(headerPanel);

        ApplyCurrentTheme();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_themeToggleButton is null)
        {
            return;
        }

        _themeToggleButton.Location = new Point(ClientSize.Width - _themeToggleButton.Width - 16, 18);
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
        _statusLabel.ForeColor = _themeManager.CurrentPalette.TextSecondary;
    }
}
