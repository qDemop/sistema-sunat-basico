using ERP.Domain.Payroll;

namespace ERP.Domain.Tests.Payroll;

public class PeriodoPlanillaTests
{
    [Fact]
    public void None_Draft_Finalized_or_Cancelled_terminal()
    {
        Assert.True(PeriodoPlanilla.CanCreateFrom(PeriodoPlanillaEstado.None));

        var draft = PeriodoPlanilla.Create(1, "2026-07");
        draft.Finalize(7, DateTime.UtcNow);

        Assert.Equal(PeriodoPlanillaEstado.Finalized, draft.Estado);
        Assert.False(draft.CanRecalculate);
        Assert.Throws<DomainValidationException>(() => draft.Cancel());
    }

    [Fact]
    public void cancelled_period_is_terminal_and_draft_can_be_cancelled()
    {
        var draft = PeriodoPlanilla.Create(1, "2026-07");
        draft.Cancel();

        Assert.Equal(PeriodoPlanillaEstado.Cancelled, draft.Estado);
        Assert.False(draft.CanRecalculate);
        Assert.Throws<DomainValidationException>(() => draft.Finalize(7, DateTime.UtcNow));
    }
}
