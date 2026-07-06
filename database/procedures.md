# Stored Procedure Contracts

All actor IDs come from the validated JWT. Every procedure is atomic and writes an audit event with actor role and correlation ID.

## Payroll Lifecycle

### sp_aprobar_horas_extra(id_horas_extra, actor_user_id, correlation_id)

- Roles: Administrador RRHH, Administrador Sistema.
- Transition: Draft -> Approved.
- Sets approval timestamp and approver; any other source state returns conflict.

### sp_cancelar_horas_extra(id_horas_extra, actor_user_id, correlation_id)

- Roles: Administrador RRHH, Administrador Sistema.
- Transition: Draft/Approved -> Cancelled.
- Rejects cancellation once any payroll-period aggregate exists for that period, including terminal history.

### sp_calcular_planilla(periodo, actor_user_id, correlation_id)

1. Validate `YYYY-MM`, actor, active employees, and Draft lifecycle.
2. Create `PERIODO_PLANILLA` in Draft if absent; reject Finalized/Cancelled.
3. Replace Draft employee results atomically.
4. Load Approved overtime only.
5. Resolve one Active pension-rate version at period end; reject missing/overlap.
6. Calculate cash gross, effective pension deduction, net pay, gratification provision, and CTS provision.
7. Persist employee/department assignment, applied base salary, and applied pension version snapshots.
8. Update aggregate totals and audit.

`descuentos_adicionales` is zero in current scope. Legal gratification payment and CTS deposit execution are out of scope.

### sp_finalizar_planilla(periodo, actor_user_id, correlation_id)

1. Lock one Draft `PERIODO_PLANILLA` by period.
2. Require the matching Open accounting period and all initial mapping accounts.
3. Create one balanced Draft journal entry with origin Planilla and source period ID.
4. Transition Draft -> Finalized and store finalizer/time.
5. Audit period, totals, and generated Draft entry ID.

### sp_cancelar_planilla(periodo, actor_user_id, correlation_id)

Transitions Draft -> Cancelled. Finalized/Cancelled are terminal and cannot be reopened.

## Versioned Configuration Lifecycle

| Procedure | Transition | Guard |
|---|---|---|
| `admin.sp_activar_config_tributaria` | Tax Draft -> Active | Administrador Sistema; nonoverlap constraint; immutable fields. |
| `admin.sp_cerrar_config_tributaria` | Tax Active -> Closed | Administrador Sistema; no field changes. |
| `admin.sp_activar_config_previsional` | Pension Draft -> Active | Administrador Sistema; nonoverlap constraint; immutable fields. |
| `admin.sp_cerrar_config_previsional` | Pension Active -> Closed | Administrador Sistema; no field changes. |
| `admin.sp_activar_formato_sunat` | Format Draft -> Active | Administrador Sistema; governed JSON and nonoverlap. |
| `admin.sp_cerrar_formato_sunat` | Format Active -> Closed | Administrador Sistema; no field changes. |

All six procedures record actor, role and correlation. Application-role direct UPDATE is not granted.

## Voucher and SUNAT Lifecycle

### sp_sincronizar_asiento_comprobante(id_comprobante, actor_user_id, correlation_id)

- Creates one source-linked Draft entry for a Registrado voucher using the canonical Compra/Venta mapping.
- Code 07 creates the documented inverse reduction mapping; code 08 uses the ordinary increase mapping. Both retain the original entry unchanged.
- Repeated calls update the same Draft atomically.
- A Posted or Cancelled source entry cannot be synchronized.

### sp_anular_comprobante(id_comprobante, periodo_reversion, actor_user_id, correlation_id)

- Transitions Registrado -> Anulado without changing historical book snapshots.
- Cancels an existing source Draft.
- If the source is Posted, requires an Open destination period and creates a linked reversal Draft.

### sp_generar_libro(tipo_libro, periodo, aceptar_observaciones, actor_user_id, correlation_id)

1. Run `fn_validar_libro` and reject Bloqueada.
2. Require explicit acknowledgement for ConObservaciones.
3. Resolve the Active SUNAT format at period end.
4. Serialize generation per type/period, capture the eligible voucher IDs once, lock those rows, reject a state/date change, rerun validation, and lock the Active format.
5. Allocate the next version and insert the immutable header and totals by joining only the captured ID set.
6. Insert complete bridge row snapshots and applied tax versions by joining the same captured ID set; vouchers committed after capture belong only to a later version.
7. Audit validation result, type, period, version, and totals.

Retrieval is never part of this procedure and cannot generate a version.

## General Ledger Lifecycle

### sp_postear_asiento(id_asiento, actor_user_id, correlation_id)

- Roles: Contador, Administrador Sistema.
- Requires Draft, Open period, date inside period, active accounts, positive balanced totals.
- Transition: Draft -> Posted; records actor/time and audit.

### sp_cancelar_asiento(id_asiento, actor_user_id, correlation_id)

Transitions Draft -> Cancelled. Posted entries cannot be cancelled.

### sp_revertir_asiento(id_asiento, periodo_destino, motivo, actor_user_id, correlation_id, id_nuevo INOUT)

- Requires a Posted original and Open destination period.
- Creates one linked Draft adjustment with debit/credit reversed.
- Original remains Posted. A second active reversal is rejected.

### sp_cerrar_periodo(id_periodo, actor_user_id, correlation_id)

- Requires Open state and zero Draft entries.
- Transition: Open -> Closed; stores actor/time.
- Reopening is out of scope.

## Supporting Functions

| Function | Contract |
|---|---|
| `fn_config_descuento_version_activa` | Resolve the one Active pension version by type/date. |
| `fn_config_tributaria_version_activa` | Resolve the one Active tax version by code/date. |
| `fn_config_sunat_formato_activo` | Resolve the one Active format by book type/date. |
| `fn_validar_libro` | Return Valida, ConObservaciones, or Bloqueada with eligible count and observations. |
| `fn_registrar_evento` | Persist actor, role, action, entity, result, correlation ID, and event data. |
| `fn_bloquear_comprobante_incluido` | Reject business-field updates after any generated-book linkage while allowing annulment. |

All lifecycle procedures are `SECURITY DEFINER`, use schema-qualified objects and validate application actor roles. This permits revoking direct state-table updates from `rol_app_write`.

## Exact Procedure Inventory

| Schema | Procedures and exact parameter order |
|---|---|
| `payroll` | `payroll.sp_aprobar_horas_extra(p_id_horas_extra, p_id_usuario, p_correlation_id)`; `payroll.sp_cancelar_horas_extra(p_id_horas_extra, p_id_usuario, p_correlation_id)`; `payroll.sp_calcular_planilla(p_periodo, p_id_usuario, p_correlation_id)`; `payroll.sp_finalizar_planilla(p_periodo, p_id_usuario, p_correlation_id)`; `payroll.sp_cancelar_planilla(p_periodo, p_id_usuario, p_correlation_id)`. |
| `admin` | `sp_activar_config_tributaria(p_id, p_id_usuario, p_correlation_id)`; `sp_cerrar_config_tributaria(p_id, p_id_usuario, p_correlation_id)`; `sp_activar_config_previsional(p_id, p_id_usuario, p_correlation_id)`; `sp_cerrar_config_previsional(p_id, p_id_usuario, p_correlation_id)`; `sp_activar_formato_sunat(p_id, p_id_usuario, p_correlation_id)`; `sp_cerrar_formato_sunat(p_id, p_id_usuario, p_correlation_id)`. |
| `accounting` | `accounting.sp_generar_libro(p_tipo_libro, p_periodo, p_aceptar_observaciones, p_id_usuario, p_correlation_id)`; `accounting.sp_sincronizar_asiento_comprobante(p_id_comprobante, p_id_usuario, p_correlation_id)`; `accounting.sp_postear_asiento(p_id_asiento_contable, p_id_usuario, p_correlation_id)`; `accounting.sp_cancelar_asiento(p_id_asiento_contable, p_id_usuario, p_correlation_id)`; `accounting.sp_revertir_asiento(p_id_asiento_contable, p_periodo_destino, p_motivo, p_id_usuario, p_correlation_id, p_id_nuevo INOUT)`; `accounting.sp_cerrar_periodo(p_id_periodo_contable, p_id_usuario, p_correlation_id)`; `accounting.sp_anular_comprobante(p_id_comprobante, p_periodo_reversion, p_id_usuario, p_correlation_id)`. |
