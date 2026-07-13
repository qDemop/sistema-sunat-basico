using FluentValidation;
using ERP.Application.Abstractions;
using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Features.Payroll.Validators;

public sealed class EmpleadoInputValidator : AbstractValidator<EmpleadoInput>
{
    public EmpleadoInputValidator(ICurrentDate currentDate)
    {
        RuleFor(x => x.DepartamentoId).GreaterThan(0);
        RuleFor(x => x.TipoDescuentoId).GreaterThan(0);
        RuleFor(x => x.Dni).Matches("^[0-9]{8}$");
        RuleFor(x => x.Nombres).Matches("^[\\p{L} ]{2,100}$");
        RuleFor(x => x.Apellidos).Matches("^[\\p{L} ]{2,100}$");
        RuleFor(x => x.Cargo).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SalarioBase).GreaterThan(0);
        RuleFor(x => x.FechaNacimiento).LessThanOrEqualTo(currentDate.Today.AddYears(-18));
        RuleFor(x => x.FechaIngreso).LessThanOrEqualTo(currentDate.Today);
        RuleFor(x => x.Banco).NotEmpty().MaximumLength(80);
        RuleFor(x => x.NumeroCuenta).Matches("^[0-9]{14,20}$");
    }
}
