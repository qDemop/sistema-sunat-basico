# Data Grid Standards

## Grid Objective

ERP grids must support large operational datasets without overwhelming the user. They should be optimized for scanning, sorting, filtering, selection, totals, and keyboard use.

Visible column names, active commands, filters, and export summaries use Peruvian Spanish. Saved views and cross-page bulk actions are `IMPLEMENTATION PENDING`. Grid IDs are referenced by `ux-traceability.md` and shortcut behavior by `keyboard-shortcuts.md`.

This document defines UX behavior for tabular data. It does not prescribe implementation controls.

## Standard Grid Anatomy

```text
[Title / scope / period]        [Primary action]
[Search] [Common filters] [Advanced filters]
[Active filters summary]        [Record count]
------------------------------------------------
| Columns...                                      |
| Rows...                                         |
------------------------------------------------
[Selected count] [Totals] [Pagination / page size]
```

## Column Standards

| Data Type | Display Standard |
|---|---|
| Identity | Keep visible and readable: DNI, RUC, username, series/number. |
| Names | Avoid unnecessary truncation; support search and sort. |
| Money | Align consistently, show currency context, two decimals. |
| Percentages | Show percent sign and consistent precision. |
| Dates | Use consistent date format and show period separately where relevant. |
| Status | Use label plus visual state; never color alone. |
| Actions | Keep row actions secondary and predictable. |

## Module Grid Inventory

| Grid ID | Modulo / Grilla | Required Columns |
|---|---|---|
| GRD-ADM-01 | Administracion / Usuarios | Usuario, Nombres y apellidos, Rol, Ultimo acceso, Estado. |
| GRD-ADM-02 | Administracion / Roles | Rol, Descripcion, Alcance por modulo, Estado. |
| GRD-ADM-03 | Administracion / Configuracion tributaria y previsional | Tipo, Codigo/Regimen, Version, Tasa, Vigente desde, Vigente hasta, Estado. |
| GRD-ADM-04 | Administracion / Formatos SUNAT | Tipo de libro, Version, Vigente desde, Vigente hasta, Estado. |
| GRD-ADM-05 | Administracion / Auditoria | Fecha y hora, Usuario, Modulo, Accion, Entidad, Resultado. |
| GRD-ADM-06 | Administracion / Respaldos | Fecha y hora, Tipo, Duracion, Resultado, Proxima ejecucion. |
| GRD-EMP-01 | Empleados / Empleados | DNI, Nombres y apellidos, Departamento, Cargo, Salario base, Regimen pensionario, Fecha de ingreso, Estado. |
| GRD-EMP-03 | Empleados / Departamentos | Departamento, Descripcion, Empleados activos, Estado. |
| GRD-EMP-04 | Empleados / Horas extra | Periodo, DNI, Empleado, Departamento, Primeras dos horas, Horas adicionales, Estado. |
| GRD-PAY-01 | Planillas / Periodos | Periodo, Estado, Empleados, Total bruto, Descuentos, Neto, Ultima actualizacion. |
| GRD-PAY-02 | Planillas / Resultados | Empleado, Departamento, Haberes brutos, AFP/ONP aplicado, Otros descuentos (S/ 0.00 en alcance actual), Neto a pagar, Provision CTS, Provision gratificacion, Costo total, Estado del periodo. |
| GRD-ACC-01 | Contabilidad / Comprobantes | Movimiento, Tipo, Serie, Numero, Referencia de nota, Fecha, RUC/DNI, Razon social, Tipo de operacion, Base gravada, Base exonerada, IGV, Total, Estado. |
| GRD-ACC-03 | Contabilidad / Plan de cuentas | Codigo, Cuenta, Tipo, Nivel, Cuenta padre, Estado. |
| GRD-ACC-04 | Contabilidad / Periodos | Periodo, Fecha de inicio, Fecha de fin, Estado. |
| GRD-ACC-05 | Contabilidad / Asientos | Periodo, Numero, Fecha, Descripcion, Debe, Haber, Estado. |
| GRD-SUN-02 | SUNAT / Vista previa | Periodo, RUC, Tipo de comprobante, Serie, Numero, Fecha, Base gravada, Base exonerada, IGV, Total, Validacion. |
| GRD-SUN-03 | SUNAT / Versiones | Periodo, Tipo de libro, Version, Generado por, Fecha y hora, Comprobantes, Total, Estado. |

## Supporting Grid Inventory

| Grid ID | Modulo / Grilla | Canonical Columns |
|---|---|---|
| GRD-DASH-01 | Inicio / Pendientes | Prioridad, Pendiente, Periodo/objeto, Estado, Accion. |
| GRD-EMP-02 | Empleados / Relaciones e historial | Fecha, Tipo, Periodo, Referencia, Estado. |
| GRD-EMP-05 | Empleados / Captura masiva de horas extra | Seleccion, DNI, Empleado, Departamento, Primeras dos horas, Horas adicionales, Validacion. |
| GRD-PAY-03 | Planillas / Detalle por empleado | Concepto, Tipo, Base, Tasa/Regla, Importe. |
| GRD-PAY-04 | Planillas / Boletas | Periodo, DNI, Empleado, Estado de generacion, Fecha y hora, Accion. |
| GRD-PAY-05 | Planillas / Exportaciones | Tipo, Periodo, Filtros, Usuario, Fecha y hora, Resultado. |
| GRD-ACC-02 | Contabilidad / Historial de comprobante | Fecha y hora, Usuario, Accion, Estado anterior, Estado nuevo, Resultado. |
| GRD-ACC-06 | Contabilidad / Lineas de asiento | Cuenta, Descripcion, Debe, Haber, Validacion. |
| GRD-SUN-01 | SUNAT / Estado por periodo | Periodo, Tipo de libro, Ultima version, Comprobantes, Validaciones, Estado, Exportacion. |
| GRD-SUN-04 | SUNAT / Exportaciones | Periodo, Tipo de libro, Version, Formato, Usuario, Fecha y hora, Resultado. |
| GRD-REP-01 | Reportes / Indicadores y pendientes | Indicador/pendiente, Periodo, Valor/estado, Accion. |
| GRD-REP-02 | Reportes / Balance general | Grupo, Cuenta, Importe del periodo. |
| GRD-REP-03 | Reportes / Estado de resultados | Grupo, Cuenta, Importe del rango. |
| GRD-REP-04 | Reportes / Planilla consolidada | Departamento, Empleados, Bruto, Descuentos, Neto. |
| GRD-REP-05 | Reportes / Exportaciones | Reporte, Periodo/filtros, Formato, Usuario, Fecha y hora, Resultado. |

## Sorting

- Column sorting must be available for primary identity, date, money, status, and name columns.
- Sorting state must be visible.
- Default sort should match the task: latest records for vouchers/books, name or DNI for employees, account order for statements.
- Sorting must not clear search or filters.
- A primary sort is always visible; optional secondary sorts show their priority.
- Monetary and date columns sort by their underlying value, not formatted text.
- Current sorting is request-local; persisted saved-view sorting is `IMPLEMENTATION PENDING`.

## Filtering

Common filters are visible; advanced filters are disclosed on demand.

Filtering rules:

- `Buscar` narrows the current filtered dataset.
- Active filters appear as removable labeled tokens and in an accessible summary.
- `Limpiar filtros` removes current request-local values; persisted saved-view structure is `IMPLEMENTATION PENDING`.
- Date and period filters show explicit inclusive boundaries.
- Filtered count and filtered totals update together.
- Returning from detail restores all filters and the selected row.

| Grid | Common Filters |
|---|---|
| Employees | Status, department, pension regime. |
| Payroll results | Period, department, payroll status. |
| Vouchers | Period/date range, movement, voucher type, status. |
| SUNAT books | Period, book type, version status. |
| Reports | Period, department, report type. |
| Users | Role, status. |
| Audit log | Date range, user, module, result. |

## Selection

- Single selection is default.
- Multi-selection is only used for explicit batch workflows.
- Selected rows remain selected after refresh only when they still belong to the refreshed result set; removed selections are cleared and announced.
- Actions dependent on selection must state when no row is selected.
- Selection across pages is allowed only for an explicit bulk workflow and must show the total selected count.
- Refresh removes only selections for records that no longer belong to the result set.

## Grouping

- Grouping is available only where it improves comparison: Departamento, Estado, Movimiento, Tipo de comprobante, Tipo de libro, Rol, or account group.
- Group headers show group label, row count, and applicable subtotals.
- Groups can be expanded or collapsed by keyboard.
- Sorting inside a group remains deterministic.
- Grouping applies to the current view/export only; persistence in saved views is `IMPLEMENTATION PENDING`.

## Column Visibility and Order

- Users may show, hide, resize, and reorder optional columns.
- Required identity, status, and primary amount columns cannot all be hidden.
- `Restablecer columnas` returns to the canonical order in this document.
- Column preferences are saved per user and per grid.
- Hidden columns are listed in `Columnas` and remain discoverable.

## Frozen Columns

- Identity columns remain frozen when horizontal scrolling is required.
- GRD-EMP-01 freezes DNI and Nombres y apellidos.
- GRD-PAY-02 freezes Empleado and Departamento.
- GRD-ACC-01 freezes Movimiento, Tipo, Serie, and Numero.
- GRD-SUN-02 freezes Periodo, RUC, Tipo de comprobante, Serie, and Numero.
- Users may add one additional frozen column; `Restablecer columnas` removes custom freezing.

## Saved Views

Status: `IMPLEMENTATION PENDING`. No current API or database object persists personal grid views. The rules below are retained as future UX constraints and must not be presented as active behavior.

A saved view stores search scope, filters, sorting, grouping, visible columns, column order, frozen columns, and density. It never stores row selection or restricted record data.

- Each grid has one system default view.
- Users may create, rename, update, duplicate, and delete personal views.
- A personal view opens automatically only when the user explicitly marks it as the default for that grid.
- If a saved view references a removed field or lost permission, show what changed and fall back safely.

## Bulk Actions

Status: cross-page and partial-result bulk execution is `IMPLEMENTATION PENDING`. Current in-scope commands operate on one identified resource or one payroll/book period; ordinary page selection may be used only for local display until a bulk API contract exists.

- Bulk actions appear only after selecting eligible rows.
- The action summary shows selected count, affected period, eligibility, exclusions, and consequence.
- Mixed eligibility does not silently skip rows; eligible and excluded counts are explicit.
- Deactivation and reactivation are reversible status changes where business rules allow.
- Finalize payroll, post entries, annul vouchers, and generate SUNAT versions are not generic bulk actions.
- Bulk results distinguish completed, rejected, and unknown rows and allow export of the result summary.

## Editing Policy

- Operational lists are read-only and open a detail or form for editing.
- Inline editing is permitted only in GRD-EMP-05 for overtime capture and GRD-ACC-06 for journal-entry lines.
- Inline edits show row-level errors, preserve valid rows, and expose uncommitted changes before navigation.

## Totals and Summaries

Financial grids must show totals in a stable summary area:

- Payroll: gross cash earnings, effective AFP/ONP deduction version and amount, other deductions fixed at S/ 0.00 in current scope, total employee deductions, net cash pay, employer CTS/gratification provisions, and total payroll cost.
- Vouchers: taxable base, exempt base, IGV, total.
- SUNAT books: eligible count, included count, taxable total, IGV total, general total.
- Reports: subtotal and total according to report structure.

Totals must always reflect active search and filters unless explicitly labeled otherwise.

## Pagination and Large Datasets

UX expectations:

- Record count is visible.
- Page size is selectable from standard options.
- Current page and total pages are visible.
- Search/filter changes reset to a sensible first page.
- Export must clearly state whether it applies to current page, filtered results, or full dataset.
- The default export scope is `Resultados filtrados`, never silently `Pagina actual`.
- Page-size and density preferences are independent.

Canonical pagination behavior:

- Default page size is 25 rows; available sizes are 10, 25, and 50.
- `Ctrl+A` selects only rows on the current page.
- Cross-page selection controls remain hidden while bulk API contracts are `IMPLEMENTATION PENDING`.
- `Inicio/Fin` moves within the current row; `Ctrl+Inicio/Ctrl+Fin` moves to first/last row of the current page. First/last dataset navigation uses pagination controls.
- Exact total count is shown when available; a delayed count uses `Calculando total` rather than a provisional number.

## Export Behavior

Before export, show dataset or report name, period/date range, active filters, grouping, sorting, scope, column set, output format, and expected record count.

SUNAT regulatory exports always use the required canonical column set regardless of personal column visibility. PDF/Excel report exports match displayed filtered values unless clearly labeled as regulatory output.

## Row States

Rows may show operational state:

- Empleado Activo/Inactivo.
- Planilla Borrador/Finalizada.
- Comprobante Registrado/Anulado; validation is displayed separately as a computed result.
- Generated/exported book.
- Periodo Abierto/Cerrado.
- Posted/cancelled journal entry.

State labels must be readable in text and should not rely only on color.

## Empty Grid States

Grids must distinguish:

- No records exist yet.
- No results match search/filter.
- User lacks access.
- Data failed to load.
- Period has no eligible data.

Each state must offer a context-appropriate next step.

## Grid Acceptance Checklist

- Search appears before advanced filters.
- Current scope and active filters are visible.
- Columns follow business priority.
- Money and totals are aligned and precise.
- Status is text plus visual cue.
- Selection behavior is clear.
- Export scope is explicit.
- Keyboard navigation is supported.
- Grouping, column visibility, frozen columns, and export scope follow the active standards above; saved views and cross-page bulk actions remain hidden as `IMPLEMENTATION PENDING`.
