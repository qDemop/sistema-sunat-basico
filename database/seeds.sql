BEGIN;

INSERT INTO "identity".rol (nombre, descripcion, nivel_acceso, activo)
VALUES
    ('Administrador RRHH', 'Gestion de empleados, departamentos, horas extra y planillas', 2, TRUE),
    ('Contador', 'Gestion contable, comprobantes, libros SUNAT y libro mayor', 3, TRUE),
    ('Gerente Financiero', 'Reportes y consultas de libro mayor en modo lectura', 1, TRUE),
    ('Administrador Sistema', 'Administracion total dentro del alcance funcional', 5, TRUE)
ON CONFLICT (nombre) DO UPDATE
SET descripcion = EXCLUDED.descripcion,
    nivel_acceso = EXCLUDED.nivel_acceso,
    activo = EXCLUDED.activo;

INSERT INTO payroll.tipo_descuento (nombre, descripcion, activo)
VALUES
    ('AFP', 'Sistema Privado de Pensiones', TRUE),
    ('ONP', 'Sistema Nacional de Pensiones', TRUE)
ON CONFLICT (nombre) DO UPDATE
SET descripcion = EXCLUDED.descripcion,
    activo = EXCLUDED.activo;

INSERT INTO payroll.config_descuento_previsional_version(
    id_tipo, version, porcentaje, fecha_inicio, fecha_fin, estado
)
SELECT id_tipo, 1,
       CASE nombre WHEN 'AFP' THEN 10.00 ELSE 13.00 END,
       DATE '2026-01-01', NULL, 'Active'
FROM payroll.tipo_descuento
WHERE nombre IN ('AFP', 'ONP')
ON CONFLICT (id_tipo, version) DO NOTHING;

INSERT INTO payroll.departamento (nombre, descripcion, activo)
VALUES
    ('Recursos Humanos', 'Gestion de personal y planillas', TRUE),
    ('Contabilidad', 'Gestion contable y SUNAT', TRUE),
    ('Gerencia', 'Direccion financiera y reportes', TRUE),
    ('Tecnologia', 'Soporte tecnologico y administracion del sistema', TRUE)
ON CONFLICT (nombre) DO UPDATE
SET descripcion = EXCLUDED.descripcion, activo = EXCLUDED.activo;

INSERT INTO admin.config_tributaria_version(
    codigo, descripcion, version, tasa_igv, fecha_inicio, fecha_fin, estado
)
VALUES ('IGV', 'Impuesto General a las Ventas', 1, 18.00, DATE '2026-01-01', NULL, 'Active')
ON CONFLICT (codigo, version) DO NOTHING;

INSERT INTO accounting.tipo_comprobante (nombre, codigo_sunat, afecto_igv, activo)
VALUES
    ('Factura', '01', TRUE, TRUE),
    ('Recibo por Honorarios', '02', FALSE, TRUE),
    ('Boleta', '03', TRUE, TRUE),
    ('Nota de Credito', '07', TRUE, TRUE),
    ('Nota de Debito', '08', TRUE, TRUE),
    ('Guia de Remision', '09', FALSE, TRUE)
ON CONFLICT (codigo_sunat) DO UPDATE
SET nombre=EXCLUDED.nombre, afecto_igv=EXCLUDED.afecto_igv, activo=EXCLUDED.activo;

INSERT INTO accounting.tipo_libro (nombre, activo)
VALUES ('Compras', TRUE), ('Ventas', TRUE)
ON CONFLICT (nombre) DO UPDATE SET activo=EXCLUDED.activo;

INSERT INTO admin.config_sunat_formato(
    tipo_libro, version_formato, estructura_json, fecha_inicio, fecha_fin, estado
)
VALUES
    ('Compras', '2026.1', '{"columns":["periodo","tipoDocumento","numeroDocumento","tipoComprobante","serie","numero","fechaEmision","baseImponible","baseExonerada","igv","total"]}'::JSONB, DATE '2026-01-01', NULL, 'Active'),
    ('Ventas', '2026.1', '{"columns":["periodo","tipoDocumento","numeroDocumento","tipoComprobante","serie","numero","fechaEmision","baseImponible","baseExonerada","igv","total"]}'::JSONB, DATE '2026-01-01', NULL, 'Active')
ON CONFLICT (tipo_libro, version_formato) DO NOTHING;

INSERT INTO accounting.cuenta_contable(codigo,nombre,tipo,naturaleza,activo)
VALUES
    ('1011','Caja','Asset','Debit',TRUE),
    ('1212','Facturas por cobrar','Asset','Debit',TRUE),
    ('40111','IGV por pagar','Liability','Credit',TRUE),
    ('40114','IGV credito fiscal','Asset','Debit',TRUE),
    ('4032','ONP por pagar','Liability','Credit',TRUE),
    ('4071','AFP por pagar','Liability','Credit',TRUE),
    ('4111','Remuneraciones por pagar','Liability','Credit',TRUE),
    ('4151','Beneficios sociales por pagar','Liability','Credit',TRUE),
    ('4212','Facturas por pagar','Liability','Credit',TRUE),
    ('6011','Compras','Expense','Debit',TRUE),
    ('6211','Sueldos y horas extra','Expense','Debit',TRUE),
    ('6292','Provision de gratificaciones','Expense','Debit',TRUE),
    ('6293','Provision de CTS','Expense','Debit',TRUE),
    ('7011','Ventas','Income','Credit',TRUE)
ON CONFLICT (codigo) DO UPDATE
SET nombre=EXCLUDED.nombre, tipo=EXCLUDED.tipo,
    naturaleza=EXCLUDED.naturaleza, activo=EXCLUDED.activo;

INSERT INTO accounting.periodo_contable(codigo,fecha_inicio,fecha_fin,estado)
VALUES ('2026-01',DATE '2026-01-01',DATE '2026-01-31','Open')
ON CONFLICT (codigo) DO NOTHING;

COMMIT;
