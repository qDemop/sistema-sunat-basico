namespace ERP.WinForms.Presenters;

public static class PayrollNavigationPolicy
{
    public static bool CanOpen(string? role) => role is "Administrador RRHH" or "Administrador Sistema";
}
