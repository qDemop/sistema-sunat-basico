using ERP.Application.Features.Authentication;
using ERP.Domain.Authentication;

namespace ERP.Application.Abstractions;

public interface IAuthenticationRepository
{
    Task<Usuario?> GetByUsernameWithRoleAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default);
    Task<AuthStateUpdateResult> RecordFailedLoginAsync(long userId, CancellationToken cancellationToken = default);
    Task RecordSuccessfulLoginAsync(long userId, CancellationToken cancellationToken = default);
}
