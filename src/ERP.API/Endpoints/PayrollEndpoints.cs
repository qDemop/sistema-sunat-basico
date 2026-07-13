using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ERP.API.Contracts;
using ERP.API.Middleware;
using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;
using ERP.Application.Features.Payroll.Commands;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Application.Features.Payroll.Queries;
using ERP.Application.Features.Payroll.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ERP.API.Endpoints;

public static class PayrollEndpoints
{
    private static readonly string[] PayrollRoles = ["Administrador RRHH", "Administrador Sistema"];

    public static void MapPayrollEndpoints(this IEndpointRouteBuilder app)
    {
        var payroll = app.MapGroup("/api").RequireAuthorization(policy => policy.RequireRole(PayrollRoles));

        payroll.MapPost("/departamentos", async ([FromBody] DepartamentoInput input, IMediator mediator, HttpContext context, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => mediator.Send(new CreateDepartamentoCommand(input), cancellationToken), ToResponse, context, StatusCodes.Status201Created));
        payroll.MapGet("/departamentos", async (int? page, int? pageSize, string? sortBy, string? sortDirection, IMediator mediator, HttpContext context, CancellationToken ct) => Page(await mediator.Send(new ListDepartamentosQuery(null), ct), page, pageSize, sortBy, sortDirection, ToResponse, context));
        payroll.MapGet("/departamentos/{id:long}", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => (await mediator.Send(new GetDepartamentoQuery(id), ct)) is { } item ? Results.Ok(ToResponse(item)) : NotFound(context));
        payroll.MapPut("/departamentos/{id:long}", async (long id, [FromBody] DepartamentoInput input, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResourceAsync(() => mediator.Send(new UpdateDepartamentoCommand(id, input), ct), () => mediator.Send(new GetDepartamentoQuery(id), ct), ToResponse, context));
        payroll.MapDelete("/departamentos/{id:long}", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResultAsync(() => mediator.Send(new SetDepartamentoActiveCommand(id, false), ct), context));
        payroll.MapPut("/departamentos/{id:long}/reactivar", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResultAsync(() => mediator.Send(new SetDepartamentoActiveCommand(id, true), ct), context));

        payroll.MapPost("/empleados", async ([FromBody] EmpleadoInput input, IValidator<EmpleadoInput> validator, IMediator mediator, HttpContext context, CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(input, cancellationToken);
            if (!validation.IsValid) return ValidationProblem(validation, context);
            return await ExecuteAsync(() => mediator.Send(new CreateEmpleadoCommand(input), cancellationToken), ToResponse, context, StatusCodes.Status201Created);
        });

        payroll.MapPut("/empleados/{id:long}/reactivar", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResultAsync(() => mediator.Send(new ReactivateEmpleadoCommand(id), ct), context));
        payroll.MapGet("/empleados", async (int? page, int? pageSize, string? sortBy, string? sortDirection, long? departamentoId, string? search, bool? activo, IMediator mediator, HttpContext context, CancellationToken ct) => Page(await mediator.Send(new ListEmpleadosQuery(departamentoId, search, activo), ct), page, pageSize, sortBy, sortDirection, ToResponse, context));
        payroll.MapGet("/empleados/{id:long}", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => (await mediator.Send(new GetEmpleadoQuery(id), ct)) is { } item ? Results.Ok(ToResponse(item)) : NotFound(context));
        payroll.MapPut("/empleados/{id:long}", async (long id, [FromBody] EmpleadoInput input, IValidator<EmpleadoInput> validator, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(input, ct);
            return !validation.IsValid ? ValidationProblem(validation, context) : await ExecuteResourceAsync(() => mediator.Send(new UpdateEmpleadoCommand(id, input), ct), () => mediator.Send(new GetEmpleadoQuery(id), ct), ToResponse, context);
        });
        payroll.MapDelete("/empleados/{id:long}", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResultAsync(() => mediator.Send(new SetEmpleadoActiveCommand(id, false), ct), context));

        payroll.MapPost("/horas-extra", async ([FromBody] HorasExtraInput input, IMediator mediator, HttpContext context, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => mediator.Send(new CreateHorasExtraCommand(input), cancellationToken), ToResponse, context, StatusCodes.Status201Created));
        payroll.MapGet("/horas-extra", async (int? page, int? pageSize, string? sortBy, string? sortDirection, long? empleadoId, string? periodo, string? estado, IMediator mediator, HttpContext context, CancellationToken ct) => Page(await mediator.Send(new ListHorasExtraQuery(empleadoId, periodo, estado), ct), page, pageSize, sortBy, sortDirection, ToResponse, context));
        payroll.MapGet("/horas-extra/{id:long}", async (long id, IMediator mediator, HttpContext context, CancellationToken ct) => (await mediator.Send(new GetHorasExtraQuery(id), ct)) is { } item ? Results.Ok(ToResponse(item)) : NotFound(context));
        payroll.MapPut("/horas-extra/{id:long}", async (long id, [FromBody] HorasExtraInput input, IMediator mediator, HttpContext context, CancellationToken ct) => await ExecuteResultAsync(() => mediator.Send(new UpdateHorasExtraCommand(id, input), ct), context));

        payroll.MapPost("/horas-extra/{id:long}/aprobar", async (long id, ClaimsPrincipal user, IMediator mediator, HttpContext context, CancellationToken cancellationToken) =>
        {
            if (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var actorUserId)) return Unauthorized(context);
            return await ExecuteOvertimeAsync(() => mediator.Send(new ApproveHorasExtraCommand(id, actorUserId, Correlation(context)), cancellationToken), id, mediator, context, cancellationToken);
        });

        payroll.MapPost("/horas-extra/{id:long}/cancelar", async (long id, ClaimsPrincipal user, IMediator mediator, HttpContext context, CancellationToken cancellationToken) =>
        {
            if (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var actorUserId)) return Unauthorized(context);
            return await ExecuteOvertimeAsync(() => mediator.Send(new CancelHorasExtraCommand(id, actorUserId, Correlation(context)), cancellationToken), id, mediator, context, cancellationToken);
        });

        payroll.MapPost("/planilla/calcular", async ([FromBody] PayrollCalculationRequest input, ClaimsPrincipal user, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            if (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var actorUserId)) return Unauthorized(context);
            return await ExecutePayrollAsync(() => mediator.Send(new CalculatePayrollCommand(input.Periodo, actorUserId, Correlation(context)), ct), actorUserId, Role(user), "CALCULAR_PLANILLA", input.Periodo, context, context.RequestServices.GetRequiredService<IAuditWriter>(), ct);
        });
        payroll.MapPost("/planilla/{periodo}/finalizar", async (string periodo, ClaimsPrincipal user, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            if (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var actorUserId)) return Unauthorized(context);
            return await ExecutePayrollAsync(() => mediator.Send(new FinalizePayrollCommand(periodo, actorUserId, Correlation(context)), ct), actorUserId, Role(user), "FINALIZAR_PLANILLA", periodo, context, context.RequestServices.GetRequiredService<IAuditWriter>(), ct);
        });
        payroll.MapPost("/planilla/{periodo}/cancelar", async (string periodo, ClaimsPrincipal user, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            if (!long.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var actorUserId)) return Unauthorized(context);
            return await ExecutePayrollAsync(() => mediator.Send(new CancelPayrollCommand(periodo, actorUserId, Correlation(context)), ct), actorUserId, Role(user), "CANCELAR_PLANILLA", periodo, context, context.RequestServices.GetRequiredService<IAuditWriter>(), ct);
        });
        payroll.MapGet("/planilla", async (string periodo, IMediator mediator, HttpContext context, CancellationToken ct) =>
            (await mediator.Send(new GetPayrollByPeriodQuery(periodo), ct)) is { } item ? Results.Ok(ToResponse(item, Correlation(context))) : NotFound(context));
        payroll.MapGet("/planilla/periodos", async (string? estado, IMediator mediator, HttpContext context, CancellationToken ct) =>
            Results.Ok((await mediator.Send(new ListPayrollPeriodsQuery(estado), ct)).Select(item => ToResponse(item, Correlation(context)))));
        payroll.MapGet("/planilla/dashboard", async (string periodo, IMediator mediator, HttpContext context, CancellationToken ct) =>
            Results.Ok(ToResponse(await mediator.Send(new GetPayrollDashboardQuery(periodo), ct), Correlation(context))));
        payroll.MapGet("/planilla/{periodo}/export/excel", async (string periodo, IMediator mediator, HttpContext context, CancellationToken ct) =>
            await mediator.Send(new GetPayrollByPeriodQuery(periodo), ct) is { } item
                ? Results.File(PayrollExportDocuments.Excel(item), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"planilla-{periodo}.xlsx")
                : NotFound(context));
        payroll.MapGet("/planilla/{periodo}/export/pdf", async (string periodo, IMediator mediator, HttpContext context, CancellationToken ct) =>
            await mediator.Send(new GetPayrollByPeriodQuery(periodo), ct) is { } item
                ? Results.File(PayrollExportDocuments.Payslips(item), "application/zip", $"boletas-{periodo}.zip")
                : NotFound(context));
    }

    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action, HttpContext context, int successStatus)
    {
        try { return Results.Json(await action(), statusCode: successStatus); }
        catch (CatalogConflictException) { return Conflict(context); }
        catch (PostgresException ex) when (ex.SqlState == "23505") { return Conflict(context); }
        catch (InvalidOperationException) { return BadRequest(context); }
        catch (PostgresException) { return BadRequest(context); }
    }
    private static async Task<IResult> ExecuteAsync<TSource, TResult>(Func<Task<TSource>> action, Func<TSource, TResult> map, HttpContext context, int successStatus)
    {
        try { return Results.Json(map(await action()), statusCode: successStatus); }
        catch (CatalogConflictException) { return Conflict(context); }
        catch (PostgresException ex) when (IsConflict(ex)) { return Conflict(context); }
        catch (InvalidOperationException) { return BadRequest(context); }
        catch (PostgresException) { return BadRequest(context); }
    }
    private static async Task<IResult> ExecuteResultAsync(Func<Task<CatalogCommandResult>> action, HttpContext context)
    {
        try
        {
            var result = await action();
            return result.Status switch { CatalogCommandStatus.NotFound => NotFound(context), CatalogCommandStatus.Conflict => Conflict(context), _ => Results.Ok(result) };
        }
        catch (CatalogConflictException) { return Conflict(context); }
        catch (PostgresException ex) when (IsConflict(ex)) { return Conflict(context); }
        catch (PostgresException) { return BadRequest(context); }
    }
    private static async Task<IResult> ExecuteResourceAsync<TSnapshot, TResponse>(Func<Task<CatalogCommandResult>> action, Func<Task<TSnapshot?>> read, Func<TSnapshot, TResponse> map, HttpContext context) where TSnapshot : class
    { var result = await ExecuteResultAsync(action, context); if (result is not Microsoft.AspNetCore.Http.HttpResults.Ok<CatalogCommandResult>) return result; return await read() is { } item ? Results.Ok(map(item)) : NotFound(context); }
    private static async Task<IResult> OvertimeResultAsync(CatalogCommandResult result, long id, IMediator mediator, HttpContext context, CancellationToken ct)
    { if (result.Status == CatalogCommandStatus.NotFound) return NotFound(context); if (result.Status == CatalogCommandStatus.Conflict) return Conflict(context); return await mediator.Send(new GetHorasExtraQuery(id), ct) is { } item ? Results.Ok(ToResponse(item)) : NotFound(context); }
    private static async Task<IResult> ExecuteOvertimeAsync(Func<Task<CatalogCommandResult>> action, long id, IMediator mediator, HttpContext context, CancellationToken ct)
    { try { return await OvertimeResultAsync(await action(), id, mediator, context, ct); } catch (PostgresException ex) when (IsConflict(ex)) { return Conflict(context); } catch (PostgresException) { return BadRequest(context); } }
    private static async Task<IResult> ExecutePayrollAsync(Func<Task<PayrollPeriodSnapshot>> action, long actorUserId, string role, string auditAction, string periodo, HttpContext context, IAuditWriter auditWriter, CancellationToken cancellationToken)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PayrollEndpoints));
        try { return Results.Ok(ToResponse(await action(), Correlation(context))); }
        catch (PayrollOperationException ex)
        {
            await WriteFailureAuditAsync(auditWriter, logger, actorUserId, role, auditAction, periodo, FailureCode(ex.Error), Correlation(context));
            return ex.Error switch
            {
                PayrollOperationError.Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
                PayrollOperationError.NotFound => NotFound(context),
                PayrollOperationError.StateConflict => Conflict(context),
                _ => PayrollValidationError(context, ex.Error)
            };
        }
        catch
        {
            await WriteFailureAuditAsync(auditWriter, logger, actorUserId, role, auditAction, periodo, "payroll.unexpected", Correlation(context));
            throw;
        }
    }
    private static async Task WriteFailureAuditAsync(IAuditWriter auditWriter, ILogger logger, long actorUserId, string role, string action, string periodo, string code, string correlationId)
    {
        try
        {
            using var auditTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await auditWriter.WriteAsync(new AuditEventRecord(actorUserId, role, "Payroll", action, "PERIODO_PLANILLA", periodo, "Failure", new Dictionary<string, object> { ["code"] = code }, correlationId), auditTimeout.Token);
        }
        catch
        {
            logger.LogWarning("Payroll failure audit could not persist. Action={Action} CorrelationId={CorrelationId}", action, correlationId);
        }
    }
    private static string FailureCode(PayrollOperationError error) => error switch
    {
        PayrollOperationError.NotFound => "payroll.not_found",
        PayrollOperationError.Forbidden => "payroll.forbidden",
        PayrollOperationError.StateConflict => "payroll.state_conflict",
        _ => "payroll.validation_or_configuration"
    };
    private static bool IsConflict(PostgresException ex) => ex.SqlState is "23505" or "40001" or "40P01" or "P0001";
    private static IResult Page<TSnapshot, TResponse>(IReadOnlyList<TSnapshot> items, int? page, int? pageSize, string? sortBy, string? sortDirection, Func<TSnapshot, TResponse> map, HttpContext context)
    { var currentPage = page.GetValueOrDefault(1); var size = pageSize.GetValueOrDefault(25); if (currentPage < 1 || size is < 1 or > 100) return BadRequest(context); var ordered = items.Select(map); return Results.Ok(new { items = ordered.Skip((currentPage - 1) * size).Take(size), pageInfo = new { page = currentPage, pageSize = size, totalItems = items.Count, totalPages = (int)Math.Ceiling(items.Count / (double)size), sortBy, sortDirection } }); }
    private static DepartamentoResponse ToResponse(DepartamentoSnapshot item) => new(item.Id, item.Input.Nombre, item.Input.Descripcion, item.Activo, item.EmpleadosActivos);
    private static EmployeeResponse ToResponse(EmpleadoSnapshot item) => new(item.Id, item.Input.DepartamentoId, item.Input.TipoDescuentoId, item.Input.Dni, item.Input.Nombres, item.Input.Apellidos, item.Input.Cargo, item.Input.SalarioBase, item.Input.FechaNacimiento, item.Input.FechaIngreso, item.Input.Banco, item.Input.NumeroCuenta, item.Activo, item.Departamento, item.TipoDescuento, item.FechaCreacion);
    private static HorasExtraResponse ToResponse(HorasExtraSnapshot item) => new(item.Id, item.Input.EmpleadoId, item.Input.Periodo, item.Input.HorasPrimerasDos, item.Input.HorasPosteriores, item.Estado, item.FechaRegistro, item.FechaAprobacion, item.AprobadoPorId);
    private static PayrollPeriodResponse ToResponse(PayrollPeriodSnapshot item, string correlationId) => new(item.Id, item.Periodo, item.Estado.ToString(), item.TotalBruto, item.TotalDescuentos, item.TotalNeto, item.TotalProvisionGratificacion, item.TotalProvisionCts, item.CostoPlanilla, item.FechaCalculo, item.FechaFinalizacion, item.FinalizadoPorId, item.AsientoDraftId, item.Resultados, correlationId);
    private static PayrollDashboardResponse ToResponse(PayrollDashboardSnapshot item, string correlationId) => new(item.Periodo, item.EmpleadosActivos, item.EmpleadosElegibles, item.DatosIncompletos, item.PeriodoEstado, item.TotalBruto, item.TotalNeto, item.CostoPlanilla, correlationId);

    private static IResult ValidationProblem(FluentValidation.Results.ValidationResult validation, HttpContext context) => Results.BadRequest(new ErrorResponse(400, "PAYROLL_VALIDATION_ERROR", "Invalid request.", Correlation(context), validation.Errors.Select(x => new ValidationError(x.PropertyName, "Validation", x.ErrorMessage)).ToList()));
    private static IResult BadRequest(HttpContext context) => Results.BadRequest(new ErrorResponse(400, "PAYROLL_VALIDATION_ERROR", "Invalid payroll request.", Correlation(context)));
    private static IResult PayrollValidationError(HttpContext context, PayrollOperationError error) => Results.BadRequest(new ErrorResponse(400, error switch { PayrollOperationError.InvalidPeriod => "PAYROLL_INVALID_PERIOD", PayrollOperationError.NoActiveEmployees => "PAYROLL_NO_ACTIVE_EMPLOYEES", PayrollOperationError.MissingPensionConfiguration => "PAYROLL_MISSING_PENSION_CONFIGURATION", _ => "PAYROLL_VALIDATION_ERROR" }, "Invalid payroll calculation configuration.", Correlation(context)));
    private static IResult Conflict(HttpContext context) => Results.Conflict(new ErrorResponse(409, "PAYROLL_STATE_CONFLICT", "The requested payroll operation conflicts with current state.", Correlation(context)));
    private static IResult NotFound(HttpContext context) => Results.NotFound(new ErrorResponse(404, "PAYROLL_NOT_FOUND", "The requested payroll resource was not found.", Correlation(context)));
    private static IResult Unauthorized(HttpContext context) => Results.Unauthorized();
    private static string Role(ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value ?? "Unknown";
    private static string Correlation(HttpContext context) => context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var value) && value is string id ? id : CorrelationIdMiddleware.ResolveCorrelationId(context.Request);
}
public sealed record DepartamentoResponse(long Id, string Nombre, string? Descripcion, bool Activo, int EmpleadosActivos);
public sealed record EmployeeResponse(long Id, long DepartamentoId, long TipoDescuentoId, string Dni, string Nombres, string Apellidos, string Cargo, decimal SalarioBase, DateOnly FechaNacimiento, DateOnly FechaIngreso, string Banco, string NumeroCuenta, bool Activo, string Departamento, string TipoDescuento, DateTimeOffset FechaCreacion);
public sealed record PayrollCalculationRequest(string Periodo);
public sealed record PayrollPeriodResponse(long Id, string Periodo, string Estado, decimal TotalBruto, decimal TotalDescuentos, decimal TotalNeto, decimal TotalProvisionGratificacion, decimal TotalProvisionCts, decimal CostoPlanilla, DateTimeOffset FechaCalculo, DateTimeOffset? FechaFinalizacion, long? FinalizadoPorId, long? AsientoDraftId, IReadOnlyList<PayrollEmployeeResultSnapshot> Resultados, string CorrelationId);
public sealed record PayrollDashboardResponse(string Periodo, int EmpleadosActivos, int EmpleadosElegibles, int DatosIncompletos, string PeriodoEstado, decimal TotalBruto, decimal TotalNeto, decimal CostoPlanilla, string CorrelationId);
public sealed record HorasExtraResponse(long Id, long EmpleadoId, string Periodo, decimal HorasPrimerasDos, decimal HorasPosteriores, string Estado, DateTimeOffset FechaRegistro, DateTimeOffset? FechaAprobacion, long? AprobadoPorId);
