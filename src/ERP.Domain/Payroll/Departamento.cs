namespace ERP.Domain.Payroll;

public sealed record Departamento
{
    public long Id { get; private init; }
    public string Nombre { get; private init; } = string.Empty;
    public string? Descripcion { get; private init; }
    public bool Activo { get; private init; }

    public static Departamento Create(long id, string nombre, string? descripcion, bool activo)
    {
        var normalizedName = NormalizeName(nombre, "Department name");
        if (descripcion?.Length > 250)
        {
            throw new DomainValidationException("Department description must not exceed 250 characters.");
        }

        return new Departamento { Id = id, Nombre = normalizedName, Descripcion = descripcion?.Trim(), Activo = activo };
    }

    public static void EnsureNameIsAvailable(string nombre, IEnumerable<Departamento> existing)
    {
        var candidate = NormalizeName(nombre, "Department name");
        if (existing.Any(department => string.Equals(department.Nombre, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainValidationException("Department name must be unique.");
        }
    }

    internal static string NormalizeName(string value, string fieldName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 100)
        {
            throw new DomainValidationException($"{fieldName} is required and must not exceed 100 characters.");
        }

        return normalized;
    }
}
