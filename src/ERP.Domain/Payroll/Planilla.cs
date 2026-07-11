namespace ERP.Domain.Payroll;

public sealed record PayrollCalculation(decimal TotalBruto, decimal ProvisionGratificacion, decimal ProvisionCts, decimal TotalDescuentos, decimal TotalNeto, decimal MontoHorasExtra);

public sealed record Planilla
{
    public long Id { get; private init; }
    public long PeriodoPlanillaId { get; private init; }
    public long EmpleadoId { get; private init; }
    public long DepartamentoId { get; private init; }
    public decimal SalarioBaseAplicado { get; private init; }
    public decimal TotalBruto { get; private init; }
    public decimal TotalDescuentos { get; private init; }
    public decimal TotalNeto { get; private init; }

    public static Planilla Create(long id, long periodoPlanillaId, long empleadoId, long departamentoId,
        decimal salarioBaseAplicado, PayrollCalculation calculation)
    {
        if (id <= 0 || periodoPlanillaId <= 0 || empleadoId <= 0 || departamentoId <= 0 || salarioBaseAplicado <= 0)
            throw new DomainValidationException("Payroll result identifiers and applied base salary are required.");
        if (calculation.TotalBruto < 0 || calculation.TotalDescuentos < 0 || calculation.TotalNeto != calculation.TotalBruto - calculation.TotalDescuentos)
            throw new DomainValidationException("Payroll result must satisfy the net pay equation.");

        return new Planilla
        {
            Id = id,
            PeriodoPlanillaId = periodoPlanillaId,
            EmpleadoId = empleadoId,
            DepartamentoId = departamentoId,
            SalarioBaseAplicado = salarioBaseAplicado,
            TotalBruto = calculation.TotalBruto,
            TotalDescuentos = calculation.TotalDescuentos,
            TotalNeto = calculation.TotalNeto
        };
    }

    public static PayrollCalculation Calculate(decimal salarioBase, decimal horasPrimerasDos, decimal horasPosteriores, decimal porcentajePension)
    {
        if (salarioBase <= 0 || horasPrimerasDos < 0 || horasPosteriores < 0 || porcentajePension is < 0 or > 100)
            throw new DomainValidationException("Payroll calculation inputs are invalid.");

        var hourlyRate = salarioBase / 240m;
        var overtime = Round((horasPrimerasDos * hourlyRate * 1.25m) + (horasPosteriores * hourlyRate * 1.35m));
        var gross = Round(salarioBase + overtime);
        var gratuityProvision = Round(gross / 6m);
        var ctsProvision = Round((gross + gratuityProvision) / 12m);
        var discounts = Round(gross * porcentajePension / 100m);
        return new PayrollCalculation(gross, gratuityProvision, ctsProvision, discounts, Round(gross - discounts), overtime);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
