using ERP.Application.Features.Payroll.Contracts;
using ERP.Infrastructure.Persistence.Payroll;
using Npgsql;

namespace ERP.Infrastructure.Tests;

public sealed class PayrollCalculationErrorTranslatorTests
{
    [Theory]
    [InlineData("23505", "duplicate key", PayrollOperationError.StateConflict)]
    [InlineData("P0001", "Invalid period: 2026-99", PayrollOperationError.InvalidPeriod)]
    [InlineData("P0001", "No active employees exist for payroll period 2026-07", PayrollOperationError.NoActiveEmployees)]
    [InlineData("P0001", "No Active pension configuration for employee 4 at 2026-07-31", PayrollOperationError.MissingPensionConfiguration)]
    public void Known_database_failures_map_to_safe_operation_codes(string sqlState, string message, PayrollOperationError expected)
    {
        var result = PayrollCalculationErrorTranslator.Translate(new PostgresException(message, "ERROR", "ERROR", sqlState));

        Assert.NotNull(result);
        Assert.Equal(expected, result.Error);
    }

    [Theory]
    [InlineData("Payroll period must exist in Draft state")]
    [InlineData("Payroll period must be Draft")]
    public void Terminal_lifecycle_database_failures_map_to_state_conflict(string message)
    {
        var result = PayrollCalculationErrorTranslator.Translate(new PostgresException(message, "ERROR", "ERROR", "P0001"));

        Assert.NotNull(result);
        Assert.Equal(PayrollOperationError.StateConflict, result.Error);
    }

    [Theory]
    [InlineData("An Open accounting period 2026-07 is required")]
    [InlineData("Canonical payroll mapping produced an unbalanced entry")]
    public void Accounting_prerequisite_failures_map_to_validation_or_configuration(string message)
    {
        var result = PayrollCalculationErrorTranslator.Translate(new PostgresException(message, "ERROR", "ERROR", "P0001"));

        Assert.NotNull(result);
        Assert.Equal(PayrollOperationError.ValidationOrConfiguration, result.Error);
    }

    [Theory]
    [InlineData("Actor is not authorized to finalize payroll")]
    [InlineData("Actor is not authorized to cancel payroll")]
    public void Canonical_procedure_authorization_failures_map_to_forbidden(string message)
    {
        var result = PayrollCalculationErrorTranslator.Translate(new PostgresException(message, "ERROR", "ERROR", "P0001"));

        Assert.NotNull(result);
        Assert.Equal(PayrollOperationError.Forbidden, result.Error);
    }

    [Theory]
    [InlineData("42501", "permission denied for procedure sp_calcular_planilla")]
    [InlineData("XX000", "unexpected database failure")]
    public void Unknown_or_authorization_database_failures_are_not_reclassified(string sqlState, string message)
    {
        Assert.Null(PayrollCalculationErrorTranslator.Translate(new PostgresException(message, "ERROR", "ERROR", sqlState)));
    }
}
