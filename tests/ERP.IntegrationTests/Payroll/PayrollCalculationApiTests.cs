using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.IO.Compression;
using System.Xml.Linq;
using ERP.Application.Abstractions;
using ERP.API.Endpoints;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Domain.Payroll;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.IntegrationTests.Payroll;

public sealed class PayrollCalculationApiTests : IDisposable
{
    private readonly PayrollCalculationApiFixture _fixture = new();
    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Calculate_returns_persisted_projection_and_passes_actor_and_correlation_to_repository()
    {
        var client = _fixture.CreateAuthorizedClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "calculation-1");

        var response = await client.PostAsJsonAsync("/api/planilla/calcular", new PayrollCalculationRequest("2026-07"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PayrollPeriodResponse>();
        Assert.NotNull(body);
        Assert.Equal("Draft", body.Estado);
        Assert.Equal(1800m, body.TotalNeto);
        Assert.Equal(new PayrollOperationContext("2026-07", 7, "calculation-1"), _fixture.Repository.Calculation);
    }

    [Fact]
    public async Task Calculation_routes_reject_non_payroll_role_and_translate_configuration_error()
    {
        var unauthorized = _fixture.CreateAuthorizedClient("Contador");
        Assert.Equal(HttpStatusCode.Forbidden, (await unauthorized.PostAsJsonAsync("/api/planilla/calcular", new PayrollCalculationRequest("2026-07"))).StatusCode);

        _fixture.Repository.ThrowConfigurationError = true;
        var response = await _fixture.CreateAuthorizedClient().PostAsJsonAsync("/api/planilla/calcular", new PayrollCalculationRequest("2026-08"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Calculation_returns_safe_configuration_code_but_leaves_unexpected_errors_as_500()
    {
        _fixture.Repository.Error = new PayrollOperationException(PayrollOperationError.MissingPensionConfiguration);
        var known = await _fixture.CreateAuthorizedClient().PostAsJsonAsync("/api/planilla/calcular", new PayrollCalculationRequest("2026-08"));
        Assert.Equal(HttpStatusCode.BadRequest, known.StatusCode);
        Assert.Equal("PAYROLL_MISSING_PENSION_CONFIGURATION", (await known.Content.ReadFromJsonAsync<PayrollError>())!.Code);

        _fixture.Repository.Error = new InvalidOperationException("unexpected");
        var unknown = await _fixture.CreateAuthorizedClient().PostAsJsonAsync("/api/planilla/calcular", new PayrollCalculationRequest("2026-08"));
        Assert.Equal(HttpStatusCode.InternalServerError, unknown.StatusCode);
    }

    [Fact]
    public async Task Lifecycle_routes_are_rbac_protected_and_return_persisted_terminal_projections()
    {
        var unauthorized = _fixture.CreateAuthorizedClient("Contador");
        Assert.Equal(HttpStatusCode.Forbidden, (await unauthorized.PostAsync("/api/planilla/2026-07/finalizar", null)).StatusCode);

        var client = _fixture.CreateAuthorizedClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "finalize-1");
        var finalized = await client.PostAsync("/api/planilla/2026-07/finalizar", null);
        var finalizedBody = await finalized.Content.ReadFromJsonAsync<PayrollPeriodResponse>();

        Assert.Equal(HttpStatusCode.OK, finalized.StatusCode);
        Assert.NotNull(finalizedBody);
        Assert.Equal("Finalized", finalizedBody.Estado);
        Assert.Equal(99, finalizedBody.AsientoDraftId);
        Assert.Equal(new PayrollOperationContext("2026-07", 7, "finalize-1"), _fixture.Repository.Finalization);

        var cancelled = await _fixture.CreateAuthorizedClient().PostAsync("/api/planilla/2026-08/cancelar", null);
        var cancelledBody = await cancelled.Content.ReadFromJsonAsync<PayrollPeriodResponse>();
        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        Assert.NotNull(cancelledBody);
        Assert.Equal("Cancelled", cancelledBody.Estado);
        Assert.Null(cancelledBody.AsientoDraftId);
    }

    [Fact]
    public async Task Lifecycle_failure_returns_stable_conflict_and_writes_sanitized_failure_audit()
    {
        _fixture.Repository.Error = new PayrollOperationException(PayrollOperationError.StateConflict);
        var client = _fixture.CreateAuthorizedClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "failure-1");

        var response = await client.PostAsync("/api/planilla/2026-07/finalizar", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var audit = Assert.Single(_fixture.Audit.Events);
        Assert.Equal(7, audit.UsuarioId);
        Assert.Equal("Administrador RRHH", audit.RolActor);
        Assert.Equal("FINALIZAR_PLANILLA", audit.Accion);
        Assert.Equal("Failure", audit.Resultado);
        Assert.Equal("failure-1", audit.CorrelationId);
        Assert.Equal("payroll.state_conflict", audit.Datos["code"]);
    }

    [Fact]
    public async Task Lifecycle_recovery_is_idempotent_and_a_response_read_failure_cannot_create_a_failure_audit()
    {
        _fixture.Repository.ThrowOnGetByPeriod = true;
        var client = _fixture.CreateAuthorizedClient();

        var first = await client.PostAsync("/api/planilla/2026-07/finalizar", null);
        var second = await client.PostAsync("/api/planilla/2026-07/finalizar", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal("Finalized", (await second.Content.ReadFromJsonAsync<PayrollPeriodResponse>())!.Estado);
        Assert.Empty(_fixture.Audit.Events);
    }

    [Fact]
    public async Task Canonical_authorization_failure_is_a_stable_forbidden_response_and_audited_once()
    {
        _fixture.Repository.Error = new PayrollOperationException(PayrollOperationError.Forbidden);

        var response = await _fixture.CreateAuthorizedClient().PostAsync("/api/planilla/2026-07/cancelar", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var audit = Assert.Single(_fixture.Audit.Events);
        Assert.Equal("payroll.forbidden", audit.Datos["code"]);
    }

    [Fact]
    public async Task Export_endpoints_render_only_the_persisted_payroll_projection()
    {
        var client = _fixture.CreateAuthorizedClient();

        var excel = await client.GetAsync("/api/planilla/2026-07/export/excel");
        var pdf = await client.GetAsync("/api/planilla/2026-07/export/pdf");

        Assert.Equal(HttpStatusCode.OK, excel.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excel.Content.Headers.ContentType!.MediaType);
        var workbook = await excel.Content.ReadAsByteArrayAsync();
        using var xlsx = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var sheet = XDocument.Load(xlsx.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        Assert.Contains(sheet.Descendants().Select(x => x.Value), value => value == "1800.00");
        Assert.Contains("t=\"n\"", Encoding.UTF8.GetString(await ReadEntryAsync(xlsx, "xl/worksheets/sheet1.xml")));
        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal("application/zip", pdf.Content.Headers.ContentType!.MediaType);
        using var payslips = new ZipArchive(new MemoryStream(await pdf.Content.ReadAsByteArrayAsync()), ZipArchiveMode.Read);
        var entry = Assert.Single(payslips.Entries);
        var document = Encoding.ASCII.GetString(await ReadEntryAsync(payslips, entry.FullName));
        var startXref = int.Parse(document[(document.LastIndexOf("startxref", StringComparison.Ordinal) + 10)..].Trim().Split('\n')[0]);
        Assert.Equal("xref", document.Substring(startXref, 4));
        Assert.Contains("Neto S/ 1800.00", document);
    }

    [Fact]
    public async Task Export_endpoints_return_not_found_for_a_missing_persisted_period()
    {
        _fixture.Repository.ReturnMissingPeriod = true;

        var response = await _fixture.CreateAuthorizedClient().GetAsync("/api/planilla/2026-08/export/excel");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class PayrollPeriodResponse { public string Estado { get; set; } = string.Empty; public decimal TotalNeto { get; set; } public long? AsientoDraftId { get; set; } }
    private sealed class PayrollError { public string Code { get; set; } = string.Empty; }
    private static async Task<byte[]> ReadEntryAsync(ZipArchive archive, string name)
    {
        await using var stream = archive.GetEntry(name)!.Open();
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        return copy.ToArray();
    }
}

public sealed class PayrollCalculationApiFixture : IDisposable
{
    private const string Key = "CHANGE_ME_PLACEHOLDER_KEY_AT_LEAST_32_CHARS_LONG_";
    public PayrollCalculationApiFixture()
    {
        Repository = new FakeCalculationRepository();
        Audit = new FakeAuditWriter();
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPayrollRepository>();
            services.RemoveAll<ITokenRevocationRepository>();
            services.AddSingleton<IPayrollRepository>(Repository);
            services.RemoveAll<IAuditWriter>();
            services.AddSingleton<IAuditWriter>(Audit);
            services.AddSingleton<ITokenRevocationRepository, TestTokenRevocationRepository>();
        }));
    }

    private WebApplicationFactory<Program> Factory { get; }
    public FakeCalculationRepository Repository { get; }
    public FakeAuditWriter Audit { get; }
    public HttpClient CreateAuthorizedClient(string role = "Administrador RRHH")
    {
        var client = Factory.CreateClient();
        var token = new JwtSecurityToken("ERP.API", "ERP.WinForms", [new Claim(JwtRegisteredClaimNames.Sub, "7"), new Claim("role", role), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))], expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }
    public void Dispose() => Factory.Dispose();

    private sealed class TestTokenRevocationRepository : ITokenRevocationRepository
    {
        public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task RevokeTokenAsync(string jti, long userId, DateTime expiraEn, string correlationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public sealed class FakeAuditWriter : IAuditWriter
    {
        public List<ERP.Application.Features.Authentication.AuditEventRecord> Events { get; } = [];
        public Task WriteAsync(ERP.Application.Features.Authentication.AuditEventRecord record, CancellationToken cancellationToken = default)
        {
            Events.Add(record);
            return Task.CompletedTask;
        }
    }

    public sealed class FakeCalculationRepository : IPayrollRepository
    {
        public PayrollOperationContext? Calculation { get; private set; }
        public PayrollOperationContext? Finalization { get; private set; }
        public PayrollOperationContext? Cancellation { get; private set; }
        public bool ThrowConfigurationError { get; set; }
        public bool ThrowOnGetByPeriod { get; set; }
        public bool ReturnMissingPeriod { get; set; }
        public Exception? Error { get; set; }
        public Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default)
        {
            Calculation = context;
            if (Error is not null) throw Error;
            if (ThrowConfigurationError) throw new PayrollOperationException(PayrollOperationError.ValidationOrConfiguration);
            return Task.CompletedTask;
        }
        public Task<PayrollPeriodSnapshot> FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { Finalization = context; if (Error is not null) throw Error; return Task.FromResult(Snapshot(context.Periodo, PeriodoPlanillaEstado.Finalized, 99)); }
        public Task<PayrollPeriodSnapshot> CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) { Cancellation = context; if (Error is not null) throw Error; return Task.FromResult(Snapshot(context.Periodo, PeriodoPlanillaEstado.Cancelled, null)); }
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default)
        {
            if (ReturnMissingPeriod) return Task.FromResult<PayrollPeriodSnapshot?>(null);
            if (ThrowOnGetByPeriod) throw new InvalidOperationException("Projection read failed.");
            var finalized = Finalization?.Periodo == periodo;
            var cancelled = Cancellation?.Periodo == periodo;
            return Task.FromResult<PayrollPeriodSnapshot?>(Snapshot(periodo, finalized ? PeriodoPlanillaEstado.Finalized : cancelled ? PeriodoPlanillaEstado.Cancelled : PeriodoPlanillaEstado.Draft, finalized ? 99 : null));
        }
        private static PayrollPeriodSnapshot Snapshot(string periodo, PeriodoPlanillaEstado state, long? asientoDraftId) => new(1, periodo, state, 2000m, 200m, 1800m, 333.33m, 194.44m) { AsientoDraftId = asientoDraftId, Resultados = [new PayrollEmployeeResultSnapshot(1, "Ana Perez", 1, "RRHH", 2000m, 0m, 2000m, "AFP", 1, 10m, 200m, 0m, 0m, 200m, 1800m, 333.33m, 194.44m, 2527.77m)] };
        public Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PayrollPeriodSnapshot>>([]);
        public Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult(new PayrollDashboardSnapshot(periodo, 1, 1, 0, "Draft", 2000m, 1800m, 2527.77m));
    }
}
