BEGIN;

CREATE UNIQUE INDEX IF NOT EXISTS uq_usuario_username_normalizado ON "identity".usuario(lower(username));
CREATE INDEX IF NOT EXISTS ix_usuario_rol ON "identity".usuario(id_rol);
CREATE INDEX IF NOT EXISTS ix_usuario_bloqueo ON "identity".usuario(bloqueado_hasta);
CREATE INDEX IF NOT EXISTS ix_login_attempt_username_fecha ON "identity".login_attempt(username, fecha_intento DESC);
CREATE INDEX IF NOT EXISTS ix_token_revocation_usuario_expira ON "identity".token_revocation(id_usuario, expira_en);

CREATE INDEX IF NOT EXISTS ix_empleado_departamento_activo ON payroll.empleado(id_departamento, activo);
CREATE INDEX IF NOT EXISTS ix_empleado_activo ON payroll.empleado(activo);
CREATE INDEX IF NOT EXISTS ix_empleado_tipo_descuento ON payroll.empleado(id_tipo_descuento);
CREATE INDEX IF NOT EXISTS ix_horas_extra_periodo_estado ON payroll.horas_extra(periodo, estado);
CREATE INDEX IF NOT EXISTS ix_config_descuento_vigencia ON payroll.config_descuento_previsional_version(id_tipo, fecha_inicio, fecha_fin);
CREATE INDEX IF NOT EXISTS ix_periodo_planilla_estado ON payroll.periodo_planilla(periodo, estado);
CREATE INDEX IF NOT EXISTS ix_planilla_periodo_departamento ON payroll.planilla(id_periodo_planilla, id_departamento);
CREATE INDEX IF NOT EXISTS ix_planilla_empleado ON payroll.planilla(id_empleado);
CREATE INDEX IF NOT EXISTS ix_detalle_planilla_config ON payroll.detalle_planilla(id_config_descuento_version);

CREATE INDEX IF NOT EXISTS ix_config_tributaria_vigencia ON admin.config_tributaria_version(codigo, fecha_inicio, fecha_fin);
CREATE INDEX IF NOT EXISTS ix_config_sunat_vigencia ON admin.config_sunat_formato(tipo_libro, fecha_inicio, fecha_fin);
CREATE UNIQUE INDEX IF NOT EXISTS uq_empresa_activa ON admin.empresa(activo) WHERE activo = TRUE;

CREATE INDEX IF NOT EXISTS ix_comprobante_documento ON accounting.comprobante(tipo_documento, numero_documento);
CREATE INDEX IF NOT EXISTS ix_comprobante_fecha ON accounting.comprobante(fecha_emision);
CREATE INDEX IF NOT EXISTS ix_comprobante_movimiento_fecha ON accounting.comprobante(tipo_movimiento, fecha_emision);
CREATE INDEX IF NOT EXISTS ix_comprobante_tipo_fecha ON accounting.comprobante(id_tipo_comp, fecha_emision);
CREATE INDEX IF NOT EXISTS ix_comprobante_estado_fecha ON accounting.comprobante(estado, fecha_emision);
CREATE INDEX IF NOT EXISTS ix_comprobante_referencia ON accounting.comprobante(id_comprobante_referencia);
CREATE INDEX IF NOT EXISTS ix_libro_periodo_tipo_version ON accounting.libro_contable(periodo, id_tipo_libro, version);
CREATE INDEX IF NOT EXISTS ix_libro_formato ON accounting.libro_contable(id_config_sunat_formato);
CREATE INDEX IF NOT EXISTS ix_comprobante_libro_comprobante ON accounting.comprobante_libro(id_comprobante);

CREATE INDEX IF NOT EXISTS ix_periodo_contable_estado ON accounting.periodo_contable(codigo, estado);
CREATE INDEX IF NOT EXISTS ix_cuenta_contable_tipo_activo ON accounting.cuenta_contable(tipo, activo);
CREATE INDEX IF NOT EXISTS ix_cuenta_contable_padre ON accounting.cuenta_contable(id_cuenta_padre);
CREATE INDEX IF NOT EXISTS ix_asiento_periodo_estado ON accounting.asiento_contable(id_periodo_contable, estado);
CREATE INDEX IF NOT EXISTS ix_asiento_revertido ON accounting.asiento_contable(id_asiento_revertido);
CREATE INDEX IF NOT EXISTS ix_detalle_asiento_cuenta ON accounting.detalle_asiento(id_cuenta_contable);

CREATE INDEX IF NOT EXISTS ix_reporte_snapshot_busqueda ON reporting.reporte_snapshot(tipo_reporte, periodo_desde, periodo_hasta, fecha_generacion DESC);
CREATE INDEX IF NOT EXISTS ix_reporte_snapshot_usuario ON reporting.reporte_snapshot(id_usuario, fecha_generacion DESC);
CREATE INDEX IF NOT EXISTS ix_reporte_linea_snapshot ON reporting.reporte_linea(id_reporte_snapshot, orden);

CREATE INDEX IF NOT EXISTS ix_audit_fecha ON audit.audit_log(fecha_evento DESC);
CREATE INDEX IF NOT EXISTS ix_audit_usuario ON audit.audit_log(id_usuario, fecha_evento DESC);
CREATE INDEX IF NOT EXISTS ix_audit_entidad ON audit.audit_log(entidad, id_entidad);
CREATE INDEX IF NOT EXISTS ix_audit_correlation ON audit.audit_log(correlation_id);

COMMIT;
