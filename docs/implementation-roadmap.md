# Implementation Roadmap

This roadmap is a specification artifact. It does not generate application code.

## Delivery Principles

- Implement from stable domain rules outward.
- Keep module boundaries aligned with Clean Architecture.
- Use CQRS naming and separation in specifications before implementation.
- Validate every sprint against acceptance and non-functional test specifications.
- Keep source traceability from PDF requirement to module spec to test.

## Sprint 0: Specification Baseline

| Goal | Deliverables | Status |
|---|---|---|
| Establish SDD architecture | Source analysis, requirements, user stories, architecture, module specs, database specs, test specs. | Complete in documentation. |
| Define module boundaries | Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration. | Updated after gap resolution. |
| Define database model | BCNF schema, versioned tax config, payroll inputs, ledger, book bridge. | Updated after gap resolution. |

## Sprint 1: Foundation, Authentication, and Payroll

| Workstream | Specification Deliverables |
|---|---|
| Architecture foundation | Confirm Clean Architecture layers, CQRS command/query list, error contract, audit contract. |
| Authentication | Validate login, logout, JWT, role matrix, account lockout, protected endpoints. |
| User and role administration | Keep user CRUD and role assignment in Administration, not Authentication. |
| Payroll employee management | Validate departments, employee form fields, list filters, logical deactivation, DNI uniqueness. |
| Payroll overtime | Validate overtime capture by employee and period before calculation. |
| Payroll calculation | Validate formulas, period policy, Draft-only recalculation, finalization policy, performance target. |
| Database | Finalize `ROL`, `USUARIO`, `DEPARTAMENTO`, `EMPLEADO`, `HORAS_EXTRA`, `PLANILLA`, `DETALLE_PLANILLA`, and `sp_calcular_planilla`. |
| Testing | Acceptance tests for US-001, US-002, US-003; performance test for 100 employees. |

### Sprint 1 Exit Criteria

- Authentication and Payroll specs are approved.
- Sprint 1 authorization is role-based only.
- Payroll recalculation is blocked unless status is `Draft`.
- Overtime and department records are part of payroll acceptance tests.
- Database constraints and indexes for identity/payroll are approved.

## Sprint 2: Accounting SUNAT

| Workstream | Specification Deliverables |
|---|---|
| Voucher registration | Validate required fields, duplicate rule, document identity, `tipoMovimiento`, states. |
| IGV calculation | Use active tax configuration version by effective date and store the applied version. |
| SUNAT books | Confirm Purchase/Sales book columns, grouping, totals, versioning policy. |
| Book versioning | Generate immutable book versions and link vouchers through `COMPROBANTE_LIBRO`. |
| Database | Finalize voucher, tax configuration version, book, and bridge-table design. |
| API | Review voucher and book contracts. |
| Testing | Acceptance tests for US-004 and US-005; performance test for 1,000 vouchers. |

### Sprint 2 Exit Criteria

- Accounting SUNAT specs are approved.
- `Compra`/`Venta` discriminator is required in voucher creation.
- SUNAT book format fields are confirmed.
- IGV calculation examples include applied tax configuration versions.
- Book generation performance criteria are testable.

## Sprint 3: General Ledger and Reports

| Workstream | Specification Deliverables |
|---|---|
| Chart of accounts | Define account types, normal balances, active/inactive rules, and hierarchy constraints. |
| Accounting periods | Define open/closed lifecycle and posting restrictions. |
| Journal entries | Define balanced debit/credit validation and posting rules. |
| Dashboard | Confirm KPI formulas and ledger source tables. |
| Balance sheet | Define account grouping and calculation rules from posted ledger entries. |
| Income statement | Define income, cost, expense, and utility formulas from posted ledger entries. |
| Consolidated payroll | Confirm totals and period/department filters. |
| Exports | Define PDF and Excel report layouts. |
| Testing | Acceptance tests for US-006 and export consistency. |

### Sprint 3 Exit Criteria

- Ledger specs are approved before financial report implementation.
- Report formulas are approved.
- Report filters and export layouts are approved.
- Report values are traceable to posted accounting entries and payroll records.

## Sprint 4: Administration, Security, and Operations

| Workstream | Specification Deliverables |
|---|---|
| User administration | Finalize user/role screens and password reset policy. |
| Configuration | Define tax-rate and SUNAT-format configuration lifecycle. |
| Audit | Finalize audit event catalog and retention policy. |
| Backup/recovery | Define backup schedule, restore steps, and verification. |
| Security | Finalize DB roles, TLS, encryption, endpoint authorization checks. |
| Testing | Security, audit, compatibility, and recovery test specifications. |

### Sprint 4 Exit Criteria

- Administration specs are approved.
- Security and operations requirements are testable.
- Audit coverage includes all sensitive operations.

## Dependency Order

| Order | Dependency | Reason |
|---:|---|---|
| 1 | Authentication before protected modules | All modules depend on user identity and role. |
| 2 | Departments and employee data before payroll calculation | Payroll requires active employees, department assignment, and discount types. |
| 3 | Overtime before payroll calculation | Payroll totals include approved overtime inputs. |
| 4 | Tax configuration before voucher registration | Vouchers must store the applied tax configuration version. |
| 5 | Vouchers before SUNAT books | Books are generated from registered vouchers. |
| 6 | Ledger before financial statements | Balance sheet and income statement must read posted entries. |
| 7 | Payroll and accounting before reports | Reports aggregate payroll and accounting records. |

## Risk Register

| Risk | Impact | Mitigation |
|---|---|---|
| SUNAT format ambiguity | Book generation may be non-compliant. | Confirm final columns and sample files before implementation. |
| Report formulas incomplete | Reports may not support management decisions. | Define formulas over `CUENTA_CONTABLE`, `ASIENTO_CONTABLE`, and `DETALLE_ASIENTO` before Sprint 3. |
| Security scope expands late | Rework in user/role model. | Keep Sprint 1 role-only; defer fine-grained permissions to a separate migration. |
| Tax rule changes during operation | Historical vouchers may be recalculated incorrectly. | Persist effective-dated tax configuration versions and store applied version references. |
| Performance target not measurable | Non-functional testing becomes subjective. | Create data volume fixtures and timing criteria in test specs. |

## Architecture Definition of Done

- Every requirement maps to at least one module spec.
- Every user story has acceptance criteria.
- Every module has requirements, business rules, database spec, API contract, and tasks.
- Every critical operation has transaction and audit rules.
- Database design identifies entities, relationships, constraints, indexes, and procedure specs.
- API contracts identify paths, authentication, inputs, outputs, and errors.
- Roadmap identifies delivery order, dependencies, risks, and exit criteria.

## Coding-Level Definition of Done

- SOLID is respected in classes, handlers, services, interfaces, and adapters.
- No business logic exists in WinForms forms or API controllers.
- CQRS handlers are used for use cases.
- Application remains independent from Infrastructure implementations.
- Authorization happens before mutations.
- Audit is recorded for sensitive operations.
- Unit tests protect critical payroll, tax, ledger, lifecycle, and authorization rules.
- No unapproved pattern overengineering is introduced.
- No MVC-as-primary-architecture, microservices, or Event Sourcing is introduced in Sprint 1.
