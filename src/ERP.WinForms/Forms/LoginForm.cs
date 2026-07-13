using ERP.WinForms.Presenters;
using ERP.WinForms.Services;
using ERP.WinForms.Theming;

namespace ERP.WinForms.Forms;

public class LoginForm : Form, ILoginFormView
{
    private readonly LoginFormPresenter _presenter;
    private readonly ThemeManager _themeManager;
    private readonly TextBox _usernameTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly Button _loginButton;
    private readonly Label _errorLabel;

    public event EventHandler? LoginSucceeded;

    public LoginForm(
        IApiAuthClient authClient,
        ISessionContext sessionContext,
        ICorrelationContext correlationContext,
        ThemeManager themeManager)
    {
        _themeManager = themeManager;
        _presenter = new LoginFormPresenter(this, authClient, sessionContext, correlationContext);
        _presenter.NavigateToDashboard += (_, _) => LoginSucceeded?.Invoke(this, EventArgs.Empty);

        Text = "ERP - Inicio de sesión";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 480;
        Height = 420;
        MinimumSize = new Size(420, 360);
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32),
            RowCount = 6,
            ColumnCount = 1
        };
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

        var titleLabel = new Label
        {
            Text = "Inicio de sesión",
            Font = new Font(Font.FontFamily, 18F, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Margin = new Padding(0, 0, 0, 16)
        };

        _usernameTextBox = CreateTextBox("Usuario");
        _passwordTextBox = CreateTextBox("Contraseña", usePasswordChar: true);

        _errorLabel = new Label
        {
            AutoSize = true,
            Visible = false,
            Margin = new Padding(0, 8, 0, 8)
        };

        _loginButton = new Button
        {
            Text = "Ingresar",
            Height = 40,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 16, 0, 0)
        };
        _loginButton.Click += LoginButton_Click;

        container.Controls.Add(titleLabel, 0, 0);
        container.Controls.Add(CreateFieldPanel("Usuario", _usernameTextBox), 0, 1);
        container.Controls.Add(CreateFieldPanel("Contraseña", _passwordTextBox), 0, 2);
        container.Controls.Add(_errorLabel, 0, 3);
        container.Controls.Add(_loginButton, 0, 4);
        Controls.Add(container);

        AcceptButton = _loginButton;
        _usernameTextBox.Focus();

        ApplyTheme();
    }

    public string Username => _usernameTextBox.Text;

    public string Password => _passwordTextBox.Text;

    public void ShowError(string message)
    {
        _errorLabel.Text = message;
        _errorLabel.Visible = true;
        _errorLabel.ForeColor = _themeManager.CurrentPalette.Danger;
    }

    public void ClearError()
    {
        _errorLabel.Text = string.Empty;
        _errorLabel.Visible = false;
    }

    public void SetBusy(bool busy)
    {
        _loginButton.Enabled = !busy;
        _usernameTextBox.Enabled = !busy;
        _passwordTextBox.Enabled = !busy;
        _loginButton.Text = busy ? "Verificando..." : "Ingresar";
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        await _presenter.SubmitAsync();
    }

    protected virtual void OnLoginSucceeded()
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTheme()
    {
        ThemeApplier.Apply(this, _themeManager.CurrentPalette);
        _errorLabel.ForeColor = _themeManager.CurrentPalette.Danger;
    }

    private static TextBox CreateTextBox(string placeholder, bool usePasswordChar = false)
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            PasswordChar = usePasswordChar ? '*' : '\0',
            TabStop = true
        };
    }

    private static Panel CreateFieldPanel(string labelText, Control input)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 8, 0, 8)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        panel.Controls.Add(label, 0, 0);
        panel.Controls.Add(input, 0, 1);
        return panel;
    }
}
