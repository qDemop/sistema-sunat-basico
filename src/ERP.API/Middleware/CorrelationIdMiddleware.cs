using System.Text.RegularExpressions;

namespace ERP.API.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    // Bounded safe format: alphanumeric, hyphens, underscores; 1-64 chars.
    private static readonly Regex CorrelationIdRegex = new(
        "^[a-zA-Z0-9_-]{1,64}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public const string CorrelationIdItemKey = "CorrelationId";
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request);
        context.Items[CorrelationIdItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    internal static string ResolveCorrelationId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            var candidate = headerValue.ToString();
            if (CorrelationIdRegex.IsMatch(candidate))
            {
                return candidate;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
