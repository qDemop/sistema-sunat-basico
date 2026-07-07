using ERP.API.Contracts;
using ERP.Application.Features.Authentication;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            [FromBody] LoginRequest request,
            [FromHeader(Name = "X-Correlation-ID")] string? correlationHeader,
            IValidator<LoginCommand> validator,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var correlationId = string.IsNullOrWhiteSpace(correlationHeader)
                ? Guid.NewGuid().ToString("N")
                : correlationHeader;

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
    }
}
