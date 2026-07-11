using System.Globalization;

namespace ERP.Domain.Payroll;

public sealed record PeriodoPlanilla
{
    public long Id { get; private init; }
    public string Periodo { get; private init; } = string.Empty;
    public PeriodoPlanillaEstado Estado { get; private set; }
    public DateTime? FechaFinalizacion { get; private set; }
    public long? UsuarioFinalizadorId { get; private set; }
    public bool CanRecalculate => Estado == PeriodoPlanillaEstado.Draft;

    public static PeriodoPlanilla Create(long id, string periodo)
    {
        ValidatePeriodo(periodo);
        return new PeriodoPlanilla { Id = id, Periodo = periodo, Estado = PeriodoPlanillaEstado.Draft };
    }

    public static bool CanCreateFrom(PeriodoPlanillaEstado state) => state == PeriodoPlanillaEstado.None;

    public void Finalize(long actorUserId, DateTime finalizedAt)
    {
        EnsureDraft();
        if (actorUserId <= 0) throw new DomainValidationException("Finalization requires a valid user.");
        Estado = PeriodoPlanillaEstado.Finalized;
        UsuarioFinalizadorId = actorUserId;
        FechaFinalizacion = finalizedAt;
    }

    public void Cancel()
    {
        EnsureDraft();
        Estado = PeriodoPlanillaEstado.Cancelled;
    }

    public static void ValidatePeriodo(string periodo)
    {
        if (!DateOnly.TryParseExact($"{periodo}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new DomainValidationException("Payroll period must use YYYY-MM format.");
    }

    private void EnsureDraft()
    {
        if (Estado != PeriodoPlanillaEstado.Draft) throw new DomainValidationException("Only Draft payroll periods can transition.");
    }
}
