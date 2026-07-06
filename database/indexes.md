# Index Contract

`database/indexes.sql` owns secondary lookup indexes; two lifecycle uniqueness indexes remain beside `ASIENTO_CONTABLE` in `schema.sql` because they are integrity constraints.

| Area | Exact index names and keys |
|---|---|
| Identity | `uq_usuario_username_normalizado(lower(username))`; `ix_usuario_rol(id_rol)`; `ix_usuario_bloqueo(bloqueado_hasta)`; `ix_login_attempt_username_fecha(username, fecha_intento DESC)`; `ix_token_revocation_usuario_expira(id_usuario, expira_en)`. |
| Payroll | `ix_empleado_departamento_activo(id_departamento, activo)`; `ix_empleado_activo(activo)`; `ix_empleado_tipo_descuento(id_tipo_descuento)`; `ix_horas_extra_periodo_estado(periodo, estado)`; `ix_config_descuento_vigencia(id_tipo, fecha_inicio, fecha_fin)`; `ix_periodo_planilla_estado(periodo, estado)`; `ix_planilla_periodo_departamento(id_periodo_planilla, id_departamento)`; `ix_planilla_empleado(id_empleado)`; `ix_detalle_planilla_config(id_config_descuento_version)`. |
| Administration | `ix_config_tributaria_vigencia(codigo, fecha_inicio, fecha_fin)`; `ix_config_sunat_vigencia(tipo_libro, fecha_inicio, fecha_fin)`. |
| Accounting SUNAT | `ix_comprobante_documento(tipo_documento, numero_documento)`; `ix_comprobante_fecha(fecha_emision)`; `ix_comprobante_movimiento_fecha(tipo_movimiento, fecha_emision)`; `ix_comprobante_tipo_fecha(id_tipo_comp, fecha_emision)`; `ix_comprobante_estado_fecha(estado, fecha_emision)`; `ix_comprobante_referencia(id_comprobante_referencia)`; `ix_libro_periodo_tipo_version(periodo, id_tipo_libro, version)`; `ix_libro_formato(id_config_sunat_formato)`; `ix_comprobante_libro_comprobante(id_comprobante)`. |
| General Ledger | `ix_periodo_contable_estado(codigo, estado)`; `ix_cuenta_contable_tipo_activo(tipo, activo)`; `ix_cuenta_contable_padre(id_cuenta_padre)`; `ix_asiento_periodo_estado(id_periodo_contable, estado)`; `ix_asiento_revertido(id_asiento_revertido)`; `ix_detalle_asiento_cuenta(id_cuenta_contable)`. Integrity: `uq_asiento_origen(origen,id_origen)` for Comprobante/Planilla and `uq_asiento_reversion_activa(id_asiento_revertido)` for non-Cancelled reversals. |
| Reporting | `ix_reporte_snapshot_busqueda(tipo_reporte, periodo_desde, periodo_hasta, fecha_generacion DESC)`; `ix_reporte_snapshot_usuario(id_usuario, fecha_generacion DESC)`; `ix_reporte_linea_snapshot(id_reporte_snapshot, orden)`. |
| Audit | `ix_audit_fecha(fecha_evento DESC)`; `ix_audit_usuario(id_usuario, fecha_evento DESC)`; `ix_audit_entidad(entidad,id_entidad)`; `ix_audit_correlation(correlation_id)`. |

Primary keys, UNIQUE table constraints and exclusion constraints create their own PostgreSQL indexes and are documented in `schema.md`; they are not duplicated in `indexes.sql`.
