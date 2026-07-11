namespace ERP.Application.Abstractions;

public interface ICurrentDate
{
    DateOnly Today { get; }
}

public sealed class SystemCurrentDate : ICurrentDate
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
