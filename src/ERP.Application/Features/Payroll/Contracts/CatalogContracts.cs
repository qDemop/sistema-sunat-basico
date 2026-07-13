namespace ERP.Application.Features.Payroll.Contracts;

public sealed record DepartamentoInput(string Nombre, string? Descripcion);
public sealed record DepartamentoSnapshot(long Id, DepartamentoInput Input, bool Activo, int EmpleadosActivos);

public sealed record HorasExtraInput(long Id, long EmpleadoId, string Periodo, decimal HorasPrimerasDos, decimal HorasPosteriores);
public sealed record HorasExtraSnapshot(long Id, HorasExtraInput Input, string Estado, DateTimeOffset FechaRegistro, DateTimeOffset? FechaAprobacion, long? AprobadoPorId);

public sealed record EmpleadoSnapshot(long Id, EmpleadoInput Input, bool Activo, string Departamento, string TipoDescuento, DateTimeOffset FechaCreacion);

public enum CatalogCommandStatus { Success, NotFound, Conflict }
public sealed record CatalogCommandResult(CatalogCommandStatus Status, bool Found, bool Changed)
{
    public static CatalogCommandResult Success() => new(CatalogCommandStatus.Success, true, true);
    public static CatalogCommandResult NotFound() => new(CatalogCommandStatus.NotFound, false, false);
    public static CatalogCommandResult Conflict() => new(CatalogCommandStatus.Conflict, true, false);
}

public sealed class CatalogConflictException : Exception
{
    public CatalogConflictException(string message) : base(message) { }
}
