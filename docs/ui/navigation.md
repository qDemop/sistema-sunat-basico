# Navigation Architecture

## Navigation Objective

Navigation must make the ERP feel like a stable desktop workspace. Users should always know where they are, what role context they are operating under, and how to return to their prior dataset without losing filters.

The normative shell and restoration behavior are defined in `layout-specifications.md`. Screen identifiers and exact permissions are defined in `screen-inventory.md` and `ux-traceability.md`.

## Navigation Model

```text
Iniciar sesion
  -> Inicio
      -> Modulo
          -> Lista / Reporte / Configuracion
              -> Detalle / Formulario / Vista previa
                  -> Registro relacionado / Exportacion / Historial
```

`Inicio` is the authenticated starting point. Modules are role-filtered. Each module maintains its in-session context, such as selected period, request-local filters, sort, grouping, and selected record. Persisted saved views are `IMPLEMENTATION PENDING`.

## Persistent Sidebar

- The left sidebar is always available in the primary window.
- It supports expanded and collapsed states without changing destination order.
- Expanded state shows icon and Spanish label; collapsed state keeps icon, accessible name, and contextual help.
- The selected module remains visible while navigating within its workspaces.
- Sidebar state is restored per user and contracts automatically at the narrow layout defined in `layout-specifications.md`.

## Global Navigation

Global navigation includes only stable destinations:

- Inicio.
- Empleados.
- Planillas.
- Contabilidad.
- SUNAT.
- Reportes.
- Administracion.

Global navigation must not include task-specific actions such as save, generate, calculate, or export. Those actions belong inside the workspace where their effect is clear.

## Role-Based Visibility

| Modulo | RRHH | Contador | Gerente Financiero | Administrador Sistema |
|---|---:|---:|---:|---:|
| Inicio | Consultar | Consultar | Consultar | Administrar |
| Empleados | Operar | Sin acceso | Sin acceso | Administrar |
| Planillas | Operar | Sin acceso | Sin acceso | Administrar |
| Contabilidad | Sin acceso | Operar | Consultar libro mayor | Administrar |
| SUNAT | Sin acceso | Operar | Sin acceso | Administrar |
| Reportes | Sin acceso | Sin acceso | Consultar y exportar | Administrar |
| Administracion | Sin acceso | Sin acceso | Sin acceso | Administrar |

If a module is unavailable to a role, it should not appear as a primary navigation item. If a user lands on a restricted destination through history or deep link, show a permission state with a clear path back.

`Operador SUNAT` uses the `Contador` role and therefore receives the same Contabilidad/SUNAT destinations; it is not rendered as a separate role or permission column.

## Workspace Navigation

Each module workspace uses a consistent internal structure:

- Section navigation for related areas inside the module.
- Current section title and status.
- Search and filters where records are involved.
- Primary action scoped to the current section.
- Detail or preview surfaces reached from selected records.

## Breadcrumbs and Location

Breadcrumbs or location labels should be used when a user moves beyond one level of detail:

```text
Planillas > Periodos > 2026-05 > Resultados
Contabilidad > Comprobantes > Factura F001-000123
SUNAT > Libro de ventas > 2026-05 > Version 2
Administracion > Usuarios > maria.gomez
```

Location should be descriptive and business-readable, not technical.

## Search Navigation

Search is a navigation primitive. Users should be able to jump directly to:

- Employee by DNI or name.
- Voucher by RUC, business name, series, or number.
- Payroll period.
- SUNAT book period and version.
- User account.
- Report by name.

`Buscar en el ERP` is `IMPLEMENTATION PENDING` because no cross-module search API exists. It remains hidden and `Ctrl+K` remains unassigned; module-local filters are the active search behavior.

## Recents and Favorites

The Dashboard should support quick return to recent work:

- Recently opened employees.
- Recent vouchers.
- Current payroll period.
- Latest generated SUNAT books.
- Frequently used reports.

Favorites are outside the current release and must not be implemented. Recents remain in scope and never reveal records outside a user's role.

## Back and Cancel Behavior

- Returning from detail to list preserves search, filters, sort, page, and selected row where practical.
- Canceling a form returns to the source context without saving.
- If there are unsaved changes, the user must choose whether to discard or continue editing.
- Back navigation must not repeat destructive or expensive actions such as finalization, calculation, or book generation.

## Primary Action Placement

Primary actions belong in the workspace header or the immediate task area:

| Screen Type | Primary Action Examples |
|---|---|
| Lista de empleados | Nuevo empleado. |
| Formulario de empleado | Guardar empleado. |
| Calculo de planilla | Calcular planilla or Finalizar planilla depending on state. |
| Lista de comprobantes | Nuevo comprobante. |
| Libro SUNAT | Generar libro. |
| Reporte | Exportar or Actualizar depending on report context. |
| Administracion de usuarios | Nuevo usuario. |

Only one action should be visually dominant for the current workflow state. For example, `Calcular planilla` dominates before a valid draft exists; `Finalizar planilla` dominates only after review succeeds.

## Window Restoration

Restore window size, position, maximized state, sidebar state, last authorized module, and safe selection. Persisted filters, sorting, grouping, and saved views are `IMPLEMENTATION PENDING` and are not restored. Never restore a confirmation, destructive action, or completed process as if it were pending. If authorization changed, return to `Inicio`.

## Escape Routes

Every workspace must provide a safe escape:

- Dashboard.
- Back to list.
- Cancel edit.
- Clear filters.
- Return to previous period.
- Logout.

Escape routes must be predictable and must not save changes silently.

## Navigation Acceptance Checklist

- Current module and section are visible.
- Role-limited modules are hidden or clearly denied.
- Frequent tasks are reachable within 3 interactions from Dashboard.
- Search can route users directly to common records.
- Filter and list state is preserved after detail navigation.
- Irreversible operations are not triggered by navigation alone.
- Session expiration provides a clear path back to login without data ambiguity.
