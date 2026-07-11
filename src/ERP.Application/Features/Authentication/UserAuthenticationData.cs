using System;

namespace ERP.Application.Features.Authentication;

[Obsolete("Use ERP.Domain.Authentication.Usuario via GetByUsernameWithRoleAsync. Kept for one-slice compatibility.")]
public sealed record UserAuthenticationData(
    long Id,
    string Username,
    string PasswordHash,
    string NombreCompleto,
    string Rol,
    bool Activo,
    int IntentosFallidos,
    DateTime? BloqueadoHasta);
