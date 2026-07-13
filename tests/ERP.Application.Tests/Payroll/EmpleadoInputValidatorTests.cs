using ERP.Application.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Application.Features.Payroll.Validators;

namespace ERP.Application.Tests.Payroll;

public class EmpleadoInputValidatorTests
{
    [Fact]
    public void accepts_a_canonical_employee_input()
    {
        var result = new EmpleadoInputValidator(new FixedCurrentDate(new DateOnly(2026, 7, 11))).Validate(ValidInput());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void rejects_invalid_identity_dates_money_and_account()
    {
        var invalid = ValidInput() with
        {
            Dni = "123",
            Nombres = "A1",
            FechaNacimiento = new DateOnly(2009, 7, 12),
            FechaIngreso = new DateOnly(2026, 7, 12),
            SalarioBase = 0,
            NumeroCuenta = "123"
        };

        var result = new EmpleadoInputValidator(new FixedCurrentDate(new DateOnly(2026, 7, 11))).Validate(invalid);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 6);
    }

    [Fact]
    public void accepts_age_and_hire_date_exactly_on_explicit_current_date_boundary()
    {
        var currentDate = new DateOnly(2026, 7, 11);
        var input = ValidInput() with { FechaNacimiento = currentDate.AddYears(-18), FechaIngreso = currentDate };

        Assert.True(new EmpleadoInputValidator(new FixedCurrentDate(currentDate)).Validate(input).IsValid);
    }

    private static EmpleadoInput ValidInput() => new(
        10, 20, "12345678", "Ana María", "Pérez Gómez", "Analista", 2500m,
        new DateOnly(2008, 7, 11), new DateOnly(2026, 7, 11), "Banco de Prueba", "12345678901234");

    private sealed class FixedCurrentDate(DateOnly today) : ICurrentDate
    {
        public DateOnly Today => today;
    }
}
