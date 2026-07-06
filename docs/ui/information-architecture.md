# Information Architecture

## IA Objective

The ERP information architecture must let users move from role-based entry points to task-specific workspaces with minimal decision cost. The structure favors predictable module boundaries, search-first lists, consistent record details, and period-based financial context.

`screen-inventory.md` is the canonical catalog. `ux-traceability.md` defines the allowed relationships between screens, journeys, forms, grids, and roles. `layout-specifications.md` defines how this architecture appears in the desktop shell.

## Primary Structure

```text
ERP.WinForms
|-- Iniciar sesion
|-- Inicio
|-- Administracion
|   |-- Usuarios
|   |-- Roles
|   |-- Configuracion tributaria y previsional
|   |-- Formatos SUNAT
|   |-- Registro de auditoria
|   |-- Estado de respaldos [IMPLEMENTATION PENDING]
|-- Empleados
|   |-- Empleados
|   |-- Departamentos
|   |-- Horas extra
|-- Planillas
|   |-- Periodos
|   |-- Calculo y revision
|   |-- Boletas de pago [persistent center IMPLEMENTATION PENDING]
|   |-- Historial de exportaciones [IMPLEMENTATION PENDING]
|-- Contabilidad
|   |-- Comprobantes
|   |-- Plan de cuentas
|   |-- Periodos contables
|   |-- Asientos contables
|-- SUNAT
|   |-- Centro de libros
|   |-- Generar y revisar libro
|   |-- Versiones
|   |-- Exportaciones [persisted history IMPLEMENTATION PENDING]
|-- Reportes
    |-- Panel financiero
    |-- Balance general
    |-- Estado de resultados
    |-- Planilla consolidada
    |-- Centro de exportaciones
```

## Module Responsibilities

| Module | User Question Answered | Primary Objects |
|---|---|---|
| Iniciar sesion | Can I access the ERP? | Session, role, credentials. |
| Inicio | What needs attention and where should I go next? | KPIs, shortcuts, recent activity, alerts. |
| Administracion | Who can access what and what configuration is active? | Users, roles, tax versions, formats and audit. Backup status is implementation pending. |
| Empleados | Who works here and what data affects payroll? | Employees, departments, pension regime, overtime. |
| Planillas | What is the payroll state for a period? | Payroll period, calculation result, payslip, finalization status. |
| Contabilidad | What vouchers and accounting entries exist? | Vouchers, accounts, periods, journal entries. |
| SUNAT | What books are generated and exportable for a period? | Purchase book, sales book, book versions, compliance totals. |
| Reportes | What is the financial position and performance? | KPIs, balance sheet, income statement, consolidated payroll. |

## Workspace Hierarchy

1. Shell: session and persistent sidebar. Global search is implementation pending and hidden.
2. Module: one authorized business domain.
3. Workspace: list, form, process, detail, or report.
4. Context: period, filters, status, selection, and saved view.
5. Detail: related records, calculations, history, validations, and exports.

A deeper level must never erase the context inherited from the level above it.

## Object Relationships for UX

| Object | Appears In | UX Relationship |
|---|---|---|
| User | Login, Administration, Audit | Determines visible modules and sensitive action ownership. |
| Role | Dashboard, Administration | Controls module visibility and action availability. |
| Employee | Employees, Payroll, Reports | Payroll source record and report filter dimension. |
| Department | Employees, Reports, Payroll | Organizational grouping and filter dimension. |
| Overtime | Employees, Payroll | Period input that affects payroll calculation. |
| Payroll Period | Payroll, Reports | Time scope for calculations, totals, and exports. |
| Voucher | Accounting, SUNAT, Reports | Source transaction for taxes, books, ledger, and KPIs. |
| SUNAT Book | SUNAT, Reports | Versioned compliance output with preview and export. |
| Journal Entry | Accounting, Reports | Posted financial source for formal statements. |
| Tax Configuration | Administration, Accounting, SUNAT | Governs calculations and preserves historical meaning. |

## Role-Based IA

| Role | Default Landing Focus | Visible Primary Modules |
|---|---|---|
| Administrador RRHH | Payroll readiness, employee changes and payroll-period operational totals. | Inicio, Empleados and Planillas. The Reports module remains hidden. |
| Contador, including Operador SUNAT persona | Voucher workload, IGV, accounting periods, SUNAT books and exports. | Inicio, Contabilidad and SUNAT. |
| Gerente Financiero | KPIs, balance sheet, income statement, and read-only ledger context. | Inicio, Reportes, and authorized read-only ledger screens. |
| Administrador Sistema | System health, users, configuration, audit. | Inicio, Administracion, and all permitted modules. |

## Content Types

| Type | UX Treatment |
|---|---|
| Operational lists | Search-first table, filters, record count, selected period, visible totals. |
| Data-entry forms | Clear groups, required markers, inline validation, save safety, keyboard order. |
| Review screens | Summary header, immutable identifiers, status, history, related records. |
| Calculation screens | Period selector, readiness checks, primary action, progress, result table, totals. |
| Compliance previews | Exact column visibility, version label, generation status, export readiness. |
| Executive reports | KPI first, filters second, drill-down and export always available. |
| Configuration screens | Current active version first, pending or historical versions behind disclosure. |
| Audit screens | Time-ordered event list with actor, action, entity, result, and filter tools. |

## IA Rules

- Every workspace must expose its current module, section, period or scope, and active filters.
- Primary actions must be located near the content they affect.
- A record detail must always provide a path back to its source list without losing filters.
- Cross-module links must be meaningful: employee to payroll results, voucher to SUNAT book, report KPI to source details.
- Destructive or irreversible states must be visually distinct from draft or editable states.
- Administration configuration must be separated from daily operations to reduce accidental changes.
- SUNAT books must surface the generated version before export; payroll must surface persisted calculated results and Draft/Finalized state before export. Cancelled payroll is not exportable.
- Screen IDs, visible labels, fields, columns, and permissions must match `screen-inventory.md` and `ux-traceability.md`.
