using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class EmpleadoTests
{
    [Fact]
    public void DNI_8_unique_names_letters_age_18_salary_pos_hire_today_account_14_20()
    {
        var employee = CreateValidEmployee(currentDate: new DateOnly(2026, 7, 11));

        Assert.Equal("12345678", employee.Dni);
        Assert.Equal("Ana María", employee.Nombres);
        Assert.Equal("Pérez Gómez", employee.Apellidos);
        Assert.True(employee.IsAtLeast18(new DateOnly(2026, 7, 11)));
        Assert.Equal(2500.00m, employee.SalarioBase);
        Assert.Equal(new DateOnly(2026, 7, 11), employee.FechaIngreso);
        Assert.Equal("12345678901234", employee.NumeroCuenta);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("1234567A")]
    public void rejects_invalid_DNI(string dni)
    {
        Assert.Throws<DomainValidationException>(() => CreateValidEmployee(dni: dni));
    }

    [Fact]
    public void rejects_underage_future_hire_nonpositive_salary_and_invalid_account()
    {
        var today = new DateOnly(2026, 7, 11);

        Assert.Throws<DomainValidationException>(() => CreateValidEmployee(currentDate: today, fechaNacimiento: today.AddYears(-17)));
        Assert.Throws<DomainValidationException>(() => CreateValidEmployee(currentDate: today, fechaIngreso: today.AddDays(1)));
        Assert.Throws<DomainValidationException>(() => CreateValidEmployee(currentDate: today, salarioBase: 0));
        Assert.Throws<DomainValidationException>(() => CreateValidEmployee(currentDate: today, numeroCuenta: "123"));
    }

    [Fact]
    public void accepts_exactly_18_and_hire_date_equal_to_explicit_current_date()
    {
        var currentDate = new DateOnly(2026, 7, 11);
        var employee = CreateValidEmployee(currentDate: currentDate, fechaNacimiento: currentDate.AddYears(-18), fechaIngreso: currentDate);

        Assert.True(employee.IsAtLeast18(currentDate));
        Assert.Equal(currentDate, employee.FechaIngreso);
    }

    private static Empleado CreateValidEmployee(
        string dni = "12345678",
        DateOnly? fechaNacimiento = null,
        DateOnly? fechaIngreso = null,
        decimal salarioBase = 2500.00m,
        string numeroCuenta = "12345678901234",
        DateOnly? currentDate = null)
    {
        var today = currentDate ?? new DateOnly(2026, 7, 11);
        return Empleado.Create(
            id: 1,
            departamentoId: 10,
            tipoDescuentoId: 20,
            dni: dni,
            nombres: "Ana María",
            apellidos: "Pérez Gómez",
            cargo: "Analista",
            salarioBase: salarioBase,
            fechaNacimiento: fechaNacimiento ?? today.AddYears(-18),
            fechaIngreso: fechaIngreso ?? today,
            banco: "Banco de Prueba",
            numeroCuenta: numeroCuenta,
            activo: true,
            currentDate: today);
    }
}
