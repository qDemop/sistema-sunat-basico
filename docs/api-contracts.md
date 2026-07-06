# API Contracts

Canonical OpenAPI files are `specs/*/api-contract.yaml`; reusable errors and pagination are in `specs/shared-api-components.yaml`.

## Shared Contract

- All operations declare `operationId`, typed success/error responses, and `x-roles`.
- Every protected request requires JWT and accepts optional `X-Correlation-ID`; the API generates one when absent.
- Actor user/role are always taken from JWT, never request bodies.
- Variable collection operations use `page`, `pageSize`, `sortBy`, and `sortDirection`; module filters are additional parameters. The fixed four-role catalog is the only unpaged collection.
- Collection responses contain `items` and `pageInfo`.
- Standard error fields are `status`, `code`, `message`, `correlationId`, and optional field validation errors.
- Money, rates, and hour quantities accepted by request DTOs use decimal values with at most two fractional digits (`multipleOf: 0.01`); reporting periods use canonical `YYYY-MM` patterns.
- Role labels in the tables are display abbreviations only: `RRHH` = `Administrador RRHH`, `Admin` = `Administrador Sistema`, `Gerente` = `Gerente Financiero`; OpenAPI `x-roles` always uses the full canonical names.

## Authentication

| Method | Path | Purpose | Roles |
|---|---|---|---|
| POST | `/api/auth/login` | Authenticate, apply lockout, issue JWT with `jti`. | Public |
| POST | `/api/auth/logout` | Revoke current JWT `jti` until expiration. | Any authenticated |

## Payroll

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/api/departamentos` | List departments. | RRHH, Admin |
| POST | `/api/departamentos` | Create department. | RRHH, Admin |
| PUT | `/api/departamentos/{id}` | Update department. | RRHH, Admin |
| DELETE | `/api/departamentos/{id}` | Deactivate department. | RRHH, Admin |
| PUT | `/api/departamentos/{id}/reactivar` | Reactivate department. | RRHH, Admin |
| GET | `/api/empleados` | List employees. | RRHH, Admin |
| POST | `/api/empleados` | Create employee. | RRHH, Admin |
| GET | `/api/empleados/{id}` | Get employee detail. | RRHH, Admin |
| PUT | `/api/empleados/{id}` | Update employee. | RRHH, Admin |
| DELETE | `/api/empleados/{id}` | Deactivate employee. | RRHH, Admin |
| PUT | `/api/empleados/{id}/reactivar` | Reactivate employee. | RRHH, Admin |
| GET | `/api/horas-extra` | List overtime. | RRHH, Admin |
| POST | `/api/horas-extra` | Register Draft overtime. | RRHH, Admin |
| POST | `/api/horas-extra/{id}/aprobar` | Draft -> Approved. | RRHH, Admin |
| POST | `/api/horas-extra/{id}/cancelar` | Draft/Approved -> Cancelled before payroll Draft. | RRHH, Admin |
| POST | `/api/planilla/calcular` | Create/recalculate Draft aggregate by period. | RRHH, Admin |
| GET | `/api/planilla/periodos` | List payroll-period aggregates for PAY-01. | RRHH, Admin |
| POST | `/api/planilla/{periodo}/finalizar` | Draft -> Finalized and create ledger Draft. | RRHH, Admin |
| POST | `/api/planilla/{periodo}/cancelar` | Draft -> Cancelled. | RRHH, Admin |
| GET | `/api/planilla` | Get period aggregate/results. | RRHH, Admin |
| GET | `/api/planilla/dashboard` | Operational payroll KPIs for Inicio. | RRHH, Admin |
| GET | `/api/planilla/{periodo}/export/pdf` | Export calculated Draft or Finalized payslips; Cancelled returns conflict. | RRHH, Admin |
| GET | `/api/planilla/{periodo}/export/excel` | Export calculated Draft or Finalized payroll; Cancelled returns conflict. | RRHH, Admin |

## Accounting SUNAT

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/api/comprobantes` | List vouchers. | Contador, Admin |
| POST | `/api/comprobantes` | Register voucher and source Draft. | Contador, Admin |
| GET | `/api/comprobantes/{id}` | Get voucher detail. | Contador, Admin |
| PUT | `/api/comprobantes/{id}` | Update only while unlocked and source Draft. | Contador, Admin |
| DELETE | `/api/comprobantes/{id}` | Annul and cancel/reverse source entry. | Contador, Admin |
| GET | `/api/comprobantes/dashboard` | Operational voucher, IGV, period and book KPIs for Inicio. | Contador, Admin |
| GET | `/api/libros/validacion` | Validate period/type without mutation. | Contador, Admin |
| POST | `/api/libros` | Generate next immutable version. | Contador, Admin |
| GET | `/api/libros` | List versions by filters. | Contador, Admin |
| GET | `/api/libros/{id}` | Get version and snapshot rows. | Contador, Admin |
| GET | `/api/libros/{id}/export/pdf` | Export selected version as PDF. | Contador, Admin |
| GET | `/api/libros/{id}/export/excel` | Export selected version using stored format. | Contador, Admin |

## General Ledger

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/api/contabilidad/periodos` | List periods. | Contador, Gerente, Admin |
| POST | `/api/contabilidad/periodos` | Create Open period. | Contador, Admin |
| POST | `/api/contabilidad/periodos/{id}/cerrar` | Open -> Closed. | Contador, Admin |
| GET | `/api/contabilidad/cuentas` | List accounts. | Contador, Gerente, Admin |
| POST | `/api/contabilidad/cuentas` | Create account. | Contador, Admin |
| GET | `/api/contabilidad/cuentas/{id}` | Get account detail and posted balance. | Contador, Gerente, Admin |
| PUT | `/api/contabilidad/cuentas/{id}` | Update account. | Contador, Admin |
| DELETE | `/api/contabilidad/cuentas/{id}` | Deactivate account. | Contador, Admin |
| PUT | `/api/contabilidad/cuentas/{id}/reactivar` | Reactivate account. | Contador, Admin |
| GET | `/api/contabilidad/asientos` | List journal entries. | Contador, Gerente, Admin |
| POST | `/api/contabilidad/asientos` | Create Draft entry. | Contador, Admin |
| GET | `/api/contabilidad/asientos/{id}` | Get journal-entry detail. | Contador, Gerente, Admin |
| PUT | `/api/contabilidad/asientos/{id}` | Edit Draft header/lines. | Contador, Admin |
| POST | `/api/contabilidad/asientos/{id}/postear` | Draft -> Posted. | Contador, Admin |
| POST | `/api/contabilidad/asientos/{id}/cancelar` | Draft -> Cancelled. | Contador, Admin |
| POST | `/api/contabilidad/asientos/{id}/revertir` | Create linked reversal Draft. | Contador, Admin |

Reopening a Closed period is out of scope and has no endpoint.

## Reports

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/api/reportes/dashboard` | Canonical KPI values. | Gerente, Admin |
| GET | `/api/reportes/balance-general` | Posted-ledger balance sheet. | Gerente, Admin |
| GET | `/api/reportes/estado-resultados` | Posted-ledger income statement. | Gerente, Admin |
| GET | `/api/reportes/planilla-consolidada` | Finalized payroll consolidation. | Gerente, Admin |
| POST | `/api/reportes/{tipo}/exportaciones` | Persist snapshot and generate PDF/Excel. | Gerente, Admin |
| GET | `/api/reportes/exportaciones` | List export snapshots. | Gerente, Admin |
| GET | `/api/reportes/exportaciones/{id}/archivo` | Download generated snapshot file. | Gerente, Admin |

## Administration

| Method | Path | Purpose | Roles |
|---|---|---|---|
| GET | `/api/admin/usuarios` | List users. | Admin |
| POST | `/api/admin/usuarios` | Create user. | Admin |
| GET | `/api/admin/usuarios/{id}` | Get user detail. | Admin |
| PUT | `/api/admin/usuarios/{id}` | Update user. | Admin |
| DELETE | `/api/admin/usuarios/{id}` | Deactivate user. | Admin |
| PUT | `/api/admin/usuarios/{id}/reactivar` | Reactivate user. | Admin |
| POST | `/api/admin/usuarios/{id}/reset-password` | Reset password. | Admin |
| GET | `/api/admin/roles` | List four predefined roles. | Admin |
| GET | `/api/admin/config/tributaria` | List IGV versions. | Admin |
| POST | `/api/admin/config/tributaria` | Create Draft IGV version. | Admin |
| PUT | `/api/admin/config/tributaria/{id}/activar` | Draft -> Active. | Admin |
| PUT | `/api/admin/config/tributaria/{id}/cerrar` | Active -> Closed. | Admin |
| GET | `/api/admin/config/previsional` | List AFP/ONP versions. | Admin |
| POST | `/api/admin/config/previsional` | Create Draft AFP/ONP version. | Admin |
| PUT | `/api/admin/config/previsional/{id}/activar` | Draft -> Active. | Admin |
| PUT | `/api/admin/config/previsional/{id}/cerrar` | Active -> Closed. | Admin |
| GET | `/api/admin/config/sunat-formatos` | List SUNAT format versions. | Admin |
| POST | `/api/admin/config/sunat-formatos` | Create Draft format version. | Admin |
| PUT | `/api/admin/config/sunat-formatos/{id}/activar` | Draft -> Active. | Admin |
| PUT | `/api/admin/config/sunat-formatos/{id}/cerrar` | Active -> Closed. | Admin |
| GET | `/api/admin/audit` | Query audit events. | Admin |
| GET | `/api/admin/audit/{id}` | Get immutable audit-event detail. | Admin |

Role creation/deletion, fine-grained permission catalogs, refresh tokens, period reopening, finalized-payroll adjustment, and direct SUNAT submission are out of scope.

## Canonical DTO Ownership

| Area | Required DTO families |
|---|---|
| Authentication | LoginRequest/Response, LogoutResponse, UserSession. |
| Payroll | DepartamentoRequest/Response, EmployeeRequest/Response, HorasExtraRequest/Response, PayrollCalculationRequest, PeriodoPlanillaResponse, PayrollEmployeeResult. |
| Accounting | ComprobanteRequest/Response, LibroValidationResponse, LibroGenerationRequest, LibroContableResponse. |
| Ledger | PeriodoContableRequest/Response, CuentaContableRequest/Response, AsientoContableRequest/Response, ReversalRequest. |
| Reports | DashboardKpis, FinancialReportResponse, PayrollReportResponse, ReportExportRequest/Response. |
| Administration | UserRequest/Response, RoleResponse, Tax/Pension/FormatVersionRequest/Response, AuditEventResponse. |
