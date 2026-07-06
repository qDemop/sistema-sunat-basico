# Interaction Patterns

## Pattern Philosophy

ERP.WinForms interactions must feel immediate, reversible when safe, explicit when risky, and predictable across modules. The interface should reward experienced users with speed while still being understandable to new staff.

Canonical shortcuts and repeated-entry flows are defined in `keyboard-shortcuts.md`. All visible commands use Peruvian Spanish.

## Search-First Lists

Primary record lists open with keyboard focus or visual priority on search. Users should be able to type a DNI, RUC, name, series, period, or username before using filters.

Rules:

- Search accepts partial terms.
- Search results update quickly without losing visible context.
- Active search terms remain visible.
- Clearing search restores the prior list state.
- Filters refine search rather than replace it.

## Command Areas

Each workspace has a command area for task actions. Command labels must use verbs and business objects:

- Nuevo empleado.
- Nuevo comprobante.
- Calcular planilla.
- Generar libro de ventas.
- Exportar PDF.
- Restablecer contrasena.

Rules:

- One primary action per active task state.
- Secondary actions are visually grouped.
- Disabled actions explain why they are unavailable.
- Risky actions are separated from routine actions.

## Keyboard Interaction

Frequent workflows must support keyboard-first operation.

Expected conventions:

- Tab order follows the business flow.
- Enter confirms the current safe primary action when context is unambiguous.
- Escape cancels transient surfaces or returns focus without saving.
- Standard copy, paste, select-all, and undo behavior is respected where text entry exists.
- Search, save, refresh, export, and new-record shortcuts are consistent across modules.
- The canonical mappings are `Ctrl+K`, `Ctrl+F`, `Ctrl+N`, `Ctrl+S`, `Ctrl+Mayus+S`, `F5`, and `Ctrl+Mayus+E` as defined in `keyboard-shortcuts.md`.

Shortcut labels should be discoverable in menus, tooltips, or command hints. Do not require memorization to complete basic tasks.

## Selection and Detail

Lists should support:

- Single record selection for edit, view, or deactivate.
- Multi-selection only where batch actions are intentionally supported.
- A clear selected state.
- Detail preview when it reduces navigation.
- Full detail workspace for complex records.

Selection never triggers destructive actions. Double activation opens record detail and never executes a mutation.

## Progressive Disclosure

Use progressive disclosure for:

- Advanced filters.
- Audit history.
- Calculation breakdowns.
- Tax version details.
- SUNAT column mapping details.
- Backup technical details.
- Role impact explanations.

Do not hide:

- Primary actions.
- Required fields.
- Current period.
- Validation errors.
- Finalization or generated status.
- Financial totals.

## Inline Validation

Validation appears as close as possible to the field or data area that caused it.

Validation must include:

- What is wrong.
- How to correct it.
- Whether the user can continue.

Examples:

- "DNI must have 8 digits."
- "RUC must have 11 digits."
- "Salary must be greater than S/ 0.00."
- "This payroll period is finalized and cannot be recalculated."
- "A voucher with this type, movement, series, and number already exists."

## Confirmation Pattern

Confirmation is not a substitute for reversibility. Actions are classified before choosing feedback:

| Class | Pattern | Examples |
|---|---|---|
| Reversible | Apply immediately, confirm result, and offer restore/undo when allowed. | Deactivate employee, deactivate user. |
| Consequential but editable | Show impact summary in context, then proceed. | Recalculate draft payroll, update future configuration. |
| Irreversible or legally significant | Require explicit review and confirmation. | Finalize payroll, post journal entry, generate a new SUNAT version, annul voucher. |
| Privileged security action | Confirm identity and consequence. | Reset password, change role. |

Explicit confirmations are required for actions that:

- Finalize payroll.
- Annul a voucher.
- Generate a new SUNAT book version when one already exists.
- Activate a new tax configuration version.
- Reset a password.

Confirmation content must state:

- The action.
- The object affected.
- The consequence.
- The safe alternative, if any.

## Feedback Pattern

Every user action must produce visible feedback:

| Action Type | Feedback |
|---|---|
| Save | Saved state and refreshed record identity. |
| Calculate | Progress, result summary, totals, and status. |
| Generate | Version, included count, totals, and export readiness. |
| Export | Export started, completed, failed, or unavailable. |
| Delete/deactivate | Status change and list refresh. |
| Filter/search | Count and active filter summary. |

## Long-Running Operations

Payroll calculation, SUNAT book generation, report refresh, and export may take noticeable time.

Rules:

- Show progress or at least a clear working state.
- Keep period and operation name visible.
- Prevent duplicate submission.
- Preserve user context after completion.
- On failure, show what was not completed and what the user can retry.

## Data Entry Acceleration

High-volume entry screens should support:

- Retaining common period/date context where appropriate.
- Keyboard entry through fields in voucher or employee order.
- Immediate calculated totals.
- Repeat action after save when the task is naturally repetitive.
- Clear separation between save-and-stay and save-and-return behavior if both are offered.
- `Guardar y crear otro` is available only for repeated voucher entry in the current scope.
- Only safe defaults are retained; identifiers, third parties, amounts, bank data, and free text are cleared.
- Bulk entry is the preferred pattern for overtime by period; row errors do not erase valid rows.
- Persisted saved views are `IMPLEMENTATION PENDING`; no current per-user view state is claimed.

## Concurrent and Uncertain Outcomes

- If another user changed a record, show the newer version and let the user compare, reload, or reapply safe changes.
- If an operation times out with an unknown result, do not invite immediate repetition. Show `Verificar estado` first.
- Partial bulk success reports successful and rejected rows separately.
- Retrying must preserve context and make clear whether it creates a new version or resumes the same operation.

## Drill-Down Pattern

KPIs and totals should support drill-down when the user needs evidence:

```text
KPI -> filtered list/report -> selected record -> detail/history/export
```

The drilled view must show the inherited filter context.

## Error Recovery Pattern

When an action fails:

- Keep user-entered data.
- Preserve search and filters.
- Identify the failed action.
- Provide retry when safe.
- Provide a route back to a stable screen.
- Avoid exposing technical messages as primary UI text.
