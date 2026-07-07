using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Features.Authentication;

namespace ERP.Application.Abstractions;

public interface IAuthenticationRepository
{
    Task<UserAuthenticationData?> FindUserByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default);
    Task<AuthStateUpdateResult> RecordFailedLoginAsync(long userId, CancellationToken cancellationToken = default);
    Task RecordSuccessfulLoginAsync(long userId, CancellationToken cancellationToken = default);
}
