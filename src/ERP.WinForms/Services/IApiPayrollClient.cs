namespace ERP.WinForms.Services;

public interface IApiPayrollClient
{
    Task<string> GetAsync(string path, CancellationToken cancellationToken = default);
    Task<string> PostAsync(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadAsync(string path, CancellationToken cancellationToken = default);
}
