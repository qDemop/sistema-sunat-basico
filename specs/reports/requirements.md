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
