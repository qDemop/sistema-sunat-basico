using ERP.Domain.Payroll;

namespace ERP.Application.Features.Payroll.Contracts;

public sealed record PayrollPeriodSnapshot(long Id, string Periodo, PeriodoPlanillaEstado Estado, decimal TotalBruto,
    decimal TotalDescuentos, decimal TotalNeto, decimal TotalProvisionGratificacion, decimal TotalProvisionCts)
{
    public DateTimeOffset FechaCalculo { get; init; }
    public DateTimeOffset? FechaFinalizacion { get; init; }
    public long? FinalizadoPorId { get; init; }
    public long? AsientoDraftId { get; init; }
    public IReadOnlyList<PayrollEmployeeResultSnapshot> Resultados { get; init; } = [];
    public decimal CostoPlanilla => TotalBruto + TotalProvisionGratificacion + TotalProvisionCts;
}

public sealed record PayrollEmployeeResultSnapshot(long EmpleadoId, string Nombre, long DepartamentoId, string Departamento,
    decimal SalarioBase, decimal HorasExtraMonto, decimal TotalBruto, string TipoDescuento, long ConfigDescuentoVersionId,
    decimal ConfigDescuentoPorcentaje, decimal Afp, decimal Onp, decimal DescuentosAdicionales, decimal TotalDescuentos,
    decimal TotalNeto, decimal ProvisionGratificacion, decimal ProvisionCts, decimal CostoTotal);

public sealed record PayrollDashboardSnapshot(string Periodo, int EmpleadosActivos, int EmpleadosElegibles,
    int DatosIncompletos, string PeriodoEstado, decimal TotalBruto, decimal TotalNeto, decimal CostoPlanilla);
