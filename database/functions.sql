BEGIN;

CREATE OR REPLACE FUNCTION admin.fn_periodo_valido(p_periodo TEXT)
RETURNS BOOLEAN
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT p_periodo ~ '^[0-9]{4}-(0[1-9]|1[0-2])$';
$$;

CREATE OR REPLACE FUNCTION payroll.fn_redondear_monto(p_monto NUMERIC)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT round(COALESCE(p_monto, 0), 2)::NUMERIC(12,2);
$$;

CREATE OR REPLACE FUNCTION payroll.fn_calcular_porcentaje(p_base NUMERIC, p_porcentaje NUMERIC)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT payroll.fn_redondear_monto(COALESCE(p_base, 0) * COALESCE(p_porcentaje, 0) / 100);
$$;

CREATE OR REPLACE FUNCTION payroll.fn_calcular_provision_gratificacion(p_total_bruto NUMERIC)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT payroll.fn_redondear_monto(COALESCE(p_total_bruto, 0) / 6);
$$;

CREATE OR REPLACE FUNCTION payroll.fn_calcular_provision_cts(
    p_total_bruto NUMERIC,
    p_provision_gratificacion NUMERIC
)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT payroll.fn_redondear_monto(
        (COALESCE(p_total_bruto, 0) + COALESCE(p_provision_gratificacion, 0)) / 12
    );
$$;

CREATE OR REPLACE FUNCTION payroll.fn_calcular_horas_extra(
    p_salario_base NUMERIC,
    p_horas_primeras_dos NUMERIC,
    p_horas_posteriores NUMERIC
)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT payroll.fn_redondear_monto(
        ((COALESCE(p_salario_base, 0) / 240) * 1.25 * COALESCE(p_horas_primeras_dos, 0)) +
        ((COALESCE(p_salario_base, 0) / 240) * 1.35 * COALESCE(p_horas_posteriores, 0))
    );
$$;

CREATE OR REPLACE FUNCTION payroll.fn_config_descuento_version_activa(
    p_id_tipo BIGINT,
    p_fecha DATE
)
RETURNS BIGINT
LANGUAGE sql
STABLE
AS $$
    SELECT cdv.id_config_descuento_version
    FROM payroll.config_descuento_previsional_version cdv
    WHERE cdv.id_tipo = p_id_tipo
      AND cdv.estado = 'Active'
      AND cdv.fecha_inicio <= p_fecha
      AND (cdv.fecha_fin IS NULL OR cdv.fecha_fin >= p_fecha)
    ORDER BY cdv.version DESC
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION admin.fn_config_tributaria_version_activa(
    p_codigo TEXT,
    p_fecha DATE
)
RETURNS BIGINT
LANGUAGE sql
STABLE
AS $$
    SELECT ctv.id_config_tributaria_version
    FROM admin.config_tributaria_version ctv
    WHERE ctv.codigo = p_codigo
      AND ctv.estado = 'Active'
      AND ctv.fecha_inicio <= p_fecha
      AND (ctv.fecha_fin IS NULL OR ctv.fecha_fin >= p_fecha)
    ORDER BY ctv.version DESC
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION admin.fn_config_sunat_formato_activo(
    p_tipo_libro TEXT,
    p_fecha DATE
)
RETURNS BIGINT
LANGUAGE sql
STABLE
AS $$
    SELECT csf.id_config_sunat_formato
    FROM admin.config_sunat_formato csf
    WHERE csf.tipo_libro = p_tipo_libro
      AND csf.estado = 'Active'
      AND csf.fecha_inicio <= p_fecha
      AND (csf.fecha_fin IS NULL OR csf.fecha_fin >= p_fecha)
    ORDER BY csf.version_formato DESC
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION accounting.fn_tasa_igv_activa(p_fecha DATE)
RETURNS NUMERIC(5,2)
LANGUAGE sql
STABLE
AS $$
    SELECT ctv.tasa_igv
    FROM admin.config_tributaria_version ctv
    WHERE ctv.id_config_tributaria_version = admin.fn_config_tributaria_version_activa('IGV', p_fecha);
$$;

CREATE OR REPLACE FUNCTION accounting.fn_calcular_igv(
    p_base_imponible NUMERIC,
    p_tipo_operacion TEXT,
    p_tasa_igv NUMERIC
)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT CASE
        WHEN p_tipo_operacion = 'Gravada'
            THEN round(COALESCE(p_base_imponible, 0) * (COALESCE(p_tasa_igv, 0) / 100), 2)::NUMERIC(12,2)
        ELSE 0::NUMERIC(12,2)
    END;
$$;

CREATE OR REPLACE FUNCTION accounting.fn_calcular_total(
    p_base_imponible NUMERIC,
    p_base_exonerada NUMERIC,
    p_igv NUMERIC
)
RETURNS NUMERIC(12,2)
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT round(
        COALESCE(p_base_imponible, 0) +
        COALESCE(p_base_exonerada, 0) +
        COALESCE(p_igv, 0), 2
    )::NUMERIC(12,2);
$$;

CREATE OR REPLACE FUNCTION accounting.fn_validar_libro(p_tipo_libro TEXT, p_periodo TEXT)
RETURNS TABLE (
    estado VARCHAR(20),
    elegibles BIGINT,
    observaciones JSONB
)
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_movimiento VARCHAR(10);
    v_inicio DATE;
    v_fin DATE;
    v_formato BIGINT;
    v_elegibles BIGINT;
    v_anulados BIGINT;
BEGIN
    IF NOT admin.fn_periodo_valido(p_periodo) OR p_tipo_libro NOT IN ('Compras', 'Ventas') THEN
        RETURN QUERY SELECT 'Bloqueada'::VARCHAR, 0::BIGINT, jsonb_build_array('Periodo o tipo de libro invalido');
        RETURN;
    END IF;

    v_movimiento := CASE p_tipo_libro WHEN 'Compras' THEN 'Compra' ELSE 'Venta' END;
    v_inicio := to_date(p_periodo || '-01', 'YYYY-MM-DD');
    v_fin := (v_inicio + INTERVAL '1 month - 1 day')::DATE;
    v_formato := admin.fn_config_sunat_formato_activo(p_tipo_libro, v_fin);

    SELECT count(*) FILTER (WHERE estado = 'Registrado'), count(*) FILTER (WHERE estado = 'Anulado')
    INTO v_elegibles, v_anulados
    FROM accounting.comprobante
    WHERE tipo_movimiento = v_movimiento
      AND fecha_emision BETWEEN v_inicio AND v_fin;

    IF v_formato IS NULL THEN
        RETURN QUERY SELECT 'Bloqueada'::VARCHAR, v_elegibles, jsonb_build_array('No existe formato SUNAT Active vigente');
    ELSIF v_elegibles = 0 THEN
        RETURN QUERY SELECT 'Bloqueada'::VARCHAR, 0::BIGINT, jsonb_build_array('No existen comprobantes Registrado elegibles');
    ELSIF v_anulados > 0 THEN
        RETURN QUERY SELECT 'ConObservaciones'::VARCHAR, v_elegibles, jsonb_build_array(v_anulados || ' comprobante(s) Anulado excluido(s)');
    ELSE
        RETURN QUERY SELECT 'Valida'::VARCHAR, v_elegibles, '[]'::JSONB;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION audit.fn_registrar_evento(
    p_id_usuario BIGINT,
    p_rol_actor VARCHAR,
    p_modulo VARCHAR,
    p_accion VARCHAR,
    p_entidad VARCHAR,
    p_id_entidad VARCHAR,
    p_resultado VARCHAR,
    p_correlation_id VARCHAR,
    p_datos_json JSONB DEFAULT '{}'::JSONB
)
RETURNS BIGINT
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
    v_id_audit BIGINT;
BEGIN
    INSERT INTO audit.audit_log (
        id_usuario, rol_actor, modulo, accion, entidad, id_entidad,
        resultado, datos_json, correlation_id
    ) VALUES (
        p_id_usuario, p_rol_actor, p_modulo, p_accion, p_entidad, p_id_entidad,
        p_resultado, COALESCE(p_datos_json, '{}'::JSONB), p_correlation_id
    ) RETURNING id_audit_log INTO v_id_audit;

    RETURN v_id_audit;
END;
$$;

CREATE OR REPLACE FUNCTION accounting.fn_bloquear_comprobante_incluido()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM accounting.comprobante_libro cl
        WHERE cl.id_comprobante = OLD.id_comprobante
    ) AND ROW(
        NEW.id_tipo_comp, NEW.id_config_tributaria_version, NEW.id_comprobante_referencia, NEW.tipo_movimiento,
        NEW.tipo_documento, NEW.numero_documento, NEW.razon_social,
        NEW.serie, NEW.numero, NEW.fecha_emision, NEW.base_imponible,
        NEW.base_exonerada, NEW.igv, NEW.total, NEW.tipo_operacion
    ) IS DISTINCT FROM ROW(
        OLD.id_tipo_comp, OLD.id_config_tributaria_version, OLD.id_comprobante_referencia, OLD.tipo_movimiento,
        OLD.tipo_documento, OLD.numero_documento, OLD.razon_social,
        OLD.serie, OLD.numero, OLD.fecha_emision, OLD.base_imponible,
        OLD.base_exonerada, OLD.igv, OLD.total, OLD.tipo_operacion
    ) THEN
        RAISE EXCEPTION 'Voucher % is immutable because it belongs to a generated book', OLD.id_comprobante;
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_bloquear_comprobante_incluido ON accounting.comprobante;
CREATE TRIGGER trg_bloquear_comprobante_incluido
BEFORE UPDATE ON accounting.comprobante
FOR EACH ROW EXECUTE FUNCTION accounting.fn_bloquear_comprobante_incluido();

CREATE OR REPLACE FUNCTION admin.fn_proteger_version_config()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Configuration versions cannot be deleted';
    END IF;

    IF OLD.estado = 'Closed' THEN
        RAISE EXCEPTION 'Closed configuration versions are immutable';
    END IF;

    IF OLD.estado = 'Active' THEN
        IF NEW.estado <> 'Closed' OR
           (to_jsonb(NEW) - 'estado') IS DISTINCT FROM (to_jsonb(OLD) - 'estado') THEN
            RAISE EXCEPTION 'Active configuration may only transition to Closed without field changes';
        END IF;
    ELSIF OLD.estado = 'Draft' AND NEW.estado NOT IN ('Draft', 'Active') THEN
        RAISE EXCEPTION 'Draft configuration may only remain Draft or transition to Active';
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_proteger_config_previsional
BEFORE UPDATE OR DELETE ON payroll.config_descuento_previsional_version
FOR EACH ROW EXECUTE FUNCTION admin.fn_proteger_version_config();

CREATE TRIGGER trg_proteger_config_tributaria
BEFORE UPDATE OR DELETE ON admin.config_tributaria_version
FOR EACH ROW EXECUTE FUNCTION admin.fn_proteger_version_config();

CREATE TRIGGER trg_proteger_config_sunat
BEFORE UPDATE OR DELETE ON admin.config_sunat_formato
FOR EACH ROW EXECUTE FUNCTION admin.fn_proteger_version_config();

CREATE OR REPLACE FUNCTION accounting.fn_validar_nota_referencia()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_codigo VARCHAR(4);
    v_movimiento VARCHAR(10);
    v_estado VARCHAR(20);
BEGIN
    SELECT codigo_sunat INTO v_codigo
    FROM accounting.tipo_comprobante
    WHERE id_tipo_comp = NEW.id_tipo_comp;

    IF v_codigo IN ('07', '08') THEN
        IF NEW.id_comprobante_referencia IS NULL THEN
            RAISE EXCEPTION 'Credit and debit notes require an original voucher reference';
        END IF;
        SELECT tipo_movimiento, estado INTO v_movimiento, v_estado
        FROM accounting.comprobante
        WHERE id_comprobante = NEW.id_comprobante_referencia;
        IF v_movimiento IS NULL OR v_movimiento <> NEW.tipo_movimiento OR v_estado <> 'Registrado' THEN
            RAISE EXCEPTION 'Referenced voucher must exist, be Registrado, and use the same movement';
        END IF;
    ELSIF NEW.id_comprobante_referencia IS NOT NULL THEN
        RAISE EXCEPTION 'Only credit and debit notes may reference an original voucher';
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_validar_nota_referencia
BEFORE INSERT OR UPDATE OF id_tipo_comp, id_comprobante_referencia, tipo_movimiento
ON accounting.comprobante
FOR EACH ROW EXECUTE FUNCTION accounting.fn_validar_nota_referencia();

CREATE OR REPLACE FUNCTION payroll.fn_validar_empleado_catalogos()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM payroll.departamento
        WHERE id_departamento = NEW.id_departamento AND activo
    ) THEN
        RAISE EXCEPTION 'Employee requires an active department';
    END IF;
    IF NOT EXISTS (
        SELECT 1 FROM payroll.tipo_descuento
        WHERE id_tipo = NEW.id_tipo_descuento AND activo
    ) THEN
        RAISE EXCEPTION 'Employee requires an active AFP/ONP type';
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_validar_empleado_catalogos
BEFORE INSERT OR UPDATE OF id_departamento, id_tipo_descuento ON payroll.empleado
FOR EACH ROW EXECUTE FUNCTION payroll.fn_validar_empleado_catalogos();

CREATE OR REPLACE FUNCTION payroll.fn_validar_horas_extra_registro()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM payroll.empleado
        WHERE id_empleado = NEW.id_empleado AND activo
    ) THEN
        RAISE EXCEPTION 'Overtime requires an active employee';
    END IF;
    IF EXISTS (SELECT 1 FROM payroll.periodo_planilla WHERE periodo = NEW.periodo) THEN
        RAISE EXCEPTION 'Overtime cannot be registered after a payroll aggregate exists';
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_validar_horas_extra_registro
BEFORE INSERT OR UPDATE OF id_empleado, periodo ON payroll.horas_extra
FOR EACH ROW EXECUTE FUNCTION payroll.fn_validar_horas_extra_registro();

CREATE OR REPLACE FUNCTION accounting.fn_proteger_asiento()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        PERFORM 1 FROM accounting.periodo_contable p
        WHERE p.id_periodo_contable = NEW.id_periodo_contable
          AND p.estado = 'Open'
          AND NEW.fecha_asiento BETWEEN p.fecha_inicio AND p.fecha_fin
        FOR SHARE;
        IF NOT FOUND THEN
            RAISE EXCEPTION 'Journal entry requires an Open period containing its date';
        END IF;
        RETURN NEW;
    END IF;
    IF TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Journal entries cannot be physically deleted';
    END IF;
    IF OLD.estado <> 'Draft' THEN
        RAISE EXCEPTION 'Posted and Cancelled journal entries are immutable';
    END IF;
    IF NEW.estado IN ('Posted', 'Cancelled') AND
       (to_jsonb(NEW) - ARRAY['estado','fecha_posteo','id_usuario_posteo'])
       IS DISTINCT FROM
       (to_jsonb(OLD) - ARRAY['estado','fecha_posteo','id_usuario_posteo']) THEN
        RAISE EXCEPTION 'Posting/cancellation cannot change journal business fields';
    END IF;
    PERFORM 1 FROM accounting.periodo_contable p
    WHERE p.id_periodo_contable = NEW.id_periodo_contable
      AND p.estado = 'Open'
      AND NEW.fecha_asiento BETWEEN p.fecha_inicio AND p.fecha_fin
    FOR SHARE;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Draft journal update requires an Open period containing its date';
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_proteger_asiento
BEFORE INSERT OR UPDATE OR DELETE ON accounting.asiento_contable
FOR EACH ROW EXECUTE FUNCTION accounting.fn_proteger_asiento();

CREATE OR REPLACE FUNCTION accounting.fn_proteger_detalle_asiento()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_id BIGINT;
BEGIN
    v_id := COALESCE(NEW.id_asiento_contable, OLD.id_asiento_contable);
    IF NOT EXISTS (
        SELECT 1 FROM accounting.asiento_contable
        WHERE id_asiento_contable = v_id AND estado = 'Draft'
    ) THEN
        RAISE EXCEPTION 'Journal lines may change only while the entry is Draft';
    END IF;
    IF TG_OP <> 'DELETE' AND NOT EXISTS (
        SELECT 1 FROM accounting.cuenta_contable
        WHERE id_cuenta_contable = NEW.id_cuenta_contable AND activo
    ) THEN
        RAISE EXCEPTION 'Journal lines require an active account';
    END IF;
    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_proteger_detalle_asiento
BEFORE INSERT OR UPDATE OR DELETE ON accounting.detalle_asiento
FOR EACH ROW EXECUTE FUNCTION accounting.fn_proteger_detalle_asiento();

CREATE OR REPLACE FUNCTION accounting.fn_proteger_cuenta_contable()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Accounts cannot be physically deleted';
    END IF;

    IF NEW.id_cuenta_padre = NEW.id_cuenta_contable THEN
        RAISE EXCEPTION 'An account cannot be its own parent';
    END IF;

    IF NEW.id_cuenta_padre IS NOT NULL THEN
        IF NOT EXISTS (
            SELECT 1 FROM accounting.cuenta_contable p
            WHERE p.id_cuenta_contable = NEW.id_cuenta_padre AND p.activo
        ) THEN
            RAISE EXCEPTION 'Parent account must exist and be active';
        END IF;

        IF TG_OP = 'UPDATE' AND EXISTS (
            WITH RECURSIVE descendants AS (
                SELECT c.id_cuenta_contable
                FROM accounting.cuenta_contable c
                WHERE c.id_cuenta_padre = NEW.id_cuenta_contable
                UNION ALL
                SELECT c.id_cuenta_contable
                FROM accounting.cuenta_contable c
                JOIN descendants d ON c.id_cuenta_padre = d.id_cuenta_contable
            )
            SELECT 1 FROM descendants WHERE id_cuenta_contable = NEW.id_cuenta_padre
        ) THEN
            RAISE EXCEPTION 'Account hierarchy cannot contain a cycle';
        END IF;
    END IF;

    IF TG_OP = 'UPDATE' AND EXISTS (
        SELECT 1
        FROM accounting.detalle_asiento d
        JOIN accounting.asiento_contable a ON a.id_asiento_contable = d.id_asiento_contable
        WHERE d.id_cuenta_contable = OLD.id_cuenta_contable AND a.estado = 'Posted'
    ) AND ROW(NEW.codigo, NEW.tipo, NEW.naturaleza, NEW.id_cuenta_padre)
          IS DISTINCT FROM ROW(OLD.codigo, OLD.tipo, OLD.naturaleza, OLD.id_cuenta_padre) THEN
        RAISE EXCEPTION 'Posted use protects account code, type, nature, and parent';
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_proteger_cuenta_contable
BEFORE INSERT OR UPDATE OR DELETE ON accounting.cuenta_contable
FOR EACH ROW EXECUTE FUNCTION accounting.fn_proteger_cuenta_contable();

CREATE OR REPLACE FUNCTION payroll.fn_proteger_periodo_planilla()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'DELETE' OR OLD.estado IN ('Finalized', 'Cancelled') THEN
        RAISE EXCEPTION 'Finalized and Cancelled payroll periods are immutable';
    END IF;
    IF NEW.estado NOT IN ('Draft', 'Finalized', 'Cancelled') THEN
        RAISE EXCEPTION 'Invalid payroll transition';
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_proteger_periodo_planilla
BEFORE UPDATE OR DELETE ON payroll.periodo_planilla
FOR EACH ROW EXECUTE FUNCTION payroll.fn_proteger_periodo_planilla();

CREATE OR REPLACE FUNCTION audit.fn_bloquear_registro_inmutable()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION '% records are immutable', TG_TABLE_NAME;
END;
$$;

CREATE TRIGGER trg_inmutable_libro
BEFORE UPDATE OR DELETE ON accounting.libro_contable
FOR EACH ROW EXECUTE FUNCTION audit.fn_bloquear_registro_inmutable();

CREATE TRIGGER trg_inmutable_comprobante_libro
BEFORE UPDATE OR DELETE ON accounting.comprobante_libro
FOR EACH ROW EXECUTE FUNCTION audit.fn_bloquear_registro_inmutable();

CREATE TRIGGER trg_inmutable_reporte_snapshot
BEFORE UPDATE OR DELETE ON reporting.reporte_snapshot
FOR EACH ROW EXECUTE FUNCTION audit.fn_bloquear_registro_inmutable();

CREATE TRIGGER trg_inmutable_reporte_linea
BEFORE UPDATE OR DELETE ON reporting.reporte_linea
FOR EACH ROW EXECUTE FUNCTION audit.fn_bloquear_registro_inmutable();

CREATE TRIGGER trg_inmutable_audit_log
BEFORE UPDATE OR DELETE ON audit.audit_log
FOR EACH ROW EXECUTE FUNCTION audit.fn_bloquear_registro_inmutable();

COMMIT;
