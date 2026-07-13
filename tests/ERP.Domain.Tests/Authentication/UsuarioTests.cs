using ERP.Domain.Authentication;

namespace ERP.Domain.Tests.Authentication;

public class UsuarioTests
{
    [Fact]
    public void Carga_desde_fila_persiste_invariantes()
    {
        var rol = Rol.Load(2, "Administrador RRHH", "Gestion de recursos humanos", 2);

        var usuario = Usuario.Load(
            id: 1,
            username: "jdoe",
            passwordHash: "$2a$11$hashed",
            nombreCompleto: "John Doe",
            rol: rol,
            activo: true,
            intentosFallidos: 0,
            bloqueadoHasta: null);

        Assert.Equal(1, usuario.Id);
        Assert.Equal("jdoe", usuario.Username);
        Assert.Equal("$2a$11$hashed", usuario.PasswordHash);
        Assert.Equal("John Doe", usuario.NombreCompleto);
        Assert.Equal(rol, usuario.Rol);
        Assert.True(usuario.Activo);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
    }

    [Fact]
    public void Carga_usuario_bloqueado_persiste_fecha_bloqueo()
    {
        var lockout = DateTime.UtcNow.AddMinutes(15);
        var rol = Rol.Load(3, "Contador", "Gestion contable", 3);

        var usuario = Usuario.Load(
            id: 5,
            username: "conta1",
            passwordHash: "hash",
            nombreCompleto: "Carlos Ruiz",
            rol: rol,
            activo: true,
            intentosFallidos: 3,
            bloqueadoHasta: lockout);

        Assert.Equal(5, usuario.Id);
        Assert.Equal("conta1", usuario.Username);
        Assert.Equal(rol, usuario.Rol);
        Assert.Equal(3, usuario.IntentosFallidos);
        Assert.Equal(lockout, usuario.BloqueadoHasta);
    }
}
