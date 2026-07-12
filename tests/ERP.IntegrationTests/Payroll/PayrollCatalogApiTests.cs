using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ERP.Application.Features.Payroll.Abstractions;
using ERP.Application.Features.Payroll.Contracts;
using ERP.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace ERP.IntegrationTests.Payroll;

public sealed class PayrollCatalogApiTests : IDisposable
{
    private readonly PayrollCatalogApiFixture _fixture;

    public PayrollCatalogApiTests() => _fixture = new PayrollCatalogApiFixture();
    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Department_create_returns_201_contract_correlation_and_duplicate_returns_409()
    {
        var client = _fixture.CreateAuthorizedClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "payroll-department-1");

        var created = await client.PostAsJsonAsync("/api/departamentos", new DepartamentoInput("Operations", "Core team"));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("payroll-department-1", Assert.Single(created.Headers.GetValues("X-Correlation-ID")));
        var body = await created.Content.ReadFromJsonAsync<DepartmentResponse>();
        Assert.NotNull(body);
        Assert.Equal("Operations", body.Nombre);
        Assert.True(body.Activo);

        var duplicate = await client.PostAsJsonAsync("/api/departamentos", new DepartamentoInput("Operations", null));
        await AssertPayrollError(duplicate, HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", "payroll-department-1");
    }

    [Fact]
    public async Task Employee_create_validates_input_returns_201_and_reactivate_returns_200()
    {
        var client = _fixture.CreateAuthorizedClient();
        var invalid = await client.PostAsJsonAsync("/api/empleados", EmployeeInput() with { Dni = "bad" });
        await AssertPayrollError(invalid, HttpStatusCode.BadRequest, "PAYROLL_VALIDATION_ERROR", null);

        var created = await client.PostAsJsonAsync("/api/empleados", EmployeeInput());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var employee = await created.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(employee);
        Assert.Equal("12345678", employee.Dni);

        var duplicate = await client.PostAsJsonAsync("/api/empleados", EmployeeInput());
        await AssertPayrollError(duplicate, HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", null);

        var deactivated = await client.DeleteAsync($"/api/empleados/{employee.Id}");
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        await AssertPayrollError(await client.DeleteAsync($"/api/empleados/{employee.Id}"), HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", null);
        var reactivated = await client.PutAsync($"/api/empleados/{employee.Id}/reactivar", null);
        Assert.Equal(HttpStatusCode.OK, reactivated.StatusCode);
        await AssertPayrollError(await client.PutAsync($"/api/empleados/{employee.Id}/reactivar", null), HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", null);
        Assert.True(_fixture.Repository.EmployeeActive);
    }

    [Fact]
    public async Task Employee_get_missing_and_department_filtered_list_use_predictable_contracts()
    {
        var client = _fixture.CreateAuthorizedClient();

        var missing = await client.GetAsync("/api/empleados/999");
        await AssertPayrollError(missing, HttpStatusCode.NotFound, "PAYROLL_NOT_FOUND", null);

        var list = await client.GetFromJsonAsync<EmployeePage>("/api/empleados?departamentoId=1&search=Ana&activo=true&page=1&pageSize=25");
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.Equal("Ana", list.Items[0].Nombres);
    }

    [Fact]
    public async Task Overtime_approve_returns_200_before_payroll_and_409_after_payroll()
    {
        var client = _fixture.CreateAuthorizedClient();
        var created = await client.PostAsJsonAsync("/api/horas-extra", new HorasExtraInput(0, 1, "2026-07", 2, 1));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var overtime = await created.Content.ReadFromJsonAsync<OvertimeResponse>();
        Assert.NotNull(overtime);

        var approved = await client.PostAsync($"/api/horas-extra/{overtime.Id}/aprobar", null);
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);
        Assert.True(_fixture.Repository.ApproveCalled);

        _fixture.Repository.PayrollExists = true;
        var conflict = await client.PostAsync($"/api/horas-extra/{overtime.Id}/aprobar", null);
        await AssertPayrollError(conflict, HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", null);
    }

    [Fact]
    public async Task Overtime_procedure_state_conflict_is_translated_to_409()
    {
        _fixture.Repository.ThrowProcedureConflict = true;
        var client = _fixture.CreateAuthorizedClient();
        var created = await client.PostAsJsonAsync("/api/horas-extra", new HorasExtraInput(0, 1, "2026-07", 2, 1));
        var overtime = await created.Content.ReadFromJsonAsync<OvertimeResponse>();

        await AssertPayrollError(await client.PostAsync($"/api/horas-extra/{overtime!.Id}/aprobar", null), HttpStatusCode.Conflict, "PAYROLL_STATE_CONFLICT", null);
    }

    [Fact]
    public async Task Payroll_routes_require_authenticated_hr_role()
    {
        var anonymous = _fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/departamentos")).StatusCode);

        var accountant = _fixture.CreateAuthorizedClient("Contador");
        Assert.Equal(HttpStatusCode.Forbidden, (await accountant.GetAsync("/api/departamentos")).StatusCode);
    }

    private static EmpleadoInput EmployeeInput() => new(1, 1, "12345678", "Ana", "Perez", "Analista", 2500m, new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "Banco", "12345678901234");

    private static async Task AssertPayrollError(HttpResponseMessage response, HttpStatusCode status, string code, string? correlation)
    {
        Assert.Equal(status, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PayrollError>();
        Assert.NotNull(body);
        Assert.Equal(code, body.Code);
        if (correlation is not null) Assert.Equal(correlation, body.CorrelationId);
    }

    private sealed class PayrollError { public string Code { get; set; } = string.Empty; public string CorrelationId { get; set; } = string.Empty; }
    private sealed class EmployeeResponse { public long Id { get; set; } public string Dni { get; set; } = string.Empty; public string Nombres { get; set; } = string.Empty; }
    private sealed class EmployeePage { public List<EmployeeResponse> Items { get; set; } = []; }
    private sealed class OvertimeResponse { public long Id { get; set; } }
    private sealed class DepartmentResponse { public string Nombre { get; set; } = string.Empty; public bool Activo { get; set; } }
}

public sealed class PayrollCatalogApiFixture : IDisposable
{
    private const string Key = "CHANGE_ME_PLACEHOLDER_KEY_AT_LEAST_32_CHARS_LONG_";
    private readonly WebApplicationFactory<Program> _factory;

    public PayrollCatalogApiFixture()
    {
        Repository = new FakeCatalogRepository();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPayrollCatalogRepository>();
            services.RemoveAll<ITokenRevocationRepository>();
            services.AddSingleton<IPayrollCatalogRepository>(Repository);
            services.AddSingleton<ITokenRevocationRepository, TestTokenRevocationRepository>();
        }));
    }

    public FakeCatalogRepository Repository { get; }
    public HttpClient CreateClient() => _factory.CreateClient();
    public HttpClient CreateAuthorizedClient(string role = "Administrador RRHH")
    {
        var client = CreateClient();
        var token = new JwtSecurityToken("ERP.API", "ERP.WinForms", [new Claim(JwtRegisteredClaimNames.Sub, "7"), new Claim("role", role), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))], expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }
    public void Dispose() => _factory.Dispose();

    private sealed class TestTokenRevocationRepository : ITokenRevocationRepository
    {
        public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task RevokeTokenAsync(string jti, long userId, DateTime expiraEn, string correlationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public sealed class FakeCatalogRepository : IPayrollCatalogRepository
    {
        private readonly Dictionary<long, DepartamentoSnapshot> _departments = new();
        private readonly Dictionary<long, EmpleadoSnapshot> _employees = new() { [1] = Employee(1) };
        private HorasExtraSnapshot? _overtime;
        public bool PayrollExists { get; set; }
        public bool EmployeeActive { get; private set; }
        public bool ApproveCalled { get; private set; }
        public bool ThrowProcedureConflict { get; set; }
        public Task<DepartamentoSnapshot> CreateDepartmentAsync(DepartamentoInput input, CancellationToken ct = default) { if (_departments.Values.Any(x => x.Input.Nombre == input.Nombre)) throw new CatalogConflictException("duplicate"); var result = new DepartamentoSnapshot(_departments.Count + 1, input, true, 0); _departments[result.Id] = result; return Task.FromResult(result); }
        public Task<DepartamentoSnapshot?> GetDepartmentAsync(long id, CancellationToken ct = default) => Task.FromResult(_departments.GetValueOrDefault(id));
        public Task<IReadOnlyList<DepartamentoSnapshot>> ListDepartmentsAsync(bool? active, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<DepartamentoSnapshot>>(_departments.Values.ToArray());
        public Task<bool> UpdateDepartmentAsync(long id, DepartamentoInput input, CancellationToken ct = default) => Task.FromResult(_departments.ContainsKey(id));
        public Task<bool> SetDepartmentActiveAsync(long id, bool active, CancellationToken ct = default) => Task.FromResult(_departments.ContainsKey(id));
        public Task<EmpleadoSnapshot> CreateEmployeeAsync(EmpleadoInput input, CancellationToken ct = default) { if (_employees.Values.Any(x => x.Input.Dni == input.Dni)) throw new CatalogConflictException("duplicate"); var result = Employee(2, input); _employees[result.Id] = result; return Task.FromResult(result); }
        public Task<EmpleadoSnapshot?> GetEmployeeAsync(long id, CancellationToken ct = default) => Task.FromResult(_employees.GetValueOrDefault(id));
        public Task<IReadOnlyList<EmpleadoSnapshot>> ListEmployeesAsync(long? departmentId, string? search, bool? active, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<EmpleadoSnapshot>>(_employees.Values.Where(x => (!departmentId.HasValue || x.Input.DepartamentoId == departmentId) && (search is null || x.Input.Nombres.Contains(search, StringComparison.OrdinalIgnoreCase))).ToArray());
        public Task<bool> UpdateEmployeeAsync(long id, EmpleadoInput input, CancellationToken ct = default) => Task.FromResult(_employees.ContainsKey(id));
        public Task<bool> SetEmployeeActiveAsync(long id, bool active, CancellationToken ct = default) { if (!_employees.TryGetValue(id, out var employee)) return Task.FromResult(false); EmployeeActive = active; _employees[id] = employee with { Activo = active }; return Task.FromResult(true); }
        public Task<bool> ReactivateEmployeeAsync(long id, CancellationToken ct = default) => SetEmployeeActiveAsync(id, true, ct);
        public Task<HorasExtraSnapshot> CreateOvertimeAsync(HorasExtraInput input, CancellationToken ct = default) { _overtime = new HorasExtraSnapshot(9, input with { Id = 9 }, "Draft", DateTimeOffset.UtcNow, null, null); return Task.FromResult(_overtime); }
        public Task<HorasExtraSnapshot?> GetOvertimeAsync(long id, CancellationToken ct = default) => Task.FromResult(id == 9 ? _overtime : null);
        public Task<IReadOnlyList<HorasExtraSnapshot>> ListOvertimeAsync(long? employeeId, string? period, string? state, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<HorasExtraSnapshot>>(_overtime is null ? [] : [_overtime]);
        public Task<bool> UpdateOvertimeAsync(long id, HorasExtraInput input, CancellationToken ct = default) => Task.FromResult(id == 9 && _overtime is not null);
        public Task<bool> PayrollPeriodExistsAsync(string period, CancellationToken ct = default) => Task.FromResult(PayrollExists);
        public Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken ct = default) { if (ThrowProcedureConflict) throw new PostgresException("state changed", "ERROR", "ERROR", "P0001"); ApproveCalled = true; return Task.CompletedTask; }
        public Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken ct = default) => Task.CompletedTask;
        private static EmpleadoSnapshot Employee(long id, EmpleadoInput? input = null) { input ??= new EmpleadoInput(1, 1, "87654321", "Ana", "Perez", "Analista", 2500m, new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "Banco", "12345678901234"); return new EmpleadoSnapshot(id, input, true, "Operations", "AFP", DateTimeOffset.UtcNow); }
    }
}
