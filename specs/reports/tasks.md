# Reports Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 3.

## Cross-Module Dependencies

- **Depends on**: Authentication (actor, role restriction), General Ledger (posted entries), Payroll (finalized payroll), Accounting SUNAT (voucher/book totals).
- **Blocks**: none (terminal read-side module).

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| REP-T01 | Domain entities: `ReporteSnapshot`, `ReporteLinea` + report type/DTO contracts | Domain | 1 | - | 3 |
| REP-T02 | `DashboardQuery` + handler: KPI cards (payroll total, income, expenses, IGV payable, utility) from posted entries + finalized payroll | Query | 3 | REP-T01 | 3 |
| REP-T03 | `BalanceSheetQuery` + handler: assets, liabilities, equity from posted ledger accounts | Query | 2 | REP-T01 | 3 |
| REP-T04 | `IncomeStatementQuery` + handler: income, costs, expenses, net result from posted ledger accounts | Query | 2 | REP-T01 | 3 |
| REP-T05 | `ConsolidatedPayrollQuery` + handler: by period/department from finalized payroll | Query | 2 | REP-T01 | 3 |
| REP-T06 | `GenerarReporteSnapshotCommand` + handler: immutable snapshot header + lines, actor/filters/cutoff | Command | 2 | REP-T02, REP-T03, REP-T04, REP-T05 | 3 |
| REP-T07 | Report export service: PDF + Excel from snapshot (layout defined during implementation) | Infra | 2 | REP-T06 | 3 |
| REP-T08 | `ReportsController`: dashboard, balance sheet, income statement, consolidated payroll, export endpoints | API | 1 | REP-T02..REP-T07 | 3 |
| REP-T09 | Integration tests: KPI correctness, balance sheet totals, income statement net result, export parity (snapshot == displayed) | Test | 2 | REP-T08 | 3 |
| REP-T10 | WinForms Reports forms: dashboard with KPI cards + charts, balance sheet, income statement, consolidated payroll | UI | 3 | REP-T08 | 3 |

**Sprint 3 total**: 20 SP. Exit criteria per `docs/implementation-roadmap.md`: reports use posted/finalized sources, export snapshots reproducible, Gerente Financiero restricted.

## Spec Closure (pre-implementation)

- [x] Define ledger entities as source for balance sheet accounts.
- [x] Define ledger entities as source for income statement accounts.
- [x] Confirm Posted-ledger sources and formulas for accounts receivable and payable.
- [x] Define typed KPI and chart data contracts for the WinForms dashboard.
- [~] Define report export layouts for PDF and Excel. → Deferred to REP-T07.
- [x] Define report authorization policy.
- [x] Review report API contract before implementation. (Closed Sprint 0.)
