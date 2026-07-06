BEGIN;

CREATE OR REPLACE PROCEDURE payroll.sp_aprobar_horas_extra(
    IN p_id_horas_extra BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_rol VARCHAR(30);
    v_periodo VARCHAR(10);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;

    IF v_rol NOT IN ('Administrador RRHH', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to approve overtime';
    END IF;

    SELECT periodo INTO v_periodo
    FROM payroll.horas_extra
    WHERE id_horas_extra = p_id_horas_extra AND estado = 'Draft'
    FOR UPDATE;

    IF v_periodo IS NULL THEN
        RAISE EXCEPTION 'Overtime record must be Draft';
    END IF;
    IF EXISTS (SELECT 1 FROM payroll.periodo_planilla WHERE periodo = v_periodo) THEN
        RAISE EXCEPTION 'Overtime cannot be approved after payroll calculation exists';
    END IF;

    UPDATE payroll.horas_extra
    SET estado = 'Approved', fecha_aprobacion = now(), id_usuario_aprobador = p_id_usuario
    WHERE id_horas_extra = p_id_horas_extra AND estado = 'Draft';

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'Payroll', 'APROBAR_HORAS_EXTRA', 'HORAS_EXTRA',
        p_id_horas_extra::VARCHAR, 'Success', p_correlation_id
    );
END;
$$;

CREATE OR REPLACE PROCEDURE payroll.sp_cancelar_horas_extra(
    IN p_id_horas_extra BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_periodo VARCHAR(10);
    v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;

    IF v_rol NOT IN ('Administrador RRHH', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to cancel overtime';
    END IF;

    SELECT periodo INTO v_periodo
    FROM payroll.horas_extra
    WHERE id_horas_extra = p_id_horas_extra AND estado IN ('Draft', 'Approved')
    FOR UPDATE;

    IF v_periodo IS NULL THEN
        RAISE EXCEPTION 'Overtime record is not cancellable';
    END IF;

    IF EXISTS (
        SELECT 1 FROM payroll.periodo_planilla
        WHERE periodo = v_periodo
    ) THEN
        RAISE EXCEPTION 'Overtime cannot be cancelled after payroll calculation exists';
    END IF;

    UPDATE payroll.horas_extra SET estado = 'Cancelled'
    WHERE id_horas_extra = p_id_horas_extra;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'Payroll', 'CANCELAR_HORAS_EXTRA', 'HORAS_EXTRA',
        p_id_horas_extra::VARCHAR, 'Success', p_correlation_id
    );
END;
$$;

CREATE OR REPLACE PROCEDURE payroll.sp_calcular_planilla(
    IN p_periodo VARCHAR(10),
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_id_periodo BIGINT;
    v_estado VARCHAR(20);
    v_fecha_fin DATE;
    v_rol VARCHAR(30);
    v_empleado RECORD;
    v_id_config BIGINT;
    v_porcentaje NUMERIC(5,2);
    v_horas_1 NUMERIC(6,2);
    v_horas_2 NUMERIC(6,2);
    v_horas_monto NUMERIC(12,2);
    v_bruto NUMERIC(12,2);
    v_afp NUMERIC(12,2);
    v_onp NUMERIC(12,2);
    v_descuentos NUMERIC(12,2);
    v_neto NUMERIC(12,2);
    v_provision_gratificacion NUMERIC(12,2);
    v_provision_cts NUMERIC(12,2);
    v_id_planilla BIGINT;
BEGIN
    IF NOT admin.fn_periodo_valido(p_periodo) THEN
        RAISE EXCEPTION 'Invalid period: %', p_periodo;
    END IF;

    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;

    IF v_rol NOT IN ('Administrador RRHH', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to calculate payroll';
    END IF;

    INSERT INTO payroll.periodo_planilla(periodo, estado)
    VALUES (p_periodo, 'Draft')
    ON CONFLICT (periodo) DO NOTHING;

    SELECT id_periodo_planilla, estado INTO v_id_periodo, v_estado
    FROM payroll.periodo_planilla
    WHERE periodo = p_periodo
    FOR UPDATE;

    IF v_estado <> 'Draft' THEN
        RAISE EXCEPTION 'Payroll period % is % and cannot be recalculated', p_periodo, v_estado;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM payroll.empleado WHERE activo) THEN
        RAISE EXCEPTION 'No active employees exist for payroll period %', p_periodo;
    END IF;

    DELETE FROM payroll.planilla WHERE id_periodo_planilla = v_id_periodo;
    v_fecha_fin := (to_date(p_periodo || '-01', 'YYYY-MM-DD') + INTERVAL '1 month - 1 day')::DATE;

    FOR v_empleado IN
        SELECT e.id_empleado, e.id_departamento, e.id_tipo_descuento,
               e.salario_base, td.nombre AS tipo_descuento
        FROM payroll.empleado e
        JOIN payroll.tipo_descuento td ON td.id_tipo = e.id_tipo_descuento
        WHERE e.activo AND td.activo
        ORDER BY e.id_empleado
    LOOP
        v_id_config := payroll.fn_config_descuento_version_activa(v_empleado.id_tipo_descuento, v_fecha_fin);
        IF v_id_config IS NULL THEN
            RAISE EXCEPTION 'No Active pension configuration for employee % at %', v_empleado.id_empleado, v_fecha_fin;
        END IF;

        SELECT porcentaje INTO v_porcentaje
        FROM payroll.config_descuento_previsional_version
        WHERE id_config_descuento_version = v_id_config;

        SELECT COALESCE(horas_primeras_dos, 0), COALESCE(horas_posteriores, 0)
        INTO v_horas_1, v_horas_2
        FROM payroll.horas_extra
        WHERE id_empleado = v_empleado.id_empleado
          AND periodo = p_periodo AND estado = 'Approved';

        v_horas_monto := payroll.fn_calcular_horas_extra(
            v_empleado.salario_base, COALESCE(v_horas_1, 0), COALESCE(v_horas_2, 0)
        );
        v_bruto := payroll.fn_redondear_monto(v_empleado.salario_base + v_horas_monto);
        v_afp := CASE WHEN v_empleado.tipo_descuento = 'AFP'
                      THEN payroll.fn_calcular_porcentaje(v_bruto, v_porcentaje) ELSE 0 END;
        v_onp := CASE WHEN v_empleado.tipo_descuento = 'ONP'
                      THEN payroll.fn_calcular_porcentaje(v_bruto, v_porcentaje) ELSE 0 END;
        v_descuentos := payroll.fn_redondear_monto(v_afp + v_onp);
        v_neto := payroll.fn_redondear_monto(v_bruto - v_descuentos);
        v_provision_gratificacion := payroll.fn_calcular_provision_gratificacion(v_bruto);
        v_provision_cts := payroll.fn_calcular_provision_cts(v_bruto, v_provision_gratificacion);

        INSERT INTO payroll.planilla(
            id_periodo_planilla, id_empleado, id_departamento,
            salario_base_aplicado, total_bruto, total_descuentos, total_neto
        ) VALUES (
            v_id_periodo, v_empleado.id_empleado, v_empleado.id_departamento,
            v_empleado.salario_base, v_bruto, v_descuentos, v_neto
        ) RETURNING id_planilla INTO v_id_planilla;

        INSERT INTO payroll.detalle_planilla(
            id_planilla, id_config_descuento_version, horas_extra_total,
            afp, onp, provision_cts, provision_gratificacion, descuentos_adicionales
        ) VALUES (
            v_id_planilla, v_id_config, v_horas_monto,
            v_afp, v_onp, v_provision_cts, v_provision_gratificacion, 0
        );
    END LOOP;

    UPDATE payroll.periodo_planilla pp
    SET total_bruto = x.total_bruto,
        total_descuentos = x.total_descuentos,
        total_neto = x.total_neto,
        total_provision_gratificacion = x.provision_gratificacion,
        total_provision_cts = x.provision_cts,
        fecha_calculo = now()
    FROM (
        SELECT p.id_periodo_planilla,
               sum(p.total_bruto) AS total_bruto,
               sum(p.total_descuentos) AS total_descuentos,
               sum(p.total_neto) AS total_neto,
               sum(d.provision_gratificacion) AS provision_gratificacion,
               sum(d.provision_cts) AS provision_cts
        FROM payroll.planilla p
        JOIN payroll.detalle_planilla d ON d.id_planilla = p.id_planilla
        WHERE p.id_periodo_planilla = v_id_periodo
        GROUP BY p.id_periodo_planilla
    ) x
    WHERE pp.id_periodo_planilla = x.id_periodo_planilla;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'Payroll', 'CALCULAR_PLANILLA', 'PERIODO_PLANILLA',
        v_id_periodo::VARCHAR, 'Success', p_correlation_id,
        jsonb_build_object('periodo', p_periodo)
    );
END;
$$;

CREATE OR REPLACE PROCEDURE payroll.sp_finalizar_planilla(
    IN p_periodo VARCHAR(10),
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_id_periodo BIGINT;
    v_id_periodo_contable BIGINT;
    v_id_asiento BIGINT;
    v_rol VARCHAR(30);
    v_total_bruto NUMERIC(14,2);
    v_total_neto NUMERIC(14,2);
    v_afp NUMERIC(14,2);
    v_onp NUMERIC(14,2);
    v_prov_grat NUMERIC(14,2);
    v_prov_cts NUMERIC(14,2);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Administrador RRHH', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to finalize payroll';
    END IF;

    SELECT id_periodo_planilla, total_bruto, total_neto,
           total_provision_gratificacion, total_provision_cts
    INTO v_id_periodo, v_total_bruto, v_total_neto, v_prov_grat, v_prov_cts
    FROM payroll.periodo_planilla
    WHERE periodo = p_periodo AND estado = 'Draft'
    FOR UPDATE;

    IF v_id_periodo IS NULL THEN
        RAISE EXCEPTION 'Payroll period must exist in Draft state';
    END IF;

    SELECT COALESCE(sum(d.afp), 0), COALESCE(sum(d.onp), 0)
    INTO v_afp, v_onp
    FROM payroll.planilla p JOIN payroll.detalle_planilla d ON d.id_planilla = p.id_planilla
    WHERE p.id_periodo_planilla = v_id_periodo;

    SELECT id_periodo_contable INTO v_id_periodo_contable
    FROM accounting.periodo_contable
    WHERE codigo = p_periodo AND estado = 'Open'
    FOR UPDATE;
    IF v_id_periodo_contable IS NULL THEN
        RAISE EXCEPTION 'An Open accounting period % is required', p_periodo;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (VALUES ('6211'), ('6292'), ('6293'), ('4032'), ('4071'), ('4111'), ('4151')) AS required(codigo)
        WHERE NOT EXISTS (
            SELECT 1 FROM accounting.cuenta_contable c
            WHERE c.codigo = required.codigo AND c.activo
        )
    ) THEN
        RAISE EXCEPTION 'All canonical payroll mapping accounts must exist and be Active';
    END IF;

    INSERT INTO accounting.asiento_contable(
        id_periodo_contable, fecha_asiento, descripcion, origen, id_origen, estado
    ) VALUES (
        v_id_periodo_contable,
        (to_date(p_periodo || '-01', 'YYYY-MM-DD') + INTERVAL '1 month - 1 day')::DATE,
        'Planilla finalizada ' || p_periodo, 'Planilla', v_id_periodo, 'Draft'
    ) RETURNING id_asiento_contable INTO v_id_asiento;

    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, v_total_bruto, 0, 'Sueldos y horas extra'
    FROM accounting.cuenta_contable WHERE codigo = '6211' AND activo;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, v_prov_grat, 0, 'Provision gratificacion'
    FROM accounting.cuenta_contable WHERE codigo = '6292' AND activo AND v_prov_grat > 0;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, v_prov_cts, 0, 'Provision CTS'
    FROM accounting.cuenta_contable WHERE codigo = '6293' AND activo AND v_prov_cts > 0;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, 0, v_onp, 'ONP por pagar'
    FROM accounting.cuenta_contable WHERE codigo = '4032' AND activo AND v_onp > 0;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, 0, v_afp, 'AFP por pagar'
    FROM accounting.cuenta_contable WHERE codigo = '4071' AND activo AND v_afp > 0;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, 0, v_total_neto, 'Remuneraciones por pagar'
    FROM accounting.cuenta_contable WHERE codigo = '4111' AND activo AND v_total_neto > 0;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable, id_cuenta_contable, debe, haber, descripcion)
    SELECT v_id_asiento, id_cuenta_contable, 0, v_prov_grat + v_prov_cts, 'Beneficios sociales por pagar'
    FROM accounting.cuenta_contable WHERE codigo = '4151' AND activo AND v_prov_grat + v_prov_cts > 0;

    IF (SELECT COALESCE(sum(debe), 0) <> COALESCE(sum(haber), 0)
        FROM accounting.detalle_asiento WHERE id_asiento_contable = v_id_asiento) THEN
        RAISE EXCEPTION 'Canonical payroll mapping produced an unbalanced entry';
    END IF;

    UPDATE payroll.periodo_planilla
    SET estado = 'Finalized', fecha_finalizacion = now(), id_usuario_finalizador = p_id_usuario
    WHERE id_periodo_planilla = v_id_periodo;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'Payroll', 'FINALIZAR_PLANILLA', 'PERIODO_PLANILLA',
        v_id_periodo::VARCHAR, 'Success', p_correlation_id,
        jsonb_build_object('periodo', p_periodo, 'idAsientoDraft', v_id_asiento)
    );
END;
$$;

CREATE OR REPLACE PROCEDURE payroll.sp_cancelar_planilla(
    IN p_periodo VARCHAR(10),
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_id BIGINT;
    v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Administrador RRHH', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to cancel payroll';
    END IF;

    UPDATE payroll.periodo_planilla SET estado = 'Cancelled'
    WHERE periodo = p_periodo AND estado = 'Draft'
    RETURNING id_periodo_planilla INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'Payroll period must be Draft'; END IF;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'Payroll', 'CANCELAR_PLANILLA', 'PERIODO_PLANILLA',
        v_id::VARCHAR, 'Success', p_correlation_id
    );
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_activar_config_tributaria(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE admin.config_tributaria_version SET estado='Active'
    WHERE id_config_tributaria_version=p_id AND estado='Draft'
    RETURNING id_config_tributaria_version INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'Tax version must be Draft'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','TAX_CONFIG_ACTIVATED','CONFIG_TRIBUTARIA_VERSION',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_cerrar_config_tributaria(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE admin.config_tributaria_version SET estado='Closed'
    WHERE id_config_tributaria_version=p_id AND estado='Active'
    RETURNING id_config_tributaria_version INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'Tax version must be Active'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','TAX_CONFIG_CLOSED','CONFIG_TRIBUTARIA_VERSION',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_activar_config_previsional(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE payroll.config_descuento_previsional_version SET estado='Active'
    WHERE id_config_descuento_version=p_id AND estado='Draft'
    RETURNING id_config_descuento_version INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'Pension version must be Draft'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','PENSION_CONFIG_ACTIVATED','CONFIG_DESCUENTO_PREVISIONAL_VERSION',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_cerrar_config_previsional(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE payroll.config_descuento_previsional_version SET estado='Closed'
    WHERE id_config_descuento_version=p_id AND estado='Active'
    RETURNING id_config_descuento_version INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'Pension version must be Active'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','PENSION_CONFIG_CLOSED','CONFIG_DESCUENTO_PREVISIONAL_VERSION',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_activar_formato_sunat(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE admin.config_sunat_formato SET estado='Active'
    WHERE id_config_sunat_formato=p_id AND estado='Draft'
    RETURNING id_config_sunat_formato INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'SUNAT format must be Draft'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','SUNAT_FORMAT_ACTIVATED','CONFIG_SUNAT_FORMATO',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE admin.sp_cerrar_formato_sunat(
    IN p_id BIGINT, IN p_id_usuario BIGINT, IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30); v_id BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol <> 'Administrador Sistema' THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE admin.config_sunat_formato SET estado='Closed'
    WHERE id_config_sunat_formato=p_id AND estado='Active'
    RETURNING id_config_sunat_formato INTO v_id;
    IF v_id IS NULL THEN RAISE EXCEPTION 'SUNAT format must be Active'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'Administration','SUNAT_FORMAT_CLOSED','CONFIG_SUNAT_FORMATO',v_id::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_generar_libro(
    IN p_tipo_libro VARCHAR(20),
    IN p_periodo VARCHAR(10),
    IN p_aceptar_observaciones BOOLEAN,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_estado VARCHAR(20);
    v_elegibles BIGINT;
    v_observaciones JSONB;
    v_movimiento VARCHAR(10);
    v_inicio DATE;
    v_fin DATE;
    v_id_tipo_libro BIGINT;
    v_id_formato BIGINT;
    v_version INTEGER;
    v_id_libro BIGINT;
    v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to generate SUNAT books';
    END IF;

    SELECT * INTO v_estado, v_elegibles, v_observaciones
    FROM accounting.fn_validar_libro(p_tipo_libro, p_periodo);
    IF v_estado = 'Bloqueada' OR (v_estado = 'ConObservaciones' AND NOT COALESCE(p_aceptar_observaciones, FALSE)) THEN
        RAISE EXCEPTION 'SUNAT validation %: %', v_estado, v_observaciones;
    END IF;

    v_movimiento := CASE p_tipo_libro WHEN 'Compras' THEN 'Compra' ELSE 'Venta' END;
    v_inicio := to_date(p_periodo || '-01', 'YYYY-MM-DD');
    v_fin := (v_inicio + INTERVAL '1 month - 1 day')::DATE;
    SELECT id_tipo_libro INTO v_id_tipo_libro FROM accounting.tipo_libro
    WHERE nombre = p_tipo_libro AND activo;
    IF v_id_tipo_libro IS NULL THEN RAISE EXCEPTION 'Active book type is required'; END IF;

    PERFORM pg_advisory_xact_lock(hashtext(p_tipo_libro || ':' || p_periodo));

    CREATE TEMP TABLE IF NOT EXISTS pg_temp.tmp_libro_comprobante (
        id_comprobante BIGINT PRIMARY KEY
    ) ON COMMIT DROP;
    TRUNCATE pg_temp.tmp_libro_comprobante;
    INSERT INTO pg_temp.tmp_libro_comprobante(id_comprobante)
    SELECT c.id_comprobante
    FROM accounting.comprobante c
    WHERE c.tipo_movimiento = v_movimiento AND c.estado = 'Registrado'
      AND c.fecha_emision BETWEEN v_inicio AND v_fin;

    PERFORM c.id_comprobante
    FROM accounting.comprobante c
    JOIN pg_temp.tmp_libro_comprobante s ON s.id_comprobante = c.id_comprobante
    ORDER BY c.fecha_emision, c.serie, c.numero, c.id_comprobante
    FOR UPDATE;

    IF EXISTS (
        SELECT 1
        FROM pg_temp.tmp_libro_comprobante s
        LEFT JOIN accounting.comprobante c ON c.id_comprobante = s.id_comprobante
        WHERE c.id_comprobante IS NULL
           OR c.tipo_movimiento <> v_movimiento OR c.estado <> 'Registrado'
           OR c.fecha_emision NOT BETWEEN v_inicio AND v_fin
    ) THEN
        RAISE EXCEPTION 'SUNAT source set changed while acquiring row locks';
    END IF;

    SELECT * INTO v_estado, v_elegibles, v_observaciones
    FROM accounting.fn_validar_libro(p_tipo_libro, p_periodo);
    IF v_estado = 'Bloqueada' OR (v_estado = 'ConObservaciones' AND NOT COALESCE(p_aceptar_observaciones, FALSE)) THEN
        RAISE EXCEPTION 'SUNAT validation changed while locking source set: %: %', v_estado, v_observaciones;
    END IF;

    v_id_formato := admin.fn_config_sunat_formato_activo(p_tipo_libro, v_fin);
    PERFORM 1 FROM admin.config_sunat_formato
    WHERE id_config_sunat_formato = v_id_formato AND estado = 'Active'
    FOR SHARE;
    IF NOT FOUND THEN RAISE EXCEPTION 'Active SUNAT format is required'; END IF;

    SELECT COALESCE(max(version), 0) + 1 INTO v_version
    FROM accounting.libro_contable
    WHERE id_tipo_libro = v_id_tipo_libro AND periodo = p_periodo;

    INSERT INTO accounting.libro_contable(
        id_tipo_libro, id_config_sunat_formato, id_usuario_generador,
        periodo, version, total_base_imponible, total_igv, total_general
    )
    SELECT v_id_tipo_libro, v_id_formato, p_id_usuario, p_periodo, v_version,
           sum(base_imponible), sum(igv), sum(total)
    FROM accounting.comprobante c
    JOIN pg_temp.tmp_libro_comprobante s ON s.id_comprobante = c.id_comprobante
    RETURNING id_libro INTO v_id_libro;

    INSERT INTO accounting.comprobante_libro(
        id_libro, id_comprobante, id_config_tributaria_version, orden,
        periodo, tipo_documento, numero_documento, codigo_tipo_comprobante,
        serie, numero, fecha_emision, base_imponible, base_exonerada,
        total_igv, total_general
    )
    SELECT v_id_libro, c.id_comprobante, c.id_config_tributaria_version,
           row_number() OVER (ORDER BY c.fecha_emision, c.serie, c.numero, c.id_comprobante),
           p_periodo, c.tipo_documento, c.numero_documento, tc.codigo_sunat,
           c.serie, c.numero, c.fecha_emision, c.base_imponible, c.base_exonerada,
           c.igv, c.total
    FROM accounting.comprobante c
    JOIN pg_temp.tmp_libro_comprobante s ON s.id_comprobante = c.id_comprobante
    JOIN accounting.tipo_comprobante tc ON tc.id_tipo_comp = c.id_tipo_comp
    ORDER BY c.fecha_emision, c.serie, c.numero, c.id_comprobante;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'AccountingSUNAT', 'GENERAR_LIBRO', 'LIBRO_CONTABLE',
        v_id_libro::VARCHAR, 'Success', p_correlation_id,
        jsonb_build_object('periodo', p_periodo, 'tipoLibro', p_tipo_libro,
                           'version', v_version, 'validacion', v_estado)
    );
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_sincronizar_asiento_comprobante(
    IN p_id_comprobante BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_comp RECORD;
    v_id_periodo BIGINT;
    v_id_asiento BIGINT;
    v_rol VARCHAR(30);
    v_codigo_sunat VARCHAR(4);
BEGIN
    SELECT c.*, tc.codigo_sunat INTO v_comp
    FROM accounting.comprobante c
    JOIN accounting.tipo_comprobante tc ON tc.id_tipo_comp = c.id_tipo_comp
    WHERE c.id_comprobante = p_id_comprobante AND c.estado = 'Registrado'
    FOR UPDATE;
    IF v_comp.id_comprobante IS NULL THEN RAISE EXCEPTION 'Registrado voucher not found'; END IF;
    v_codigo_sunat := v_comp.codigo_sunat;

    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador', 'Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized to synchronize voucher ledger entry';
    END IF;

    SELECT id_periodo_contable INTO v_id_periodo
    FROM accounting.periodo_contable
    WHERE estado = 'Open' AND v_comp.fecha_emision BETWEEN fecha_inicio AND fecha_fin;
    IF v_id_periodo IS NULL THEN RAISE EXCEPTION 'Open accounting period is required for voucher date'; END IF;

    IF v_comp.tipo_movimiento = 'Venta' AND EXISTS (
        SELECT 1
        FROM (VALUES ('1212'), ('7011'), ('40111')) AS required(codigo)
        WHERE NOT EXISTS (
            SELECT 1 FROM accounting.cuenta_contable c
            WHERE c.codigo = required.codigo AND c.activo
        )
    ) THEN
        RAISE EXCEPTION 'All canonical sales voucher mapping accounts must exist and be Active';
    ELSIF v_comp.tipo_movimiento = 'Compra' AND EXISTS (
        SELECT 1
        FROM (VALUES ('6011'), ('40114'), ('4212')) AS required(codigo)
        WHERE NOT EXISTS (
            SELECT 1 FROM accounting.cuenta_contable c
            WHERE c.codigo = required.codigo AND c.activo
        )
    ) THEN
        RAISE EXCEPTION 'All canonical purchase voucher mapping accounts must exist and be Active';
    END IF;

    SELECT id_asiento_contable INTO v_id_asiento
    FROM accounting.asiento_contable
    WHERE origen = 'Comprobante' AND id_origen = p_id_comprobante
    FOR UPDATE;
    IF v_id_asiento IS NOT NULL AND EXISTS (
        SELECT 1 FROM accounting.asiento_contable WHERE id_asiento_contable = v_id_asiento AND estado <> 'Draft'
    ) THEN RAISE EXCEPTION 'Posted/cancelled source entry cannot be synchronized'; END IF;

    IF v_id_asiento IS NULL THEN
        INSERT INTO accounting.asiento_contable(
            id_periodo_contable, fecha_asiento, descripcion, origen, id_origen
        ) VALUES (
            v_id_periodo, v_comp.fecha_emision,
            'Comprobante ' || v_comp.tipo_movimiento || ' ' || v_comp.serie || '-' || v_comp.numero,
            'Comprobante', p_id_comprobante
        ) RETURNING id_asiento_contable INTO v_id_asiento;
    ELSE
        UPDATE accounting.asiento_contable
        SET id_periodo_contable = v_id_periodo, fecha_asiento = v_comp.fecha_emision,
            descripcion = 'Comprobante ' || v_comp.tipo_movimiento || ' ' || v_comp.serie || '-' || v_comp.numero
        WHERE id_asiento_contable = v_id_asiento;
        DELETE FROM accounting.detalle_asiento WHERE id_asiento_contable = v_id_asiento;
    END IF;

    IF v_codigo_sunat = '07' AND v_comp.tipo_movimiento = 'Venta' THEN
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.total,'Reduccion de cuenta por cobrar' FROM accounting.cuenta_contable WHERE codigo='1212' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.base_imponible+v_comp.base_exonerada,0,'Reduccion de venta' FROM accounting.cuenta_contable WHERE codigo='7011' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.igv,0,'Reduccion de IGV por pagar' FROM accounting.cuenta_contable WHERE codigo='40111' AND activo AND v_comp.igv>0;
    ELSIF v_codigo_sunat = '07' AND v_comp.tipo_movimiento = 'Compra' THEN
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.base_imponible+v_comp.base_exonerada,'Reduccion de compra' FROM accounting.cuenta_contable WHERE codigo='6011' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.igv,'Reduccion de IGV credito fiscal' FROM accounting.cuenta_contable WHERE codigo='40114' AND activo AND v_comp.igv>0;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.total,0,'Reduccion de cuenta por pagar' FROM accounting.cuenta_contable WHERE codigo='4212' AND activo;
    ELSIF v_comp.tipo_movimiento = 'Venta' THEN
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.total,0,'Cuenta por cobrar' FROM accounting.cuenta_contable WHERE codigo='1212' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.base_imponible+v_comp.base_exonerada,'Venta' FROM accounting.cuenta_contable WHERE codigo='7011' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.igv,'IGV por pagar' FROM accounting.cuenta_contable WHERE codigo='40111' AND activo AND v_comp.igv>0;
    ELSE
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.base_imponible+v_comp.base_exonerada,0,'Compra' FROM accounting.cuenta_contable WHERE codigo='6011' AND activo;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,v_comp.igv,0,'IGV credito fiscal' FROM accounting.cuenta_contable WHERE codigo='40114' AND activo AND v_comp.igv>0;
        INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
        SELECT v_id_asiento,id_cuenta_contable,0,v_comp.total,'Cuenta por pagar' FROM accounting.cuenta_contable WHERE codigo='4212' AND activo;
    END IF;

    IF (SELECT COALESCE(sum(debe), 0) <> COALESCE(sum(haber), 0)
        FROM accounting.detalle_asiento WHERE id_asiento_contable = v_id_asiento) THEN
        RAISE EXCEPTION 'Canonical voucher mapping produced an unbalanced entry';
    END IF;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'GeneralLedger', 'SINCRONIZAR_COMPROBANTE', 'ASIENTO_CONTABLE',
        v_id_asiento::VARCHAR, 'Success', p_correlation_id,
        jsonb_build_object('idComprobante', p_id_comprobante)
    );
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_postear_asiento(
    IN p_id_asiento_contable BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_debe NUMERIC(14,2);
    v_haber NUMERIC(14,2);
    v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol = u.id_rol
    WHERE u.id_usuario = p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador', 'Administrador Sistema') THEN RAISE EXCEPTION 'Actor is not authorized to post'; END IF;

    IF NOT EXISTS (
        SELECT 1 FROM accounting.asiento_contable a
        JOIN accounting.periodo_contable p ON p.id_periodo_contable = a.id_periodo_contable
        WHERE a.id_asiento_contable = p_id_asiento_contable AND a.estado = 'Draft'
          AND p.estado = 'Open' AND a.fecha_asiento BETWEEN p.fecha_inicio AND p.fecha_fin
        FOR UPDATE OF a, p
    ) THEN RAISE EXCEPTION 'Draft entry in Open matching period is required'; END IF;

    IF EXISTS (
        SELECT 1 FROM accounting.detalle_asiento d
        JOIN accounting.cuenta_contable c ON c.id_cuenta_contable = d.id_cuenta_contable
        WHERE d.id_asiento_contable = p_id_asiento_contable AND NOT c.activo
    ) THEN RAISE EXCEPTION 'Inactive account exists in entry'; END IF;

    SELECT COALESCE(sum(debe),0), COALESCE(sum(haber),0) INTO v_debe, v_haber
    FROM accounting.detalle_asiento WHERE id_asiento_contable = p_id_asiento_contable;
    IF v_debe <= 0 OR v_debe <> v_haber THEN RAISE EXCEPTION 'Entry is unbalanced'; END IF;

    UPDATE accounting.asiento_contable
    SET estado='Posted', fecha_posteo=now(), id_usuario_posteo=p_id_usuario
    WHERE id_asiento_contable=p_id_asiento_contable;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario, v_rol, 'GeneralLedger', 'POSTEAR_ASIENTO', 'ASIENTO_CONTABLE',
        p_id_asiento_contable::VARCHAR, 'Success', p_correlation_id,
        jsonb_build_object('debe', v_debe, 'haber', v_haber)
    );
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_cancelar_asiento(
    IN p_id_asiento_contable BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador','Administrador Sistema') THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    UPDATE accounting.asiento_contable SET estado='Cancelled'
    WHERE id_asiento_contable=p_id_asiento_contable AND estado='Draft';
    IF NOT FOUND THEN RAISE EXCEPTION 'Only Draft entries can be cancelled'; END IF;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'GeneralLedger','CANCELAR_ASIENTO','ASIENTO_CONTABLE',p_id_asiento_contable::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_revertir_asiento(
    IN p_id_asiento_contable BIGINT,
    IN p_periodo_destino VARCHAR(10),
    IN p_motivo VARCHAR(250),
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR,
    INOUT p_id_nuevo BIGINT DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_periodo BIGINT; v_rol VARCHAR(30); v_fecha DATE; v_estado_original VARCHAR(20);
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador','Administrador Sistema') THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    SELECT estado INTO v_estado_original FROM accounting.asiento_contable
    WHERE id_asiento_contable=p_id_asiento_contable
    FOR UPDATE;
    IF v_estado_original IS DISTINCT FROM 'Posted' THEN
        RAISE EXCEPTION 'Only Posted entries can be reversed';
    END IF;
    IF EXISTS (SELECT 1 FROM accounting.asiento_contable WHERE id_asiento_revertido=p_id_asiento_contable AND estado<>'Cancelled') THEN
        RAISE EXCEPTION 'Entry already has an active reversal';
    END IF;
    SELECT id_periodo_contable, fecha_fin INTO v_periodo, v_fecha FROM accounting.periodo_contable
    WHERE codigo=p_periodo_destino AND estado='Open';
    IF v_periodo IS NULL THEN RAISE EXCEPTION 'Open destination period is required'; END IF;
    INSERT INTO accounting.asiento_contable(id_periodo_contable,id_asiento_revertido,fecha_asiento,descripcion,origen,estado)
    VALUES(v_periodo,p_id_asiento_contable,v_fecha,p_motivo,'Ajuste','Draft') RETURNING id_asiento_contable INTO p_id_nuevo;
    INSERT INTO accounting.detalle_asiento(id_asiento_contable,id_cuenta_contable,debe,haber,descripcion)
    SELECT p_id_nuevo,id_cuenta_contable,haber,debe,'Reversion: '||COALESCE(descripcion,'')
    FROM accounting.detalle_asiento WHERE id_asiento_contable=p_id_asiento_contable;
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'GeneralLedger','REVERTIR_ASIENTO','ASIENTO_CONTABLE',p_id_nuevo::VARCHAR,'Success',p_correlation_id,jsonb_build_object('idOriginal',p_id_asiento_contable));
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_cerrar_periodo(
    IN p_id_periodo_contable BIGINT,
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE v_rol VARCHAR(30);
BEGIN
    SELECT r.nombre INTO v_rol FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador','Administrador Sistema') THEN RAISE EXCEPTION 'Actor is not authorized'; END IF;
    PERFORM 1 FROM accounting.periodo_contable
    WHERE id_periodo_contable=p_id_periodo_contable AND estado='Open'
    FOR UPDATE;
    IF NOT FOUND THEN RAISE EXCEPTION 'Only Open period can close'; END IF;
    IF EXISTS (SELECT 1 FROM accounting.asiento_contable WHERE id_periodo_contable=p_id_periodo_contable AND estado='Draft') THEN
        RAISE EXCEPTION 'Period with Draft entries cannot close';
    END IF;
    UPDATE accounting.periodo_contable
    SET estado='Closed',fecha_cierre=now(),id_usuario_cierre=p_id_usuario
    WHERE id_periodo_contable=p_id_periodo_contable AND estado='Open';
    PERFORM audit.fn_registrar_evento(p_id_usuario,v_rol,'GeneralLedger','CERRAR_PERIODO','PERIODO_CONTABLE',p_id_periodo_contable::VARCHAR,'Success',p_correlation_id);
END;
$$;

CREATE OR REPLACE PROCEDURE accounting.sp_anular_comprobante(
    IN p_id_comprobante BIGINT,
    IN p_periodo_reversion VARCHAR(10),
    IN p_id_usuario BIGINT,
    IN p_correlation_id VARCHAR
)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_rol VARCHAR(30);
    v_id_asiento BIGINT;
    v_estado_asiento VARCHAR(20);
    v_id_reversion BIGINT;
BEGIN
    SELECT r.nombre INTO v_rol
    FROM "identity".usuario u JOIN "identity".rol r ON r.id_rol=u.id_rol
    WHERE u.id_usuario=p_id_usuario AND u.activo;
    IF v_rol NOT IN ('Contador','Administrador Sistema') THEN
        RAISE EXCEPTION 'Actor is not authorized';
    END IF;

    UPDATE accounting.comprobante SET estado='Anulado'
    WHERE id_comprobante=p_id_comprobante AND estado='Registrado';
    IF NOT FOUND THEN RAISE EXCEPTION 'Only Registrado voucher can be annulled'; END IF;

    SELECT id_asiento_contable, estado INTO v_id_asiento, v_estado_asiento
    FROM accounting.asiento_contable
    WHERE origen='Comprobante' AND id_origen=p_id_comprobante
    FOR UPDATE;

    IF v_estado_asiento = 'Draft' THEN
        UPDATE accounting.asiento_contable SET estado='Cancelled'
        WHERE id_asiento_contable=v_id_asiento;
    ELSIF v_estado_asiento = 'Posted' THEN
        IF p_periodo_reversion IS NULL THEN
            RAISE EXCEPTION 'Destination Open period is required to reverse a Posted source entry';
        END IF;
        CALL accounting.sp_revertir_asiento(
            v_id_asiento, p_periodo_reversion,
            'Anulacion de comprobante ' || p_id_comprobante,
            p_id_usuario, p_correlation_id, v_id_reversion
        );
    END IF;

    PERFORM audit.fn_registrar_evento(
        p_id_usuario,v_rol,'AccountingSUNAT','ANULAR_COMPROBANTE','COMPROBANTE',
        p_id_comprobante::VARCHAR,'Success',p_correlation_id,
        jsonb_build_object('idAsiento',v_id_asiento,'idReversionDraft',v_id_reversion)
    );
END;
$$;

COMMIT;
