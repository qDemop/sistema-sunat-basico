# Database Function Contracts

`database/functions.sql` is the executable PostgreSQL 16 form of these contracts. This document lists every function in that file; it does not define additional routines.

## Validation and Calculation

| Function | Volatility / return | Contract |
|---|---|---|
| `admin.fn_periodo_valido(p_periodo)` | Immutable boolean | Accept only `YYYY-MM` with months 01-12. |
| `payroll.fn_redondear_monto(p_monto)` | Immutable numeric(12,2) | Treat null as zero and round to two decimals. |
| `payroll.fn_calcular_porcentaje(p_base, p_porcentaje)` | Immutable numeric(12,2) | Calculate and round `base * porcentaje / 100`. |
| `payroll.fn_calcular_provision_gratificacion(p_total_bruto)` | Immutable numeric(12,2) | Calculate the monthly `total_bruto / 6` provision. |
| `payroll.fn_calcular_provision_cts(p_total_bruto, p_provision_gratificacion)` | Immutable numeric(12,2) | Calculate monthly `(total_bruto + provision_gratificacion) / 12`. |
| `payroll.fn_calcular_horas_extra(p_salario_base, p_horas_primeras_dos, p_horas_posteriores)` | Immutable numeric(12,2) | Use the canonical 240-hour divisor and 1.25/1.35 factors. |
| `accounting.fn_calcular_igv(p_base_imponible, p_tipo_operacion, p_tasa_igv)` | Immutable numeric(12,2) | Apply IGV only to Gravada and round to two decimals. |
| `accounting.fn_calcular_total(p_base_imponible, p_base_exonerada, p_igv)` | Immutable numeric(12,2) | Return the rounded sum of both bases and IGV. |
| `accounting.fn_validar_libro(p_tipo_libro, p_periodo)` | Stable table result | Return `Valida`, `ConObservaciones`, or `Bloqueada` with eligible count and JSON observations without mutation. |

## Effective Configuration Resolution

| Function | Volatility / return | Contract |
|---|---|---|
| `payroll.fn_config_descuento_version_activa(p_id_tipo, p_fecha)` | Stable bigint | Resolve the one Active pension version effective on the date. |
| `admin.fn_config_tributaria_version_activa(p_codigo, p_fecha)` | Stable bigint | Resolve the one Active tax version effective on the date. |
| `admin.fn_config_sunat_formato_activo(p_tipo_libro, p_fecha)` | Stable bigint | Resolve the one Active book format effective on the date. |
| `accounting.fn_tasa_igv_activa(p_fecha)` | Stable numeric(5,2) | Resolve the effective `IGV` rate through the active tax version. |

## Audit and Integrity Triggers

| Function | Trigger/use | Contract |
|---|---|---|
| `audit.fn_registrar_evento(p_id_usuario, p_rol_actor, p_modulo, p_accion, p_entidad, p_id_entidad, p_resultado, p_correlation_id, p_datos_json)` | SECURITY DEFINER audit insert, returns audit ID | Persist actor, role, module, action, entity, result, safe JSON and correlation without granting direct audit-table INSERT. |
| `accounting.fn_bloquear_comprobante_incluido()` | `comprobante` before update | Once linked to a generated book, reject business-field changes while allowing state-only annulment. |
| `admin.fn_proteger_version_config()` | Three configuration tables before update/delete | Reject delete, protect Closed, and allow Active only to transition field-preservingly to Closed. |
| `accounting.fn_validar_nota_referencia()` | `comprobante` before insert/reference update | Codes 07/08 require an existing Registrado original of the same movement; other types prohibit a reference. |
| `payroll.fn_validar_empleado_catalogos()` | `empleado` before catalog assignment | Require an Active department and Active AFP/ONP type. |
| `payroll.fn_validar_horas_extra_registro()` | `horas_extra` before employee/period assignment | Require an Active employee and reject registration after any payroll aggregate exists for the period. |
| `accounting.fn_proteger_asiento()` | `asiento_contable` before insert/update/delete | Require an Open containing period, reject physical delete, protect terminal entries and limit lifecycle field changes. |
| `accounting.fn_proteger_detalle_asiento()` | `detalle_asiento` before DML | Permit line mutation only while the parent entry is Draft and require an Active account for insert/update. |
| `accounting.fn_proteger_cuenta_contable()` | `cuenta_contable` before DML | Reject physical delete and hierarchy cycles/inactive parents; after Posted use protect code, type, nature and parent while allowing name/status changes. |
| `payroll.fn_proteger_periodo_planilla()` | `periodo_planilla` before update/delete | Protect Finalized/Cancelled aggregates and reject invalid states. |
| `audit.fn_bloquear_registro_inmutable()` | Books, bridge, report snapshots/lines and audit log | Reject every update or delete on immutable records. |

## Exact Trigger Inventory

| Trigger | Function |
|---|---|
| `trg_bloquear_comprobante_incluido` | `accounting.fn_bloquear_comprobante_incluido()` |
| `trg_inmutable_audit_log` | `audit.fn_bloquear_registro_inmutable()` |
| `trg_inmutable_comprobante_libro` | `audit.fn_bloquear_registro_inmutable()` |
| `trg_inmutable_libro` | `audit.fn_bloquear_registro_inmutable()` |
| `trg_inmutable_reporte_linea` | `audit.fn_bloquear_registro_inmutable()` |
| `trg_inmutable_reporte_snapshot` | `audit.fn_bloquear_registro_inmutable()` |
| `trg_proteger_asiento` | `accounting.fn_proteger_asiento()` |
| `trg_proteger_config_previsional` | `admin.fn_proteger_version_config()` |
| `trg_proteger_config_sunat` | `admin.fn_proteger_version_config()` |
| `trg_proteger_config_tributaria` | `admin.fn_proteger_version_config()` |
| `trg_proteger_cuenta_contable` | `accounting.fn_proteger_cuenta_contable()` |
| `trg_proteger_detalle_asiento` | `accounting.fn_proteger_detalle_asiento()` |
| `trg_proteger_periodo_planilla` | `payroll.fn_proteger_periodo_planilla()` |
| `trg_validar_empleado_catalogos` | `payroll.fn_validar_empleado_catalogos()` |
| `trg_validar_horas_extra_registro` | `payroll.fn_validar_horas_extra_registro()` |
| `trg_validar_nota_referencia` | `accounting.fn_validar_nota_referencia()` |

## Consistency Rules

- Monetary calculation functions return two-decimal values consistent with SQL numeric scales and OpenAPI money fields.
- Effective resolvers return null when no Active version exists; calling procedures convert that absence into a blocking business error.
- Trigger functions are invoked by the exact triggers declared in `database/functions.sql` and do not create additional lifecycle states.
