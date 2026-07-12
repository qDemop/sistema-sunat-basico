using ERP.Application.Features.Payroll.Contracts;

namespace ERP.Application.Features.Payroll.Abstractions;

public interface IPayrollCatalogRepository
{
    Task<DepartamentoSnapshot> CreateDepartmentAsync(DepartamentoInput input, CancellationToken cancellationToken = default);
    Task<DepartamentoSnapshot?> GetDepartmentAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartamentoSnapshot>> ListDepartmentsAsync(bool? active, CancellationToken cancellationToken = default);
    Task<bool> UpdateDepartmentAsync(long id, DepartamentoInput input, CancellationToken cancellationToken = default);
    Task<bool> SetDepartmentActiveAsync(long id, bool active, CancellationToken cancellationToken = default);
    Task<EmpleadoSnapshot> CreateEmployeeAsync(EmpleadoInput input, CancellationToken cancellationToken = default);
    Task<EmpleadoSnapshot?> GetEmployeeAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmpleadoSnapshot>> ListEmployeesAsync(long? departmentId, string? search, bool? active, CancellationToken cancellationToken = default);
    Task<bool> UpdateEmployeeAsync(long id, EmpleadoInput input, CancellationToken cancellationToken = default);
    Task<bool> SetEmployeeActiveAsync(long id, bool active, CancellationToken cancellationToken = default);
    Task<HorasExtraSnapshot> CreateOvertimeAsync(HorasExtraInput input, CancellationToken cancellationToken = default);
    Task<HorasExtraSnapshot?> GetOvertimeAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HorasExtraSnapshot>> ListOvertimeAsync(long? employeeId, string? period, string? state, CancellationToken cancellationToken = default);
    Task<bool> UpdateOvertimeAsync(long id, HorasExtraInput input, CancellationToken cancellationToken = default);
    Task<bool> PayrollPeriodExistsAsync(string period, CancellationToken cancellationToken = default);
    Task<bool> ReactivateEmployeeAsync(long id, CancellationToken cancellationToken = default);
    Task ApproveOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
    Task CancelOvertimeAsync(OvertimeOperationContext context, CancellationToken cancellationToken = default);
}
