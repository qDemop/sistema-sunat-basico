namespace ERP.Domain.Payroll;

public enum TipoDescuentoNombre
{
    Afp,
    Onp
}

public enum ConfigDescuentoPrevisionalEstado
{
    Draft,
    Active,
    Closed
}

public enum HorasExtraEstado
{
    Draft,
    Approved,
    Cancelled
}

public enum PeriodoPlanillaEstado
{
    None,
    Draft,
    Finalized,
    Cancelled
}
