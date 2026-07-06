# Database Schema Specification

Target: PostgreSQL 16. `database/schema.sql` is the executable form of this specification.

Notation below: `!` means NOT NULL; `?` means nullable; `PK`, `UQ` and `FK` have their usual meanings. Numeric precision and string lengths are canonical.

## Canonical Schemas

| Schema | Tables |
|---|---|
| identity | ROL, USUARIO, LOGIN_ATTEMPT, TOKEN_REVOCATION |
| payroll | DEPARTAMENTO, TIPO_DESCUENTO, CONFIG_DESCUENTO_PREVISIONAL_VERSION, EMPLEADO, HORAS_EXTRA, PERIODO_PLANILLA, PLANILLA, DETALLE_PLANILLA |
| admin | CONFIG_TRIBUTARIA_VERSION, CONFIG_SUNAT_FORMATO |
| accounting | TIPO_COMPROBANTE, COMPROBANTE, TIPO_LIBRO, LIBRO_CONTABLE, COMPROBANTE_LIBRO, PERIODO_CONTABLE, CUENTA_CONTABLE, ASIENTO_CONTABLE, DETALLE_ASIENTO |
| reporting | REPORTE_SNAPSHOT, REPORTE_LINEA |
| audit | AUDIT_LOG |

## Identity and Security

### ROL

`id_rol` PK; `nombre` unique and limited to the four predefined roles; `descripcion`; `nivel_acceso`; `activo`.

### USUARIO

`id_usuario` PK; case-insensitively unique alphanumeric `username`; `password_hash`; `nombre_completo`; required `id_rol`; `ultimo_acceso`; `activo`; `intentos_fallidos`; `bloqueado_hasta`; `fecha_creacion`.

### LOGIN_ATTEMPT

`id_login_attempt` PK; optional resolved `id_usuario`; submitted `username`; `exitoso`; optional `ip_origen`; `fecha_intento`; required `correlation_id`.

### TOKEN_REVOCATION

JWT `jti` PK; required user; revocation/expiration timestamps; reason; correlation ID. Rows may be purged only after expiration.

## Payroll

### DEPARTAMENTO

`id_departamento` PK; unique `nombre`; `descripcion`; `activo`. Employees use a required FK with delete Restrict.

### TIPO_DESCUENTO

`id_tipo` PK; unique `nombre` (`AFP` or `ONP`); description; active flag. It owns regime identity, not percentage.

### CONFIG_DESCUENTO_PREVISIONAL_VERSION

`id_config_descuento_version` PK; required pension type; positive version; percentage 0-100; effective dates; state Draft/Active/Closed. Active ranges for one type cannot overlap.

### EMPLEADO

Required department and pension type; unique 8-digit DNI; names; job; positive base salary; birth/hire dates; bank/account; active flag; creation timestamp.

### HORAS_EXTRA

One row per employee/period. Values are nonnegative with a positive sum. State is Draft, Approved, or Cancelled. Approved rows require approval timestamp and actor.

### PERIODO_PLANILLA

One row per `YYYY-MM`; aggregate state Draft/Finalized/Cancelled; period totals for cash gross, discounts, net, gratification provision, and CTS provision; calculation/finalization timestamps and finalizer. `total_neto = total_bruto - total_descuentos`.

### PLANILLA

One employee result per `PERIODO_PLANILLA`; required employee, department assignment snapshot and `salario_base_aplicado`; cash gross, discounts, and net. Unique `(id_periodo_planilla, id_empleado)`. The applied salary is immutable with a Finalized period and does not follow later employee-master changes.

### DETALLE_PLANILLA

Exactly one row per PLANILLA; required applied pension configuration; overtime amount; AFP/ONP; CTS/gratification provisions; additional discounts. Additional discounts are fixed at zero in current scope.

## Administration Configuration

### CONFIG_TRIBUTARIA_VERSION

Versioned IGV rule with code fixed to `IGV`, percentage 0-100, effective dates, and Draft/Active/Closed state. Active ranges cannot overlap.

### CONFIG_SUNAT_FORMATO

Book type Compras/Ventas; unique format version; governed JSON object whose only property is the eleven-token `columns` array; effective dates; Draft/Active/Closed state. Active ranges cannot overlap. Active/Closed rows are trigger-protected and application roles cannot update them directly.

### EMPRESA

Own-company (emitter) identity required for SUNAT voucher and book emission. RUC is 11 digits and unique; razón social and domicilio fiscal are required; régimen is one of `Regimen MYPE Tributo Especial`, `Regimen General`, `Otro`. A partial unique index enforces at most one active row. Added in Sprint 0 remediation to close the SUNAT-emit gap; before this, `COMPROBANTE` only stored the counterparty.

## Accounting SUNAT

### TIPO_COMPROBANTE

Unique name and SUNAT code; IGV applicability; active flag.

### COMPROBANTE

Required voucher type, applied tax version, optional original-voucher self-reference, Compra/Venta movement, party identity, series/number/date, taxable/exempt bases, IGV, total, operation type, and Registrado/Anulado state. Codes 07/08 require a same-movement Registrado reference; other types prohibit it. Bases are non-negative with at least one positive base. Identity is unique by type, movement, series, and number. Linked voucher business fields are immutable; annulment remains allowed.

### TIPO_LIBRO

Compras or Ventas and active flag.

### LIBRO_CONTABLE

Required book type, applied SUNAT format, generator, period, sequential version, timestamp, immutable Generado state, and totals. Unique by type/period/version.

### COMPROBANTE_LIBRO

Immutable bridge with book, voucher, tax-version snapshot, row order, period, party document, voucher code, series/number/date, taxable/exempt bases, IGV and total. PK `(id_libro, id_comprobante)`.

## General Ledger

### PERIODO_CONTABLE

Unique `YYYY-MM`, start/end dates, Open/Closed state, closing timestamp and actor. Closing metadata is present if and only if state is Closed; Closed is terminal.

### CUENTA_CONTABLE

Unique code; name; Asset/Liability/Equity/Income/Expense/Cost type; Debit/Credit nature; optional parent; active flag.

### ASIENTO_CONTABLE

Required period and date; description; origin Manual/Comprobante/Planilla/Ajuste; source ID; Draft/Posted/Cancelled state; posting actor/time; optional self-reference to the Posted entry being reversed. Posting metadata is present if and only if state is Posted. Non-adjustment source identity is unique.

### DETALLE_ASIENTO

Required entry/account; debit or credit amount, exactly one positive; optional description. Posting additionally validates at least two lines, active accounts, date inside period, and equal totals.

Posted/Cancelled entries and their lines are trigger-immutable. One partial unique index permits only one active reversal per original entry.

Account hierarchy is trigger-validated: the parent must be Active, self/descendant cycles are rejected, physical deletion is rejected, and Posted use protects code/type/nature/parent while allowing name and logical active-state changes.

## Reporting

### REPORTE_SNAPSHOT

Immutable export header: report type, PDF/Excel format, period range, optional department, actor/role, filters/totals JSON, source cutoff, generation time, Generated/Failed state, file reference, and correlation ID. Generated rows require a file reference.

### REPORTE_LINEA

Immutable ordered line for one snapshot with code, label, amount, and structured data. Unique order per snapshot.

## Audit

### AUDIT_LOG

Optional actor user for unauthenticated failures; actor-role snapshot when known; module; action; entity/ID; Success/Failure/Blocked result; event JSON; timestamp; correlation ID. SQL requires a role whenever an actor user is present; sensitive authenticated mutations require both at the procedure/application boundary.

## Canonical Relationships

| Relationship | Cardinality |
|---|---|
| ROL -> USUARIO | 1:N |
| USUARIO -> LOGIN_ATTEMPT | 1:0..N; attempt user is optional |
| USUARIO -> TOKEN_REVOCATION | 1:N |
| TIPO_DESCUENTO -> EMPLEADO | 1:N |
| TIPO_DESCUENTO -> CONFIG_DESCUENTO_PREVISIONAL_VERSION | 1:N |
| DEPARTAMENTO -> EMPLEADO | 1:N required |
| EMPLEADO -> HORAS_EXTRA | 1:N |
| PERIODO_PLANILLA -> PLANILLA | 1:N |
| EMPLEADO -> PLANILLA | 1:N |
| DEPARTAMENTO -> PLANILLA | 1:N assignment snapshots |
| PLANILLA -> DETALLE_PLANILLA | 1:1 |
| CONFIG_DESCUENTO_PREVISIONAL_VERSION -> DETALLE_PLANILLA | 1:N |
| CONFIG_TRIBUTARIA_VERSION -> COMPROBANTE | 1:N |
| COMPROBANTE original -> notes | 1:0..N |
| CONFIG_SUNAT_FORMATO -> LIBRO_CONTABLE | 1:N |
| CONFIG_TRIBUTARIA_VERSION -> COMPROBANTE_LIBRO | 1:N snapshots |
| LIBRO_CONTABLE <-> COMPROBANTE | N:M through COMPROBANTE_LIBRO |
| PERIODO_CONTABLE -> ASIENTO_CONTABLE | 1:N |
| ASIENTO_CONTABLE -> DETALLE_ASIENTO | 1:N |
| ASIENTO_CONTABLE -> reversal ASIENTO_CONTABLE | 1:0..1 active reversal |
| USUARIO -> ASIENTO_CONTABLE | 1:0..N; posting actor is optional until Posted |
| REPORTE_SNAPSHOT -> REPORTE_LINEA | 1:N |
| DEPARTAMENTO -> REPORTE_SNAPSHOT | 1:0..N; snapshot filter is optional |
| USUARIO -> AUDIT_LOG | 1:0..N; actor is optional only when unresolved |

## Lifecycle Ownership

- User activity: `USUARIO.activo`; lockout: `intentos_fallidos` and `bloqueado_hasta`.
- Overtime: HORAS_EXTRA Draft/Approved/Cancelled.
- Payroll: PERIODO_PLANILLA Draft/Finalized/Cancelled; PLANILLA has no state.
- Voucher: COMPROBANTE Registrado/Anulado; validation is computed.
- Book: LIBRO_CONTABLE Generado only; supersession is derived from version order.
- Configuration: Draft/Active/Closed.
- Accounting period: Open/Closed, no reopen.
- Journal entry: Draft/Posted/Cancelled; reversal is a linked Draft adjustment.
- Report snapshot: Generated/Failed.

## Exact Column Contract

| Table | Columns |
|---|---|
| `identity.rol` | `id_rol bigint PK`; `nombre varchar(30)! UQ`; `descripcion varchar(200)?`; `nivel_acceso integer! default 1`; `activo boolean! default true`. |
| `identity.usuario` | `id_usuario bigint PK`; `username varchar(50)! UQ`; `password_hash varchar(256)!`; `nombre_completo varchar(200)!`; `id_rol bigint! FK`; `ultimo_acceso timestamptz?`; `activo boolean!`; `intentos_fallidos integer!`; `bloqueado_hasta timestamptz?`; `fecha_creacion timestamptz!`. |
| `identity.login_attempt` | `id_login_attempt bigint PK`; `id_usuario bigint? FK`; `username varchar(50)!`; `exitoso boolean!`; `ip_origen inet?`; `fecha_intento timestamptz!`; `correlation_id varchar(64)!`. |
| `identity.token_revocation` | `jti varchar(128) PK`; `id_usuario bigint! FK`; `revocado_en timestamptz!`; `expira_en timestamptz!`; `motivo varchar(80)!`; `correlation_id varchar(64)!`. |
| `payroll.departamento` | `id_departamento bigint PK`; `nombre varchar(100)! UQ`; `descripcion varchar(250)?`; `activo boolean!`. |
| `payroll.tipo_descuento` | `id_tipo bigint PK`; `nombre varchar(20)! UQ`; `descripcion varchar(200)?`; `activo boolean!`. |
| `payroll.config_descuento_previsional_version` | `id_config_descuento_version bigint PK`; `id_tipo bigint! FK`; `version integer!`; `porcentaje numeric(5,2)!`; `fecha_inicio date!`; `fecha_fin date?`; `estado varchar(20)!`. |
| `payroll.empleado` | `id_empleado bigint PK`; `id_departamento bigint! FK`; `id_tipo_descuento bigint! FK`; `dni varchar(8)! UQ`; `nombres varchar(100)!`; `apellidos varchar(100)!`; `cargo varchar(80)!`; `salario_base numeric(10,2)!`; `fecha_nacimiento date!`; `fecha_ingreso date!`; `banco varchar(80)!`; `numero_cuenta varchar(20)!`; `activo boolean!`; `fecha_creacion timestamptz!`. |
| `payroll.horas_extra` | `id_horas_extra bigint PK`; `id_empleado bigint! FK`; `periodo varchar(10)!`; `horas_primeras_dos numeric(6,2)!`; `horas_posteriores numeric(6,2)!`; `estado varchar(20)!`; `fecha_registro timestamptz!`; `fecha_aprobacion timestamptz?`; `id_usuario_aprobador bigint? FK`. |
| `payroll.periodo_planilla` | `id_periodo_planilla bigint PK`; `periodo varchar(10)! UQ`; `estado varchar(20)!`; `total_bruto`, `total_descuentos`, `total_neto`, `total_provision_gratificacion`, `total_provision_cts numeric(14,2)!`; `fecha_calculo timestamptz!`; `fecha_finalizacion timestamptz?`; `id_usuario_finalizador bigint? FK`. |
| `payroll.planilla` | `id_planilla bigint PK`; `id_periodo_planilla bigint! FK`; `id_empleado bigint! FK`; `id_departamento bigint! FK`; `salario_base_aplicado numeric(10,2)!`; `total_bruto`, `total_descuentos`, `total_neto numeric(10,2)!`. |
| `payroll.detalle_planilla` | `id_detalle bigint PK`; `id_planilla bigint! UQ/FK`; `id_config_descuento_version bigint! FK`; `horas_extra_total`, `afp`, `onp`, `provision_cts`, `provision_gratificacion`, `descuentos_adicionales numeric(10,2)!`. |
| `admin.config_tributaria_version` | `id_config_tributaria_version bigint PK`; `codigo varchar(50)!`; `descripcion varchar(250)?`; `version integer!`; `tasa_igv numeric(5,2)!`; `fecha_inicio date!`; `fecha_fin date?`; `estado varchar(20)!`. |
| `admin.config_sunat_formato` | `id_config_sunat_formato bigint PK`; `tipo_libro varchar(20)!`; `version_formato varchar(20)!`; `estructura_json jsonb!`; `fecha_inicio date!`; `fecha_fin date?`; `estado varchar(20)!`. |
| `accounting.tipo_comprobante` | `id_tipo_comp bigint PK`; `nombre varchar(60)! UQ`; `codigo_sunat varchar(4)! UQ`; `afecto_igv boolean!`; `activo boolean!`. |
| `accounting.comprobante` | `id_comprobante bigint PK`; `id_tipo_comp bigint! FK`; `id_config_tributaria_version bigint! FK`; `id_comprobante_referencia bigint? self-FK`; `tipo_movimiento varchar(10)!`; `tipo_documento varchar(10)!`; `numero_documento varchar(20)!`; `razon_social varchar(200)!`; `serie varchar(10)!`; `numero varchar(20)!`; `fecha_emision date!`; `base_imponible`, `base_exonerada`, `igv`, `total numeric(10,2)!`; `tipo_operacion varchar(20)!`; `estado varchar(20)!`; `fecha_creacion timestamptz!`. |
| `accounting.tipo_libro` | `id_tipo_libro bigint PK`; `nombre varchar(30)! UQ`; `activo boolean!`. |
| `accounting.libro_contable` | `id_libro bigint PK`; `id_tipo_libro bigint! FK`; `id_config_sunat_formato bigint! FK`; `id_usuario_generador bigint! FK`; `periodo varchar(10)!`; `version integer!`; `fecha_generacion timestamptz!`; `estado varchar(20)!`; `total_base_imponible`, `total_igv`, `total_general numeric(12,2)!`. |
| `accounting.comprobante_libro` | `id_libro bigint! FK`; `id_comprobante bigint! FK`; `id_config_tributaria_version bigint! FK`; `orden integer!`; `periodo varchar(10)!`; `tipo_documento varchar(10)!`; `numero_documento varchar(20)!`; `codigo_tipo_comprobante varchar(4)!`; `serie varchar(10)!`; `numero varchar(20)!`; `fecha_emision date!`; `base_imponible`, `base_exonerada`, `total_igv`, `total_general numeric(12,2)!`; composite PK `(id_libro,id_comprobante)`. |
| `accounting.periodo_contable` | `id_periodo_contable bigint PK`; `codigo varchar(10)! UQ`; `fecha_inicio date!`; `fecha_fin date!`; `estado varchar(20)!`; `fecha_cierre timestamptz?`; `id_usuario_cierre bigint? FK`. |
| `accounting.cuenta_contable` | `id_cuenta_contable bigint PK`; `codigo varchar(20)! UQ`; `nombre varchar(150)!`; `tipo varchar(20)!`; `naturaleza varchar(10)!`; `id_cuenta_padre bigint? self-FK`; `activo boolean!`. |
| `accounting.asiento_contable` | `id_asiento_contable bigint PK`; `id_periodo_contable bigint! FK`; `id_asiento_revertido bigint? self-FK`; `fecha_asiento date!`; `descripcion varchar(250)!`; `origen varchar(30)!`; `id_origen bigint?`; `estado varchar(20)!`; `fecha_posteo timestamptz?`; `id_usuario_posteo bigint? FK`. |
| `accounting.detalle_asiento` | `id_detalle_asiento bigint PK`; `id_asiento_contable bigint! FK`; `id_cuenta_contable bigint! FK`; `debe numeric(12,2)!`; `haber numeric(12,2)!`; `descripcion varchar(250)?`. |
| `reporting.reporte_snapshot` | `id_reporte_snapshot bigint PK`; `tipo_reporte varchar(30)!`; `formato varchar(10)!`; `periodo_desde`, `periodo_hasta varchar(10)!`; `id_departamento bigint? FK`; `id_usuario bigint! FK`; `rol_actor varchar(30)!`; `filtros_json`, `totales_json jsonb!`; `fuente_corte`, `fecha_generacion timestamptz!`; `estado varchar(20)!`; `archivo_referencia varchar(500)?`; `correlation_id varchar(64)!`. |
| `reporting.reporte_linea` | `id_reporte_linea bigint PK`; `id_reporte_snapshot bigint! FK`; `orden integer!`; `codigo varchar(50)!`; `etiqueta varchar(200)!`; `monto numeric(14,2)!`; `datos_json jsonb!`. |
| `audit.audit_log` | `id_audit_log bigint PK`; `id_usuario bigint? FK`; `rol_actor varchar(30)?`; `modulo varchar(50)!`; `accion varchar(80)!`; `entidad varchar(80)!`; `id_entidad varchar(50)?`; `resultado varchar(20)!`; `datos_json jsonb!`; `fecha_evento timestamptz!`; `correlation_id varchar(64)!`. |

## Exact CHECK and Lifecycle Contract

- Identity: predefined four-role enum; positive access level; username pattern; nonblank full name; nonnegative failed attempts; token expiration after revocation.
- Payroll: nonblank department/employee required text; AFP/ONP enum; positive versions; rates 0-100; valid/nonoverlapping Active ranges; DNI/account/date checks; valid periods; overtime positive hours and Draft/Approved/Cancelled; payroll Draft/Finalized/Cancelled with exact net equations and Finalized metadata; positive applied salary; additional discounts exactly zero.
- Administration: tax code `IGV`; tax/format Draft/Active/Closed; valid/nonoverlapping Active ranges; format object contains only the exact eleven unique governed tokens.
- Accounting SUNAT: nonblank required identities; Compra/Venta, RUC/DNI/Otro, Gravada/Exonerada/Inafecta, Registrado/Anulado; document patterns; positive source base; exact total equation; no self-reference; Compras/Ventas; book state exactly Generado and sequential positive versions.
- General Ledger: nonblank account/entry identities; valid period dates and Open/Closed with closing metadata iff Closed; account type/nature enums; entry origin/state enums with posting metadata iff Posted; adjustment-reference invariant; exactly one positive debit/credit side per line.
- Reporting/audit: report type/format/state enums, ordered valid periods, nonblank line code/label, Generated requires a nonblank file reference; audit Success/Failure/Blocked and actor role required whenever actor user is present.
