using System;

namespace ERP.Application.Features.Authentication;

public sealed record UserAuthenticationData(
    long Id,
    string Username,
    string PasswordHash,
    string NombreCompleto,
    string Rol,
    bool Activo,
    int IntentosFallidos,
    DateTime? BloqueadoHasta);
