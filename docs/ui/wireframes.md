# Wireframes

## Wireframe Purpose

These wireframes define screen structure, hierarchy, content priority, and workflow intent. They are low-fidelity UX artifacts only. They do not specify implementation controls, code, or exact visual styling.

All visible labels use Peruvian Spanish. Every wireframe inherits the persistent sidebar, breadcrumbs, workspace hierarchy, and restoration rules in `layout-specifications.md`. Field and column definitions come only from `forms.md` and `data-grids.md`.

## Canonical Desktop Shell

```text
--------------------------------------------------------------------------
| ERP | Periodo/contexto                         Usuario | Rol | Sesion  |
|-----------------------------------------------------------------------|
| Inicio          | Migas de pan                                        |
| Empleados       | Titulo | Periodo/alcance | Estado | Accion dominante|
| Planillas       |-----------------------------------------------------|
| Contabilidad    | Buscar | Filtros | Orden | Columnas                |
| SUNAT           |-----------------------------------------------------|
| Reportes        | Contenido principal                                 |
| Administracion  |                                                     |
| [Contraer]      |-----------------------------------------------------|
|                 | Conteo | Seleccion | Totales | Estado               |
--------------------------------------------------------------------------
```

Only authorized destinations appear. In collapsed mode, the left column keeps icons, accessible names, selection, and order.

## 1. Iniciar Sesion

```text
------------------------------------------------------------
| ERP - Gestion Financiera y Contable                       |
| Institutional identity                                    |
------------------------------------------------------------
|                                                            |
|                 Iniciar sesion                            |
|                 Usuario                                   |
|                 [ campo ]                                 |
|                 Contrasena                                |
|                 [ campo ]                                 |
|                 [ Iniciar sesion ]                        |
|                 Mensaje de error o estado de cuenta       |
|                                                            |
------------------------------------------------------------
```

UX notes:

- Keep the screen quiet and centered.
- Show only authentication needs and account state.
- Error text remains close to the sign-in action.
- Successful login routes to role-specific Dashboard.

## 2. Inicio

```text
------------------------------------------------------------
| ERP | Periodo/contexto                     Usuario | Rol    |
------------------------------------------------------------
| Barra lateral: Inicio, Empleados, Planillas, Contabilidad, SUNAT, Reportes |
------------------------------------------------------------
| Periodo de trabajo: Mayo 2026 (2026-05)                  |
|----------------------------------------------------------|
| KPI: Total planilla | KPI: Comprobantes | KPI: IGV       |
| KPI: Utilidad       | KPI: Empleados    | KPI: Pendientes |
|----------------------------------------------------------|
| Pendientes / alertas                | Acciones frecuentes|
| - Planilla en borrador              | - Nuevo empleado   |
| - Libro SUNAT pendiente             | - Nuevo comprobante|
| - Configuracion por revisar         | - Ver reporte      |
|----------------------------------------------------------|
| Actividad reciente                  | Reportes frecuentes|
------------------------------------------------------------
```

UX notes:

- Dashboard is role-filtered.
- KPIs drill down to source data.
- Quick actions are limited to frequent tasks.

## 3. Administracion - Usuarios

```text
------------------------------------------------------------
| Administracion > Usuarios                    [Nuevo usuario] |
------------------------------------------------------------
| Buscar usuarios     Rol           Estado                  |
| Filtros activos: Rol=Todos, Estado=Activo                 |
------------------------------------------------------------
| Usuario | Nombres y apellidos | Rol | Ultimo acceso | Estado |
|----------------------------------------------------------|
| ...                                                        |
------------------------------------------------------------
| Conteo | Resumen del usuario | Acciones autorizadas       |
------------------------------------------------------------
```

UX notes:

- User status and role must be visible in the list.
- Risky actions such as deactivate or reset password require confirmation.
- Role management and audit review are nearby but separate sections.

## 4. Empleados - Lista

```text
------------------------------------------------------------
| Empleados                                      [Nuevo empleado] |
------------------------------------------------------------
| Buscar por DNI, nombre, departamento o cargo                |
| Departamento | Regimen pensionario | Estado                 |
| Filtros activos | Conteo | Total de salario activo         |
------------------------------------------------------------
| DNI | Nombres y apellidos | Departamento | Cargo | Salario base | Regimen | Fecha ingreso | Estado |
|--------------------------------------------------------------------|
| ...                                                                |
------------------------------------------------------------
| Resumen seleccionado | Editar | Desactivar/Reactivar | Horas extra |
------------------------------------------------------------
```

UX notes:

- Search comes before filters.
- Salary total reflects active filters.
- Returning from detail preserves list context.

## 5. Empleados - Formulario de Empleado

```text
------------------------------------------------------------
| Nuevo empleado / Editar empleado: [Nombre]       Estado   |
------------------------------------------------------------
| Identidad                                                  |
| DNI | Nombres | Apellidos | Fecha de nacimiento             |
|------------------------------------------------------------|
| Empleo                                                     |
| Departamento | Cargo | Fecha de ingreso                     |
|------------------------------------------------------------|
| Remuneracion y pension                                     |
| Salario base | Regimen pensionario                         |
|------------------------------------------------------------|
| Banco                                                      |
| Banco | Numero de cuenta                                   |
|------------------------------------------------------------|
| Resumen de validacion                                      |
| [Guardar empleado] [Cancelar]                              |
------------------------------------------------------------
```

UX notes:

- Field order follows HR data-entry flow.
- Required and invalid fields are explicit.
- Save failure preserves all entered data.

## 6. Planillas - Calculo y Revision

```text
------------------------------------------------------------
| Planillas > Calculo y revision              Estado: Borrador |
------------------------------------------------------------
| Periodo: Mayo 2026   Preparacion: empleados, horas, reglas |
| [Accion dominante segun estado: Calcular / Recalcular / Finalizar] |
------------------------------------------------------------
| Progreso o estado del calculo                               |
------------------------------------------------------------
| Empleado | Departamento | Haberes brutos | AFP/ONP | Otros descuentos | Neto a pagar | Prov. CTS | Prov. gratificacion | Costo total | Estado periodo |
|-------------------------------------------------------------------|
| ...                                                               |
------------------------------------------------------------
| Totales: Haberes brutos | Descuentos empleado | Neto a pagar | Prov. CTS | Prov. gratificacion | Costo total |
| Exportar boletas | Exportar planilla                              |
------------------------------------------------------------
```

UX notes:

- Estado Borrador/Finalizada is prominent.
- Only one of Calcular, Recalcular, or Finalizar is rendered as the dominant action for the current state.
- Finalization explains recalculation impact.
- Totals remain visible with results.

## 7. Contabilidad - Lista de Comprobantes

```text
------------------------------------------------------------
| Contabilidad > Comprobantes              [Nuevo comprobante] |
------------------------------------------------------------
| Buscar por RUC/DNI, razon social, serie o numero            |
| Periodo/fechas | Movimiento | Tipo | Estado                 |
| Filtros activos | Conteo de comprobantes                    |
------------------------------------------------------------
| Movimiento | Tipo | Serie | Numero | Fecha | RUC/DNI | Razon social | Operacion | Base gravada | Base exonerada | IGV | Total | Estado |
|--------------------------------------------------------------------------------------|
| ...                                                                                  |
------------------------------------------------------------
| Totales: Base gravada | Base exonerada | IGV | Total | Editar | Anular | Actualizar |
------------------------------------------------------------
```

UX notes:

- Movement Compra/Venta is visible.
- Totals reflect active filters.
- Annulment requires confirmation.

## 8. Contabilidad - Formulario de Comprobante

```text
------------------------------------------------------------
| Nuevo comprobante / Editar comprobante            Estado |
------------------------------------------------------------
| Movimiento y documento                                     |
| Compra/Venta | Tipo | Serie | Numero | Fecha de emision      |
|------------------------------------------------------------|
| Tercero                                                     |
| Tipo documento | Numero documento | Razon social            |
|------------------------------------------------------------|
| Operacion, importes e impuesto                              |
| Tipo operacion | Base gravada | Base exonerada              |
| IGV calculado | Total calculado | Version tributaria        |
|------------------------------------------------------------|
| Resumen de validacion                                      |
| [Guardar comprobante] [Guardar y crear otro] [Cancelar]    |
------------------------------------------------------------
```

UX notes:

- Entry order follows a physical voucher.
- Derived tax values stay close to amount fields.
- Duplicate identity errors suggest opening the existing voucher.

## 9. SUNAT - Generar y Revisar Libro

```text
------------------------------------------------------------
| SUNAT > Generar y revisar libro                            |
------------------------------------------------------------
| Periodo: 2026-05 | Libro: Compras / Ventas | Version existente |
| Elegibles | Exclusiones | Estado de validacion              |
| [Generar libro] [Revisar ultima version] [Exportar]        |
------------------------------------------------------------
| Periodo | RUC | Tipo | Serie | Numero | Fecha | Base gravada | Base exonerada | IGV | Total | Validacion |
|--------------------------------------------------------------------------------|
| ...                                                                            |
------------------------------------------------------------
| Totales: elegibles | incluidos | Base gravada | Base exonerada | IGV | Total |
------------------------------------------------------------
```

UX notes:

- Tipo de libro and Periodo must remain visible.
- Existing versions are visible before generating a new version.
- Export is available only when a generated preview exists.

## 10. Reportes - Panel Financiero

```text
------------------------------------------------------------
| Reportes > Panel financiero                               |
------------------------------------------------------------
| Periodo/fechas | Departamento | Reporte | Actualizar       |
------------------------------------------------------------
| KPI: Ingresos | KPI: Costos y gastos | KPI: Utilidad | KPI: Margen |
| KPI: Planilla | KPI: IGV por pagar                         |
------------------------------------------------------------
| Tendencia: ingresos y gastos                               |
| Distribucion: categorias de costo                          |
------------------------------------------------------------
| Reportes: Balance general | Estado de resultados | Planilla consolidada |
| Exportar reporte actual                                    |
------------------------------------------------------------
```

UX notes:

- Charts never replace table/detail access.
- Current filters must be visible in exported context.
- KPI drill-down routes to source report detail.

## 11. Reportes - Detalle de Estado Financiero

```text
------------------------------------------------------------
| Reportes > Balance general / Estado de resultados         |
------------------------------------------------------------
| Periodo/rango | Ultima actualizacion | Exportar              |
------------------------------------------------------------
| Grupo / Cuenta                           | Importe del alcance |
|------------------------------------------------------------|
| Activo / Ingresos                                           |
| Pasivo / Costos                                             |
| Patrimonio / Gastos                                         |
|------------------------------------------------------------|
| Totales y notas de validacion                               |
------------------------------------------------------------
```

UX notes:

- Reports must show period and source context.
- Exported values must match displayed filtered values.
- Missing ledger data is shown as a state, not a silent zero.

## 12. Administracion - Configuracion

```text
------------------------------------------------------------
| Administracion > Configuracion tributaria y previsional / Formatos SUNAT |
------------------------------------------------------------
| Resumen de version activa                                  |
| Vigencia | Tasa/codigo/formato | Estado | Motivo            |
------------------------------------------------------------
| Versiones futuras                                          |
| Versiones historicas                                       |
------------------------------------------------------------
| [Nueva version] [Ver auditoria]                            |
------------------------------------------------------------
```

UX notes:

- Active, future, and historical versions are separated.
- Historical meaning must be protected.
- Configuration changes require explicit confirmation and audit context.

## Canonical Pattern - List and Detail

```text
--------------------------------------------------------------------------
| Migas de pan                                                            |
| Titulo | Alcance/periodo | Estado                     [Accion dominante] |
| Buscar | Filtros comunes | Filtros avanzados | Vista | Columnas        |
|-------------------------------------------------------------------------|
| Columnas canonicas y filas                                               |
|-------------------------------------------------------------------------|
| Conteo | Seleccion | Subtotales/totales | Paginacion                   |
| Panel contextual opcional: identidad, relaciones, historial, acciones   |
--------------------------------------------------------------------------
```

## Canonical Pattern - Form

```text
--------------------------------------------------------------------------
| Migas de pan                                                            |
| Nuevo/Editar [objeto] | Identidad | Estado                              |
|-------------------------------------------------------------------------|
| Grupo 1: campos en orden canonico                                        |
| Grupo 2: campos en orden canonico                                        |
| Valores calculados o derivados                                           |
|-------------------------------------------------------------------------|
| Resumen de validacion                                                     |
| [Guardar objeto] [Guardar y crear otro si aplica] [Cancelar]             |
--------------------------------------------------------------------------
```

## Canonical Pattern - Process and Report

```text
--------------------------------------------------------------------------
| Migas de pan                                                            |
| Titulo | Periodo/alcance | Estado                     [Accion dominante] |
| Preparacion / validaciones / filtros                                     |
|-------------------------------------------------------------------------|
| Progreso, KPIs, grilla o estructura del reporte                          |
|-------------------------------------------------------------------------|
| Conteos | Totales | Estado | Exportar / Verificar estado                |
--------------------------------------------------------------------------
```

## Complete Wireframe Coverage

| Screen ID | Pattern / Section | Required Specific Content |
|---|---|---|
| AUT-01 | Section 1 | Usuario, Contrasena, account/error state. |
| DASH-01 | Section 2 | Up to six role KPIs, one work queue, up to four frequent actions. |
| ADM-01 | Section 3 / List | GRD-ADM-01. |
| ADM-02 | Form | FRM-ADM-01 and separate security actions. |
| ADM-03 | List | GRD-ADM-02. |
| ADM-04 | Detail | Role description and module scope; no fine-grained permission editor. |
| ADM-05 | Section 12 / List | GRD-ADM-03 with active, future, historical groups. |
| ADM-06 | Form | FRM-ADM-02. |
| ADM-07 | Section 12 / List | GRD-ADM-04 grouped by book type. |
| ADM-08 | Form | FRM-ADM-03. |
| ADM-09 | List and Detail | GRD-ADM-05 with actor, action, entity, result, and timestamp. |
| ADM-10 | IMPLEMENTATION PENDING | No canonical backup persistence/API; do not render as active. |
| EMP-01 | Section 4 | GRD-EMP-01 including Fecha de ingreso. |
| EMP-02 | Section 5 | FRM-EMP-01 exact order. |
| EMP-03 | Detail | Employee identity, employment summary, status, payroll/overtime links, history. |
| EMP-04 | List | GRD-EMP-03. |
| EMP-05 | Form | FRM-EMP-02. |
| EMP-06 | List | GRD-EMP-04 by period. |
| EMP-07 | IMPLEMENTATION PENDING | Bulk/partial-result contract absent; use single-row EMP-06 registration. |
| PAY-01 | List | GRD-PAY-01; default sort latest period. |
| PAY-02 | Section 6 / Process | FRM-PAY-01 and GRD-PAY-02 separating cash payment from employer provisions. |
| PAY-03 | Detail | Employee, period, salary, overtime, effective AFP/ONP deduction, employee deductions, net cash pay, CTS/gratification provisions and total cost. |
| PAY-04 | IMPLEMENTATION PENDING | Direct period PDF/ZIP export remains available from PAY-02. |
| PAY-05 | IMPLEMENTATION PENDING | No payroll-export history persistence/list contract. |
| ACC-01 | Section 7 | GRD-ACC-01 including Tipo de operacion and Base exonerada. |
| ACC-02 | Section 8 | FRM-ACC-01 exact order. |
| ACC-03 | Detail | Voucher identity, tax version, amounts, state, SUNAT links, history. |
| ACC-04 | List | GRD-ACC-03 hierarchy and request-local filters. |
| ACC-05 | Form | FRM-ACC-02. |
| ACC-06 | List/Configuration | GRD-ACC-04 and FRM-ACC-03 for controlled state transitions. |
| ACC-07 | List | GRD-ACC-05. |
| ACC-08 | Form plus editable grid | FRM-ACC-04 and GRD-ACC-06 with persistent debit/credit totals. |
| ACC-09 | Detail | Header, balanced lines, source, state, and history. |
| SUN-01 | Process/List | Period by book status, latest version, validation, and pending exports. |
| SUN-02 | Section 9 | FRM-SUN-01 and GRD-SUN-02 exact columns. |
| SUN-03 | List | GRD-SUN-03. |
| SUN-04 | Detail | Version metadata, included vouchers, exclusions, totals, validations. |
| SUN-05 | IMPLEMENTATION PENDING | Direct immutable-version export remains available from SUN-04. |
| REP-01 | Section 10 | FRM-REP-01, role KPIs, trends, table equivalents. |
| REP-02 | Section 11 / Report | Assets, liabilities, equity and totals for the requested period. |
| REP-03 | Section 11 / Report | Income, costs, expenses and net result for the requested range. |
| REP-04 | Report | Period, department, employee/concept totals, drill-down. |
| REP-05 | List and Detail | Report, filters, format, user, timestamp, result. |

## Screen Region and Action Map

This map is normative. It completes the annotated wireframe for each screen by fixing header context, dominant action, main region, and persistent summary/context.

| ID | Header and Dominant Action | Main Region | Summary / Context Region |
|---|---|---|---|
| AUT-01 | Product identity; `Iniciar sesion`. | Usuario, Contrasena, generic authentication error. | Account lock timer when applicable. |
| DASH-01 | `Inicio`; no global dominant mutation. | Up to six role KPIs and one work queue. | Up to four frequent actions and recent authorized activity. |
| ADM-01 | `Administracion > Usuarios`; `Nuevo usuario`. | GRD-ADM-01 with search, Rol, Estado. | Selected account status and allowed security actions. |
| ADM-02 | User identity; `Guardar usuario` in create/edit state. | FRM-ADM-01 grouped as identity, access, status. | Last access, audit context, Desactivar/Reactivar/Restablecer contrasena. |
| ADM-03 | `Administracion > Roles`; no create action. | GRD-ADM-02 predefined roles. | Selected role module scope. |
| ADM-04 | Role name; no mutation. | Read-only role description and module/action scope. | Audit of role assignments. |
| ADM-05 | `Configuracion tributaria y previsional`; `Nueva version`. | GRD-ADM-03 grouped by IGV/AFP/ONP and state. | Selected version dates, usage, and status. |
| ADM-06 | Code/version; `Guardar borrador`, `Activar version`, or `Cerrar version` according to state. | FRM-ADM-02. | Overlap validation and historical-use warning. |
| ADM-07 | `Formatos SUNAT`; `Nueva version`. | GRD-ADM-04 grouped by book type. | Selected version validity and usage. |
| ADM-08 | Book type/version; `Guardar borrador`, `Activar formato`, or `Cerrar formato` according to state. | FRM-ADM-03. | Structural validation and immutability warning. |
| ADM-09 | `Registro de auditoria`; no export command in current scope. | GRD-ADM-05 with date, user, module, result filters. | Immutable event detail and correlation reference. |
| ADM-10 | IMPLEMENTATION PENDING; no active command. | No canonical data region. | Backup operations remain external. |
| EMP-01 | `Empleados`; `Nuevo empleado`. | GRD-EMP-01 with search and filters. | Count, salary total for authorized role, selected status/actions. |
| EMP-02 | New/edit identity; `Guardar empleado`. | FRM-EMP-01 in canonical focus order. | Validation summary; privacy state; Cancelar. |
| EMP-03 | Employee identity/state; `Editar` only when allowed. | Personal/employment summary and overtime relation. | Payroll-history panel is implementation pending; Desactivar/Reactivar. |
| EMP-04 | `Departamentos`; `Nuevo departamento`. | GRD-EMP-03. | Active employee count and selected status. |
| EMP-05 | Department identity; `Guardar departamento`. | FRM-EMP-02. | Assignment impact and Desactivar/Reactivar. |
| EMP-06 | `Horas extra`; `Registrar horas extra`. | GRD-EMP-04 filtered by period/state. | Included/excluded count and status legend. |
| EMP-07 | IMPLEMENTATION PENDING; no bulk command. | Use EMP-06 single-row registration. | No partial-result summary is active. |
| PAY-01 | `Periodos de planilla`; `Calcular planilla` when selected period has no record. | GRD-PAY-01. | Selected period state, employees, totals, last update. |
| PAY-02 | Period/state; exactly one of Calculate, Recalculate, Finalize. | Readiness region plus GRD-PAY-02. | Persistent totals, blocking errors, operation status, export actions. |
| PAY-03 | Employee/period/state; no mutation after Finalized. | GRD-PAY-03 concept breakdown. | Gross, discounts, benefits, net, applied rules. |
| PAY-04 | IMPLEMENTATION PENDING as a persistent center. | Direct PDF/ZIP export is exposed from PAY-02. | No stored generation status. |
| PAY-05 | IMPLEMENTATION PENDING; no active command. | No canonical history grid. | Direct export only. |
| ACC-01 | `Comprobantes`; `Nuevo comprobante`. | GRD-ACC-01 with period/movement/type/status filters. | Filtered base, IGV, total and selected lifecycle actions. |
| ACC-02 | New/edit voucher identity; `Guardar comprobante` or `Guardar y crear otro`. | FRM-ACC-01. | Calculated IGV/total, duplicate state, tax version, validation summary. |
| ACC-03 | Voucher identity/state; `Editar` or `Anular` only when allowed. | Voucher fields, tax calculation and book links. | Inline audit is implementation pending; immutability reason remains visible. |
| ACC-04 | `Plan de cuentas`; `Nueva cuenta` for Contador/Admin. | GRD-ACC-03 hierarchical list. | Selected account balance, children, state; Gerente read-only. |
| ACC-05 | Account identity; `Guardar cuenta` only for Contador/Admin. | FRM-ACC-02. | Usage lock, hierarchy validation, Desactivar/Reactivar. |
| ACC-06 | `Periodos contables`; `Nuevo periodo` for Contador/Admin. | GRD-ACC-04. | Selected period entry counts; `Cerrar periodo` only when eligible. |
| ACC-07 | `Asientos contables`; `Nuevo asiento` for Contador/Admin. | GRD-ACC-05. | Filtered debit/credit totals and selected state. |
| ACC-08 | Draft entry identity; `Guardar borrador` or `Contabilizar`, state-dependent. | FRM-ACC-04 plus GRD-ACC-06. | Persistent debit, credit, difference, errors. |
| ACC-09 | Entry identity/state; `Revertir` only for Posted and authorized role. | Header, source, lines and linked reversal. | Inline audit is implementation pending; Gerente read-only. |
| SUN-01 | `Centro de libros SUNAT`; `Generar libro`. | GRD-SUN-01 by period/type. | Latest version, validation, pending export. |
| SUN-02 | Period/type/version; `Validar` then `Generar nueva version` by validation state. | FRM-SUN-01 plus GRD-SUN-02 preview. | Eligibility, exclusions, warnings, totals, operation state. |
| SUN-03 | `Versiones de libros`; no mutation. | GRD-SUN-03. | Current/superseded indicator and selected version totals. |
| SUN-04 | Version identity/state; `Exportar version`. | Immutable voucher snapshot and validation evidence. | Generator, timestamp, applied configuration, superseding version. |
| SUN-05 | IMPLEMENTATION PENDING as persisted history. | Direct PDF/Excel export is exposed from SUN-04. | No stored retry state. |
| REP-01 | `Panel financiero`; `Actualizar`. | Role-authorized KPIs, trends, equivalent table. | Period/freshness and report links. |
| REP-02 | `Balance general`; `Exportar`. | GRD-REP-02 grouped Asset/Liability/Equity. | Period, totals and freshness. |
| REP-03 | `Estado de resultados`; `Exportar`. | GRD-REP-03 grouped Income/Cost/Expense. | Range, net result and freshness. |
| REP-04 | `Planilla consolidada`; `Exportar`. | GRD-REP-04 with period/department filters. | Employee count, gross, discounts, net; no bank data. |
| REP-05 | `Centro de exportaciones`; `Actualizar`. | GRD-REP-05. | Selected operation filters/status; Open, Retry, or Verify. |

Every mapped screen uses the canonical shell and one dominant action for its current state. Lists use the exact columns in `data-grids.md`; forms use the exact fields and focus order in `forms.md`; permissions use `ux-traceability.md`; lifecycles use `states.md`.
