# Requirements Catalog

This catalog consolidates all requirements extracted from the PDFs.

## Consolidated Functional Requirements

| ID | Name | Actor | Priority | Sprint | Requirement |
|---|---|---|---|---|---|
| RF-001 | Gestion de Empleados | Administrador RRHH | Alta | Sprint 1 | The system must allow registering, editing, consulting, searching, filtering, and logically deactivating employee records. |
| RF-002 | Calculo de Planillas | Administrador RRHH | Alta | Sprint 1 | The system must calculate monthly payroll using one period-level aggregate, including cash gross pay, effective-dated AFP/ONP deductions, monthly CTS and gratification provisions, and net pay. `descuentos_adicionales` is persisted as S/ 0.00 in this scope; capture/configuration is out of scope. Provisions are excluded from cash gross and net pay. |
| RF-003 | Reporte de Pago | Administrador RRHH | Alta | Sprint 1 | The system must generate employee payslips in PDF and payroll exports in Excel. |
| RF-004 | Registro de Comprobantes | Contador | Alta | Sprint 2 | The system must register purchase and sales vouchers with Compra/Venta discriminator, document validation, duplicate prevention, IGV calculation, applied tax version, and status tracking. Credit/debit notes require an original voucher reference; credit notes reduce the original mapping and debit notes increase it. |
| RF-005 | Libro de Compras y Ventas | Contador | Alta | Sprint 2 | The system must generate immutable electronic Purchase and Sales book versions for a selected period in SUNAT format, linking included vouchers through a bridge table. |
| RF-006 | Normativa Tributaria SUNAT | Contador | Alta | Sprint 2 | The system must apply persisted and versioned SUNAT tax rules, IGV rates, codes, formats, and configurable tax parameters. |
| RF-007 | Reportes Financieros | Gerente Financiero | Media | Sprint 3 | The system must generate and display balance sheet, income statement, financial KPIs, and consolidated payroll reports. |
| RF-008 | Autenticacion JWT y RBAC por Roles | Administrador Sistema | Alta | Sprint 1 | The system must authenticate users with credentials, issue revocable JWT tokens with `jti`, enforce temporary lockout, and restrict access using the four predefined roles; Sprint 1 defers fine-grained permission catalogs. |
| RF-010 | Gestion de Departamentos | Administrador RRHH | Alta | Sprint 1 | The system must allow registering, editing, listing, and deactivating departments used by employees and reports. |
| RF-011 | Registro de Horas Extra | Administrador RRHH | Alta | Sprint 1 | The system must register, approve, and cancel overtime by employee and period; only Approved overtime is included in payroll. |
| RF-015 | Libro Mayor Contable | Contador | Alta | Sprint 3 | The system must maintain the approved initial chart of accounts, accounting periods, source-linked journal entries, cancellation, posting, and reversal workflows for financial statements. |
| RF-016 | Ciclo de Periodo de Planilla | Administrador RRHH | Alta | Sprint 1 | One payroll-period aggregate must own the Draft, Finalized, or Cancelled state. Recalculation is allowed only in Draft; Finalized and Cancelled are terminal. |
| RF-017 | Reactivacion Logica | Responsable del modulo | Alta | Sprint 1 | Users, employees, departments, and accounts must support explicit reactivation commands after logical deactivation when their dependencies remain valid. |
| RF-018 | Tasas Previsionales Versionadas | Administrador Sistema | Alta | Sprint 1 | AFP and ONP percentages must be persisted as non-overlapping effective-dated versions and the applied version must be stored with each payroll result. |
| RF-019 | Contabilizacion de Origenes | Contador | Alta | Sprint 3 | Finalized payroll and registered vouchers must create deterministic source-linked Draft journal entries using the approved mapping; posting remains explicit. |
| RF-020 | Validacion y Formato SUNAT | Contador | Alta | Sprint 2 | SUNAT book generation must run a documented validation, consume the Active effective governed-column format version, persist immutable sequential versions, and snapshot one locked voucher set including row fields and each voucher's applied tax version. |
| RF-021 | Fuentes de Reportes y Snapshots | Gerente Financiero | Alta | Sprint 3 | Financial reports must use Posted ledger entries, payroll reports must use Finalized payroll, and exports must persist reproducible snapshots with actor, filters, cutoff, and lines. |
| RF-022 | Auditoria de Operaciones Sensibles | Administrador Sistema | Alta | Sprint 1 | Login, logout, lockout, lifecycle commands, payroll, voucher, book, ledger, configuration, and export operations must record actor user, actor role, correlation ID, result, and affected entity. |
| RF-023 | Contratos API Completos | Todos | Alta | Sprint 1 | Every API operation must specify request and response schemas, standard errors including 404 for missing ID resources, pagination where applicable, allowed roles, and optional `X-Correlation-ID` for every protected operation. |
| RF-024 | Trazabilidad de Flujos | Product Owner | Alta | Sprint 0 | Every in-scope workflow must trace from requirement and rule through UI/API/domain/database/audit to an acceptance test; out-of-scope workflows must be marked explicitly. |

## Requirements by Role: Analysis and Documentation

| ID | Name | Requirement |
|---|---|---|
| RF-ALV-01 | Levantamiento de requisitos | Allow registering stakeholder interview records with date, interviewee, questions, answers, observations, and requirement traceability. |
| RF-ALV-02 | Redaccion de historias de usuario | Allow creating user stories with ID, priority, effort points, and acceptance criteria using the "Como... quiero... para..." format. |
| RF-ALV-03 | Gestion del backlog | Allow creating, prioritizing, reordering, and filtering backlog items by sprint, state, and priority. |
| RF-ALV-04 | Validacion de requisitos | Allow recording Product Owner/team validation with date, result, and comments. |
| RF-ALV-05 | Generacion de documentacion tecnica | Allow generating SRS documents from the backlog with actors, RF/RNF matrices, and UML references. |

## Requirements by Role: WinForms UI

| ID | Name | Requirement |
|---|---|---|
| RF-LUI-01 | Navegacion por modulos | Provide a lateral menu for Payroll, Accounting SUNAT, General Ledger, Reports, and Administration, showing only authorized modules. Authentication remains the session entry boundary. |
| RF-LUI-02 | Formularios de entrada | Provide real-time validation for employee, voucher, and configuration forms with field-specific errors. |
| RF-LUI-03 | Tablas y resultados | Show payroll and accounting results in professional tables with row styling, sorting, period filters, pagination, and numeric totals. |
| RF-LUI-04 | Dashboard financiero | Show KPI cards for payroll total, income, expenses, IGV payable, utility, and dynamic charts. |
| RF-LUI-05 | Exportacion de reportes | Provide PDF and Excel export actions in all report views. |

## Requirements by Role: Backend API

| ID | Name | Requirement |
|---|---|---|
| RF-ELI-01 | Autenticacion JWT | Provide `POST /api/auth/login`, validate credentials against PostgreSQL, and return a signed JWT with user claims and configurable expiration. |
| RF-ELI-02 | CRUD de empleados | Provide REST endpoints for listing, getting, creating, updating, and logically deleting employees. |
| RF-ELI-03 | Ciclo de planilla | Provide calculate, recalculate, finalize, and cancel commands identified by `periodo`; calculation executes `sp_calcular_planilla(periodo, actor, correlation)` and returns the period aggregate with employee results. |
| RF-ELI-04 | Registro y consulta de comprobantes | Provide endpoints to register and query vouchers with RUC validation and automatic IGV calculation. |
| RF-ELI-05 | Generacion de libros SUNAT | Provide a validation query, a `POST` generation command, immutable-version queries, and exports by period/type with format and tax-version traceability. |
| RF-ELI-06 | Contratos de libro mayor | Provide typed endpoints for account lifecycle, period closing, journal creation/editing, posting, cancellation, reversal, and read-only Gerente queries. |
| RF-ELI-07 | Contratos compartidos | Every operation must declare typed success/error responses, role policy, correlation ID behavior, and standard pagination/sort parameters for collections. |

## Requirements by Role: Database

| ID | Name | Requirement |
|---|---|---|
| RF-EDI-01 | Esquema BCNF | Implement a relational schema normalized to BCNF to remove redundancy and update anomalies. |
| RF-EDI-02 | Stored procedures | Specify PL/pgSQL procedures for payroll calculation and SUNAT book generation. |
| RF-EDI-03 | FK and constraints | Specify FK rules, CHECK constraints, UNIQUE constraints, NOT NULL rules, and default values. |
| RF-EDI-04 | Indexes | Specify indexes for frequent searches by period, RUC, date, username, voucher type, employee ID, and FK columns. |
| RF-EDI-05 | Database roles | Specify PostgreSQL roles with least privilege for read, write, and admin access. |
| RF-EDI-06 | Versioned configuration | Specify effective-dated tax configuration and SUNAT format version tables. |
| RF-EDI-07 | Ledger schema | Specify ledger tables required for balance sheet and income statement. |
| RF-EDI-08 | Payroll aggregate schema | Specify `PERIODO_PLANILLA` as period lifecycle owner and employee results as its children. |
| RF-EDI-09 | Security persistence | Specify lockout counters, token revocation, audit actor context, and executable least-privilege database grants. |
| RF-EDI-10 | Reporting snapshots | Specify reproducible report export snapshot headers and lines. |

## Consolidated Non-Functional Requirements

| ID | Category | Measurable Criterion | Requirement |
|---|---|---|---|
| RNF-001 | Rendimiento | Payroll calculation <= 30 s for 100 employees | The system must calculate payroll for 100 employees within 30 seconds. |
| RNF-002 | Seguridad | AES-256, TLS, BCrypt, JWT, RBAC | The system must protect credentials and financial data with strong encryption, BCrypt password hashing, JWT authentication, and role-based access. |
| RNF-003 | Usabilidad | Frequent tasks <= 3 interactions | The UI must allow frequent tasks such as registering employee, calculating payroll, and viewing reports in no more than 3 interactions. |
| RNF-004 | Compatibilidad | Windows 10+, Windows 11, .NET 10 | The system must run on Windows 10/11 with .NET 10 Desktop/ASP.NET Core Runtime and support 1280x720 to 1920x1080. The development baseline is Visual Studio Community 2026 (18.6). |
| RNF-005 | Mantenibilidad | Independent modules, > 70% coverage target | The architecture must allow modules to be updated, corrected, or replaced without affecting other modules. |

## Additional Non-Functional Requirements

| ID | Category | Requirement |
|---|---|---|
| RNF-ALV-01 | Mantenibilidad | 100% of requirements must have bidirectional traceability to source. |
| RNF-ALV-02 | Usabilidad | Generated SRS documentation must follow IEEE 830-style sections. |
| RNF-ALV-03 | Calidad | Conflicting or duplicate requirements must be identified during registration. |
| RNF-ALV-04 | Rendimiento | Backlog changes must propagate to linked documentation in less than 5 seconds. |
| RNF-ALV-05 | Seguridad | Only Analyst or Product Owner roles may modify requirements. |
| RNF-LUI-01 | Rendimiento | Main dashboard visible within 3 seconds after application start, excluding login time. |
| RNF-LUI-02 | Rendimiento | UI interactions must respond in less than 200 ms without freezing the main window. |
| RNF-LUI-03 | Usabilidad | UI must adapt from 1280x720 to 1920x1080 while preserving readability. |
| RNF-LUI-04 | Usabilidad | UI must use consistent semantic theme colors, Segoe UI, and spacing, with light and dark appearance modes preserving WCAG AA contrast. |
| RNF-LUI-05 | Accesibilidad | UI must support keyboard navigation and screen reader labels aligned with WCAG 2.1 AA. |
| RNF-ELI-01 | Rendimiento | API must respond within 500 ms for 95% of requests, excluding network time. |
| RNF-ELI-02 | Seguridad | All endpoints except login must require a valid JWT and return 401 when missing or expired. |
| RNF-ELI-03 | Mantenibilidad | API errors must include HTTP status, descriptive message, and correlation ID. |
| RNF-ELI-04 | Rendimiento | Backend must support at least 50 concurrent users with average response <= 800 ms. |
| RNF-ELI-05 | Mantenibilidad | API must be documented using Swagger/OpenAPI. |
| RNF-EDI-01 | Rendimiento | `sp_calcular_planilla` must complete for 100 employees within 30 seconds. |
| RNF-EDI-02 | Fiabilidad | Database availability must be 99.5% during working hours. |
| RNF-EDI-03 | Seguridad | Database connections must use SSL/TLS and sensitive data must be encrypted at rest. |
| RNF-EDI-04 | Escalabilidad | Database must support at least 5 years of operational payroll and voucher history. |
| RNF-EDI-05 | Mantenibilidad | Schema changes must be managed through versioned migrations with rollback procedures. |
