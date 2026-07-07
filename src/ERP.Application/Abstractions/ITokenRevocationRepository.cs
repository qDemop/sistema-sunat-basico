using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Abstractions;

public interface ITokenRevocationRepository
{
    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string jti, long userId, DateTime expiraEn, string correlationId, CancellationToken cancellationToken = default);
}
