# Reports Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Data Sources

Reports read from:

- `PLANILLA`
- `DETALLE_PLANILLA`
- `EMPLEADO`
- `DEPARTAMENTO`
- `COMPROBANTE`
- `LIBRO_CONTABLE`
- `TIPO_COMPROBANTE`
- `PERIODO_CONTABLE`
- `CUENTA_CONTABLE`
- `ASIENTO_CONTABLE`
- `DETALLE_ASIENTO`
- `PERIODO_PLANILLA`
- `REPORTE_SNAPSHOT`
- `REPORTE_LINEA`

## Reporting Views

The following report projections are canonical:

- Payroll consolidated view by period and department.
- Voucher totals view by period and operation type.
- IGV payable view by period.
- Income vs expenses view by period.
- Financial KPI view for dashboard.
- Balance sheet view by accounting period from posted ledger entries.
- Income statement view by accounting period from posted ledger entries.

## Data Integrity Requirements

- Report totals must be traceable to persisted payroll, voucher, and ledger records.
- Reports must not mutate source business data.
- Every export snapshot includes report type, period/range, department filter, format, generated timestamp, actor user, actor role, source cutoff, status, totals JSON, and immutable lines.
- Balance sheet and income statement must use posted journal entries only.

## Index Dependencies

Reports depend on these index groups:

- `PERIODO_PLANILLA(periodo, estado)`.
- `PLANILLA(id_periodo_planilla, id_departamento)`.
- `COMPROBANTE(fecha_emision)`.
- `COMPROBANTE(id_tipo_comp, fecha_emision)`.
- `LIBRO_CONTABLE(periodo, id_tipo_libro)`.
- `ASIENTO_CONTABLE(id_periodo_contable, estado)`.
- `DETALLE_ASIENTO(id_cuenta_contable)`.
- `EMPLEADO(id_departamento)`.
- `REPORTE_SNAPSHOT(tipo_reporte, periodo_desde, periodo_hasta, fecha_generacion)`.
- `REPORTE_LINEA(id_reporte_snapshot, orden)`.
