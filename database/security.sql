BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'rol_app_read') THEN
        CREATE ROLE rol_app_read NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'rol_app_write') THEN
        CREATE ROLE rol_app_write NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'rol_app_admin') THEN
        CREATE ROLE rol_app_admin NOLOGIN;
    END IF;
END;
$$;

REVOKE ALL ON SCHEMA "identity", payroll, accounting, admin, audit, reporting FROM PUBLIC;
REVOKE ALL ON ALL TABLES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting FROM PUBLIC;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting FROM PUBLIC;
REVOKE ALL ON ALL ROUTINES IN SCHEMA payroll, accounting, admin, audit FROM PUBLIC;

GRANT USAGE ON SCHEMA payroll, accounting, reporting TO rol_app_read;
GRANT SELECT ON ALL TABLES IN SCHEMA payroll, accounting, reporting TO rol_app_read;

GRANT USAGE ON SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_write;
REVOKE ALL ON ALL TABLES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting FROM rol_app_write;
GRANT SELECT ON ALL TABLES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_write;

GRANT INSERT (username, password_hash, nombre_completo, id_rol)
    ON "identity".usuario TO rol_app_write;
GRANT UPDATE ON "identity".usuario TO rol_app_write;
GRANT INSERT ON "identity".login_attempt, "identity".token_revocation TO rol_app_write;

GRANT INSERT (nombre, descripcion) ON payroll.departamento TO rol_app_write;
GRANT UPDATE ON payroll.departamento TO rol_app_write;
GRANT INSERT (
    id_departamento, id_tipo_descuento, dni, nombres, apellidos, cargo,
    salario_base, fecha_nacimiento, fecha_ingreso, banco, numero_cuenta
) ON payroll.empleado TO rol_app_write;
GRANT UPDATE ON payroll.empleado TO rol_app_write;
GRANT INSERT (id_empleado, periodo, horas_primeras_dos, horas_posteriores)
    ON payroll.horas_extra TO rol_app_write;
GRANT INSERT (id_tipo, version, porcentaje, fecha_inicio, fecha_fin)
    ON payroll.config_descuento_previsional_version TO rol_app_write;

GRANT INSERT (codigo, descripcion, version, tasa_igv, fecha_inicio, fecha_fin)
    ON admin.config_tributaria_version TO rol_app_write;
GRANT INSERT (tipo_libro, version_formato, estructura_json, fecha_inicio, fecha_fin)
    ON admin.config_sunat_formato TO rol_app_write;

GRANT INSERT (
    id_tipo_comp, id_config_tributaria_version, id_comprobante_referencia,
    tipo_movimiento, tipo_documento, numero_documento, razon_social,
    serie, numero, fecha_emision, base_imponible, base_exonerada,
    igv, total, tipo_operacion
) ON accounting.comprobante TO rol_app_write;
GRANT INSERT (codigo, fecha_inicio, fecha_fin)
    ON accounting.periodo_contable TO rol_app_write;
GRANT INSERT (codigo, nombre, tipo, naturaleza, id_cuenta_padre)
    ON accounting.cuenta_contable TO rol_app_write;
GRANT INSERT (id_periodo_contable, fecha_asiento, descripcion)
    ON accounting.asiento_contable TO rol_app_write;
GRANT INSERT ON accounting.detalle_asiento TO rol_app_write;
GRANT UPDATE (
    id_tipo_comp, id_config_tributaria_version, id_comprobante_referencia,
    tipo_movimiento, tipo_documento, numero_documento, razon_social,
    serie, numero, fecha_emision, base_imponible, base_exonerada,
    igv, total, tipo_operacion
) ON accounting.comprobante TO rol_app_write;
GRANT UPDATE (nombre, tipo, naturaleza, id_cuenta_padre, activo)
    ON accounting.cuenta_contable TO rol_app_write;
GRANT UPDATE (id_periodo_contable, fecha_asiento, descripcion)
    ON accounting.asiento_contable TO rol_app_write;
GRANT UPDATE, DELETE ON accounting.detalle_asiento TO rol_app_write;

GRANT INSERT ON reporting.reporte_snapshot, reporting.reporte_linea TO rol_app_write;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_write;
GRANT EXECUTE ON ALL ROUTINES IN SCHEMA payroll, accounting, admin, audit TO rol_app_write;

GRANT USAGE, CREATE ON SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting TO rol_app_admin;
GRANT ALL PRIVILEGES ON ALL ROUTINES IN SCHEMA payroll, accounting, admin, audit TO rol_app_admin;

ALTER DEFAULT PRIVILEGES IN SCHEMA payroll, accounting, reporting
    GRANT SELECT ON TABLES TO rol_app_read;
ALTER DEFAULT PRIVILEGES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting
    GRANT SELECT ON TABLES TO rol_app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA "identity", payroll, accounting, admin, audit, reporting
    GRANT USAGE, SELECT ON SEQUENCES TO rol_app_write;
ALTER DEFAULT PRIVILEGES IN SCHEMA payroll, accounting, admin, audit
    REVOKE EXECUTE ON ROUTINES FROM PUBLIC;
ALTER DEFAULT PRIVILEGES IN SCHEMA payroll, accounting, admin, audit
    GRANT EXECUTE ON ROUTINES TO rol_app_write;

COMMIT;
