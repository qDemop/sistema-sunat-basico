namespace ERP.Domain.Payroll;

public sealed record TipoDescuento
{
    public long Id { get; private init; }
    public TipoDescuentoNombre Nombre { get; private init; }
    public string? Descripcion { get; private init; }
    public bool Activo { get; private init; }

    public static TipoDescuento Create(long id, string nombre, string? descripcion, bool activo)
    {
        if (!Enum.TryParse<TipoDescuentoNombre>(nombre, true, out var regimen))
        {
            throw new DomainValidationException("Discount type must be AFP or ONP.");
        }

        return new TipoDescuento { Id = id, Nombre = regimen, Descripcion = descripcion?.Trim(), Activo = activo };
    }
}
