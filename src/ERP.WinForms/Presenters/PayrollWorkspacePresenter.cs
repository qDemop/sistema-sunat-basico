using System.Text.Json;
using ERP.WinForms.Services;

namespace ERP.WinForms.Presenters;

public sealed class PayrollWorkspacePresenter(IApiPayrollClient api, IPayrollWorkspaceView view)
{
    private int _busy;
    public async Task LoadAsync(string period)
    {
        if (Interlocked.Exchange(ref _busy, 1) != 0) return;
        view.SetBusy(true);
        try { var json = await api.GetAsync($"/api/planilla?periodo={Uri.EscapeDataString(period)}"); using var document = JsonDocument.Parse(json); if (document.RootElement.GetProperty("resultados").GetArrayLength() == 0) view.ShowEmpty(); else view.ShowPayroll(json); }
        catch (Exception ex) { view.ShowError(ex.Message); }
        finally { view.SetBusy(false); Volatile.Write(ref _busy, 0); }
    }
    public Task CalculateAsync(string period) => ExecuteAsync($"/api/planilla/calcular", new { periodo = period }, period);
    public Task FinalizeAsync(string period) => ExecuteAsync($"/api/planilla/{period}/finalizar", null, period);
    public Task CancelAsync(string period) => ExecuteAsync($"/api/planilla/{period}/cancelar", null, period);
    public async Task ExportAsync(string period, string type, string extension)
    { if (Interlocked.Exchange(ref _busy, 1) != 0) return; view.SetBusy(true); try { await view.SaveExportAsync(extension, await api.DownloadAsync($"/api/planilla/{period}/export/{type}")); view.ShowSuccess("Exportación completada."); } catch (Exception ex) { view.ShowError(ex.Message); } finally { view.SetBusy(false); Volatile.Write(ref _busy, 0); } }
    private async Task ExecuteAsync(string path, object? body, string period)
    { if (Interlocked.Exchange(ref _busy, 1) != 0) return; view.SetBusy(true); try { await api.PostAsync(path, body); view.ShowSuccess("Operación completada."); var json = await api.GetAsync($"/api/planilla?periodo={Uri.EscapeDataString(period)}"); view.ShowPayroll(json); } catch (Exception ex) { view.ShowError(ex.Message); } finally { view.SetBusy(false); Volatile.Write(ref _busy, 0); } }
}
