using System.Threading;
using System.Threading.Tasks;
using ERP.Domain.Authentication;
using ERP.Application.Features.Authentication;

namespace ERP.Application.Abstractions;

public interface IAuthenticationRepository
{
#pragma warning disable CS0618
    Task<UserAuthenticationData?> FindUserByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
#pragma warning restore CS0618
    Task<Usuario?> GetByUsernameWithRoleAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default);
    Task<AuthStateUpdateResult> RecordFailedLoginAsync(long userId, CancellationToken cancellationToken = default);
    Task RecordSuccessfulLoginAsync(long userId, CancellationToken cancellationToken = default);
}
