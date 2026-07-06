# SDD Architecture

## Source Documents

This architecture is derived from:

- `BITACORA -ERP.pdf`
- `Entregable.pdf`
- Existing extracted SDD files under `docs/`, `specs/`, `database/`, and `tests/`.

## Architecture Objective

Define the complete Spec Driven Development architecture for the ERP Financial and Accounting module before implementation. This document organizes the system into domain model, database design, API contracts, module specifications, and implementation roadmap.

The repository remains specification-first. Only compile-ready API/WinForms startup scaffolding and project structure exist; no business feature implementation is included.

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
| Persistence | EF Core/Npgsql and PostgreSQL stored procedures for critical operations. |
| Reports | PDF and Excel export providers. |

## Applied Decisions

| Topic | Decision |
|---|---|
| Authorization | Role-based RBAC only for Sprint 1; fine-grained permissions are deferred. |
| Payroll organization | `Departamento` is a required entity and report filter. |
| Payroll inputs | `HorasExtra` is persisted per employee and period. |
| Payroll lifecycle | Recalculation is allowed only while payroll status is `Draft`. |
| Voucher classification | `Comprobante.tipoMovimiento` separates `Compra` and `Venta`. |
| Ledger | `CuentaContable`, `PeriodoContable`, `AsientoContable`, and `DetalleAsiento` support financial statements. |
| Tax configuration | Tax parameters are persisted as effective-dated versions. |
| SUNAT books | Book versions link vouchers through `ComprobanteLibro`. |

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
| Gerente Financiero | View KPIs, balance sheet, income statement, exports. |
| Administrador Sistema | Manage users, roles, configuration and logs; backup execution is operational and its application status is implementation pending. |

## Architectural Style

The system is specified as a modular desktop-plus-API application:

- WinForms presents screens, navigation, tables, validation, and export actions.
- API exposes use cases through role-protected endpoints.
- Application layer coordinates commands and queries.
- Domain layer owns business concepts and rules.
- Infrastructure layer integrates with PostgreSQL, JWT, logging, exports, and optional cache.

## CQRS Boundary

| Module | Commands | Queries |
|---|---|---|
| Authentication | Login, logout. | Current user session and role claims. |
| Payroll | Create department, create employee, update employee, deactivate employee, register overtime, calculate draft payroll, finalize payroll, export payroll. | Department list, employee list, employee detail, overtime by period, payroll by period, payroll totals. |
| Accounting SUNAT | Register voucher, update voucher, annul voucher, generate purchase/sales book version. | Voucher list, voucher detail, purchase book versions, sales book versions, accounting totals. |
| General Ledger | Create account, update account, open period, close period, create journal entry, post journal entry. | Chart of accounts, accounting periods, journal entries, account balances. |
| Reports | Generate report snapshot, export report. | Dashboard KPIs, balance sheet, income statement, consolidated payroll by period/department. |
| Administration | Create user, update role assignment, reset password, create/activate/close configuration versions, review audit. Backup scheduling remains an external operational responsibility. | Users, roles, audit logs and active configuration. Application backup status is implementation pending. |

## Cross-Cutting Requirements

- All protected operations require JWT authentication.
- Authorization is role-based for Sprint 1.
- Sensitive operations must be audited.
- Errors must include status, message, and correlation ID.
- Data must remain traceable to source transactions and applied configuration versions.
- Database model targets BCNF.
- Payroll, tax, book, and accounting calculations must be reproducible and testable.
