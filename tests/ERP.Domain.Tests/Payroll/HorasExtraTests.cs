using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class HorasExtraTests
{
    [Fact]
    public void Approve_only_Draft_and_cancel_requires_valid_actor()
    {
        var overtime = HorasExtra.Create(1, 10, "2026-07", 2, 3);

        overtime.Approve(99, new DateTime(2026, 7, 11));

        Assert.Equal(HorasExtraEstado.Approved, overtime.Estado);
        Assert.Equal(99, overtime.UsuarioAprobadorId);
        Assert.NotNull(overtime.FechaAprobacion);
        Assert.Throws<DomainValidationException>(() => overtime.Approve(99, new DateTime(2026, 7, 12)));
        Assert.Throws<DomainValidationException>(() => overtime.Cancel(0));
        overtime.Cancel(88);
        Assert.Equal(HorasExtraEstado.Cancelled, overtime.Estado);
        Assert.Throws<DomainValidationException>(() => overtime.Cancel(88));
    }

    [Fact]
    public void rejects_invalid_actor_and_cannot_approve_after_cancellation()
    {
        var overtime = HorasExtra.Create(1, 10, "2026-07", 2, 0);

        Assert.Throws<DomainValidationException>(() => overtime.Approve(0, new DateTime(2026, 7, 11)));
        overtime.Cancel(99);
        Assert.Throws<DomainValidationException>(() => overtime.Approve(99, new DateTime(2026, 7, 11)));
    }
}
