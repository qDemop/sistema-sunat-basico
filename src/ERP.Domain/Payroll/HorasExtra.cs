namespace ERP.Domain.Payroll;

public sealed record HorasExtra
{
    public long Id { get; private init; }
    public long EmpleadoId { get; private init; }
    public string Periodo { get; private init; } = string.Empty;
    public decimal HorasPrimerasDos { get; private init; }
    public decimal HorasPosteriores { get; private init; }
    public HorasExtraEstado Estado { get; private set; }
    public DateTime? FechaAprobacion { get; private set; }
    public long? UsuarioAprobadorId { get; private set; }

    public static HorasExtra Create(long id, long empleadoId, string periodo, decimal horasPrimerasDos, decimal horasPosteriores)
    {
        PeriodoPlanilla.ValidatePeriodo(periodo);
        if (empleadoId <= 0 || horasPrimerasDos < 0 || horasPosteriores < 0 || horasPrimerasDos + horasPosteriores <= 0)
            throw new DomainValidationException("Overtime must belong to an employee and include positive hours.");

        return new HorasExtra { Id = id, EmpleadoId = empleadoId, Periodo = periodo, HorasPrimerasDos = horasPrimerasDos, HorasPosteriores = horasPosteriores, Estado = HorasExtraEstado.Draft };
    }

    public void Approve(long actorUserId, DateTime approvedAt)
    {
        if (Estado != HorasExtraEstado.Draft || actorUserId <= 0) throw new DomainValidationException("Only Draft overtime can be approved by a valid user.");
        Estado = HorasExtraEstado.Approved;
        UsuarioAprobadorId = actorUserId;
        FechaAprobacion = approvedAt;
    }

    public void Cancel(long actorUserId)
    {
        if (actorUserId <= 0 || Estado is not (HorasExtraEstado.Draft or HorasExtraEstado.Approved))
            throw new DomainValidationException("Only Draft or Approved overtime can be cancelled by a valid user.");
        Estado = HorasExtraEstado.Cancelled;
    }
}
