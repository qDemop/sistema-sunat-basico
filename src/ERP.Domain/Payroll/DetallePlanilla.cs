namespace ERP.Domain.Payroll;

public sealed record DetallePlanilla
{
    public long PlanillaId { get; private init; }
    public long ConfigDescuentoVersionId { get; private init; }
    public decimal HorasExtraTotal { get; private init; }
    public decimal Afp { get; private init; }
    public decimal Onp { get; private init; }
    public decimal ProvisionCts { get; private init; }
    public decimal ProvisionGratificacion { get; private init; }
    public decimal DescuentosAdicionales { get; private init; }

    public static DetallePlanilla Create(long planillaId, long configDescuentoVersionId, decimal horasExtraTotal, decimal afp,
        decimal onp, decimal provisionCts, decimal provisionGratificacion)
    {
        if (planillaId <= 0 || configDescuentoVersionId <= 0 || new[] { horasExtraTotal, afp, onp, provisionCts, provisionGratificacion }.Any(value => value < 0))
            throw new DomainValidationException("Payroll detail amounts are invalid.");
        if (afp > 0 && onp > 0) throw new DomainValidationException("Payroll detail cannot apply AFP and ONP together.");
        return new DetallePlanilla { PlanillaId = planillaId, ConfigDescuentoVersionId = configDescuentoVersionId, HorasExtraTotal = horasExtraTotal,
            Afp = afp, Onp = onp, ProvisionCts = provisionCts, ProvisionGratificacion = provisionGratificacion, DescuentosAdicionales = 0m };
    }
}
