namespace ERP.Application.Features.Payroll.Contracts;

public sealed record PayrollOperationContext(string Periodo, long ActorUserId, string CorrelationId);

public sealed record OvertimeOperationContext(long HorasExtraId, long ActorUserId, string CorrelationId);
