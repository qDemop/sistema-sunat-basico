# Consistency Report

**Status: HISTORICAL / NON-CANONICAL. Do not use this report to guide implementation; use the canonical sources listed in `docs/README.md`.**

Historical baseline notice: this report records an earlier normalization pass. Its remaining-item list and procedure summaries are superseded by the current canonical specs, `database/schema.md`, `database/functions.md`, `database/procedures.md`, OpenAPI contracts, and the latest remediation report.

## Scope

This review normalized the SDD documentation after gap remediation. No application code, SQL, or .NET project files were generated.

Reviewed artifacts:

- `docs/api-contracts.md`
- `docs/domain-model.md`
- `docs/database-design.md`
- `database/schema.md`
- `database/procedures.md`
- `specs/*`
- `tests/traceability/requirements-traceability.md`
- `tests/acceptance/user-story-acceptance.md`

## Canonical Ownership Rules

| Area | Canonical Source | Rule |
|---|---|---|
| Endpoint ownership | `docs/api-contracts.md` | Each method/path pair is owned by exactly one module. |
| Endpoint detail | `specs/*/api-contract.yaml` | The owning module OpenAPI file contains request/response details. |
| Domain entities and rules | `docs/domain-model.md` | Domain concepts use final approved names and process rules. |
| Database tables and relationships | `database/schema.md` | One canonical table definition per table. |
| Database design rationale | `docs/database-design.md` | Explains relationships, constraints, indexes, and transaction boundaries. |
| Stored procedure behavior | `database/procedures.md` | One canonical process flow per procedure. |
| Module requirements and rules | `specs/<module>/requirements.md` and `business-rules.md` | Module-specific requirements must not redefine another module's ownership. |

## Endpoint Consistency

Result: consistent.

- No duplicate endpoint ownership remains across `specs/*/api-contract.yaml`.
- `docs/api-contracts.md` and module OpenAPI files contain the same method/path set.
- Authentication owns only `/api/auth/login` and `/api/auth/logout`.
- Administration owns user and role management under `/api/admin/*`.
- Tax configuration creation is consistently `POST /api/admin/config/tributaria`.
- Query parameters are not embedded in canonical paths; they are described as parameters.

## Requirement Normalization

| Duplicate Area | Final Approved Requirement |
|---|---|
| Authentication and RBAC | `RF-008` now covers JWT authentication and Sprint 1 role-only RBAC. |
| Voucher movement | `RF-004` now includes the required `Compra`/`Venta` discriminator. |
| SUNAT book versioning | `RF-005` now includes immutable versions and `COMPROBANTE_LIBRO`. |
| Tax configuration | `RF-006` now includes persisted, versioned SUNAT tax configuration. |
| Authentication user management | User and role CRUD requirements remain in Administration, not Authentication. |
| Payroll API exposure | Endpoint definitions remain in API contracts, not duplicated as a payroll requirement. |
| Dashboard KPIs | Report KPI requirements were consolidated into one KPI requirement. |
| Administration tax configuration | Tax configuration fields were consolidated into `ADM-FR-006`. |

## Domain and Table Normalization

| Concept | Final Canonical Name |
|---|---|
| Department | `Departamento` / `DEPARTAMENTO` |
| Overtime | `HorasExtra` / `HORAS_EXTRA` |
| Voucher | `Comprobante` / `COMPROBANTE` |
| Voucher-book bridge | `ComprobanteLibro` / `COMPROBANTE_LIBRO` |
| Tax configuration version | `ConfiguracionTributariaVersion` / `CONFIG_TRIBUTARIA_VERSION` |
| Accounting period | `PeriodoContable` / `PERIODO_CONTABLE` |
| Account | `CuentaContable` / `CUENTA_CONTABLE` |
| Journal entry | `AsientoContable` / `ASIENTO_CONTABLE` |
| Journal entry line | `DetalleAsiento` / `DETALLE_ASIENTO` |

## Business Rule Normalization

| Process | Final Canonical Rule |
|---|---|
| RBAC | Sprint 1 authorization is role-based only; permission catalogs are deferred. |
| Payroll recalculation | Existing payroll can be recalculated only while the owning `PERIODO_PLANILLA.estado = Draft`; employee results have no independent lifecycle. |
| Payroll finalization | Finalized payroll cannot be overwritten; cancellation/adjustment is outside Sprint 1. |
| HorasExtra | Overtime uses `horas_primeras_dos` at 25% surcharge and `horas_posteriores` at 35% surcharge. |
| Comprobante | `tipo_movimiento` is required and must be `Compra` or `Venta`. |
| Tax versioning | Vouchers and books retain the applied `CONFIG_TRIBUTARIA_VERSION`. |
| SUNAT books | Generated books are immutable versions linked through `COMPROBANTE_LIBRO`. |
| Ledger posting | Entries can be posted only when the period is open and debit equals credit. |
| Reports | Balance sheet and income statement read posted ledger entries only. |

## Procedure Normalization

| Procedure | Normalized Result |
|---|---|
| `sp_calcular_planilla(periodo)` | One flow: validate period, reject non-Draft recalculation, calculate overtime and payroll totals, upsert Draft payroll, replace one detail row per payroll. |
| `sp_finalizar_planilla(periodo)` | One flow: validate Draft period, finalize payroll rows, set finalization date, audit. |
| `sp_generar_libro(tipo, periodo)` | One flow: map book type to Compra/Venta, create next immutable version, insert bridge rows, preserve previous versions. |
| `sp_postear_asiento(id_asiento_contable)` | One flow: validate Draft entry, open period, active accounts, balanced totals, post, audit. |

## Remaining Non-Blocking Items

These are not duplicate/conflict issues, but they remain future refinement tasks before implementation:

- Define final SUNAT export layouts for PDF/Excel.
- Define audit retention policy.
- Define backup and restore runbooks.
- Define initial chart of accounts catalog.
- Define source-event mappings from payroll and vouchers to ledger entries.
- Define full shared API error schemas in every OpenAPI file.

## Verification Summary

| Check | Result |
|---|---|
| Duplicate endpoint ownership across specs | Passed. |
| Method/path parity between `docs/api-contracts.md` and OpenAPI specs | Passed. |
| Removed stale Sprint 1 refresh endpoint references | Passed. |
| Removed obsolete extra payroll status | Passed. |
| Removed duplicated post-remediation requirement rows | Passed. |
| Acceptance and traceability specs aligned with final decisions | Passed. |
| Avoided application code, SQL, and .NET project generation | Passed. |
