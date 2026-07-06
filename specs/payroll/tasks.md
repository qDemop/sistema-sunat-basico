# Payroll Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 1.

## Cross-Module Dependencies

- **Depends on**: Authentication (actor, audit).
- **Blocks**: General Ledger (finalized payroll creates source-linked Draft entry), Reports (payroll totals, consolidated payroll).

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| PAY-T01 | Domain entities: `Departamento`, `Empleado`, `TipoDescuento`, `ConfigDescuentoPrevisionalVersion`, `HorasExtra`, `PeriodoPlanilla`, `Planilla`, `DetallePlanilla` + status enums | Domain | 3 | - | 1 |
| PAY-T02 | `DepartamentoCommand`/`Query` + handlers: CRUD, list, deactivate, reactivate | Command/Query | 2 | PAY-T01 | 1 |
| PAY-T03 | `EmpleadoCommand`/`Query` + handlers: CRUD, search, filter by department, logical deactivate, DNI uniqueness | Command/Query | 3 | PAY-T01, PAY-T02 | 1 |
| PAY-T04 | `HorasExtraCommand`/`Query` + handlers: register, approve, cancel by employee/period | Command/Query | 2 | PAY-T01, PAY-T03 | 1 |
| PAY-T05 | `CalcularPlanillaCommand` + handler: calls `sp_calcular_planilla(periodo, actor, correlation)`; Draft-only guard; resolves effective pension version | Command | 3 | PAY-T01, PAY-T04 | 1 |
| PAY-T06 | `FinalizarPlanillaCommand` + `CancelarPlanillaCommand` + handlers: terminal-state transitions, create ledger Draft on finalize | Command | 2 | PAY-T05 | 1 |
| PAY-T07 | `PlanillaQuery` + handlers: period aggregate, employee results, totals | Query | 2 | PAY-T05 | 1 |
| PAY-T08 | `PayrollController`: departments, employees, overtime, payroll lifecycle endpoints | API | 2 | PAY-T02..PAY-T07 | 1 |
| PAY-T09 | FluentValidation: `EmpleadoCommandValidator`, `HorasExtraCommandValidator`, `CalcularPlanillaCommandValidator` | Validation | 2 | PAY-T03, PAY-T04, PAY-T05 | 1 |
| PAY-T10 | Payslip PDF export service (layout fields defined during implementation) | Infra | 2 | PAY-T07 | 1 |
| PAY-T11 | Payroll Excel export service (columns and totals defined during implementation) | Infra | 1 | PAY-T07 | 1 |
| PAY-T12 | Integration tests: employee CRUD, DNI unique, overtime approve/cancel, payroll calc (100 emp perf), finalize blocks recalc | Test | 3 | PAY-T08 | 1 |
| PAY-T13 | WinForms Payroll forms: departments, employees, overtime, payroll results | UI | 3 | PAY-T08 | 1 |

**Sprint 1 total**: 28 SP. Exit criteria per `docs/implementation-roadmap.md`: Draft-only recalc, overtime before calculation, 100 employees in 30s, department filter.

## Spec Closure (pre-implementation)

- [x] Confirm overtime input model as `HORAS_EXTRA` by employee and period.
- [x] Define payroll recalculation policy as Draft-only.
- [x] Add `DEPARTAMENTO` for employee assignment and report filtering.
- [~] Define payslip PDF layout fields. → Deferred to PAY-T10 (layout defined during implementation).
- [~] Define Excel export columns and totals. → Deferred to PAY-T11.
- [x] Define audit events for employee changes and payroll lifecycle commands.
- [x] Validate payroll formulas with documented sample cases, including cash/provision separation and effective pension versions.
- [x] Review payroll API contract before implementation. (Closed Sprint 0.)
