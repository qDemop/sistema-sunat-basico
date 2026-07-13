namespace ERP.Application.Features.Payroll.Contracts;

public sealed record EmpleadoInput(long DepartamentoId, long TipoDescuentoId, string Dni, string Nombres, string Apellidos,
    string Cargo, decimal SalarioBase, DateOnly FechaNacimiento, DateOnly FechaIngreso, string Banco, string NumeroCuenta);
