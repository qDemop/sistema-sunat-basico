# Reports Requirements

Source: `Entregable.pdf`, pages 10-11, 13, 21-22, 27, 36-37, 88-92, 105-106.

## Purpose

Provide financial visibility to management through dashboards, reports, KPIs, and exports.

## Actors

- Gerente Financiero.
- Administrador Sistema.

## Functional Requirements

| ID | Requirement |
|---|---|
| REP-FR-001 | The system must show a financial dashboard with KPIs. |
| REP-FR-002 | KPIs must include finalized payroll cash net and total cost, posted income, posted costs/expenses, net utility, margin, IGV payable/credit, accounts receivable, and accounts payable using the canonical account formulas. |
| REP-FR-004 | The system must generate a balance sheet with assets, liabilities, and equity from `CUENTA_CONTABLE`, `ASIENTO_CONTABLE`, and `DETALLE_ASIENTO`. |
| REP-FR-005 | The system must generate an income statement with income, costs, expenses, and net result from posted ledger entries. |
| REP-FR-006 | The system must generate consolidated payroll reports by period. |
| REP-FR-007 | Reports must support period and department filters backed by `DEPARTAMENTO`. |
| REP-FR-008 | Reports must export to PDF and Excel. |
| REP-FR-009 | Visual charts must include income vs expenses and cost distribution where data is available. |
| REP-FR-010 | Financial report values must use Posted journal entries only; consolidated payroll must use Finalized payroll and its department snapshot. |
| REP-FR-011 | Every PDF/Excel export must persist an immutable report snapshot with actor, filters, source cutoff, totals, and lines. |
| REP-FR-012 | Report export history and file retrieval must be available only to Gerente Financiero and Administrador Sistema. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| REP-NFR-001 | Report queries should respond within 500 ms at p95 for standard reads. |
| REP-NFR-002 | Report exports must match values shown in the UI. |
| REP-NFR-003 | Reports must be restricted to Gerente Financiero and Administrador Sistema unless role policy changes. |
| REP-NFR-004 | Financial calculations must be traceable to posted ledger entries and source payroll/accounting records. |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| REP-FR-001..009 | RF-007 (Reportes Financieros) |
| REP-FR-010, 011 | RF-021 (Fuentes de Reportes y Snapshots) |
| REP-FR-012 | RF-022 (Auditoria - export history), RF-023 (Contratos API) |
| REP-NFR-001 | RNF-ELI-01 (API p95 500ms) |
| REP-NFR-002 | RNF-005 (Mantenibilidad - export parity) |
| REP-NFR-003 | RF-022 (role restriction) |
| REP-NFR-004 | RNF-005 (traceability) |

### User Stories and Acceptance Tests

| User story | Workflows | Acceptance test IDs |
|---|---|---|
| US-006 (Financial reports) | WF-048 Balance sheet, WF-049 Income statement, WF-050 Payroll report, WF-051 SUNAT report, WF-052 Export report, WF-053 Permission filtering | WF-AT-048..WF-AT-053 |

Full workflow detail: `tests/traceability/workflow-traceability.md` (Reports and UX Governance table).
