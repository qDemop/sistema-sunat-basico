namespace ERP.API.Contracts;

public sealed record ErrorResponse(
    int Status,
    string Code,
    string Message,
    string CorrelationId,
    IReadOnlyList<ValidationError>? ValidationErrors = null);

public sealed record ValidationError(string Field, string Code, string Message);
