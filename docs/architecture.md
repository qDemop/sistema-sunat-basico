# Architecture Specification

## Target Architecture

The target architecture combines the PDF specification with the requested SDD stack:

- C# and .NET 10.
- WinForms desktop UI.
- ASP.NET Core 10 Web API.
- Visual Studio Community 2026 (18.6) development baseline.
- PostgreSQL 16.
- JWT authentication and role-based RBAC for Sprint 1.
- Clean Architecture boundaries.
- CQRS for separating commands from queries.
- Repository, Unit of Work, Dependency Injection, and Service Layer patterns.

## Architecture Decisions Applied

| Decision | Architecture Result |
|---|---|
| RBAC is role-based only for Sprint 1. | Authentication owns login/logout only; Administration owns user and role management. No permission catalog is required in Sprint 1. |
| Departments are required. | Payroll and Reports use `Departamento` as a first-class entity and filter dimension. |
| Overtime is required. | Payroll uses `HorasExtra` as period input before payroll calculation. |
| Vouchers need purchase/sales direction. | `Comprobante.tipoMovimiento` is mandatory with values `Compra` and `Venta`. |
| Financial reports need ledger data. | General Ledger is a dedicated module with accounts, periods, journal entries, and entry lines. |
| Tax configuration must be versioned. | Voucher and book records reference the applied tax configuration version. |
| Payroll recalculation is Draft-only. | Payroll periods can be recalculated only while status is `Draft`. |
| SUNAT books must be versioned. | Generated books use `ComprobanteLibro` bridge records instead of a direct voucher-to-book FK. |
| Payroll lifecycle needs one identity. | `PeriodoPlanilla` is the aggregate root addressed by `periodo`; employee results are children without independent state. |
| Pension rates need one owner. | Effective-dated `ConfiguracionDescuentoPrevisionalVersion` replaces hardcoded/domain-catalog percentages. |
| Source events need deterministic accounting. | Payroll finalization and voucher registration create source-linked Draft entries using the approved account mapping. |
| Reports need reproducible exports. | Live queries use Posted/Finalized sources and exports persist snapshot headers/lines. |

## Architectural Principles

- Keep domain rules independent from UI and persistence.
- Separate commands that mutate state from queries that read state.
- Use module boundaries: Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration.
- Require JWT authentication for all protected operations.
- Persist business records in PostgreSQL using a BCNF-oriented relational model.
- Execute critical data operations transactionally.
- Keep exports and report generation as application services, not UI logic.

## Logical Layers

| Layer | Responsibility |
|---|---|
| Presentation | WinForms views, navigation, validation feedback, tables, dashboards, exports initiated by user. |
| Application | Use cases, CQRS handlers, authorization checks, orchestration, DTO contracts. |
| Domain | Entities, value objects, formulas, business rules, invariants. |
| Infrastructure | PostgreSQL, EF Core/Npgsql, stored procedures, JWT provider, export providers, logging. |

## Module Boundaries

| Module | Commands | Queries |
|---|---|---|
| Authentication | Login with lockout, logout with `jti` revocation. | Current user session and role claims. |
| Payroll | Department/employee lifecycle, overtime approve/cancel, calculate/recalculate/finalize/cancel period, export payslips. | Department/employee/overtime lists and period-level payroll results. |
| Accounting SUNAT | Register/update/annul voucher, validate book, generate next book version. | Voucher and immutable book-version queries/exports. |
| General Ledger | Account lifecycle, create/close period, create/edit/post/cancel/reverse entry. | Chart, periods, entries, balances; Gerente is read-only. |
| Reports | Generate report snapshot, export financial report. | Dashboard KPIs, balance sheet, income statement, consolidated payroll. |
| Administration | User lifecycle, assign predefined role, activate/close tax/pension/format versions, review logs. | Users, predefined roles, configuration versions and audit logs. Backup status UI/persistence is implementation pending. |

## Component Model

| Component | Description |
|---|---|
| WinForms UI | Desktop application with login, dashboard, module views, data grids, validation, and exports. |
| API Gateway | Single entry point for REST requests, JWT middleware, logging, error handling, and routing. |
| AuthModule | JWT, BCrypt, role claims, sessions, token validation. |
| PlanillaModule | Department, employee, overtime, payroll calculations, PDF/Excel export orchestration. |
| SunatModule | Voucher registration, IGV, tax-version application, SUNAT book generation. |
| LedgerModule | Chart of accounts, accounting periods, journal entries, posting and balances. |
| ReporteModule | KPIs, balance sheet, income statement, consolidated payroll reports. |
| AdminModule | User/role administration, tax configuration versions, SUNAT formats and audit. Backup execution remains an external operational specification; application tracking is implementation pending. |
| ErpDbContext | PostgreSQL persistence abstraction and migration boundary. |
| PostgreSQL | Normalized relational data, transactions, stored procedures, indexes, constraints. |
| Logging/Audit | Structured logs, correlation IDs, business operation audit trail. |

## Key Flows

### Authentication

1. WinForms sends credentials to `POST /api/auth/login`.
2. API validates user and BCrypt password hash.
3. API applies persistent lockout and issues signed JWT with user ID, role, `jti`, issued-at, and expiration.
4. UI stores token in memory and sends it in protected requests.
5. Dashboard shows modules allowed by the role matrix.

### Payroll Calculation

1. User registers departments, employees, and overtime inputs for a period.
2. User requests payroll calculation for the period.
3. API validates JWT and role.
4. Application verifies that the payroll status is absent or `Draft`.
5. Calculation locks one Draft `PERIODO_PLANILLA`, resolves effective pension versions, and uses Approved overtime.
6. Employee identity, department assignment and applied base-salary snapshots plus applied rates are stored in `PLANILLA`/`DETALLE_PLANILLA`.
7. Finalization changes the period to Finalized, creates one ledger Draft, and blocks recalculation; Draft cancellation is terminal.

### Voucher Registration and Tax Application

1. Accountant submits voucher data with `tipoMovimiento`, document identity, taxable values, and issue date.
2. Application resolves the active tax configuration version for the issue date.
3. Voucher stores the applied tax configuration version.
4. Purchase and sales totals are separated by `tipoMovimiento` and one source-linked ledger Draft is created.

### SUNAT Book Generation

1. Accountant selects period and book type.
2. A non-mutating validation query returns Valida, ConObservaciones, or Bloqueada.
3. `POST /api/libros` resolves the Active SUNAT format and creates the next immutable version.
4. Included vouchers are linked through snapshot `COMPROBANTE_LIBRO` rows retaining each tax version.
5. UI previews the generated version and allows PDF/Excel export.

### Ledger Posting and Reports

1. Accounting operations create draft journal entries in `ASIENTO_CONTABLE`.
2. Posting validates open period, balanced debit/credit totals, and active accounts.
3. Posted entries feed balances by account and period.
4. Balance sheet, income statement, receivables, payables, IGV and KPIs use the canonical account formulas.
5. Report exports persist immutable snapshots before returning files.

## Technical Acceptance Criteria

- New ERP modules can be added without changing existing financial module internals.
- Accounting and payroll operations include audit information: user, date, operation type, and result.
- Purchase and Sales books follow SUNAT electronic-book format requirements.
- Payroll calculations are verifiable within S/ 0.01 per employee.
- All endpoints except login require JWT authentication.
- Database relationships use FK constraints and defined delete/update behavior.
- API errors include structured messages and correlation IDs.
- API read queries respond in less than 500 ms at p95.
