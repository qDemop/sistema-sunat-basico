# SDD Architecture

> Merged from `sdd-architecture.md` and `architecture.md` on Sprint 0 remediation. The former `architecture.md` is superseded; its unique content (Logical Layers, Component Model, Key Flows, Technical Acceptance Criteria, and four extra applied decisions) is incorporated here.

## Source Documents

This architecture is derived from:

- `BITACORA -ERP.pdf`
- `Entregable.pdf`
- Existing extracted SDD files under `docs/`, `specs/`, `database/`, and `tests/`.

## Architecture Objective

Define the complete Spec Driven Development architecture for the ERP Financial and Accounting module before implementation. This document organizes the system into domain model, database design, API contracts, module specifications, and implementation roadmap.

The repository remains specification-first. Only compile-ready API/WinForms startup scaffolding and project structure exist; no business feature implementation is included.

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

## Target Stack

| Area | Decision |
|---|---|
| UI | WinForms desktop application. |
| Language/runtime | C# with .NET 10. |
| Backend | ASP.NET Core 10 Web API. |
| Development environment | Visual Studio Community 2026 (18.6). |
| Database | PostgreSQL 16. |
| Security | JWT, BCrypt, role-based RBAC for Sprint 1. |
| Architecture | Clean Architecture with CQRS. |
| Persistence | Dapper + Npgsql and PostgreSQL stored procedures for critical operations. |
| Reports | PDF and Excel export providers. |

## Applied Decisions

| Topic | Decision |
|---|---|
| Authorization | Role-based RBAC only for Sprint 1; fine-grained permissions are deferred. The four predefined roles are `Administrador Sistema`, `Administrador RRHH`, `Contador`, and `Gerente Financiero` (Consulta). Authentication owns login/logout only; Administration owns user and role management. No permission catalog is required in Sprint 1. |
| Payroll organization | `Departamento` is a required entity and report filter. |
| Payroll inputs | `HorasExtra` is persisted per employee and period; only Approved overtime is included in payroll. |
| Payroll lifecycle | Recalculation is allowed only while payroll status is `Draft`; `Finalized` and `Cancelled` are terminal. One `PeriodoPlanilla` aggregate owns the period state; employee results are children without independent state. |
| Voucher classification | `Comprobante.tipoMovimiento` separates `Compra` and `Venta`. |
| Ledger | `CuentaContable`, `PeriodoContable`, `AsientoContable`, and `DetalleAsiento` support financial statements. |
| Tax configuration | Tax parameters are persisted as non-overlapping effective-dated versions; the applied version is stored with each voucher and payroll result. |
| SUNAT books | Book versions link vouchers through `ComprobanteLibro` snapshot bridge rows; generated versions are immutable and sequential. |
| Pension rates | Effective-dated `ConfiguracionDescuentoPrevisionalVersion` replaces hardcoded/domain-catalog percentages; the applied version is stored with each payroll result. |
| Source-linked accounting | Finalized payroll and registered vouchers create deterministic source-linked Draft journal entries using the approved account mapping; posting remains explicit. |
| Reproducible exports | Live queries use Posted/Finalized sources and exports persist reproducible snapshot headers and lines with actor, filters, and cutoff. |

## Architecture Views

| View | File |
|---|---|
| Domain Model | `docs/domain-model.md` |
| Database Design | `docs/database-design.md` |
| API Contracts | `docs/api-contracts.md` |
| Module Specifications | `docs/module-specifications.md` |
| Implementation Roadmap | `docs/implementation-roadmap.md` |

## System Context

| Actor | Primary Goals |
|---|---|
| Administrador RRHH | Manage departments and employees, register overtime, calculate payroll, export payslips. |
| Contador | Register vouchers, calculate IGV, generate SUNAT books, maintain accounting entries. |
| Gerente Financiero | View KPIs, balance sheet, income statement, exports (read-only on ledger). |
| Administrador Sistema | Manage users, roles, configuration and logs; backup execution is operational and its application status is implementation pending. |

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
| Infrastructure | PostgreSQL, Dapper/Npgsql, stored procedures, JWT provider, export providers, logging. |

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

## CQRS Boundary

| Module | Commands | Queries |
|---|---|---|
| Authentication | Login, logout. | Current user session and role claims. |
| Payroll | Create department, create employee, update employee, deactivate employee, register overtime, calculate draft payroll, finalize payroll, export payroll. | Department list, employee list, employee detail, overtime by period, payroll by period, payroll totals. |
| Accounting SUNAT | Register voucher, update voucher, annul voucher, generate purchase/sales book version. | Voucher list, voucher detail, purchase book versions, sales book versions, accounting totals. |
| General Ledger | Create account, update account, open period, close period, create journal entry, post journal entry. | Chart of accounts, accounting periods, journal entries, account balances. |
| Reports | Generate report snapshot, export report. | Dashboard KPIs, balance sheet, income statement, consolidated payroll by period/department. |
| Administration | Create user, update role assignment, reset password, create/activate/close configuration versions, review audit. Backup scheduling remains an external operational responsibility. | Users, roles, audit logs and active configuration. Application backup status is implementation pending. |

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
2. A non-mutating validation query returns `Valida`, `ConObservaciones`, or `Bloqueada`.
3. `POST /api/libros` resolves the Active SUNAT format and creates the next immutable version.
4. Included vouchers are linked through snapshot `COMPROBANTE_LIBRO` rows retaining each tax version.
5. UI previews the generated version and allows PDF/Excel export.

### Ledger Posting and Reports

1. Accounting operations create draft journal entries in `ASIENTO_CONTABLE`.
2. Posting validates open period, balanced debit/credit totals, and active accounts.
3. Posted entries feed balances by account and period.
4. Balance sheet, income statement, receivables, payables, IGV and KPIs use the canonical account formulas.
5. Report exports persist immutable snapshots before returning files.

## Cross-Cutting Requirements

- All protected operations require JWT authentication.
- Authorization is role-based for Sprint 1.
- Sensitive operations must be audited.
- Errors must include status, message, and correlation ID.
- Data must remain traceable to source transactions and applied configuration versions.
- Database model targets BCNF.
- Payroll, tax, book, and accounting calculations must be reproducible and testable.

## Technical Acceptance Criteria

- New ERP modules can be added without changing existing financial module internals.
- Accounting and payroll operations include audit information: user, date, operation type, and result.
- Purchase and Sales books follow SUNAT electronic-book format requirements.
- Payroll calculations are verifiable within S/ 0.01 per employee.
- All endpoints except login require JWT authentication.
- Database relationships use FK constraints and defined delete/update behavior.
- API errors include structured messages and correlation IDs.
- API read queries respond in less than 500 ms at p95.
