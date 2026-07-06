# Accounting SUNAT Requirements

Source: `BITACORA -ERP.pdf`, page 2; `Entregable.pdf`, pages 9-11, 12-13, 16, 20-22, 30, 36-37, 91-92, 104-105.

## Purpose

Register accounting vouchers, calculate IGV using persisted tax configuration versions, and generate versioned SUNAT electronic books for purchases and sales.

## Actors

- Contador.
- Administrador Sistema.

## Functional Requirements

| ID | Requirement |
|---|---|
| ACC-FR-001 | The system must register purchase and sales vouchers. |
| ACC-FR-002 | Vouchers must include Compra/Venta movement type, type, series, number, issue date, document type, document number/RUC, business name, taxable base, IGV, total, and operation type. |
| ACC-FR-003 | The system must calculate IGV from the active persisted tax configuration version for the voucher issue date. |
| ACC-FR-004 | The system must validate document format and reject duplicate vouchers. |
| ACC-FR-005 | The system must list vouchers with filters by type, Compra/Venta movement, period/date range, document number/RUC, and business name. |
| ACC-FR-006 | The system must generate Purchase Book and Sales Book by period. |
| ACC-FR-007 | Generated books must use SUNAT fields and group totals by voucher type. |
| ACC-FR-008 | Books must be previewed in the UI and exportable to PDF and Excel. |
| ACC-FR-009 | Tax rates, SUNAT codes, and book formats must be configurable without source-code changes. |
| ACC-FR-010 | Purchase books must include only `Compra` vouchers and Sales books must include only `Venta` vouchers. |
| ACC-FR-011 | SUNAT books must be versioned and linked to vouchers through `COMPROBANTE_LIBRO`. |
| ACC-FR-012 | Voucher records must persist the applied tax configuration version. |
| ACC-FR-013 | Voucher persisted state must be Registrado or Anulado; SUNAT validation is a separate computed result. |
| ACC-FR-014 | A voucher linked to any generated book version must be immutable except for logical annulment; a later book version reflects the annulment. |
| ACC-FR-015 | Book validation must return Valida, ConObservaciones, or Bloqueada before generation. |
| ACC-FR-016 | Book generation must be a POST command that creates the next immutable version and records generator, format version, and voucher tax-version snapshots. |
| ACC-FR-017 | Registered vouchers must create one source-linked Draft journal entry using the approved Compra/Venta mapping. |
| ACC-FR-018 | Credit/debit notes must reference an original same-movement voucher and use the documented reduction/increase ledger mapping. |
| ACC-FR-019 | Book generation must consume the governed eleven-token format and snapshot one locked eligible voucher set with all row fields. |
| ACC-FR-020 | `GET /api/comprobantes/dashboard` must provide Contador/Admin operational voucher, IGV, period and current-book KPIs distinct from Posted-ledger reporting. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| ACC-NFR-001 | Generating books for 1,000 vouchers must complete in 60 seconds or less. |
| ACC-NFR-002 | SUNAT book generation must be auditable by user, period, type, version, totals, and result. |
| ACC-NFR-003 | Voucher operations must preserve referential integrity and prevent inconsistent totals. |
| ACC-NFR-004 | Accounting APIs must require JWT and role Contador or Administrador Sistema. |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| ACC-FR-001..004, 012, 013, 017, 018 | RF-004 (Registro de Comprobantes), RF-019 (Contabilizacion) |
| ACC-FR-005 | RF-023 (Contratos API - voucher list filters) |
| ACC-FR-006..008, 010, 011, 014, 015, 016, 019 | RF-005 (Libro de Compras y Ventas), RF-020 (Validacion y Formato SUNAT) |
| ACC-FR-003, 009 | RF-006 (Normativa Tributaria SUNAT) |
| ACC-FR-020 | RF-023 (Contratos API - dashboard) |
| ACC-NFR-001 | RNF-001 (Rendimiento 60s/1000 vouchers) |
| ACC-NFR-002 | RF-022 (Auditoria) |
| ACC-NFR-004 | RNF-ELI-02 (JWT required) |

### User Stories and Acceptance Tests

| User story | Workflows | Acceptance test IDs |
|---|---|---|
| US-004 (Voucher registration) | WF-025 Register, WF-026 Update, WF-027 Validate, WF-028 Annul, WF-035 Voucher-to-GL | WF-AT-025..WF-AT-028, WF-AT-035 |
| US-005 (Book generation) | WF-029 Purchase Book, WF-030 Sales Book, WF-031 Replace version, WF-032 Immutability, WF-033 Included voucher changed, WF-034 Format versioning | WF-AT-029..WF-AT-034 |

Full workflow detail: `tests/traceability/workflow-traceability.md` (Accounting SUNAT table).
