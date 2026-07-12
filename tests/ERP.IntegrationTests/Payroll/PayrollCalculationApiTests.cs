using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
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

    private sealed class PayrollPeriodResponse { public string Estado { get; set; } = string.Empty; public decimal TotalNeto { get; set; } }
    private sealed class PayrollError { public string Code { get; set; } = string.Empty; }
}

public sealed class PayrollCalculationApiFixture : IDisposable
{
    private const string Key = "CHANGE_ME_PLACEHOLDER_KEY_AT_LEAST_32_CHARS_LONG_";
    public PayrollCalculationApiFixture()
    {
        Repository = new FakeCalculationRepository();
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPayrollRepository>();
            services.RemoveAll<ITokenRevocationRepository>();
            services.AddSingleton<IPayrollRepository>(Repository);
            services.AddSingleton<ITokenRevocationRepository, TestTokenRevocationRepository>();
        }));
    }

    private WebApplicationFactory<Program> Factory { get; }
    public FakeCalculationRepository Repository { get; }
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

    public sealed class FakeCalculationRepository : IPayrollRepository
    {
        public PayrollOperationContext? Calculation { get; private set; }
        public bool ThrowConfigurationError { get; set; }
        public Exception? Error { get; set; }
        public Task CalculateAsync(PayrollOperationContext context, CancellationToken cancellationToken = default)
        {
            Calculation = context;
            if (Error is not null) throw Error;
            if (ThrowConfigurationError) throw new PayrollOperationException(PayrollOperationError.ValidationOrConfiguration);
            return Task.CompletedTask;
        }
        public Task FinalizeAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelAsync(PayrollOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PayrollPeriodSnapshot?> GetByPeriodAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult<PayrollPeriodSnapshot?>(new PayrollPeriodSnapshot(1, periodo, PeriodoPlanillaEstado.Draft, 2000m, 200m, 1800m, 333.33m, 194.44m));
        public Task<IReadOnlyList<PayrollPeriodSnapshot>> ListPeriodsAsync(string? estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PayrollPeriodSnapshot>>([]);
        public Task<PayrollDashboardSnapshot> GetDashboardAsync(string periodo, CancellationToken cancellationToken = default) => Task.FromResult(new PayrollDashboardSnapshot(periodo, 1, 1, 0, "Draft", 2000m, 1800m, 2527.77m));
    }
}
