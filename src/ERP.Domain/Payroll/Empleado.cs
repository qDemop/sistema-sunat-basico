using System.Text.RegularExpressions;

namespace ERP.Domain.Payroll;

public sealed record Empleado
{
    private static readonly Regex DniPattern = new("^[0-9]{8}$", RegexOptions.Compiled);
    private static readonly Regex NamePattern = new("^[\\p{L} ]{2,100}$", RegexOptions.Compiled);
    private static readonly Regex AccountPattern = new("^[0-9]{14,20}$", RegexOptions.Compiled);

    public long Id { get; private init; }
    public long DepartamentoId { get; private init; }
    public long TipoDescuentoId { get; private init; }
    public string Dni { get; private init; } = string.Empty;
    public string Nombres { get; private init; } = string.Empty;
    public string Apellidos { get; private init; } = string.Empty;
    public string Cargo { get; private init; } = string.Empty;
    public decimal SalarioBase { get; private init; }
    public DateOnly FechaNacimiento { get; private init; }
    public DateOnly FechaIngreso { get; private init; }
    public string Banco { get; private init; } = string.Empty;
    public string NumeroCuenta { get; private init; } = string.Empty;
    public bool Activo { get; private init; }

    public static Empleado Create(long id, long departamentoId, long tipoDescuentoId, string dni, string nombres,
        string apellidos, string cargo, decimal salarioBase, DateOnly fechaNacimiento, DateOnly fechaIngreso,
        string banco, string numeroCuenta, bool activo, DateOnly currentDate)
    {
        var normalizedDni = dni?.Trim() ?? string.Empty;
        var normalizedNames = nombres?.Trim() ?? string.Empty;
        var normalizedSurnames = apellidos?.Trim() ?? string.Empty;
        var normalizedJobTitle = cargo?.Trim() ?? string.Empty;
        var normalizedBank = banco?.Trim() ?? string.Empty;
        var normalizedAccount = numeroCuenta?.Trim() ?? string.Empty;
        if (departamentoId <= 0 || tipoDescuentoId <= 0) throw new DomainValidationException("Department and discount type are required.");
        if (!DniPattern.IsMatch(normalizedDni)) throw new DomainValidationException("DNI must contain exactly 8 digits.");
        if (!NamePattern.IsMatch(normalizedNames) || !NamePattern.IsMatch(normalizedSurnames)) throw new DomainValidationException("Names and surnames must contain letters and spaces only.");
        if (fechaNacimiento > currentDate.AddYears(-18)) throw new DomainValidationException("Employee must be at least 18 years old.");
        if (fechaIngreso > currentDate) throw new DomainValidationException("Hire date cannot be in the future.");
        if (salarioBase <= 0) throw new DomainValidationException("Base salary must be greater than zero.");
        if (string.IsNullOrWhiteSpace(normalizedJobTitle) || string.IsNullOrWhiteSpace(normalizedBank)) throw new DomainValidationException("Job title and bank are required.");
        if (!AccountPattern.IsMatch(normalizedAccount)) throw new DomainValidationException("Bank account must contain between 14 and 20 digits.");

        return new Empleado { Id = id, DepartamentoId = departamentoId, TipoDescuentoId = tipoDescuentoId, Dni = normalizedDni,
            Nombres = normalizedNames, Apellidos = normalizedSurnames, Cargo = normalizedJobTitle, SalarioBase = salarioBase,
            FechaNacimiento = fechaNacimiento, FechaIngreso = fechaIngreso, Banco = normalizedBank, NumeroCuenta = normalizedAccount, Activo = activo };
    }

    public bool IsAtLeast18(DateOnly asOf) => FechaNacimiento <= asOf.AddYears(-18);
}
