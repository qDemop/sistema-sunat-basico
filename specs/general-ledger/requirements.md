# General Ledger Requirements

## Purpose

Provide the accounting ledger foundation required for balance sheet, income statement, accounts receivable/payable, and financial KPIs.

## Actors

- Contador.
- Gerente Financiero.
- Administrador Sistema.

## Functional Requirements

| ID | Requirement |
|---|---|
| GL-FR-001 | The system must maintain accounting periods through `PERIODO_CONTABLE`. |
| GL-FR-002 | The system must maintain a chart of accounts through `CUENTA_CONTABLE`. |
| GL-FR-003 | The system must create accounting journal entries through `ASIENTO_CONTABLE`. |
| GL-FR-004 | The system must store debit and credit lines through `DETALLE_ASIENTO`. |
| GL-FR-005 | The system must reject posting of unbalanced journal entries. |
| GL-FR-006 | Posted entries must be immutable except through cancellation/adjustment workflow. |
| GL-FR-007 | Financial reports must use posted ledger entries as source for balance sheet and income statement. |
| GL-FR-008 | Contador and Administrador Sistema must be able to update and deactivate/reactivate accounts subject to usage restrictions; Gerente Financiero has read-only queries. |
| GL-FR-009 | Open periods must support an explicit close command; reopening a Closed period is out of scope. |
| GL-FR-010 | Draft entries must support edit and cancellation; Posted entries must support correction only by a linked reversal Draft in an Open period. |
| GL-FR-011 | Entry date must fall within the selected accounting period. |
| GL-FR-012 | Finalized payroll and registered vouchers must create one deterministic source-linked Draft entry using the approved initial chart and mappings. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| GL-NFR-001 | Ledger posting must be auditable by user, date, source module, source entity, and result. |
| GL-NFR-002 | Ledger queries must support report generation by accounting period. |
| GL-NFR-003 | Ledger records must preserve at least 5 years of operational history. |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| GL-FR-001..011 | RF-015 (Libro Mayor Contable) |
| GL-FR-012 | RF-019 (Contabilizacion de Origenes) |
| GL-FR-007 | RF-021 (Fuentes de Reportes) - cross-ref with Reports |
| GL-NFR-001 | RF-022 (Auditoria) |
| GL-NFR-002 | RF-023 (Contratos API - ledger queries) |
| GL-NFR-003 | RNF-EDI-04 (5 years history) |

### User Stories and Acceptance Tests

No dedicated user story; ledger workflows support US-003 (payroll finalize -> Draft entry) and US-004/US-005 (voucher registration -> Draft entry).

| Workflows | Acceptance test IDs |
|---|---|
| WF-036 Create account, WF-037 Update account, WF-038 Account deactivate/reactivate | WF-AT-036, WF-AT-037, WF-AT-038 |
| WF-039 Create period, WF-040 Close period, WF-041 Reopen (OUT OF SCOPE) | WF-AT-039, WF-AT-040, WF-AT-041 |
| WF-042 Create journal, WF-043 Post, WF-044 Cancel Draft, WF-045 Reverse Posted, WF-046 Balance validation, WF-047 Date/period validation | WF-AT-042..WF-AT-047 |

Full workflow detail: `tests/traceability/workflow-traceability.md` (General Ledger table).
