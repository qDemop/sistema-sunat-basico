using System.Data;
using System.Text.Json;
using ERP.WinForms.Services;
using ERP.WinForms.Theming;
using ERP.WinForms.Presenters;

namespace ERP.WinForms.Forms;

public sealed class PayrollWorkspaceForm : Form, IPayrollWorkspaceView
{
    private readonly IApiPayrollClient _api;
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(12) };
    private readonly TextBox _period = new() { Text = DateTime.Today.ToString("yyyy-MM"), Width = 110 };
    private readonly DataGridView _results = Grid();
    private readonly PayrollWorkspacePresenter _presenter;

    public PayrollWorkspaceForm(IApiPayrollClient api, ThemeManager theme)
    {
        _api = api;
        _presenter = new PayrollWorkspacePresenter(api, this);
        Text = "Planillas"; Width = 1180; Height = 760; StartPosition = FormStartPosition.CenterParent;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CatalogTab("Departamentos", "/api/departamentos"));
        tabs.TabPages.Add(CatalogTab("Empleados", "/api/empleados"));
        tabs.TabPages.Add(CatalogTab("Horas extra", "/api/horas-extra"));
        tabs.TabPages.Add(PayrollTab());
        Controls.Add(tabs); Controls.Add(_status); ThemeApplier.Apply(this, theme.CurrentPalette);
    }
    private TabPage CatalogTab(string title, string endpoint)
    {
        var page = new TabPage(title); var grid = Grid(); var reload = new Button { Text = "Actualizar", Dock = DockStyle.Top };
        reload.Click += async (_, _) => await LoadGridAsync(grid, endpoint); page.Controls.Add(grid); page.Controls.Add(reload); page.Enter += async (_, _) => await LoadGridAsync(grid, endpoint); return page;
    }
    private TabPage PayrollTab()
    {
        var page = new TabPage("Cálculo y resultados"); var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8) };
        foreach (var (text, action) in new (string, Func<Task>)[] { ("Calcular", () => _presenter.CalculateAsync(_period.Text)), ("Revisar", () => _presenter.LoadAsync(_period.Text)), ("Finalizar", () => _presenter.FinalizeAsync(_period.Text)), ("Cancelar", () => _presenter.CancelAsync(_period.Text)), ("Excel", () => _presenter.ExportAsync(_period.Text, "excel", "xlsx")), ("Boletas PDF", () => _presenter.ExportAsync(_period.Text, "pdf", "zip")) }) { var button = new Button { Text = text, AutoSize = true }; button.Click += async (_, _) => await action(); actions.Controls.Add(button); }
        actions.Controls.Add(new Label { Text = "Periodo:", AutoSize = true, Padding = new Padding(12, 8, 0, 0) }); actions.Controls.Add(_period);
        page.Controls.Add(_results); page.Controls.Add(actions); return page;
    }
    private async Task LoadGridAsync(DataGridView grid, string endpoint) { SetBusy(true); try { var json = await _api.GetAsync(endpoint); using var document = JsonDocument.Parse(json); Bind(grid, document.RootElement.GetProperty("items")); ShowSuccess("Operación completada."); } catch (Exception ex) { ShowError(ex.Message); } finally { SetBusy(false); } }
    public void SetBusy(bool busy) { UseWaitCursor = busy; foreach (var control in Controls.OfType<TabControl>().SelectMany(x => x.TabPages.Cast<TabPage>()).SelectMany(x => x.Controls.OfType<FlowLayoutPanel>()).SelectMany(x => x.Controls.OfType<Button>())) control.Enabled = !busy; _status.Text = busy ? "Cargando..." : _status.Text; }
    public void ShowPayroll(string json) { using var document = JsonDocument.Parse(json); Bind(_results, document.RootElement.GetProperty("resultados")); }
    public void ShowEmpty() { _results.DataSource = null; _status.Text = "No hay resultados para el período."; }
    public void ShowSuccess(string message) => _status.Text = message;
    public void ShowError(string message) => _status.Text = $"No se pudo completar la operación: {message}";
    public async Task SaveExportAsync(string extension, byte[] content) { using var dialog = new SaveFileDialog { FileName = $"planilla-{_period.Text}.{extension}" }; if (dialog.ShowDialog(this) == DialogResult.OK) await File.WriteAllBytesAsync(dialog.FileName, content); }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private static void Bind(DataGridView grid, JsonElement array) { var table = new DataTable(); var rows = array.EnumerateArray().ToList(); foreach (var p in rows.SelectMany(x => x.EnumerateObject()).Select(x => x.Name).Distinct()) table.Columns.Add(p); foreach (var item in rows) { var row = table.NewRow(); foreach (var p in item.EnumerateObject()) row[p.Name] = p.Value.ToString(); table.Rows.Add(row); } grid.DataSource = table; }
}
