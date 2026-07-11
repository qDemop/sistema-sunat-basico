using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class PensionResolverTests
{
    [Fact]
    public void Active_version_at_period_end_in_Detalle()
    {
        var type = TipoDescuento.Create(1, "AFP", null, true);
        var version = ConfigDescuentoPrevisionalVersion.Create(7, type.Id, 2, 10m,
            new DateOnly(2026, 1, 1), null, ConfigDescuentoPrevisionalEstado.Active);

        var resolved = PensionVersionResolver.ResolveForPeriodEnd(
            new[] { version }, type.Id, "2026-07");

        var detail = DetallePlanilla.Create(1, resolved.Id, 80m, 308m, 0m, 299.44m, 513.33m);

        Assert.Equal(7, resolved.Id);
        Assert.Equal(7, detail.ConfigDescuentoVersionId);
        Assert.Equal(308m, detail.Afp);
    }

    [Fact]
    public void rejects_missing_or_overlapping_active_versions()
    {
        Assert.Throws<DomainValidationException>(() =>
            PensionVersionResolver.ResolveForPeriodEnd(Array.Empty<ConfigDescuentoPrevisionalVersion>(), 1, "2026-07"));

        var first = ConfigDescuentoPrevisionalVersion.Create(1, 1, 1, 10m,
            new DateOnly(2026, 1, 1), null, ConfigDescuentoPrevisionalEstado.Active);
        var second = ConfigDescuentoPrevisionalVersion.Create(2, 1, 2, 11m,
            new DateOnly(2026, 6, 1), null, ConfigDescuentoPrevisionalEstado.Active);

        Assert.Throws<DomainValidationException>(() =>
            PensionVersionResolver.ResolveForPeriodEnd(new[] { first, second }, 1, "2026-07"));
    }

    [Fact]
    public void resolves_only_matching_type_active_version_at_inclusive_period_end()
    {
        var activeAtEnd = ConfigDescuentoPrevisionalVersion.Create(3, 1, 1, 10m, new DateOnly(2026, 1, 1), new DateOnly(2026, 7, 31), ConfigDescuentoPrevisionalEstado.Active);
        var wrongType = ConfigDescuentoPrevisionalVersion.Create(4, 2, 1, 13m, new DateOnly(2026, 1, 1), null, ConfigDescuentoPrevisionalEstado.Active);
        var draft = ConfigDescuentoPrevisionalVersion.Create(5, 1, 2, 11m, new DateOnly(2026, 1, 1), null, ConfigDescuentoPrevisionalEstado.Draft);
        var closed = ConfigDescuentoPrevisionalVersion.Create(6, 1, 3, 12m, new DateOnly(2026, 1, 1), null, ConfigDescuentoPrevisionalEstado.Closed);

        var resolved = PensionVersionResolver.ResolveForPeriodEnd(new[] { activeAtEnd, wrongType, draft, closed }, 1, "2026-07");

        Assert.Equal(activeAtEnd.Id, resolved.Id);
        Assert.Equal(10m, resolved.Porcentaje);
    }

    [Theory]
    [InlineData("2026-00")]
    [InlineData("2026-13")]
    [InlineData("July-2026")]
    public void rejects_invalid_payroll_period(string period)
    {
        Assert.Throws<DomainValidationException>(() => PensionVersionResolver.ResolveForPeriodEnd(Array.Empty<ConfigDescuentoPrevisionalVersion>(), 1, period));
    }
}
