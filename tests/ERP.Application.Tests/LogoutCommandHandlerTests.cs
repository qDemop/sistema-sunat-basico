using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;
using Xunit;

namespace ERP.Application.Tests;

public class LogoutCommandHandlerTests
{
    private const string TestJti = "jti-revoke-123";
    private const long TestUserId = 7;
    private const string TestRol = "Administrador Sistema";
    private static readonly DateTime TestExpiresAt = DateTime.UtcNow.AddHours(1).ToUniversalTime();

    [Fact]
    public async Task Handle_RevokesTokenAndWritesAudit()
    {
        var fakes = new Fakes();
        var handler = new LogoutCommandHandler(fakes, fakes);
        var command = new LogoutCommand(TestJti, TestUserId, TestRol, TestExpiresAt, "corr-logout-456");

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.True(response.Revoked);
        Assert.Equal(TestExpiresAt, response.ExpiresAt);
        Assert.Equal("corr-logout-456", response.CorrelationId);

        Assert.Single(fakes.RevokedTokens);
        Assert.Equal(TestJti, fakes.RevokedTokens[0].Jti);
        Assert.Equal(TestUserId, fakes.RevokedTokens[0].UserId);
        Assert.Equal(TestExpiresAt, fakes.RevokedTokens[0].ExpiresAt);
        Assert.Equal("corr-logout-456", fakes.RevokedTokens[0].CorrelationId);

        Assert.Single(fakes.AuditEvents);
        var audit = fakes.AuditEvents[0];
        Assert.Equal(TestUserId, audit.UsuarioId);
        Assert.Equal(TestRol, audit.RolActor);
        Assert.Equal("Authentication", audit.Modulo);
        Assert.Equal("Logout", audit.Accion);
        Assert.Equal("Token", audit.Entidad);
        Assert.Equal(TestJti, audit.EntidadId);
        Assert.Equal("Success", audit.Resultado);
        Assert.Equal("corr-logout-456", audit.CorrelationId);
        Assert.True(audit.Datos.ContainsKey("jti"));
        Assert.Equal(TestJti, audit.Datos["jti"]);
    }

    [Fact]
    public async Task Handle_MissingCorrelationId_GeneratesOne()
    {
        var fakes = new Fakes();
        var handler = new LogoutCommandHandler(fakes, fakes);
        var command = new LogoutCommand(TestJti, TestUserId, TestRol, TestExpiresAt, null);

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.NotEmpty(response.CorrelationId);
        Assert.Equal(32, response.CorrelationId.Length);
        Assert.True(response.CorrelationId.All(c => char.IsLetterOrDigit(c)));
        Assert.Equal(response.CorrelationId, fakes.RevokedTokens[0].CorrelationId);
        Assert.Equal(response.CorrelationId, fakes.AuditEvents[0].CorrelationId);
    }

    private sealed class Fakes : ITokenRevocationRepository, IAuditWriter
    {
        public List<RevokedToken> RevokedTokens { get; } = new();
        public List<AuditEventRecord> AuditEvents { get; } = new();

        public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RevokedTokens.Any(t => t.Jti == jti));
        }

        public Task RevokeTokenAsync(
            string jti,
            long userId,
            DateTime expiraEn,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            RevokedTokens.Add(new RevokedToken(jti, userId, expiraEn, correlationId));
            return Task.CompletedTask;
        }

        public Task WriteAsync(AuditEventRecord record, CancellationToken cancellationToken = default)
        {
            AuditEvents.Add(record);
            return Task.CompletedTask;
        }
    }

    private sealed record RevokedToken(string Jti, long UserId, DateTime ExpiresAt, string CorrelationId);
}
