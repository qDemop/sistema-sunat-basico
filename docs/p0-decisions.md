# P0 Architecture Decision Baseline

This document records only the decisions required to resolve the certified P0 blockers. It does not expand the ERP scope.

## P0-001 Payroll Period Aggregate

- `PeriodoPlanilla` is the Payroll aggregate root and maps to `PERIODO_PLANILLA`.
- One `PeriodoPlanilla` exists per `YYYY-MM` period and owns the lifecycle `Draft`, `Finalized`, or `Cancelled`.
- `PLANILLA` is one employee result within a period and has no independent lifecycle state.
- Payroll calculation, recalculation, finalization, cancellation, export, and audit identify the aggregate by `periodo`.
- Recalculation is allowed only while `PERIODO_PLANILLA.estado = Draft`.

## P0-002 Payroll Amount Ownership

- `total_bruto` is cash remuneration for the month: base salary plus approved overtime.
- `total_descuentos` is the effective pension deduction plus configured additional discounts.
- `total_neto = total_bruto - total_descuentos`.
- `provision_gratificacion = total_bruto / 6` and `provision_cts = (total_bruto + provision_gratificacion) / 12` are monthly accounting provisions.
- The provisions are displayed and posted to the ledger but are not added to `total_bruto` or `total_neto`.
- `descuentos_adicionales` is fixed at zero in the current scope. Defining additional-discount catalogs, inputs, and liability mappings requires a future approved specification.
- Calculation of the legal July/December gratification payment and execution of CTS deposits are outside the current scope. They require a future approved specification; developers must not infer them.

## P0-003 Effective-Dated Pension Rates

- `TIPO_DESCUENTO` identifies the pension regime (`AFP` or `ONP`) and does not own a mutable percentage.
- `CONFIG_DESCUENTO_PREVISIONAL_VERSION` owns percentage, version, effective dates, and status.
- Payroll resolves the single Active version effective on the period end date and stores the applied version in `DETALLE_PLANILLA`.
- Active versions for the same pension regime cannot overlap.

## P0-004 Role-Only Authorization

- The only application roles are `Administrador RRHH`, `Contador`, `Gerente Financiero`, and `Administrador Sistema`.
- Roles are seeded and cannot be created or deleted through the API in Sprint 1.
- Gerente Financiero has read-only access to General Ledger queries and cannot mutate ledger data.
- Administrador RRHH sees payroll totals inside Payroll screens but has no access to the Reports module.
- The authenticated JWT role, not a client-supplied role or user ID, drives authorization and audit identity.

## P0-005 Authentication State

- JWTs include `sub`, user name, role, `jti`, issued-at, and expiration claims.
- Three consecutive failed attempts block the account for 15 minutes. Successful login resets the counter.
- Logout revokes the current `jti` until token expiration; clearing the client token alone is insufficient.
- Authentication events and all sensitive mutations include actor user, actor role, correlation ID, result, and affected entity.

## P0-006 Lifecycle Commands

- Reactivation is an explicit command for users, employees, departments, and accounts.
- Overtime follows `Draft -> Approved` or `Draft/Approved -> Cancelled`; Approved overtime can be cancelled only before a payroll Draft exists for the period.
- Tax and SUNAT format versions follow `Draft -> Active -> Closed`; Active and Closed versions are immutable.
- Payroll follows `None -> Draft -> Finalized` or `Draft -> Cancelled`; Finalized and Cancelled are terminal.
- Accounting periods follow `Open -> Closed`; reopening is out of scope.
- Journal entries follow `Draft -> Posted` or `Draft -> Cancelled`. Reversing a Posted entry creates a linked Draft adjustment in an Open period; the original remains Posted.

## P0-007 Ledger Event Mapping

- `PayrollFinalized` creates exactly one Draft journal entry for the payroll period.
- `VoucherRegistered` creates exactly one Draft journal entry for the voucher.
- Source uniqueness is enforced by `(origen, id_origen)` for non-adjustment entries.
- Updating an unposted voucher updates its source Draft. A voucher linked to a generated book is immutable except for annulment.
- Annulment cancels an unposted source Draft or creates a linked reversal Draft when the source entry is Posted.
- Posting remains an explicit Contador/Administrador Sistema action.
- Nota de Credito owns a source-linked reduction entry that inverts the ordinary mapping for its positive amount; Nota de Debito owns an ordinary increase entry. Both require an original voucher reference and never mutate the original entry.

## P0-008 SUNAT Generation

- Validation is a query and returns `Valida`, `ConObservaciones`, or `Bloqueada`; `Pendiente` is UI-only before validation runs.
- Vouchers persist only `Registrado` or `Anulado`; `Validado` is not a persisted voucher state.
- Generation is `POST /api/libros`; retrieval is `GET`. A successful generation always creates the next immutable version.
- Generation resolves one Active `CONFIG_SUNAT_FORMATO` effective for the period end date and stores it on the book.
- Each bridge row stores the voucher tax-configuration version used by that voucher. A book does not claim one tax version for the entire period.
- The format JSON contains exactly the eleven governed SUNAT column tokens; generation locks one eligible voucher set and snapshots every exported row field in bridge order.

## P0-009 Financial Reporting

- Balance sheet and income statement use Posted journal lines only.
- Balance sheet values are cumulative through the requested period end.
- Income statement values cover the requested period or explicit range.
- Accounts receivable are the signed balance of account `1212`; accounts payable are the signed balance of account `4212`.
- IGV payable is the credit balance of `40111` minus the debit balance of `40114`; a negative result is tax credit.
- Consolidated payroll uses Finalized payroll results and the department snapshot stored on each employee result.
- Report exports persist `REPORTE_SNAPSHOT` and `REPORTE_LINEA` so the exported values, filters, actor, and source cutoff remain reproducible.

## Explicitly Out of Scope

- Fine-grained permission catalogs.
- Reopening a Closed accounting period.
- Reopening or adjusting a Finalized payroll.
- Legal gratification payment and CTS deposit execution.
- Direct submission of electronic books to SUNAT.
- Automatic posting of Draft journal entries.
- Additional payroll discount configuration or capture.
