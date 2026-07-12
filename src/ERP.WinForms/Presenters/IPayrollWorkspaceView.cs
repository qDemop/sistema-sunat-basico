namespace ERP.WinForms.Presenters;

public interface IPayrollWorkspaceView
{
    void SetBusy(bool busy);
    void ShowPayroll(string json);
    void ShowEmpty();
    void ShowSuccess(string message);
    void ShowError(string message);
    Task SaveExportAsync(string extension, byte[] content);
}
