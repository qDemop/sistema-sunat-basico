namespace ERP.Domain.Authentication;

public sealed record Rol
{
    public long Id { get; private init; }
    public string Nombre { get; private init; } = string.Empty;
    public string Descripcion { get; private init; } = string.Empty;
    public int NivelAcceso { get; private init; }

    public static Rol Load(long id, string nombre, string descripcion, int nivelAcceso)
    {
        return new Rol
        {
            Id = id,
            Nombre = nombre,
            Descripcion = descripcion,
            NivelAcceso = nivelAcceso
        };
    }
}
