using System;

namespace ERP.Application.Features.Authentication;

public sealed record AuthStateUpdateResult(
    int IntentosFallidos,
    DateTime? BloqueadoHasta,
    bool LockoutTriggered);