using ERP.WinForms.Presenters;
using ERP.WinForms.Services;

namespace ERP.WinForms.Tests.Presenters;

public sealed class PayrollWorkspacePresenterTests
{
    [Fact]
    public async Task Load_renders_results_and_balances_loading_state()
    {
        var view = new View(); var presenter = new PayrollWorkspacePresenter(new Client("{\"resultados\":[{\"nombre\":\"Ana\"}]}"), view);
        await presenter.LoadAsync("2026-07");
        Assert.Equal(new[] { true, false }, view.Busy); Assert.Contains("Ana", view.Json);
    }
    [Fact]
    public async Task Failed_operation_shows_error_and_reenables_actions()
    {
        var view = new View(); var presenter = new PayrollWorkspacePresenter(new Client(error: new HttpRequestException("offline")), view);
        await presenter.CalculateAsync("2026-07");
        Assert.Equal(new[] { true, false }, view.Busy); Assert.Contains("offline", view.Error);
    }
    private sealed class View : IPayrollWorkspaceView { public List<bool> Busy { get; } = []; public string Json { get; private set; } = ""; public string Error { get; private set; } = ""; public void SetBusy(bool busy) => Busy.Add(busy); public void ShowPayroll(string json) => Json = json; public void ShowEmpty() { } public void ShowSuccess(string message) { } public void ShowError(string message) => Error = message; public Task SaveExportAsync(string extension, byte[] content) => Task.CompletedTask; }
    private sealed class Client(string json = "{\"resultados\":[]}", Exception? error = null) : IApiPayrollClient { public Task<string> GetAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(json); public Task<string> PostAsync(string path, object? body = null, CancellationToken cancellationToken = default) => error is null ? Task.FromResult("") : Task.FromException<string>(error); public Task<byte[]> DownloadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<byte>()); }
}
