using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class DepartamentoTests
{
    [Fact]
    public void rejects_duplicate_name()
    {
        var existing = Departamento.Create(1, "Recursos Humanos", "People operations", true);

        Assert.Throws<DomainValidationException>(() =>
            Departamento.EnsureNameIsAvailable(" recursos humanos ", new[] { existing }));
    }

    [Fact]
    public void allows_a_distinct_normalized_name()
    {
        var existing = Departamento.Create(1, "Recursos Humanos", null, true);

        Departamento.EnsureNameIsAvailable("Contabilidad", new[] { existing });
    }
}
