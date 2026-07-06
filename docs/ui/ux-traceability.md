# Matriz de Trazabilidad UX

## Proposito

Esta matriz conecta pantallas, recorridos, formularios, grillas y permisos. Los identificadores de pantalla provienen de `screen-inventory.md`. Los permisos de pantalla son: `V` consultar, `O` operar segun la matriz de acciones, `A` acceso total dentro del alcance funcional existente, `-` sin acceso. La matriz de acciones es autoritativa cuando una pantalla contiene varias acciones.

El sistema tiene cuatro roles RBAC: `Administrador RRHH`, `Contador`, `Gerente Financiero` y `Administrador Sistema`. `Operador SUNAT` es una persona de trabajo asignada al rol `Contador`, no un quinto rol.

## Recorridos Canonicos

| ID | Recorrido |
|---|---|
| J-01 | Iniciar sesion y entrar por rol. |
| J-02 | Registrar, editar o desactivar empleado. |
| J-03 | Registrar horas extra de un periodo. |
| J-04 | Calcular, revisar y finalizar planilla. |
| J-05 | Registrar, revisar o anular comprobante. |
| J-06 | Mantener plan de cuentas, periodos y asientos. |
| J-07 | Generar, versionar y exportar libros SUNAT. |
| J-08 | Consultar y exportar reportes financieros. |
| J-09 | Administrar usuarios y roles. |
| J-10 | Versionar configuracion tributaria y formatos SUNAT. |
| J-11 | Revisar auditoria y respaldos. |

## P1 Support Boundary

ADM-10, EMP-07 bulk semantics, PAY-04/PAY-05 persistent centers, SUN-05 persisted export history, saved grid views, cross-page bulk execution, and report comparison are `IMPLEMENTATION PENDING`. They do not participate in the 64 in-scope workflow chains. Supported single-record, direct-export, audit-query, filter, sort and pagination actions remain governed by the API operations named in `tests/traceability/workflow-traceability.md`.

## Matriz por Pantalla

| Pantalla | Recorrido | Formulario | Grilla | RRHH | Contador | Gerente | Admin. Sistema |
|---|---|---|---|---:|---:|---:|---:|
| AUT-01 | J-01 | FRM-AUT-01 | - | O | O | O | O |
| DASH-01 | J-01 | - | GRD-DASH-01 | V | V | V | V |
| ADM-01 | J-09 | - | GRD-ADM-01 | - | - | - | A |
| ADM-02 | J-09 | FRM-ADM-01 | - | - | - | - | A |
| ADM-03 | J-09 | - | GRD-ADM-02 | - | - | - | A |
| ADM-04 | J-09 | - | GRD-ADM-02 | - | - | - | A |
| ADM-05 | J-10 | - | GRD-ADM-03 | - | - | - | A |
| ADM-06 | J-10 | FRM-ADM-02 | - | - | - | - | A |
| ADM-07 | J-10 | - | GRD-ADM-04 | - | - | - | A |
| ADM-08 | J-10 | FRM-ADM-03 | - | - | - | - | A |
| ADM-09 | J-11 | - | GRD-ADM-05 | - | - | - | A |
| ADM-10 | J-11 | - | GRD-ADM-06 | - | - | - | A |
| EMP-01 | J-02 | - | GRD-EMP-01 | O | - | - | A |
| EMP-02 | J-02 | FRM-EMP-01 | - | O | - | - | A |
| EMP-03 | J-02 | - | GRD-EMP-02 | O | - | - | A |
| EMP-04 | J-02 | - | GRD-EMP-03 | O | - | - | A |
| EMP-05 | J-02 | FRM-EMP-02 | - | O | - | - | A |
| EMP-06 | J-03 | - | GRD-EMP-04 | O | - | - | A |
| EMP-07 | J-03 | FRM-EMP-03 | GRD-EMP-05 | O | - | - | A |
| PAY-01 | J-04 | - | GRD-PAY-01 | O | - | - | A |
| PAY-02 | J-04 | FRM-PAY-01 | GRD-PAY-02 | O | - | - | A |
| PAY-03 | J-04 | - | GRD-PAY-03 | O | - | - | A |
| PAY-04 | J-04 | - | GRD-PAY-04 | O | - | - | A |
| PAY-05 | J-04 | - | GRD-PAY-05 | O | - | - | A |
| ACC-01 | J-05 | - | GRD-ACC-01 | - | O | - | A |
| ACC-02 | J-05 | FRM-ACC-01 | - | - | O | - | A |
| ACC-03 | J-05 | - | GRD-ACC-02 | - | O | - | A |
| ACC-04 | J-06 | - | GRD-ACC-03 | - | O | V | A |
| ACC-05 | J-06 | FRM-ACC-02 | - | - | O | V | A |
| ACC-06 | J-06 | FRM-ACC-03 | GRD-ACC-04 | - | O | V | A |
| ACC-07 | J-06 | - | GRD-ACC-05 | - | O | V | A |
| ACC-08 | J-06 | FRM-ACC-04 | GRD-ACC-06 | - | O | - | A |
| ACC-09 | J-06 | - | GRD-ACC-06 | - | O | V | A |
| SUN-01 | J-07 | - | GRD-SUN-01 | - | O | - | A |
| SUN-02 | J-07 | FRM-SUN-01 | GRD-SUN-02 | - | O | - | A |
| SUN-03 | J-07 | - | GRD-SUN-03 | - | O | - | A |
| SUN-04 | J-07 | - | GRD-SUN-03 | - | O | - | A |
| SUN-05 | J-07 | - | GRD-SUN-04 | - | O | - | A |
| REP-01 | J-08 | FRM-REP-01 | GRD-REP-01 | - | - | V | A |
| REP-02 | J-08 | FRM-REP-01 | GRD-REP-02 | - | - | V | A |
| REP-03 | J-08 | FRM-REP-01 | GRD-REP-03 | - | - | V | A |
| REP-04 | J-08 | FRM-REP-01 | GRD-REP-04 | - | - | V | A |
| REP-05 | J-08 | - | GRD-REP-05 | - | - | V | A |

## Matriz de Permisos por Accion

`Si` means the role may execute the action when lifecycle conditions are satisfied. `No` means the action must not appear or execute. `N/A` means the functional scope has no such action.

| Accion y alcance | RRHH | Contador | Gerente | Admin. Sistema | Restriccion autoritativa |
|---|---:|---:|---:|---:|---|
| Crear empleado, departamento u horas extra | Si | No | No | Si | Employee and department rules apply. |
| Editar empleado o departamento | Si | No | No | Si | Only active records; salary/bank privacy applies. |
| Desactivar/Reactivar empleado o departamento | Si | No | No | Si | Logical status change; no physical deletion. |
| Crear o editar usuario | No | No | No | Si | Role is one of the four predefined roles. |
| Desactivar/Reactivar usuario | No | No | No | Si | Logical status change; effective on next authentication. |
| Restablecer contrasena | No | No | No | Si | Confirmation required; temporary credential is never redisplayed. |
| Crear version tributaria o previsional | No | No | No | Si | Creates Borrador; Active/Closed are immutable. |
| Activar version tributaria o previsional | No | No | No | Si | Borrador -> Activa; valid dates and no overlap. |
| Cerrar version tributaria o previsional | No | No | No | Si | Activa -> Cerrada; no field changes. |
| Crear version de formato SUNAT | No | No | No | Si | Creates Borrador with governed structure. |
| Activar version de formato SUNAT | No | No | No | Si | Borrador -> Activa; structure valid and no overlap. |
| Cerrar version de formato SUNAT | No | No | No | Si | Activa -> Cerrada; generated books remain unchanged. |
| Crear o editar comprobante | No | Si | No | Si | Only Registrado and not locked by a generated book. |
| Anular comprobante | No | Si | No | Si | No physical deletion; Anulado is terminal. |
| Crear o editar cuenta, periodo o asiento | No | Si | No | Si | Draft/open-state restrictions apply. |
| Desactivar/Reactivar cuenta contable | No | Si | No | Si | Inactive accounts cannot receive new lines. |
| Aprobar horas extra | Si | No | No | Si | Draft -> Approved before payroll calculation. |
| Aprobar genericamente otros registros | N/A | N/A | N/A | N/A | No generic approval state exists in scope. |
| Finalizar planilla | Si | No | No | Si | Draft -> Finalized after successful calculation/review. |
| Reabrir planilla finalizada | No | No | No | No | Prohibited by Draft-only recalculation rule. |
| Cancelar planilla en borrador | Si | No | No | Si | Draft -> Cancelled; terminal. |
| Exportar planilla y boletas | Si | No | No | Si | Direct period PDF/ZIP or Excel export only; persistent export centers are implementation pending. |
| Contabilizar asiento | No | Si | No | Si | Balanced Draft in Open period -> Posted. |
| Cancelar asiento en borrador | No | Si | No | Si | Draft -> Cancelled; terminal. |
| Revertir asiento contabilizado | No | Si | No | Si | Creates linked adjustment Draft; original remains Posted. |
| Reabrir periodo contable cerrado | No | No | No | No | Prohibited; corrections use an adjustment in an Open period. |
| Cerrar periodo contable | No | Si | No | Si | Requires no Draft entries in the period. |
| Generar nueva version de libro SUNAT | No | Si | No | Si | Sequential immutable version; previous version becomes superseded by relation. |
| Generar archivo/exportacion SUNAT | No | Si | No | Si | Only a generated version with nonblocking validation. |
| Exportar reportes financieros | No | No | Si | Si | Values and filters must match the displayed report. |
| Eliminar fisicamente registros financieros | No | No | No | No | Vouchers, payroll, entries, books, and exports are preserved. |
| Ver salario individual y datos bancarios | Si | No | No | Si | Full values only for payroll responsibility. |
| Ver totales operativos del periodo de planilla | Si | No | No | Si | RRHH/Admin use PAY-02; this does not grant access to the Reports module. |
| Ver reporte consolidado de planilla | No | No | Si | Si | REP-04; never exposes bank account data. |
| Ver saldos contables operativos | No | Si | Si | Si | Gerente is read-only; Contador/Admin may operate through ledger screens. |
| Ver balance general y estado de resultados | No | No | Si | Si | Reports module only. |

## Reglas de Trazabilidad

- Un permiso `V` no permite mutaciones. Export is allowed only when the action matrix explicitly grants it.
- Un permiso `O` permits only the actions marked `Si` for that role and lifecycle state.
- Un permiso `A` incluye consulta y operacion dentro del alcance administrativo.
- La interfaz oculta destinos no autorizados y explica una denegacion cuando se intenta restaurar un destino antiguo.
- Todo cambio en pantalla, formulario o grilla debe actualizar esta matriz y el inventario en la misma revision documental.
