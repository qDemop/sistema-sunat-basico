# General Ledger Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Tables

- `PERIODO_CONTABLE`
- `CUENTA_CONTABLE`
- `ASIENTO_CONTABLE`
- `DETALLE_ASIENTO`

## Relationships

| Relationship | Cardinality |
|---|---|
| PERIODO_CONTABLE to ASIENTO_CONTABLE | 1:N |
| ASIENTO_CONTABLE to DETALLE_ASIENTO | 1:N |
| CUENTA_CONTABLE to DETALLE_ASIENTO | 1:N |
| CUENTA_CONTABLE to CUENTA_CONTABLE | 1:N optional parent/child hierarchy |

## Constraints

- `PERIODO_CONTABLE.codigo` is unique and follows `YYYY-MM`.
- `CUENTA_CONTABLE.codigo` is unique.
- `CUENTA_CONTABLE.tipo` is one of Asset, Liability, Equity, Income, Cost, Expense.
- `ASIENTO_CONTABLE.estado` is one of Draft, Posted, Cancelled.
- `ASIENTO_CONTABLE.fecha_asiento` must be within its `PERIODO_CONTABLE` dates before save/post.
- `ASIENTO_CONTABLE.id_asiento_revertido` optionally references one Posted original; one original can have at most one active reversal chain in the current scope.
- `(ASIENTO_CONTABLE.origen, ASIENTO_CONTABLE.id_origen)` is unique when origin is Planilla or Comprobante.
- `DETALLE_ASIENTO.debe >= 0`.
- `DETALLE_ASIENTO.haber >= 0`.
- Each detail line must have either debit or credit amount, not both.
- Posting requires total debit equal total credit.

## Indexes

- `PERIODO_CONTABLE(codigo)`.
- `CUENTA_CONTABLE(codigo)`.
- `CUENTA_CONTABLE(tipo)`.
- `ASIENTO_CONTABLE(id_periodo_contable, estado)`.
- `ASIENTO_CONTABLE(origen, id_origen)`.
- `ASIENTO_CONTABLE(id_asiento_revertido)`.
- `DETALLE_ASIENTO(id_cuenta_contable)`.
