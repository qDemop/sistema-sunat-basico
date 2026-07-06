# Payroll Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Tables

- `EMPLEADO`
- `TIPO_DESCUENTO`
- `DEPARTAMENTO`
- `HORAS_EXTRA`
- `PERIODO_PLANILLA`
- `PLANILLA`
- `DETALLE_PLANILLA`
- `CONFIG_DESCUENTO_PREVISIONAL_VERSION`

## Relationships

| Relationship | Cardinality |
|---|---|
| TIPO_DESCUENTO to EMPLEADO | 1:N |
| DEPARTAMENTO to EMPLEADO | 1:N |
| EMPLEADO to HORAS_EXTRA | 1:N |
| PERIODO_PLANILLA to PLANILLA | 1:N |
| EMPLEADO to PLANILLA | 1:N |
| PLANILLA to DETALLE_PLANILLA | 1:1 |
| TIPO_DESCUENTO to CONFIG_DESCUENTO_PREVISIONAL_VERSION | 1:N |
| CONFIG_DESCUENTO_PREVISIONAL_VERSION to DETALLE_PLANILLA | 1:N |

## Constraints

- `EMPLEADO.dni` unique.
- `EMPLEADO.salario_base > 0`.
- `EMPLEADO.fecha_ingreso <= current date`.
- `EMPLEADO.id_departamento` references `DEPARTAMENTO`.
- `TIPO_DESCUENTO.nombre` unique.
- `TIPO_DESCUENTO` owns regime identity only; percentage is not stored here.
- `DEPARTAMENTO.nombre` unique.
- `HORAS_EXTRA.periodo` follows `YYYY-MM`.
- `HORAS_EXTRA(id_empleado, periodo)` is unique.
- `HORAS_EXTRA.horas_primeras_dos >= 0`.
- `HORAS_EXTRA.horas_posteriores >= 0`.
- At least one overtime hour value must be greater than 0.
- `PERIODO_PLANILLA.periodo` is unique and follows `YYYY-MM`.
- `PERIODO_PLANILLA.estado` is `Draft`, `Finalized`, or `Cancelled`.
- Existing employee results can be recalculated only when the parent period is `Draft`.
- `PLANILLA(id_periodo_planilla, id_empleado)` is unique and stores `id_departamento` plus positive `salario_base_aplicado` as result snapshots.
- `PLANILLA.total_neto = total_bruto - total_descuentos` as an invariant.
- Active pension configuration versions for the same type cannot overlap.
- `DETALLE_PLANILLA` monetary fields default to 0.

## Indexes

- `EMPLEADO(dni)`.
- `EMPLEADO(activo)`.
- `EMPLEADO(id_departamento)`.
- `EMPLEADO(id_tipo_descuento)`.
- `HORAS_EXTRA(id_empleado, periodo)`.
- `PERIODO_PLANILLA(periodo, estado)`.
- `PLANILLA(id_periodo_planilla, id_empleado)`.
- `DETALLE_PLANILLA(id_planilla)`.

## Procedure Dependency

Payroll calculation depends on `sp_calcular_planilla(periodo, actor_user_id, correlation_id)`; finalization and cancellation use the same period identity and actor/correlation contract.

The procedure must execute atomically and return structured calculation results for the UI and API.
