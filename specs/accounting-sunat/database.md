# Accounting SUNAT Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Tables

- `COMPROBANTE`
- `TIPO_COMPROBANTE`
- `LIBRO_CONTABLE`
- `TIPO_LIBRO`
- `COMPROBANTE_LIBRO`
- `CONFIG_TRIBUTARIA_VERSION`
- `CONFIG_SUNAT_FORMATO`

## Relationships

| Relationship | Cardinality |
|---|---|
| TIPO_COMPROBANTE to COMPROBANTE | 1:N |
| CONFIG_TRIBUTARIA_VERSION to COMPROBANTE | 1:N |
| TIPO_LIBRO to LIBRO_CONTABLE | 1:N |
| LIBRO_CONTABLE to COMPROBANTE_LIBRO | 1:N |
| COMPROBANTE to COMPROBANTE_LIBRO | 1:N |
| CONFIG_SUNAT_FORMATO to LIBRO_CONTABLE | 1:N |
| CONFIG_TRIBUTARIA_VERSION to COMPROBANTE_LIBRO | 1:N snapshot |
| COMPROBANTE original to credit/debit notes | 1:N self-reference |

## Constraints

- `COMPROBANTE.id_tipo_comp` is required.
- `COMPROBANTE.tipo_movimiento` is required and must be `Compra` or `Venta`.
- `COMPROBANTE.serie` and `COMPROBANTE.numero` are required.
- Voucher duplicate prevention must include type, movement, series, and number.
- `COMPROBANTE.fecha_emision` is required.
- `COMPROBANTE.tipo_documento` and `COMPROBANTE.numero_documento` are required.
- RUC document numbers must be 11 digits.
- DNI document numbers must be 8 digits.
- `COMPROBANTE.base_imponible >= 0`.
- `COMPROBANTE.igv >= 0`.
- `COMPROBANTE.base_imponible > 0 OR base_exonerada > 0`; derived `total > 0`.
- `COMPROBANTE.id_config_tributaria_version` references the applied tax configuration version.
- SUNAT codes 07/08 require `id_comprobante_referencia`; the reference must be Registrado and use the same movement. Other types prohibit it.
- `COMPROBANTE.estado` is Registrado or Anulado.
- `TIPO_COMPROBANTE.codigo_sunat` is required.
- `LIBRO_CONTABLE.periodo` follows `YYYY-MM`.
- `LIBRO_CONTABLE.version` is unique by type and period.
- `COMPROBANTE_LIBRO(id_libro, id_comprobante)` is unique.
- `LIBRO_CONTABLE.id_config_sunat_formato` and `LIBRO_CONTABLE.id_usuario_generador` are required.
- `COMPROBANTE_LIBRO.id_config_tributaria_version` is required and snapshots the voucher tax version.
- `COMPROBANTE_LIBRO` snapshots all eleven governed SUNAT row fields from one locked eligible set.
- Tax configuration versions for the same code must not overlap by effective date range.
- Active/Closed tax and format versions are immutable; only audited transition procedures can activate/close them.

## Indexes

- `COMPROBANTE(tipo_documento, numero_documento)`.
- `COMPROBANTE(fecha_emision)`.
- `COMPROBANTE(tipo_movimiento, fecha_emision)`.
- `COMPROBANTE(id_tipo_comp, fecha_emision)`.
- Unique `COMPROBANTE(id_tipo_comp, tipo_movimiento, serie, numero)`.
- `LIBRO_CONTABLE(periodo, id_tipo_libro, version)`.
- `COMPROBANTE_LIBRO(id_libro)`.
- `COMPROBANTE_LIBRO(id_comprobante)`.
- `CONFIG_TRIBUTARIA_VERSION(codigo, version)`.

## Procedure Dependency

SUNAT book generation depends on `sp_generar_libro(tipo_libro, periodo, aceptar_observaciones, actor_user_id, correlation_id)` or an equivalent application service with the same transactional and output guarantees.

The procedure must create a new `LIBRO_CONTABLE.version` for the selected type/period and link included vouchers through `COMPROBANTE_LIBRO`. It captures eligible voucher IDs once after serialization and uses only that captured, row-locked set for header totals and every bridge snapshot; later concurrent inserts are eligible only for a later version.
