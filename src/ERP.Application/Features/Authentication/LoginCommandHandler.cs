using ERP.Application.Abstractions;
using ERP.Application.Security;
using MediatR;

namespace ERP.Application.Features.Authentication;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthenticationRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditWriter _auditWriter;

    public LoginCommandHandler(
        IAuthenticationRepository authRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IAuditWriter auditWriter)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _auditWriter = auditWriter;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;
        var normalizedUsername = request.Username.Trim();

        var user = await _authRepository.FindUserByUsernameAsync(normalizedUsername, cancellationToken);

        if (user is null)
        {
            await RecordFailedAttemptAndAudit(
                usuarioId: null,
                username: normalizedUsername,
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            throw new AuthenticationException("Invalid credentials.");
        }

        if (!user.Activo)
        {
            await RecordFailedAttemptAndAudit(
                usuarioId: user.Id,
                username: normalizedUsername,
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            throw new AuthenticationException("Invalid credentials.");
        }

        if (user.BloqueadoHasta is not null && user.BloqueadoHasta > DateTime.UtcNow)
        {
            await _authRepository.RecordLoginAttemptAsync(
                new LoginAttemptRecord(user.Id, normalizedUsername, Exitoso: false, IpOrigen: null, correlationId),
                cancellationToken);

            await WriteAuditEvent(
                usuarioId: user.Id,
                rolActor: user.Rol,
                accion: "LoginBlocked",
                resultado: "Blocked",
                correlationId: correlationId,
                datos: new Dictionary<string, object> { ["bloqueadoHasta"] = user.BloqueadoHasta.Value },
                cancellationToken: cancellationToken);

            throw new AuthenticationException("Invalid credentials.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            var stateResult = await _authRepository.RecordFailedLoginAsync(user.Id, cancellationToken);

            await _authRepository.RecordLoginAttemptAsync(
                new LoginAttemptRecord(user.Id, normalizedUsername, Exitoso: false, IpOrigen: null, correlationId),
                cancellationToken);

            var failureData = new Dictionary<string, object> { ["intentosFallidos"] = stateResult.IntentosFallidos };
            if (stateResult.LockoutTriggered)
            {
                failureData["lockoutTriggered"] = true;
                if (stateResult.BloqueadoHasta is not null)
                {
                    failureData["bloqueadoHasta"] = stateResult.BloqueadoHasta.Value;
                }
            }

            await WriteAuditEvent(
                usuarioId: user.Id,
                rolActor: user.Rol,
                accion: "LoginFailure",
                resultado: "Failure",
                correlationId: correlationId,
                datos: failureData,
                cancellationToken: cancellationToken);

            throw new AuthenticationException("Invalid credentials.");
        }

        await _authRepository.RecordSuccessfulLoginAsync(user.Id, cancellationToken);

        await _authRepository.RecordLoginAttemptAsync(
            new LoginAttemptRecord(user.Id, normalizedUsername, Exitoso: true, IpOrigen: null, correlationId),
            cancellationToken);

        var jti = Guid.NewGuid().ToString("N");
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.NombreCompleto, user.Rol, jti);
        var modules = RoleModuleMapping.GetVisibleModules(user.Rol);

        await WriteAuditEvent(
            usuarioId: user.Id,
            rolActor: user.Rol,
            accion: "LoginSuccess",
            resultado: "Success",
            correlationId: correlationId,
            datos: new Dictionary<string, object> { ["jti"] = jti },
            cancellationToken: cancellationToken);

        return new LoginResponse(
            Token: token,
            ExpiresAt: expiresAt,
            User: new UserSession(user.Id, user.NombreCompleto, user.Rol),
            Modules: modules,
            CorrelationId: correlationId);
    }

    private async Task RecordFailedAttemptAndAudit(
        long? usuarioId,
        string username,
        string correlationId,
        CancellationToken cancellationToken)
    {
        await _authRepository.RecordLoginAttemptAsync(
            new LoginAttemptRecord(usuarioId, username, Exitoso: false, IpOrigen: null, correlationId),
            cancellationToken);

        await WriteAuditEvent(
            usuarioId: usuarioId,
            rolActor: null,
            accion: "LoginFailure",
            resultado: "Failure",
            correlationId: correlationId,
            datos: new Dictionary<string, object>(),
            cancellationToken: cancellationToken);
    }

    private async Task WriteAuditEvent(
        long? usuarioId,
        string? rolActor,
        string accion,
        string resultado,
        string correlationId,
        IReadOnlyDictionary<string, object> datos,
        CancellationToken cancellationToken)
    {
        await _auditWriter.WriteAsync(
            new AuditEventRecord(
                UsuarioId: usuarioId,
                RolActor: rolActor,
                Modulo: "Authentication",
                Accion: accion,
                Entidad: "Usuario",
                EntidadId: usuarioId?.ToString(),
                Resultado: resultado,
                Datos: datos,
                CorrelationId: correlationId),
            cancellationToken);
    }
}