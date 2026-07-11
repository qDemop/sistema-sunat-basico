namespace ERP.Application.Features.Authentication;

public sealed class AuthenticationException : Exception
{
    public DateTime? AccountLockedUntil { get; }

    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, DateTime accountLockedUntil) : base(message)
    {
        AccountLockedUntil = accountLockedUntil;
    }
}