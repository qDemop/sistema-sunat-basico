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
