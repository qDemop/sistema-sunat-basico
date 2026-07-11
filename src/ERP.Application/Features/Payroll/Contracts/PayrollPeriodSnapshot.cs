using ERP.Domain.Payroll;

namespace ERP.Application.Features.Payroll.Contracts;

public sealed record PayrollPeriodSnapshot(long Id, string Periodo, PeriodoPlanillaEstado Estado, decimal TotalBruto,
    decimal TotalDescuentos, decimal TotalNeto, decimal TotalProvisionGratificacion, decimal TotalProvisionCts);
