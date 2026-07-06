# Gap Analysis

**Status: HISTORICAL / NON-CANONICAL. Do not use this report to guide implementation; use the canonical sources listed in `docs/README.md`.**

Historical baseline notice: this pre-P0 gap list is retained for provenance only. Its “remaining” rows and readiness wording are not current findings. Current decisions and evidence are owned by `docs/p0-decisions.md`, canonical module specs/SQL/OpenAPI, `tests/traceability/workflow-traceability.md`, and the latest remediation report.

## Review Status

This document reflects the earlier SDD state at the time of its original gap remediation; it is not a current certification artifact.

No application code, SQL, or .NET project files were generated.

## Resolved Blocking and High Severity Gaps

| Gap | Final Resolution |
|---|---|
| Financial reporting data model was insufficient. | General Ledger is canonical through `CUENTA_CONTABLE`, `PERIODO_CONTABLE`, `ASIENTO_CONTABLE`, and `DETALLE_ASIENTO`. |
| Purchase vs sales voucher classification was missing. | `COMPROBANTE.tipo_movimiento` is required with values `Compra` and `Venta`. |
| Tax configuration was not connected end to end. | `CONFIG_TRIBUTARIA_VERSION` is persisted, versioned, effective-dated, and referenced by vouchers/books. |
| User and role APIs were duplicated. | Authentication owns login/logout only; Administration owns user and role management. |
| RBAC model was inconsistent. | Sprint 1 uses role-based RBAC only; permission catalogs are deferred. |
| Department filter was not modeled. | `DEPARTAMENTO` is canonical and required for employee/report filtering. |
| Overtime input model was missing. | `HORAS_EXTRA` is canonical with `horas_primeras_dos` and `horas_posteriores`. |
| Payroll recalculation policy was unresolved. | Recalculation is allowed only while the owning `PERIODO_PLANILLA.estado = Draft`; employee `PLANILLA` rows have no independent lifecycle. |
| SUNAT book versioning relationships conflicted. | `LIBRO_CONTABLE` versions link vouchers through `COMPROBANTE_LIBRO`. |

## Remaining Non-Blocking Gaps

| Gap | Severity | Required Before |
|---|---|---|
| Shared OpenAPI error schemas are still high level. | Medium | Implementation handoff. |
| Audit retention policy is not fully specified. | Medium | Administration implementation. |
| Backup and restore runbooks are not fully specified. | Medium | Operations implementation. |
| Initial chart of accounts catalog is not defined. | Medium | General Ledger implementation. |
| Payroll/voucher to ledger mapping rules are not fully specified. | Medium | Automated ledger posting. |
| SUNAT export layouts need final column confirmation. | Low | Export implementation. |

## Final Readiness Assessment

| Area | Status |
|---|---|
| Requirements | Blocking/high duplicates resolved and final decisions folded into canonical requirements. |
| User stories | Main business flows remain valid; acceptance tests should use the final tax, ledger, and versioning rules. |
| Domain model | Canonical entities and process rules are aligned with approved decisions. |
| Database design | Canonical tables, relationships, constraints, and procedure dependencies are aligned. |
| API contracts | Endpoint ownership is unique and consistent between registry and module OpenAPI files. |
| Module specifications | Duplicated rules and requirements introduced during remediation were consolidated. |

## Cross-Reference

See `docs/consistency-report.md` for the detailed consistency cleanup summary and verification results.
