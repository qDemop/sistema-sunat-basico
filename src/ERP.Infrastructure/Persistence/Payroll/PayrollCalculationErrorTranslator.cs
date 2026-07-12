using ERP.Application.Features.Payroll.Contracts;
using Npgsql;

namespace ERP.Infrastructure.Persistence.Payroll;

public static class PayrollCalculationErrorTranslator
{
    public static PayrollOperationException? Translate(PostgresException exception)
    {
        if (exception.SqlState == "23505") return new PayrollOperationException(PayrollOperationError.StateConflict);
        if (exception.SqlState != "P0001") return null;

        return exception.MessageText switch
        {
            var message when message.StartsWith("Invalid period:", StringComparison.Ordinal) => new PayrollOperationException(PayrollOperationError.InvalidPeriod),
            var message when message.StartsWith("No active employees exist", StringComparison.Ordinal) => new PayrollOperationException(PayrollOperationError.NoActiveEmployees),
            var message when message.StartsWith("No Active pension configuration", StringComparison.Ordinal) => new PayrollOperationException(PayrollOperationError.MissingPensionConfiguration),
            var message when message.Contains("cannot be recalculated", StringComparison.OrdinalIgnoreCase) => new PayrollOperationException(PayrollOperationError.StateConflict),
            _ => null
        };
    }
}
