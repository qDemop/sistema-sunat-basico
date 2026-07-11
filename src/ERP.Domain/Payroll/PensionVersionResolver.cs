using System.Globalization;

namespace ERP.Domain.Payroll;

public static class PensionVersionResolver
{
    public static ConfigDescuentoPrevisionalVersion ResolveForPeriodEnd(
        IEnumerable<ConfigDescuentoPrevisionalVersion> versions, long tipoDescuentoId, string periodo)
    {
        PeriodoPlanilla.ValidatePeriodo(periodo);
        var periodStart = DateOnly.ParseExact($"{periodo}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var candidates = versions.Where(v => v.TipoDescuentoId == tipoDescuentoId && v.IsActiveAt(periodEnd)).ToList();
        return candidates.Count == 1 ? candidates[0] : throw new DomainValidationException("Exactly one active pension configuration is required at period end.");
    }
}
