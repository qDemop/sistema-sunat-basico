# Empty, Error, and Loading States

All user-visible state messages use Peruvian Spanish and name the affected object, period, and consequence when known.

## State Design Objective

States must preserve user confidence. Empty states guide the next useful action, loading states show progress without erasing context, and error states explain recovery without exposing technical internals as the main message.

## Payroll Lifecycle

The persisted aggregate is one `PeriodoPlanilla` per `YYYY-MM`. Its state, finalizing actor and finalization timestamp govern every employee result in that period; individual result rows never own an independent lifecycle.

### Payroll States

| State | Persisted | Meaning | Editable | Dominant Action |
|---|---:|---|---:|---|
| Sin calcular | No | No payroll record exists for the selected period. | N/A | `Calcular planilla`. |
| Borrador (`Draft`) | Yes | Calculation exists and may be reviewed, exported or recalculated. | Yes, through recalculation only. | `Recalcular planilla`, `Exportar`, or `Finalizar planilla`. |
| Finalizada (`Finalized`) | Yes | Payroll is immutable and available for payslips/exports. | No | `Exportar` or view detail. |
| Cancelada (`Cancelled`) | Yes | An abandoned Draft is retained for audit and excluded from normal active work. | No | View detail/audit; export is unavailable. |

### Allowed Payroll Transitions

| From | Action | To | Authorized Roles | Preconditions |
|---|---|---|---|---|
| Sin calcular | Calcular | Borrador | Administrador RRHH, Administrador Sistema | Valid period, active employees, valid employee data, approved overtime only. |
| Borrador | Recalcular | Borrador | Administrador RRHH, Administrador Sistema | Existing state is Draft; operation completes atomically. |
| Borrador | Finalizar | Finalizada | Administrador RRHH, Administrador Sistema | Successful calculation, no blocking readiness errors, reviewed employee count and totals. |
| Borrador | Cancelar borrador | Cancelada | Administrador RRHH, Administrador Sistema | Explicit confirmation; no finalized output. |

### Invalid Payroll Transitions

- Finalizada -> Borrador is prohibited.
- Finalizada -> Finalizada through recalculation is prohibited.
- Finalizada -> Cancelada is outside the current functional scope and must not be displayed.
- Cancelada -> Borrador or Finalizada is prohibited.
- Finalization without a successful Draft calculation is prohibited.
- Recalculation while another calculation/finalization has an unknown outcome is prohibited until `Verificar estado` resolves it.

### Correction After Finalization

The current business scope does not include cancellation, adjustment, replacement, or reopening of a finalized payroll. The implementation-ready UX behavior is therefore explicit:

1. Keep the finalized payroll read-only and preserve all totals, details, exports, user, and timestamp.
2. Show `La planilla finalizada no puede reabrirse ni sobrescribirse`.
3. Allow review of employee detail, export evidence, and audit history.
4. Do not show Reabrir, Recalcular, Editar, Cancelar, or Crear reemplazo.
5. Direct the user to the documented future cancellation/adjustment process outside the current release; developers must not create an alternative correction behavior.

### Overtime States Used by Payroll

| State | Allowed Actions | Payroll Effect |
|---|---|---|
| Borrador (`Draft`) | Edit, Approve, Cancel by Administrador RRHH or Administrador Sistema. | Excluded from calculation. |
| Aprobada (`Approved`) | View; cancel only before a payroll Draft exists for the period. | Included in calculation. |
| Cancelada (`Cancelled`) | View only. | Excluded. |

## Accounting Lifecycle

### Journal Entry States

| State | Meaning | Editable | Included in Reports |
|---|---|---:|---:|
| Borrador (`Draft`) | Entry header and lines are under preparation. | Yes, Contador/Admin. | No. |
| Contabilizado (`Posted`) | Balanced entry was posted in an Open period. | No. | Yes. |
| Cancelado (`Cancelled`) | Draft was cancelled and retained for audit. | No. | No. |

`Revertido` is a derived indicator on a Posted entry when a linked posted adjustment reverses it; it is not a fourth persisted state.

### Journal Entry Transitions

| From | Action | To | Authorized Roles | Preconditions |
|---|---|---|---|---|
| New | Save | Borrador | Contador, Administrador Sistema | Open period, valid header, at least two valid lines. |
| Borrador | Edit/Save | Borrador | Contador, Administrador Sistema | Period remains Open. |
| Borrador | Contabilizar | Contabilizado | Contador, Administrador Sistema | Open period; debit equals credit; active accounts; no invalid lines. |
| Borrador | Cancelar | Cancelado | Contador, Administrador Sistema | Explicit confirmation. |
| Contabilizado | Revertir | New reversal Borrador | Contador, Administrador Sistema | Select an Open period; original remains immutable. |

### Posting Workflow

1. Show period, date, source, line count, debit, credit, and difference.
2. Keep `Contabilizar asiento` unavailable until difference is S/ 0.00 and all accounts are active.
3. On confirmation, post atomically and show user/timestamp.
4. If outcome is unknown, block retry and offer `Verificar estado`.

### Cancellation and Reversal

- Cancel applies only to Draft and produces Cancelled; Cancelled is terminal.
- Posted entries are never cancelled, edited, or deleted.
- Reverse creates a new Draft with `Origen = Ajuste`, source reference to the original, and debit/credit values inverted.
- The reversal may be posted only after normal validation. When posted, both entries remain visible and linked.
- If the original period is Closed, create the reversal in a selected Open period; never reopen the original period.

### Accounting Period States

| State | Allowed Actions | Restrictions |
|---|---|---|
| Abierto (`Open`) | Create/edit Draft entries, post balanced entries, close period. | Entry date must belong to the period. |
| Cerrado (`Closed`) | View entries and balances only. | No new, edited, or posted entries; no reopen action. |

| From | Action | To | Authorized Roles | Preconditions |
|---|---|---|---|---|
| New | Crear periodo | Abierto | Contador, Administrador Sistema | Unique `YYYY-MM`, valid start/end dates. |
| Abierto | Cerrar periodo | Cerrado | Contador, Administrador Sistema | No Draft entries remain; no posting operation has unknown outcome. |

Closed -> Open is prohibited in current scope. Corrections use an adjustment in another Open period. There is no separate Reopen command.

### Voucher States

| State | Allowed Actions | SUNAT Effect |
|---|---|---|
| Registrado | Edit while not locked by a generated book; Annul; View. | Eligible for book generation. |
| Anulado | View and audit only; terminal. | Excluded from new book versions. |

A voucher included in a generated version is immutable. It may be annulled; the historical book snapshot remains unchanged and a later replacement book version reflects current eligible vouchers.

## SUNAT Lifecycle

### Validation States

| State | Meaning | Generation Allowed |
|---|---|---:|
| Pendiente | Validation has not run for the selected period/type. | No. |
| Valida | Eligible rows, configuration, totals, and required fields pass. | Yes. |
| Con observaciones | Only nonblocking warnings exist; warnings are listed and acknowledged. | Yes. |
| Bloqueada | Missing configuration, invalid required data, unresolved operation, or no eligible registered vouchers. | No. |

### Version Lifecycle

| Display State | Persisted State | Meaning | Editable |
|---|---|---|---:|
| Sin version | None | No generated book exists for period/type. | N/A. |
| Generada vigente | Generado | Highest generated sequential version for period/type. | No. |
| Generada sustituida | Generado | An immutable earlier version superseded by a higher version. | No. |

Supersession is derived from version order; no new persisted state or deletion is introduced.

### Generation and Replacement Workflow

1. Select period and book type.
2. Run the non-mutating validation query for configuration, active SUNAT format, registered eligible vouchers, exclusions, tax versions, totals, and existing versions.
3. If validation is Valida or Con observaciones, explicitly acknowledge warnings when present and execute `Generar nueva version`; this command reruns validation and creates the next positive sequential version atomically.
4. The new version becomes Generada vigente; the previous current version becomes Generada sustituida by relation.
5. Every version retains its voucher snapshots, each voucher's tax configuration version, the active SUNAT format version, totals, generator, and timestamp.
6. Replacement always means generating a new version. Existing versions are never edited, reopened, annulled, or deleted.

### Export States

For direct payroll and SUNAT file endpoints these are transient UI/request states only; they are not database enums and no persistent export-history record is claimed. `Generated`/`Failed` is persisted only for financial-report snapshots in `reporting.reporte_snapshot`.

| State | Meaning | Next Action |
|---|---|---|
| No exportada | No export operation exists for the selected version/format. | Exportar. |
| En proceso | Export is running. | Wait; no cancellation action is exposed after export starts. |
| Exportada | Output completed for the exact version, format, and filters. | Open or export again. |
| Fallida | No valid output was produced. | Retry. |
| Resultado desconocido | Completion could not be confirmed. | Verificar estado before retry. |

Only Contador and Administrador Sistema may generate versions and SUNAT exports. Historical superseded versions may be exported only after the user explicitly selects the version; the default is the current version.

## Master-Data and Configuration Lifecycles

| Object | Persisted states | Allowed transitions/actions | Roles |
|---|---|---|---|
| Usuario | `activo=true/false` (Activo/Inactivo) | Create Active; Activo -> Inactivo; Inactivo -> Activo. Physical delete is absent. | Administrador Sistema. |
| Empleado | `activo=true/false` | Create Active; deactivate/reactivate. Reactivation affects future or recalculated Draft payroll only. | Administrador RRHH, Administrador Sistema. |
| Departamento | `activo=true/false` | Create Active; deactivate/reactivate. Inactive departments cannot receive new employee assignments. | Administrador RRHH, Administrador Sistema. |
| CuentaContable | `activo=true/false` | Create Active; deactivate/reactivate. Inactive accounts cannot receive lines; Posted use protects classification/hierarchy. | Contador, Administrador Sistema; Gerente read-only. |
| Tax/pension/SUNAT format version | `Draft`, `Active`, `Closed` | New -> Draft; Draft -> Active; Active -> Closed. Closed is terminal; Active/Closed business fields are immutable. | Administrador Sistema. |
| ReporteSnapshot | `Generated`, `Failed` | Created once by an export attempt; no update/delete transition. Generated requires a file reference. | Gerente Financiero, Administrador Sistema. |

Display labels may be localized (`Borrador`, `Activa`, `Cerrada`, `Generado`, `Fallido`) but API/SQL values remain exactly those listed above.

## Empty States

Empty states must distinguish the reason for emptiness.

| Situation | Message Direction | Primary Next Step |
|---|---|---|
| No employees exist | `Aun no hay empleados registrados.` | `Nuevo empleado`. |
| No vouchers exist | `No hay comprobantes registrados para este periodo.` | `Nuevo comprobante`. |
| No payroll calculated | `La planilla de este periodo aun no fue calculada.` | `Calcular planilla`. |
| No SUNAT book generated | `No existe una version del libro para este periodo.` | `Generar libro`. |
| No report data | `No hay informacion financiera para el periodo seleccionado.` | `Cambiar periodo` or review source data. |
| No users | `No hay cuentas de usuario disponibles.` | `Nuevo usuario`, if authorized. |
| No search results | `No hay resultados para la busqueda y los filtros actuales.` | `Limpiar filtros`. |

Empty states should be quiet and useful. Avoid congratulatory or decorative messaging.

## Error States

Error states must include:

- What failed.
- Why it likely failed, when known.
- What the user can do next.
- Whether data was saved, partially completed, or unchanged.

| Error Type | UX Treatment |
|---|---|
| Field validation | Field-level message and optional summary. |
| Duplicate record | Explain matching identity and suggest search/open existing record. |
| Permission denied | Explain lack of access and offer Dashboard return. |
| Session expired | Preserve context when possible and require login. |
| Calculation blocked | State blocking rule, such as finalized payroll period. |
| Export failed | Keep report visible and offer retry. |
| Load failed | Keep shell stable and provide retry. |
| Configuration conflict | Explain overlapping version/date issue. |

## Error Message Examples

Use business language:

- "DNI must have 8 digits."
- "This employee already exists. Search by DNI to open the record."
- "Payroll for 2026-05 is finalized and cannot be recalculated."
- "RUC must have 11 digits for this document type."
- "No active IGV configuration is available for the voucher date."
- "You do not have access to Administration."

Canonical Spanish equivalents are `El DNI debe tener 8 digitos`, `El RUC debe tener 11 digitos`, `La planilla 2026-05 esta finalizada y no se puede recalcular`, and `No tienes acceso a Administracion`.

Avoid:

- "Exception occurred."
- "Bad request."
- "Operation failed."
- "Unhandled error."

## Loading States

Loading states should retain context.

| Operation | Loading Guidance |
|---|---|
| Login | Show authentication in progress and prevent duplicate submit. |
| Dashboard load | Show KPI placeholders and keep navigation stable. |
| List search | Show inline searching state without hiding existing layout. |
| Payroll calculation | Show selected period and calculation progress. |
| SUNAT generation | Show period, book type, and generation progress. |
| Report refresh | Show report name and filter context. |
| Export | Show export target and completion/failure status. |

Long operations show `En preparacion`, `En proceso`, `Completado`, `Completado parcialmente`, `No completado`, or `Resultado desconocido`; they never use an indeterminate success message.

## Concurrency and Stale Data

| Situation | Required UX |
|---|---|
| Record changed by another user | Show who/when if permitted, compare current and attempted values, and offer `Recargar` or `Reaplicar cambios seguros`. |
| Record deactivated while open | Preserve entered data, block save, explain the new state, and offer return to detail. |
| Period closed or finalized while open | Block mutation, refresh state, and preserve a read-only review. |
| Saved view became invalid | IMPLEMENTATION PENDING with saved views; no current persisted state is claimed. |

## Unknown Outcome and Connectivity

- When a connection fails before an operation starts, state that nothing changed and offer `Reintentar`.
- When a timeout occurs after submission, use `No se pudo confirmar el resultado` and offer `Verificar estado`; do not offer immediate duplicate submission.
- Payroll finalization, journal posting, and book generation expose stable persisted state after reconnection. Financial-report exports expose persisted snapshot state; direct payroll/SUNAT exports have no history and may be requested again only after source-state verification.
- Session expiration preserves non-sensitive context and returns to `Iniciar sesion`; after authentication, the user returns only to an authorized safe view.

## Partial Success

Bulk overtime and generic partial-result batches are `IMPLEMENTATION PENDING`; no active workflow may claim these states. Financial-report snapshot export is atomic (`Generated` or `Failed`), while direct payroll/SUNAT file requests use transient request states only.

## Cancellation

Cancellation is offered only when the operation can stop safely. The message distinguishes `Cancelado antes de iniciar`, `Cancelacion solicitada`, and `No se puede cancelar porque el proceso ya finalizo`.

## Skeletons and Placeholders

Use placeholders only when they reduce perceived waiting and preserve layout. Do not show fake financial values. For finance and compliance screens, placeholders must clearly be loading surfaces, not provisional results.

## Functional Motion States

This document defines which state changes may use motion. The canonical duration, reduced-motion, focus-retention, and loading-feedback requirements are defined only in `design-system.md`.

## Success States

Success feedback should be visible but restrained.

| Operation | Success Feedback |
|---|---|
| Guardar empleado | Empleado guardado y visible en lista/detalle. |
| Guardar comprobante | Comprobante registrado con estado y totales calculados. |
| Calcular planilla | Resultados, totales y estado Borrador visibles. |
| Finalizar planilla | Estado Finalizada visible y recalculo no disponible. |
| Generar libro | Version, incluidos, totales y acciones de exportacion visibles. |
| Exportar | Exportacion completada con archivo y contexto confirmados. |
| Restablecer contrasena | Restablecimiento completado y siguiente paso claro. |

## Warning States

Warnings should appear before the user commits to a risky or irreversible action:

- Finalizing payroll blocks recalculation.
- Generating a new book creates a new version.
- Deactivation removes an entity from new operations.
- Tax configuration changes affect future transactions only.
- Export reflects current filters.

Warnings must not block routine work unless a business rule requires it.

## Permission States

Permission states should be calm and specific:

- "This area is restricted to Administrador Sistema."
- "Your role can view this report but cannot export it."
- "This action requires Accounting access."

Offer a route back to Dashboard or the previous screen.

## State Acceptance Checklist

- Empty states identify the reason and next step.
- Errors are specific and recoverable.
- Loading keeps context visible.
- Success confirms what changed.
- Warnings explain consequence before commitment.
- Permission states do not expose restricted data.
