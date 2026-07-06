# Search Experience Guidelines

## P1 Support Status

Cross-module `Buscar en el ERP` is `IMPLEMENTATION PENDING` and must remain hidden because no canonical global-search API exists. The global-search rules below are retained as future constraints. Module-local search and filters are active only when represented by parameters in the corresponding list operation.

## Search Objective

Search is the fastest path to work. The ERP should assume users often know the record, period, RUC, DNI, voucher number, or report they need. Search must be visible, forgiving, and consistent across modules.

Visible labels use Peruvian Spanish: `Buscar en el ERP`, `Buscar en esta pantalla`, `Filtros`, `Limpiar filtros`, and `Sin resultados`.

## Search Types

| Search Type | Scope | Examples |
|---|---|---|
| Global search (`IMPLEMENTATION PENDING`) | Across authorized modules; hidden in the current release. | Future constraint only. |
| Module search | Current module records. | Employees, vouchers, users, books. |
| Grid search | Current table and filters. | Name, series, number, status. |
| Report search | Within report rows or report catalog. | Account name, department, line item. |

## Search Placement

- Global search has no active placement or shortcut while it remains `IMPLEMENTATION PENDING`.
- Module search appears at the top of list workspaces.
- Search should be available before advanced filters.
- Search input remains visible after results are shown.

## Searchable Entities

| Entity | Search Keys |
|---|---|
| Empleado | DNI, Nombres y apellidos, Departamento, Cargo. |
| Comprobante | RUC/DNI, Razon social, Serie, Numero, Movimiento, Tipo. |
| Planilla | Periodo, Empleado, Departamento, Estado. |
| Libro SUNAT | Periodo, Tipo de libro, Version, Generado por. |
| Usuario | Usuario, Nombres y apellidos, Rol, Estado. |
| Reporte | Nombre, Periodo, Departamento, Grupo de cuentas. |
| Evento de auditoria | Usuario, Modulo, Accion, Resultado, Rango de fechas. |

## Query Behavior

Search should support:

- Partial matches.
- Case-insensitive matching.
- Numeric identity search without requiring punctuation.
- Search within active role permissions only.
- Stable results after navigation and return.
- Clear no-results messaging.
- Exact normalized DNI, RUC, username, period, series-number, and account-code matches rank before partial text matches.
- Results are grouped by authorized entity type and ordered by exactness, recency only as a tie-breaker, and business relevance.
- Punctuation and spaces in DNI, RUC, series, and number are normalized without changing the displayed value.
- The current scope is always visible: `Todo el ERP` or the active module.

Search should not require users to choose an exact field before typing.

Future constraint: once contracted, global search is navigation-oriented and does not replace module filters. This behavior is not active in the current release.

## Filter Interaction

Search and filters combine:

```text
Search term + visible filters + advanced filters = current result set
```

Rules:

- Active filters summary must show all constraints.
- Users can remove individual filters.
- Clearing search does not clear filters unless the user chooses clear all.
- Filtered totals reflect the visible result set.

## Search Results

Search results should show enough information to disambiguate:

| Result Type | Required Context |
|---|---|
| Empleado | DNI, Nombres y apellidos, Departamento, Estado. |
| Comprobante | Movimiento, Tipo, Serie-Numero, Fecha, RUC/Razon social, Total, Estado. |
| Periodo de planilla | Periodo, Estado, Neto, Empleados. |
| Libro SUNAT | Periodo, Tipo de libro, Version, Fecha de generacion, Estado. |
| Usuario | Usuario, Nombres y apellidos, Rol, Estado. |
| Reporte | Nombre, Periodo o filtros disponibles, Ultima actualizacion. |

## No Results

No-results states must indicate whether:

- The term has no matches.
- Filters are too restrictive.
- The user lacks access.
- The dataset is empty.

Good message pattern:

```text
No vouchers found for "F001-123" in period 2026-05.
Clear filters or search all periods.
```

## Performance Perception

Search must feel immediate even when data is large.

UX rules:

- Show a lightweight loading state when results are delayed.
- Do not clear old results before new results are ready unless necessary.
- Avoid full-screen blocking for ordinary searches.
- Preserve typed input during loading or failure.

## Search Shortcuts

Keyboard shortcuts should support:

- Global-search focus is unavailable while that capability remains `IMPLEMENTATION PENDING`.
- Focus current module search.
- Clear current search.
- Move through results.
- Open selected result.

Shortcut hints should be discoverable and consistent.

## Privacy and Security

- Future global-search results must be authorization-filtered; no current results are exposed.
- Restricted matches should not reveal sensitive record details.
- Search-history storage and named saved views are outside the current release.

## Saved Filters and Views

Status: `IMPLEMENTATION PENDING`. No search term or personal view is persisted in the current release. The future constraints remain in `data-grids.md` and must not be rendered as active behavior.
- Audit-sensitive searches should follow the system audit policy where required.

## Search Acceptance Checklist

- Search appears in every primary list.
- Users can search by the identifiers they naturally know.
- Search and filters combine predictably.
- Active constraints and counts are visible.
- No-results states are actionable.
- Keyboard access is supported.
- Unauthorized records are not exposed.
