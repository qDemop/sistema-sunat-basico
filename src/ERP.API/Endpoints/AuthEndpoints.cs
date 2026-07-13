using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ERP.API.Contracts;
using ERP.Application.Features.Authentication;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            HttpContext httpContext,
            [FromBody] LoginRequest request,
            IValidator<LoginCommand> validator,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var correlationId = GetRequestCorrelationId(httpContext);

            var command = new LoginCommand(request.Username, request.Password, correlationId);

            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new ValidationError(e.PropertyName, "Validation", e.ErrorMessage))
                    .ToList();

                return Results.BadRequest(new ErrorResponse(
                    Status: 400,
                    Code: "VALIDATION_ERROR",
                    Message: "Invalid request.",
                    CorrelationId: correlationId,
                    ValidationErrors: errors));
            }

            try
            {
                var response = await mediator.Send(command, cancellationToken);

                return Results.Ok(new
                {
                    response.Token,
                    ExpiresAt = response.ExpiresAt,
                    User = new
                    {
                        response.User.Id,
                        response.User.Nombre,
                        response.User.Rol
                    },
                    response.Modules,
                    response.CorrelationId
                });
            }
            catch (AuthenticationException ex) when (ex.AccountLockedUntil is not null)
            {
                return Results.Json(
                    new ErrorResponse(
                        Status: 423,
                        Code: "AUTH_ACCOUNT_LOCKED",
                        Message: $"Account locked until {ex.AccountLockedUntil:O}.",
                        CorrelationId: correlationId,
                        Data: new Dictionary<string, object> { ["bloqueadoHasta"] = ex.AccountLockedUntil.Value }),
                    statusCode: StatusCodes.Status423Locked);
            }
            catch (AuthenticationException)
            {
                return Results.Json(
                    new ErrorResponse(
                        Status: 401,
                        Code: "AUTH_INVALID_CREDENTIALS",
                        Message: "Invalid credentials.",
                        CorrelationId: correlationId),
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .AllowAnonymous();

        app.MapGet("/api/auth/me", [Authorize] async (
            HttpContext httpContext,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = GetRequiredClaimLong(user, JwtRegisteredClaimNames.Sub);
            var nombre = GetRequiredClaim(user, JwtRegisteredClaimNames.Name);
            var rol = GetRequiredClaim(user, "role");
            var correlationId = GetRequestCorrelationId(httpContext);

            var response = await mediator.Send(
                new GetCurrentUserQuery(userId, nombre, rol, correlationId),
                cancellationToken);

            return Results.Ok(new
            {
                User = new
                {
                    response.User.Id,
                    response.User.Nombre,
                    response.User.Rol
                },
                response.Modules,
                response.CorrelationId
            });
        });

        app.MapPost("/api/auth/logout", [Authorize] async (
            HttpContext httpContext,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var correlationId = GetRequestCorrelationId(httpContext);
            var token = ReadBearerToken(httpContext.Request);
            var jwt = ReadJwtToken(token);
            var jti = jwt.Id;
            var userId = GetRequiredClaimLong(user, JwtRegisteredClaimNames.Sub);
            var rol = GetRequiredClaim(user, "role");

            if (string.IsNullOrWhiteSpace(jti))
            {
                return Results.BadRequest(new ErrorResponse(
                    Status: 400,
                    Code: "AUTH_MISSING_JTI",
                    Message: "Token does not contain a valid session identifier.",
                    CorrelationId: correlationId));
            }

            var response = await mediator.Send(
                new LogoutCommand(jti, userId, rol, jwt.ValidTo, correlationId),
                cancellationToken);

            return Results.Ok(new
            {
                response.Revoked,
                ExpiresAt = response.ExpiresAt,
                response.CorrelationId
            });
        });
    }

    private static string GetRequestCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(Middleware.CorrelationIdMiddleware.CorrelationIdItemKey, out var value)
            && value is string correlationId
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        // Fallback for contexts where the middleware is not present (e.g., isolated endpoint tests).
        return Middleware.CorrelationIdMiddleware.ResolveCorrelationId(context.Request);
    }

    private static string GetRequiredClaim(ClaimsPrincipal user, string claimType)
    {
        var claim = user.FindFirst(claimType);
        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
        {
            throw new InvalidOperationException($"Required claim '{claimType}' is missing from the authenticated user.");
        }

        return claim.Value;
    }

    private static long GetRequiredClaimLong(ClaimsPrincipal user, string claimType)
    {
        var value = GetRequiredClaim(user, claimType);
        if (!long.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            throw new InvalidOperationException($"Claim '{claimType}' must be a valid integer.");
        }

        return result;
    }

    private static string ReadBearerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Authorization header must use Bearer scheme.");
        }

        return header[prefix.Length..];
    }

    private static JwtSecurityToken ReadJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }
}
