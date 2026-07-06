# Dashboard UX Specification

## Dashboard Purpose

The Dashboard is the authenticated command center. It gives each user a role-appropriate view of the current operational state and the shortest safe path to daily work.

The visible screen label is `Inicio`. It does not duplicate the persistent sidebar; it prioritizes current work, exceptions, and evidence.

The Dashboard is not a marketing or welcome page. It is a productivity surface.

## Dashboard Principles

- Show what needs attention first.
- Make role-specific work reachable immediately.
- Keep KPIs tied to current period and data freshness.
- Offer drill-down to source details.
- Avoid decorative widgets that do not support decisions.

## Role-Based Dashboard Focus

| Role | Primary Dashboard Focus |
|---|---|
| Administrador RRHH | Active employees, payroll period status, missing payroll inputs, payroll total. |
| Contador, including Operador SUNAT persona | Vouchers this period, IGV payable, accounting period status, book generation, latest versions, pending exports, validation warnings. |
| Gerente Financiero | Income, expenses, utility, payroll cost, IGV payable, report shortcuts. |
| Administrador Sistema | Users, access issues, configuration status and audit alerts. Backup status is implementation pending. |

## Dashboard Layout

```text
------------------------------------------------------------
| Usuario / rol / periodo de trabajo / sesion               |
------------------------------------------------------------
| Busqueda global (IMPLEMENTATION PENDING; hidden)           |
------------------------------------------------------------
| KPIs por rol con periodo y ultima actualizacion            |
------------------------------------------------------------
| Pendientes y alertas               | Acciones frecuentes    |
| Actividad reciente (Admin only)    | Reportes frecuentes    |
------------------------------------------------------------
```

## KPI Standards

Every KPI must include:

- Label.
- Value.
- Period or date range.
- Status or comparison when meaningful.
- Drill-down target when authorized.
- Last refreshed context when data may be stale.

A role dashboard shows at most six KPIs, one prioritized work queue, and four frequent actions. It must not repeat every sidebar destination as a shortcut.

Recent audit activity is shown only to Administrador Sistema through the Administration audit API. It is hidden for other roles; no cross-role activity feed is implied.

## Canonical API Sources

| Role | API | Source semantics |
|---|---|---|
| Administrador RRHH | `GET /api/planilla/dashboard?periodo=YYYY-MM` | Active/eligible employees, missing payroll inputs, period state, persisted payroll totals and payroll cost. |
| Contador | `GET /api/comprobantes/dashboard?periodo=YYYY-MM` | Registrado vouchers, operational sales/purchase IGV, accounting-period state and current book versions. Operational IGV is not the Posted-ledger financial KPI. |
| Gerente Financiero | `GET /api/reportes/dashboard?periodo=YYYY-MM` | Posted-ledger financial KPIs plus Finalized payroll cost. |
| Administrador Sistema | Same role-authorized endpoints above | Administration may compose permitted operational/financial cards without a separate invented data source. |

## Core KPI Catalog

| KPI | Intended Users | Drill-Down |
|---|---|---|
| Total Planilla Mes | RRHH, Gerente, Admin | Payroll results for period. |
| Empleados Activos | RRHH, Admin | Employees filtered to active. |
| Comprobantes Mes | Contador, Administrador | Comprobantes del periodo. |
| IGV por Pagar | Contador, Gerente, Administrador | Contador sees operational detail; Gerente sees report detail. |
| Ingresos totales | Gerente, Administrador | Estado de resultados from Posted ledger entries. |
| Costos y gastos | Gerente, Administrador | Detalle del estado de resultados from Posted ledger entries. |
| Utilidad neta | Gerente, Administrador | Estado de resultados. |
| Margen de utilidad | Gerente, Administrador | Estado de resultados del rango. |
| Libros SUNAT pendientes | Contador, Administrador | Generar y revisar libro. |
| Usuarios activos | Administrador | Administracion de usuarios. |

## Work Queue

The work queue lists actionable items, not generic notifications.

Examples:

- La planilla 2026-05 sigue en Borrador.
- 3 vouchers need validation.
- Sales Book 2026-05 has no generated version.
- Tax configuration changes become active next month.
- An Active configuration is approaching its effective end date.

Each item should include:

- Short title.
- Affected period or object.
- Severity or status.
- Direct route to action or detail.

## Quick Actions

Quick actions should be role-specific and limited.

| Role | Quick Actions |
|---|---|
| RRHH | Nuevo empleado, Registrar horas extra, Calcular planilla, Abrir planilla actual. |
| Contador | Nuevo comprobante, Abrir comprobantes, Generar libro de compras, Generar libro de ventas. |
| Gerente | Ver balance general, Ver estado de resultados, Exportar reporte actual. |
| Administrador | Nuevo usuario, Ver auditoria, Nueva version tributaria, Revisar formatos SUNAT. |

## Period and Freshness Semantics

- `Periodo de trabajo` is the selected operational month and is not silently applied to reports that require a date range.
- Each KPI displays its own period or range when it differs from the work period.
- `Ultima actualizacion` distinguishes fresh data, delayed data, unavailable data, and data awaiting source completion.
- A zero value is shown only when confirmed; missing data uses an explicit state instead of `0`.

## Alerts

Alert hierarchy:

1. Blocking compliance or access issue.
2. Finalization or generation pending.
3. Validation warning.
4. Informational update.

Blocking and required-work alerts are not dismissible. Persisted personal dismissal of informational alerts is `IMPLEMENTATION PENDING`; current alerts may be hidden only for the active view and this never changes source state.

## Dashboard Empty States

| Situation | UX Direction |
|---|---|
| New installation | Show setup tasks appropriate to Administrador Sistema. |
| No payroll data | Show employee and payroll setup route for RRHH. |
| No vouchers | Show voucher registration route for Accounting. |
| No report data | Explain required source data and offer period change. |
| Restricted role | Show only authorized metrics and actions. |

## Dashboard Acceptance Checklist

- Dashboard content changes by role.
- Current period or reporting scope is visible.
- KPIs have labels, values, and drill-downs.
- Work queue items are actionable.
- Frequent tasks are reachable in no more than 3 interactions.
- Global search remains hidden while its API contract is implementation pending; module-local filters remain available.
- Alerts are prioritized by consequence.
- Decorative content does not compete with data.
