using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using MediatR;

namespace ERP.Application.Features.Payroll.Commands;

public sealed record CalculatePayrollCommand(string Periodo, long ActorUserId, string CorrelationId) : IRequest<PayrollPeriodSnapshot>;

public sealed class CalculatePayrollCommandHandler(IPayrollRepository repository) : IRequestHandler<CalculatePayrollCommand, PayrollPeriodSnapshot>
{
    public async Task<PayrollPeriodSnapshot> Handle(CalculatePayrollCommand request, CancellationToken cancellationToken)
    {
        await repository.CalculateAsync(new PayrollOperationContext(request.Periodo, request.ActorUserId, request.CorrelationId), cancellationToken);
        return await repository.GetByPeriodAsync(request.Periodo, cancellationToken)
            ?? throw new PayrollOperationException(PayrollOperationError.NotFound);
    }
}
