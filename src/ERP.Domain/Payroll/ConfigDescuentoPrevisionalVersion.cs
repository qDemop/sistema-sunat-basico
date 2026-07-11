namespace ERP.Domain.Payroll;

public sealed record ConfigDescuentoPrevisionalVersion
{
    public long Id { get; private init; }
    public long TipoDescuentoId { get; private init; }
    public int Version { get; private init; }
    public decimal Porcentaje { get; private init; }
    public DateOnly FechaInicio { get; private init; }
    public DateOnly? FechaFin { get; private init; }
    public ConfigDescuentoPrevisionalEstado Estado { get; private init; }

    public static ConfigDescuentoPrevisionalVersion Create(long id, long tipoDescuentoId, int version, decimal porcentaje,
        DateOnly fechaInicio, DateOnly? fechaFin, ConfigDescuentoPrevisionalEstado estado)
    {
        if (tipoDescuentoId <= 0 || version <= 0 || porcentaje is < 0 or > 100 || (fechaFin.HasValue && fechaFin < fechaInicio))
            throw new DomainValidationException("Pension configuration version is invalid.");
        return new ConfigDescuentoPrevisionalVersion { Id = id, TipoDescuentoId = tipoDescuentoId, Version = version,
            Porcentaje = porcentaje, FechaInicio = fechaInicio, FechaFin = fechaFin, Estado = estado };
    }

    public bool IsActiveAt(DateOnly date) => Estado == ConfigDescuentoPrevisionalEstado.Active && FechaInicio <= date && (!FechaFin.HasValue || FechaFin >= date);
}
