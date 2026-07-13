namespace ERP.WinForms.Services;

public sealed record LogoutResult(bool IsSuccess, string? CorrelationId = null);
