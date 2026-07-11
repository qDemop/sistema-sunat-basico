namespace ERP.Domain.Authentication;

public sealed record Usuario
{
    public long Id { get; private init; }
    public string Username { get; private init; } = string.Empty;
    public string PasswordHash { get; private init; } = string.Empty;
    public string NombreCompleto { get; private init; } = string.Empty;
    public Rol Rol { get; private init; } = null!;
    public bool Activo { get; private init; }
    public int IntentosFallidos { get; private init; }
    public DateTime? BloqueadoHasta { get; private init; }

    public static Usuario Load(
        long id,
        string username,
        string passwordHash,
        string nombreCompleto,
        Rol rol,
        bool activo,
        int intentosFallidos,
        DateTime? bloqueadoHasta)
    {
        return new Usuario
        {
            Id = id,
            Username = username,
            PasswordHash = passwordHash,
            NombreCompleto = nombreCompleto,
            Rol = rol,
            Activo = activo,
            IntentosFallidos = intentosFallidos,
            BloqueadoHasta = bloqueadoHasta
        };
    }
}
