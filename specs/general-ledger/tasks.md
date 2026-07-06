# General Ledger Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 3.

## Cross-Module Dependencies

- **Depends on**: Authentication (actor, audit), Payroll (finalized payroll -> Draft entry), Accounting SUNAT (registered voucher -> Draft entry).
- **Blocks**: Reports (balance sheet and income statement read posted entries).

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| GL-T01 | Domain entities: `CuentaContable`, `PeriodoContable`, `AsientoContable`, `DetalleAsiento` + status enums + source-event mapping | Domain | 2 | - | 3 |
| GL-T02 | `CuentaContableCommand`/`Query` + handlers: CRUD, hierarchy (parent account), activate/deactivate | Command/Query | 2 | GL-T01 | 3 |
| GL-T03 | `PeriodoContableCommand` + handlers: open, close (reopen out of scope) | Command | 1 | GL-T01 | 3 |
| GL-T04 | `CrearAsientoCommand` + handler: draft entry, balance validation (sum debit = sum credit), source-link optional | Command | 2 | GL-T01, GL-T03 | 3 |
| GL-T05 | `PostearAsientoCommand` + `CancelarAsientoCommand` + `RevertirAsientoCommand` + handlers: lifecycle, audit | Command | 3 | GL-T04 | 3 |
| GL-T06 | `AsientoQuery` + handlers: list by period, detail, balances by account/period | Query | 2 | GL-T05 | 3 |
| GL-T07 | `LedgerController`: accounts, periods, journal entries endpoints | API | 2 | GL-T02..GL-T06 | 3 |
| GL-T08 | FluentValidation: `CuentaContableCommandValidator`, `CrearAsientoCommandValidator` (balance rule) | Validation | 2 | GL-T02, GL-T04 | 3 |
| GL-T09 | Integration tests: account CRUD, period open/close, entry draft/post/cancel/reverse, balance enforcement | Test | 2 | GL-T07 | 3 |
| GL-T10 | WinForms Ledger forms: chart of accounts, periods, journal entries, balances | UI | 3 | GL-T07 | 3 |

**Sprint 3 total**: 19 SP. Exit criteria per `docs/implementation-roadmap.md`: posted entries balance, closed periods block posting, reversal workflow, source-linked Drafts from payroll/voucher.

## Spec Closure (pre-implementation)

- [x] Add `CuentaContable`, `PeriodoContable`, `AsientoContable`, and `DetalleAsiento` to the SDD.
- [x] Define balance requirement for journal posting.
- [x] Define reports as consumers of posted ledger entries.
- [x] Define initial chart of accounts catalog.
- [x] Define mapping from payroll and voucher events to journal entries.
- [x] Define cancellation/adjustment workflow for posted entries.
