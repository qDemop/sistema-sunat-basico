namespace ERP.Application.Features.Authentication;

public sealed record LogoutResponse(bool Revoked, DateTime ExpiresAt, string CorrelationId);
