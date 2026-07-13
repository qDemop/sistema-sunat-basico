namespace ERP.Domain.Payroll;

public sealed class DomainValidationException : InvalidOperationException
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
