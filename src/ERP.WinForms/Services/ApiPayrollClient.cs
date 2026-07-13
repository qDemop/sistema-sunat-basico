using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ERP.WinForms.Services;

public sealed class ApiPayrollClient(IHttpClientFactory clients, ISessionContext session, ICorrelationContext correlation) : IApiPayrollClient
{
    public Task<string> GetAsync(string path, CancellationToken cancellationToken = default) => SendAsync(HttpMethod.Get, path, null, cancellationToken);
    public Task<string> PostAsync(string path, object? body = null, CancellationToken cancellationToken = default) => SendAsync(HttpMethod.Post, path, body, cancellationToken);
    public async Task<byte[]> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await RequestAsync(HttpMethod.Get, path, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
    private async Task<string> SendAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var response = await RequestAsync(method, path, body, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
    private async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        request.Headers.Add("X-Correlation-ID", correlation.NewCorrelationId());
        return await clients.CreateClient("ERP.API").SendAsync(request, ct);
    }
}
