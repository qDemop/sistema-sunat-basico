# Database Normalization Specification

Source: `Entregable.pdf`, pages 78-84.

## Initial Unnormalized Structure

The source document starts from a conceptual unnormalized table named `PLANILLA_GENERAL` that mixes:

- Payroll data.
- Employee data.
- Voucher data.
- User and role data.
- Multivalued payroll concepts such as pension deductions, CTS provision, and gratification provision in one field.

## Problems Identified

- Employee data repeats for every voucher.
- Discount values are stored as multivalued text.
- Entities are not separated.
- Salary changes would require multiple updates.
- Deleting a voucher could accidentally remove employee information.

## First Normal Form

Rules:

- All attributes must be atomic.
- No repeated groups.
- Each row must be unique.

Applied changes:

- Split employee pension deduction, CTS provision, and gratification provision into atomic values with distinct cash/provision semantics.
- Separate employee, voucher, and user data conceptually.
- Use a composite key during transition: `(id_empleado, periodo, id_comprobante)`.

Remaining issue:

- Partial dependencies still exist because employee fields depend only on `id_empleado`, voucher fields depend only on `id_comprobante`, and user fields depend only on `id_usuario`.

## Second Normal Form

Rules:

- Must be in 1NF.
- Non-key attributes must depend on the entire key.

Applied changes:

- Create `EMPLEADO`.
- Create `PLANILLA`.
- Create `COMPROBANTE`.
- Create `USUARIO`.

Remaining issue:

- Transitive dependencies remain, such as a pension regime identifying effective-dated rate versions and `rol_usuario` determining access level.

## Third Normal Form

Rules:

- Must be in 2NF.
- Non-key attributes must depend directly on the primary key.

Applied changes:

- Create `TIPO_DESCUENTO`.
- Update `EMPLEADO` to reference `TIPO_DESCUENTO`.
- Create `CONFIG_DESCUENTO_PREVISIONAL_VERSION` so mutable percentages are effective-dated rather than owned by `TIPO_DESCUENTO`.
- Create `PERIODO_PLANILLA` as the unique period aggregate and update employee `PLANILLA` results to reference it.
- Create `ROL`.
- Update `USUARIO` to reference `ROL`.
- Create `TIPO_COMPROBANTE`.
- Update `COMPROBANTE` to reference `TIPO_COMPROBANTE`.
- Create `LIBRO_CONTABLE`.
- Create `DETALLE_PLANILLA`.
- Store CTS/gratification as employer provision values and the applied pension version in `DETALLE_PLANILLA`; they are not interchangeable discounts.

## BCNF Target

Rule:

- For every non-trivial functional dependency `X -> Y`, `X` must be a superkey.

The final schema is considered BCNF because each non-key attribute depends on the table key and no non-key attribute determines another non-key attribute.

## BCNF Benefits

- Removes redundancy.
- Prevents update, insert, and delete anomalies.
- Enforces referential integrity.
- Improves JOIN efficiency with indexes.
- Supports future modules without redesigning existing tables.
