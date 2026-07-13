using ERP.Domain.Authentication;

namespace ERP.Domain.Tests.Authentication;

public class RolTests
{
    [Fact]
    public void Carga_nombre_descripcion_nivel_acceso()
    {
        var rol = Rol.Load(2, "Administrador RRHH", "Gestion de empleados", 2);

        Assert.Equal(2, rol.Id);
        Assert.Equal("Administrador RRHH", rol.Nombre);
        Assert.Equal("Gestion de empleados", rol.Descripcion);
        Assert.Equal(2, rol.NivelAcceso);
    }

    [Fact]
    public void Carga_rol_contador_nivel_mayor()
    {
        var rol = Rol.Load(3, "Contador", "Gestion contable", 3);

        Assert.Equal(3, rol.Id);
        Assert.Equal("Contador", rol.Nombre);
        Assert.Equal(3, rol.NivelAcceso);
    }
}
