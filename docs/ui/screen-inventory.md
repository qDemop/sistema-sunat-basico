# Inventario de Pantallas

## Proposito

Este inventario es la fuente canonica de pantallas de ERP.WinForms. Cada identificador se usa en navegacion, formularios, grillas, recorridos, wireframes y trazabilidad. La interfaz visible al usuario se expresa en espanol del Peru.

## Leyenda

| Codigo | Significado |
|---|---|
| L | Lista o centro de trabajo. |
| F | Formulario de alta o edicion. |
| D | Detalle o consulta. |
| P | Proceso, vista previa o reporte. |
| C | Configuracion. |

## Estado de Soporte P1

The following auxiliary experiences remain documented but are not active workflow links because no canonical API/database contract exists. `IMPLEMENTATION PENDING` is authoritative over action wording elsewhere in UX documents and does not add scope:

| Screen / capability | Status | Supported alternative |
|---|---|---|
| ADM-10 backup status | IMPLEMENTATION PENDING | Operational backup procedures remain external; no `BACKUP_LOG` or API is claimed. |
| EMP-07 multi-row/partial-result overtime save | IMPLEMENTATION PENDING | EMP-06 may create one overtime row through `createHorasExtra`; approval/cancellation commands remain supported. |
| PAY-04 persistent payslip center | IMPLEMENTATION PENDING | PAY-02 may call the period PDF/ZIP export directly. |
| PAY-05 payroll-export history | IMPLEMENTATION PENDING | Direct payroll PDF/Excel export has no persisted history contract. |
| SUN-05 persisted SUNAT-export history/retry | IMPLEMENTATION PENDING | SUN-04 may call the version PDF/Excel export directly. |
| Saved personal grid views and cross-page bulk operations | IMPLEMENTATION PENDING | Current filters/sort/page are request-local only. |
| Report comparison columns/period | IMPLEMENTATION PENDING | Canonical APIs return the requested period/range only. |
| Employee payroll-history panel | IMPLEMENTATION PENDING | Overtime can be filtered by employee; no payroll-history-by-employee API exists. |
| Voucher/journal inline audit-history panels for operational roles | IMPLEMENTATION PENDING | Canonical audit review remains ADM-09 for Administrador Sistema only. |

## Autenticacion e Inicio

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| AUT-01 | Iniciar sesion | F | Todos | Usuario, contrasena, estado de cuenta. | Inicio o mensaje recuperable. |
| DASH-01 | Inicio | P | Todos | KPIs por rol, pendientes, accesos frecuentes y actividad reciente. | Modulo, registro o reporte autorizado. |

## Administracion

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| ADM-01 | Usuarios | L | Administrador Sistema | Busqueda, rol, estado, ultimo acceso. | ADM-02. |
| ADM-02 | Usuario | F/D | Administrador Sistema | Identidad, rol, estado y acciones de seguridad. | Usuario guardado o actualizado. |
| ADM-03 | Roles | L | Administrador Sistema | Roles predefinidos y alcance por modulo. | ADM-04. |
| ADM-04 | Detalle de rol | D | Administrador Sistema | Descripcion y alcance de acceso basado en rol. | Regreso a ADM-03. |
| ADM-05 | Configuracion tributaria y previsional | L/C | Administrador Sistema | Versiones IGV/AFP/ONP activas, futuras e historicas. | ADM-06. |
| ADM-06 | Version tributaria o previsional | F/D | Administrador Sistema | Tipo, codigo/regimen, version, tasa, vigencia y estado; descripcion only for IGV. | Nueva version. |
| ADM-07 | Formatos SUNAT | L/C | Administrador Sistema | Versiones por tipo de libro y vigencia. | ADM-08. |
| ADM-08 | Version de formato SUNAT | F/D | Administrador Sistema | Tipo de libro, version, estructura, vigencia y estado. | Nueva version. |
| ADM-09 | Registro de auditoria | L/D | Administrador Sistema | Fecha, usuario, modulo, accion, entidad y resultado. | Detalle de evento. |
| ADM-10 | Estado de respaldos | L/D | Administrador Sistema | IMPLEMENTATION PENDING: no canonical persistence/API. | No active output. |

## Empleados

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| EMP-01 | Empleados | L | Administrador RRHH; Administrador Sistema | Busqueda, filtros, grilla y totales. | EMP-02 o EMP-03. |
| EMP-02 | Nuevo/Editar empleado | F | Administrador RRHH; Administrador Sistema | Identidad, empleo, remuneracion, pension y banco. | EMP-03 o EMP-01. |
| EMP-03 | Detalle de empleado | D | Administrador RRHH; Administrador Sistema | Resumen, estado and overtime relations; payroll-history panel is implementation pending. | Edicion, horas extra o regreso. |
| EMP-04 | Departamentos | L | Administrador RRHH; Administrador Sistema | Nombre, empleados activos y estado. | EMP-05. |
| EMP-05 | Departamento | F/D | Administrador RRHH; Administrador Sistema | Nombre, descripcion y estado. | EMP-04. |
| EMP-06 | Horas extra | L | Administrador RRHH; Administrador Sistema | Periodo, empleado, departamento y horas aprobadas. | EMP-07. |
| EMP-07 | Registro masivo de horas extra | F | Administrador RRHH; Administrador Sistema | IMPLEMENTATION PENDING: bulk/partial-result contract absent. | Use single-row registration from EMP-06. |

## Planillas

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| PAY-01 | Periodos de planilla | L | Administrador RRHH; Administrador Sistema | Periodo, estado, empleados y totales. | PAY-02. |
| PAY-02 | Calculo y revision de planilla | P | Administrador RRHH; Administrador Sistema | Preparacion, calculo, resultados y totales. | Recalculo, finalizacion o exportacion. |
| PAY-03 | Detalle de resultado por empleado | D | Administrador RRHH; Administrador Sistema | Haberes brutos, descuentos del empleado, neto a pagar, provisiones CTS/gratificacion y costo total. | PAY-02 o boleta. |
| PAY-04 | Boletas de pago | L/D | Administrador RRHH; Administrador Sistema | IMPLEMENTATION PENDING as a persistent center. | Direct PDF/ZIP export remains in PAY-02. |
| PAY-05 | Historial de exportaciones de planilla | L/D | Administrador RRHH; Administrador Sistema | IMPLEMENTATION PENDING: no snapshot/history table or list API. | Direct export only. |

## Contabilidad

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| ACC-01 | Comprobantes | L | Contador; Administrador Sistema | Busqueda, filtros, grilla y totales. | ACC-02 o ACC-03. |
| ACC-02 | Nuevo/Editar comprobante | F | Contador; Administrador Sistema | Movimiento, documento, referencia de nota, tercero, operacion e importes. | ACC-03, ACC-01 o nuevo comprobante. |
| ACC-03 | Detalle de comprobante | D | Contador; Administrador Sistema | Identidad, calculo tributario and estado; inline audit history is implementation pending. | Edicion, anulacion o regreso. |
| ACC-04 | Plan de cuentas | L | Contador; Gerente Financiero en consulta; Administrador Sistema | Codigo, cuenta, tipo, nivel y estado. | ACC-05. |
| ACC-05 | Cuenta contable | F/D | Contador; Gerente Financiero en consulta; Administrador Sistema | Codigo, nombre, tipo, naturaleza, cuenta padre y estado. | ACC-04. |
| ACC-06 | Periodos contables | L/C | Contador; Gerente Financiero en consulta; Administrador Sistema | Periodo, vigencia, estado y alertas. | Apertura, cierre o detalle. |
| ACC-07 | Asientos contables | L | Contador; Gerente Financiero en consulta; Administrador Sistema | Periodo, numero, fecha, descripcion, debe, haber y estado. | ACC-08 o ACC-09. |
| ACC-08 | Nuevo/Editar asiento | F | Contador; Administrador Sistema | Cabecera y lineas contables balanceadas. | ACC-09 o ACC-07. |
| ACC-09 | Detalle de asiento | D | Contador; Gerente Financiero en consulta; Administrador Sistema | Lineas, balance, origen and estado; inline audit history is implementation pending. | Contabilizacion o regreso. |

## SUNAT

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| SUN-01 | Centro de libros SUNAT | P/L | Contador; Administrador Sistema | Estado por periodo, libro y version. | SUN-02 o SUN-03. |
| SUN-02 | Generar y revisar libro | P | Contador; Administrador Sistema | Periodo, tipo, elegibles, validaciones, vista previa y totales. | Nueva version o exportacion. |
| SUN-03 | Versiones de libros | L | Contador; Administrador Sistema | Periodo, libro, version, fecha, usuario y estado. | SUN-04. |
| SUN-04 | Detalle de version | D | Contador; Administrador Sistema | Metadatos, comprobantes incluidos, totales y validaciones. | SUN-05 o regreso. |
| SUN-05 | Exportaciones SUNAT | L/D | Contador; Administrador Sistema | IMPLEMENTATION PENDING as persisted history. | Direct version export remains in SUN-04. |

## Reportes

| ID | Pantalla | Tipo | Usuarios | Contenido principal | Salida principal |
|---|---|---|---|---|---|
| REP-01 | Panel financiero | P | Gerente Financiero; Administrador Sistema | KPIs del periodo y accesos a reportes. | REP-02, REP-03 o REP-04. |
| REP-02 | Balance general | P | Gerente Financiero; Administrador Sistema | Activo, pasivo, patrimonio y totales del periodo. | Detalle o exportacion. |
| REP-03 | Estado de resultados | P | Gerente Financiero; Administrador Sistema | Ingresos, costos, gastos y utilidad del rango. | Detalle o exportacion. |
| REP-04 | Planilla consolidada | P | Gerente Financiero; Administrador Sistema | Periodo, departamento, haberes, descuentos y neto. | Detalle o exportacion. |
| REP-05 | Centro de exportaciones | L/D | Gerente Financiero; Administrador Sistema | Reporte, filtros, formato, usuario, fecha y resultado. | Apertura o reintento. |

## Criterios de Cobertura

- Toda pantalla tiene identificador, rol, entrada, salida y patron de contenido.
- Toda pantalla con datos operativos referencia una grilla o formulario canonico.
- Toda accion sensible tiene estado previo, confirmacion proporcional y resultado auditable.
- Ninguna pantalla revela modulos, registros o acciones fuera del rol activo.
- Los wireframes usan estos identificadores y no crean campos o estados alternativos.
