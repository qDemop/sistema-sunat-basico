using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.Application.Security;

public static class RoleModuleMapping
{
    public static readonly IReadOnlyList<string> AllModules = new[]
    {
        "Authentication",
        "Payroll",
        "AccountingSUNAT",
        "GeneralLedger",
        "Reports",
        "Administration"
    };

    private static readonly Dictionary<string, IReadOnlyList<string>> _mapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Administrador RRHH", new[] { "Authentication", "Payroll" } },
        { "Contador", new[] { "Authentication", "AccountingSUNAT", "GeneralLedger" } },
        { "Gerente Financiero", new[] { "Authentication", "GeneralLedger", "Reports" } },
        { "Administrador Sistema", AllModules }
    };

    public static IReadOnlyList<string> GetVisibleModules(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Array.Empty<string>();
        }

        return _mapping.TryGetValue(role, out var modules)
            ? modules
            : Array.Empty<string>();
    }

    public static bool IsKnownRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role) && _mapping.ContainsKey(role);
    }

    public static IReadOnlyList<string> GetKnownRoles()
    {
        return _mapping.Keys.ToList().AsReadOnly();
    }
}
