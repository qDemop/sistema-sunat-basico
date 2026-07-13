using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;
using ERP.Application.Security;
using ERP.Domain.Authentication;
using Xunit;

namespace ERP.Application.Tests;

public class LoginCommandHandlerTests
{
    private const string TestToken = "test-jwt-token";
    private static readonly DateTime TestExpiry = DateTime.UtcNow.AddHours(1);
    private const long TestUserId = 1;
    private const string TestUsername = "admin";
    private const string TestPassword = "correctPassword";
    private const string TestHash = "$2a$11$fakeHash";
    private const string TestNombre = "Admin User";
    private const string TestRol = "Administrador Sistema";
    private const string TestUnknownUsername = "doesnotexist";

    private static Usuario ActiveUser() => Usuario.Load(
        TestUserId,
        TestUsername,
        TestHash,
        TestNombre,
        Rol.Load(1, TestRol, string.Empty, 0),
        activo: true,
        intentosFallidos: 0,
        bloqueadoHasta: null);

    private static Usuario WithActivo(Usuario user, bool activo) => Usuario.Load(
        user.Id,
        user.Username,
        user.PasswordHash,
        user.NombreCompleto,
        user.Rol,
        activo,
        user.IntentosFallidos,
        user.BloqueadoHasta);

    private static Usuario WithBloqueadoHasta(Usuario user, DateTime? bloqueadoHasta) => Usuario.Load(
        user.Id,
        user.Username,
        user.PasswordHash,
        user.NombreCompleto,
        user.Rol,
        user.Activo,
        user.IntentosFallidos,
        bloqueadoHasta);

    private static Usuario WithIntentosFallidos(Usuario user, int intentos) => Usuario.Load(
        user.Id,
        user.Username,
        user.PasswordHash,
        user.NombreCompleto,
        user.Rol,
        user.Activo,
        intentos,
        user.BloqueadoHasta);

    [Fact]
    public async Task ValidLogin_ReturnsTokenUserRoleModulesAndCorrelationId()
    {
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        var response = await handler.Handle(
            new LoginCommand(TestUsername, TestPassword),
            CancellationToken.None);

        Assert.Equal(TestToken, response.Token);
        Assert.Equal(TestUserId, response.User.Id);
        Assert.Equal(TestNombre, response.User.Nombre);
        Assert.Equal(TestRol, response.User.Rol);
        Assert.NotEmpty(response.Modules);
        Assert.NotEmpty(response.CorrelationId);
    }

    [Fact]
    public async Task ValidLogin_ReturnsModulesFromRoleModuleMapping()
    {
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        var response = await handler.Handle(
            new LoginCommand(TestUsername, TestPassword),
            CancellationToken.None);

        Assert.Equal(RoleModuleMapping.GetVisibleModules(TestRol), response.Modules);
    }

    [Fact]
    public async Task ValidLogin_ResetsAuthState_RecordsSuccessAttempt_WritesSuccessAudit()
    {
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        await handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None);

        Assert.True(fakes.SuccessfulStateUpdated);
        Assert.NotEmpty(fakes.LoginAttempts);
        Assert.True(fakes.LoginAttempts[^1].Exitoso);
        Assert.Contains(fakes.AuditEvents, e => e.Resultado == "Success" && e.Accion == "LoginSuccess");
    }

    [Fact]
    public async Task UnknownUsername_FailsGenerically_RecordsFailedAttemptAndAudit_NoJwt()
    {
        var fakes = new Fakes { User = null };
        var handler = CreateHandler(fakes);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUnknownUsername, TestPassword), CancellationToken.None));

        Assert.DoesNotContain(fakes.JwtTokens, t => t.userId == TestUserId);
        Assert.Contains(fakes.LoginAttempts, a => a.Exitoso is false && a.UsuarioId is null);
        Assert.Contains(fakes.AuditEvents, e => e.Resultado == "Failure" && e.Accion == "LoginFailure");
    }

    [Fact]
    public async Task InactiveUser_Fails_RecordsAttemptAndAudit_DoesNotVerifyPassword_NoJwt()
    {
        var fakes = new Fakes
        {
            User = WithActivo(ActiveUser(), false),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None));

        Assert.False(fakes.PasswordVerifyCalled);
        Assert.Contains(fakes.LoginAttempts, a => a.Exitoso is false);
        Assert.Contains(fakes.AuditEvents, e => e.Resultado == "Failure");
        Assert.DoesNotContain(fakes.JwtTokens, t => t.userId == TestUserId);
    }

    [Fact]
    public async Task BlockedUser_Fails_RecordsAttemptAndAudit_DoesNotVerifyPassword_NoJwt()
    {
        var fakes = new Fakes
        {
            User = WithBloqueadoHasta(ActiveUser(), DateTime.UtcNow.AddMinutes(10)),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None));

        Assert.False(fakes.PasswordVerifyCalled);
        Assert.Contains(fakes.LoginAttempts, a => a.Exitoso is false);
        Assert.Contains(fakes.AuditEvents, e => e.Resultado == "Blocked" && e.Accion == "LoginBlocked");
        Assert.DoesNotContain(fakes.JwtTokens, t => t.userId == TestUserId);
    }

    [Fact]
    public async Task InvalidPassword_RecordsFailedAttempt_UpdatesFailedState_WritesAudit_NoJwt()
    {
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = false
        };
        var handler = CreateHandler(fakes);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None));

        Assert.True(fakes.PasswordVerifyCalled);
        Assert.True(fakes.FailedStateRecorded);
        Assert.Contains(fakes.LoginAttempts, a => a.Exitoso is false);
        Assert.Contains(fakes.AuditEvents, e => e.Resultado == "Failure" && e.Accion == "LoginFailure");
        Assert.DoesNotContain(fakes.JwtTokens, t => t.userId == TestUserId);
    }

    [Fact]
    public async Task LockoutTriggered_IncludedInAuditData()
    {
        var fakes = new Fakes
        {
            User = WithIntentosFallidos(ActiveUser(), 2),
            PasswordVerified = false,
            FailedResult = new AuthStateUpdateResult(3, DateTime.UtcNow.AddMinutes(15), LockoutTriggered: true)
        };
        var handler = CreateHandler(fakes);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None));

        var audit = Assert.Single(fakes.AuditEvents, e =>
            e.Accion == "LoginFailure" && e.Datos.ContainsKey("lockoutTriggered"));
        Assert.True((bool)audit.Datos["lockoutTriggered"]);
    }

    [Fact]
    public async Task ThirdFailedAttempt_ThrowsWithAccountLockedUntil()
    {
        var expectedLockout = DateTime.UtcNow.AddMinutes(15);
        var fakes = new Fakes
        {
            User = WithIntentosFallidos(ActiveUser(), 2),
            PasswordVerified = false,
            FailedResult = new AuthStateUpdateResult(3, expectedLockout, LockoutTriggered: true)
        };
        var handler = CreateHandler(fakes);

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() =>
            handler.Handle(new LoginCommand(TestUsername, TestPassword), CancellationToken.None));

        Assert.NotNull(ex.AccountLockedUntil);
        Assert.Equal(expectedLockout, ex.AccountLockedUntil.Value);
    }

    [Fact]
    public async Task ExpiredBlock_TreatsAsNotBlocked_AndVerifiesPassword()
    {
        var fakes = new Fakes
        {
            User = WithIntentosFallidos(
                WithBloqueadoHasta(ActiveUser(), DateTime.UtcNow.AddMinutes(-5)),
                3),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        var response = await handler.Handle(
            new LoginCommand(TestUsername, TestPassword),
            CancellationToken.None);

        Assert.True(fakes.PasswordVerifyCalled);
        Assert.True(fakes.SuccessfulStateUpdated);
        Assert.Equal(TestToken, response.Token);
    }

    [Fact]
    public async Task SuppliedCorrelationId_IsPreservedInResponseAndAudit()
    {
        const string suppliedCorrelationId = "supplied-corr-id";
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        var response = await handler.Handle(
            new LoginCommand(TestUsername, TestPassword, suppliedCorrelationId),
            CancellationToken.None);

        Assert.Equal(suppliedCorrelationId, response.CorrelationId);
        Assert.All(fakes.AuditEvents, e => Assert.Equal(suppliedCorrelationId, e.CorrelationId));
        Assert.All(fakes.LoginAttempts, a => Assert.Equal(suppliedCorrelationId, a.CorrelationId));
    }

    [Fact]
    public async Task MissingCorrelationId_GeneratesOne()
    {
        var fakes = new Fakes
        {
            User = ActiveUser(),
            PasswordVerified = true
        };
        var handler = CreateHandler(fakes);

        var response = await handler.Handle(
            new LoginCommand(TestUsername, TestPassword, null),
            CancellationToken.None);

        Assert.NotEmpty(response.CorrelationId);
    }

    private static LoginCommandHandler CreateHandler(Fakes fakes) =>
        new(fakes, fakes, fakes, fakes);

    private sealed class Fakes :
        IAuthenticationRepository,
        IPasswordHasher,
        IJwtTokenService,
        IAuditWriter
    {
        public Usuario? User { get; set; }
        public bool PasswordVerified { get; set; }
        public bool PasswordVerifyCalled { get; private set; }
        public AuthStateUpdateResult FailedResult { get; set; } = new(1, null, false);
        public bool FailedStateRecorded { get; private set; }
        public bool SuccessfulStateUpdated { get; private set; }
        public List<LoginAttemptRecord> LoginAttempts { get; } = new();
        public List<AuditEventRecord> AuditEvents { get; } = new();
        public List<(long userId, string nombre, string rol, string jti)> JwtTokens { get; } = new();

        public Task<Usuario?> GetByUsernameWithRoleAsync(string normalizedUsername, CancellationToken cancellationToken = default)
        {
            if (User is null || normalizedUsername != User.Username)
                return Task.FromResult<Usuario?>(null);

            return Task.FromResult<Usuario?>(User);
        }

        public Task RecordLoginAttemptAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default)
        {
            LoginAttempts.Add(record);
            return Task.CompletedTask;
        }

        public Task<AuthStateUpdateResult> RecordFailedLoginAsync(long userId, CancellationToken cancellationToken = default)
        {
            FailedStateRecorded = true;
            return Task.FromResult(FailedResult);
        }

        public Task RecordSuccessfulLoginAsync(long userId, CancellationToken cancellationToken = default)
        {
            SuccessfulStateUpdated = true;
            return Task.CompletedTask;
        }

        public bool VerifyPassword(string password, string hash)
        {
            PasswordVerifyCalled = true;
            return PasswordVerified;
        }

        public string HashPassword(string password) => "$2a$11$fakeHash";

        public (string Token, DateTime ExpiresAt) GenerateToken(long userId, string nombre, string rol, string jti)
        {
            JwtTokens.Add((userId, nombre, rol, jti));
            return (TestToken, TestExpiry);
        }

        public Task WriteAsync(AuditEventRecord record, CancellationToken cancellationToken = default)
        {
            AuditEvents.Add(record);
            return Task.CompletedTask;
        }
    }
}
