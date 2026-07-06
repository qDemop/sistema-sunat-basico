# UX Vision Document

## Purpose

ERP.WinForms must feel like a focused desktop productivity tool: calm, fast, legible, and trustworthy under daily operational pressure. The experience adapts the Apple Human Interface Guidelines philosophy to an enterprise ERP context: clarity, deference to content, consistency, progressive disclosure, meaningful depth, and simplicity that preserves operational power.

This document defines the UX foundation for the user-visible modules `Iniciar sesion`, `Inicio`, `Administracion`, `Empleados`, `Planillas`, `Contabilidad`, `SUNAT`, and `Reportes` before business implementation begins. It is a specification artifact only. It does not define application code, control classes, or implementation mechanics.

## Canonical UX Baseline

The following artifacts are normative and resolve any ambiguity in this document set:

- `screen-inventory.md`: complete screen catalog and identifiers.
- `layout-specifications.md`: persistent desktop shell, workspace hierarchy, breadcrumbs, resizing, and restoration.
- `ux-traceability.md`: screens, journeys, forms, grids, and role permissions.
- `keyboard-shortcuts.md`: shortcuts and high-productivity workflows.

All visible interface labels, statuses, messages, commands, exports, and confirmations use Peruvian Spanish. Domain identifiers remain `DNI`, `RUC`, `IGV`, `AFP`, `ONP`, `CTS`, and `SUNAT`.

## Product Experience Statement

The ERP should help HR, Accounting, Administration, and SUNAT operators complete repetitive, high-consequence financial work with confidence. The interface must make the next action obvious, keep large datasets scannable, expose validation at the point of decision, and avoid visual noise that competes with payroll, accounting, and compliance data.

## Source Alignment

The UX foundation is aligned with:

- `BITACORA -ERP.pdf`: modular ERP objective, C#, PostgreSQL, payroll, SUNAT books, financial reports, UI responsibility, accessibility responsibility.
- `Entregable.pdf`: product vision, actors, payroll/accounting formulas, backlog, WinForms UI requirements, dashboard and wireframe annex.
- `docs/product-vision.md`: automated payroll, SUNAT accounting, reports, role-based access, success criteria.
- `docs/requirements.md`: RF-LUI and RNF-LUI usability, performance, accessibility, and compatibility constraints.
- `docs/user-stories.md`: authentication, employee registration, payroll calculation, voucher registration, SUNAT books, financial reports, user administration.

## Design Principles

### 1. Clarity

Every screen must answer three questions within seconds:

- Where am I?
- What data am I looking at?
- What is the next useful action?

Labels use business language familiar to users: DNI, RUC, planilla, comprobante, IGV, libro de compras, libro de ventas, periodo, empleado, rol. Screens avoid decorative text and explain only what is required to decide or act.

### 2. Deference to Content

Financial and operational data are the main visual subject. Navigation, chrome, and decoration must stay restrained so that tables, totals, statuses, forms, and exceptions are visually dominant.

Primary content areas must prioritize:

- Current period.
- Current filter scope.
- Record count and totals.
- Validation or compliance status.
- Next required action.

### 3. Consistency

The same task shape must look and behave the same across modules:

- Lists use the same search, filter, sort, selection, export, and pagination conventions.
- Forms use the same required-field, validation, save, cancel, and dirty-state behavior.
- Risky actions use the same confirmation pattern.
- Reports use the same period selector, filter summary, totals, export, and drill-down pattern.

### 4. Progressive Disclosure

Frequent actions remain visible; advanced controls appear only when needed. A user registering daily vouchers should not pass through report configuration. A manager reading KPIs should not see administration settings. Advanced filters, audit details, historical versions, and technical metadata are available but secondary.

### 5. Depth With Meaningful Hierarchy

Depth is used to show relationship and priority, not decoration. The interface should make it clear when the user is moving from a summary to a detail, from a record to its history, or from a draft to a finalized state.

Hierarchy levels:

- Application level: current module and user session.
- Workspace level: current section, period, filters, and record set.
- Record level: selected entity, status, validations, and actions.
- Detail level: history, audit, calculations, generated outputs.

### 6. Simplicity Without Reducing Functionality

The ERP must be efficient, not sparse. High-frequency operations may expose more controls when that reduces work. Visual minimalism must never hide essential data-entry, review, validation, or export actions.

## UX Success Criteria

| Criterion | Target |
|---|---|
| Frequent task reach | Registrar empleado, registrar comprobante, calcular planilla o ver reporte in no more than 3 interactions from `Inicio`. |
| Search-first access | Any primary list can be narrowed by text search immediately after entry. |
| Keyboard efficiency | Frequent workflows can be completed without mouse dependency. |
| Data confidence | Tables expose count, active filters, selected period, status, and financial totals. |
| Validation clarity | Field errors identify the field, cause, and corrective action. |
| Role clarity | Users see only modules and actions appropriate to their role. |
| Cognitive load | Each screen has one primary task and one clear primary action. |
| Accessibility | Meets WCAG 2.1 AA intent for contrast, keyboard access, focus visibility, labels, and error messaging. |

## Target Users

| User | Primary Need | UX Priority |
|---|---|---|
| Administrador RRHH | Maintain employees, overtime, payroll, payslips. | Fast entry, clear employee status, accurate totals, safe finalization. |
| Contador | Register vouchers, calculate IGV, generate SUNAT books. | High-volume entry, duplicate prevention, period accuracy, compliance confidence. |
| Operador SUNAT (persona) | Review books, export files, validate compliance data while authenticated with role `Contador`. | Exact columns, visible version/status, export traceability. |
| Gerente Financiero | Monitor KPIs, balance sheet, income statement, payroll cost. | At-a-glance clarity, drill-down and export; period comparison is implementation pending. |
| Administrador Sistema | Manage users, roles, configuration and audit. | Controlled access, auditability and safe configuration changes; backup status is implementation pending. |

## Experience Qualities

- Fast: screens respond quickly, show immediate feedback, and avoid blocking the whole application during long tasks.
- Quiet: visual emphasis is reserved for status, errors, totals, and primary actions.
- Precise: money, tax, period, and identity values are formatted consistently.
- Recoverable: errors explain what happened and how to continue.
- Auditable: sensitive actions expose status, user, time, and history where relevant.
- Familiar: follows desktop productivity expectations for navigation, keyboarding, selection, sorting, and exports.

## UX Non-Goals

- No marketing-style landing page.
- No decorative hero screens.
- No custom business logic hidden in UI copy.
- No implementation-specific control inventory.
- No code, API, database, or framework prescriptions.
- No visual minimalism that removes needed enterprise functionality.
- No mixed-language interface or alternate labels for the same business state.
- No screen, form, grid, journey, or permission outside the canonical traceability matrix.
