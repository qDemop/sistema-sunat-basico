namespace ERP.WinForms.Services;

public sealed class CorrelationContext : ICorrelationContext
{
    public string NewCorrelationId() => Guid.NewGuid().ToString("N");
}
