using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;
using ERP.Application.Security;
using ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.IntegrationTests.Authentication;

public sealed class InMemoryAuthApiFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public InMemoryAuthApiFixture()
    {
        PasswordHasher = new BCryptPasswordHasher();
        JwtTokenService = new TestJwtTokenService();
        AuthRepository = new TestAuthenticationRepository(PasswordHasher);
        AuditWriter = new TestAuditWriter();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IAuthenticationRepository>(AuthRepository);
                    services.AddSingleton<IPasswordHasher>(PasswordHasher);
                    services.AddSingleton<IJwtTokenService>(JwtTokenService);
                    services.AddSingleton<IAuditWriter>(AuditWriter);
                    services.AddSingleton<ITokenRevocationRepository>(new TestTokenRevocationRepository());
                });
            });
    }

    public TestAuthenticationRepository AuthRepository { get; }
    public BCryptPasswordHasher PasswordHasher { get; }
    public TestJwtTokenService JwtTokenService { get; }
    public TestAuditWriter AuditWriter { get; }

    public HttpClient CreateClient() => _factory.CreateClient();

    public void Dispose()
    {
        _factory.Dispose();
    }

    public sealed class TestAuthenticationRepository : IAuthenticationRepository
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly Dictionary<string, ERP.Domain.Authentication.Usuario> _users = new(StringComparer.OrdinalIgnoreCase);

        public TestAuthenticationRepository(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public void AddUser(string username, string password, string nombreCompleto, string rol)
        {
            var hash = _passwordHasher.HashPassword(password);
            _users[username] = ERP.Domain.Authentication.Usuario.Load(
                _users.Count + 1,
                username,
                hash,
                nombreCompleto,
                ERP.Domain.Authentication.Rol.Load(1, rol, string.Empty, 0),
                activo: true,
                intentosFallidos: 0,
                bloqueadoHasta: null);
        }

        public Task<ERP.Domain.Authentication.Usuario?> GetByUsernameWithRoleAsync(string normalizedUsername, CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(normalizedUsername, out var user);
            return Task.FromResult(user);
        }

        public Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default)
        {
            LoginAttempts.Add(record);
            return Task.CompletedTask;
        }

        public Task<AuthStateUpdateResult> RecordFailedLoginAsync(long userId, CancellationToken cancellationToken = default)
        {
            var username = _users.Values.Single(u => u.Id == userId).Username;
            var user = _users[username];
            var intentos = user.IntentosFallidos + 1;
            DateTime? bloqueadoHasta = intentos >= 3 ? DateTime.UtcNow.AddMinutes(15) : null;

            _users[username] = ERP.Domain.Authentication.Usuario.Load(
                user.Id,
                user.Username,
                user.PasswordHash,
                user.NombreCompleto,
                user.Rol,
                user.Activo,
                intentos,
                bloqueadoHasta);

            return Task.FromResult(new AuthStateUpdateResult(
                intentos,
                bloqueadoHasta,
                LockoutTriggered: bloqueadoHasta is not null));
        }

        public Task RecordSuccessfulLoginAsync(long userId, CancellationToken cancellationToken = default)
        {
            var username = _users.Values.Single(u => u.Id == userId).Username;
            var user = _users[username];
            _users[username] = ERP.Domain.Authentication.Usuario.Load(
                user.Id,
                user.Username,
                user.PasswordHash,
                user.NombreCompleto,
                user.Rol,
                user.Activo,
                0,
                null);
            return Task.CompletedTask;
        }

        public List<LoginAttemptRecord> LoginAttempts { get; } = new();
    }

    public sealed class TestJwtTokenService : IJwtTokenService
    {
        public (string Token, DateTime ExpiresAt) GenerateToken(long userId, string nombre, string rol, string jti)
        {
            return ($"token-{userId}-{jti}", DateTime.UtcNow.AddHours(1));
        }
    }

    public sealed class TestAuditWriter : IAuditWriter
    {
        public List<AuditEventRecord> Events { get; } = new();

        public Task WriteAsync(AuditEventRecord record, CancellationToken cancellationToken = default)
        {
            Events.Add(record);
            return Task.CompletedTask;
        }
    }

    public sealed class TestTokenRevocationRepository : ITokenRevocationRepository
    {
        private readonly HashSet<string> _revoked = new();

        public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_revoked.Contains(jti));
        }

        public Task RevokeTokenAsync(string jti, long userId, DateTime expiraEn, string correlationId, CancellationToken cancellationToken = default)
        {
            _revoked.Add(jti);
            return Task.CompletedTask;
        }
    }
}
