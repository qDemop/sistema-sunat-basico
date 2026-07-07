using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Features.Authentication;

namespace ERP.Application.Abstractions;

public interface IAuthenticationRepository
{
    Task<UserAuthenticationData?> FindUserByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default);
    Task UpdateAuthenticationStateAsync(long userId, bool success, CancellationToken cancellationToken = default);
}
