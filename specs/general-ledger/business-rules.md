# General Ledger Business Rules

## Accounting Period Rules

- Accounting period code follows `YYYY-MM`.
- Journal entries can be created only in open periods.
- Closed periods cannot receive new posted entries.
- Closing requires no Draft entries in the period and is terminal; reopening is out of scope.

## Account Rules

- Account code is unique.
- Account type must classify the account for reporting: Asset, Liability, Equity, Income, Cost, or Expense.
- Inactive accounts cannot be used in new journal entry lines.
- An account referenced by a Posted line cannot change code, type, nature, or parent; it may be logically deactivated/reactivated.

## Journal Entry Rules

- A journal entry starts as `Draft`.
- A journal entry can be posted only when total debits equal total credits.
- Posted entries cannot be edited.
- Cancelled entries must preserve audit traceability.
- Journal entries can reference source module and source entity, such as Payroll or Accounting SUNAT.
- Entry date must be between the selected period start and end dates.
- A Draft can be edited, posted, or cancelled. A Posted entry is corrected only by a linked reversal Draft in an Open period.
- Non-adjustment entries are unique by `(origen, id_origen)`.

## Initial Chart of Accounts

| Code | Name | Type | Nature | Required Use |
|---|---|---|---|---|
| 1011 | Caja | Asset | Debit | Manual cash operations. |
| 1212 | Facturas por cobrar | Asset | Debit | Sales voucher receivable. |
| 40111 | IGV por pagar | Liability | Credit | Sales IGV. |
| 40114 | IGV credito fiscal | Asset | Debit | Purchase IGV. |
| 4032 | ONP por pagar | Liability | Credit | ONP withholding. |
| 4071 | AFP por pagar | Liability | Credit | AFP withholding. |
| 4111 | Remuneraciones por pagar | Liability | Credit | Payroll net payable. |
| 4151 | Beneficios sociales por pagar | Liability | Credit | CTS and gratification provisions. |
| 4212 | Facturas por pagar | Liability | Credit | Purchase voucher payable. |
| 6011 | Compras | Expense | Debit | Purchase base and exempt amount. |
| 6211 | Sueldos y horas extra | Expense | Debit | Payroll cash gross. |
| 6292 | Provision de gratificaciones | Expense | Debit | Monthly gratification provision. |
| 6293 | Provision de CTS | Expense | Debit | Monthly CTS provision. |
| 7011 | Ventas | Income | Credit | Sales base and exempt amount. |

These codes are the minimum Sprint scope. Adding or remapping accounts requires an approved specification change.

## Source Event Mapping

### PayrollFinalized

- One Draft entry uses `origen = Planilla` and `id_origen = id_periodo_planilla`.
- Debit `6211` by total cash gross.
- Debit `6292` by total gratification provision.
- Debit `6293` by total CTS provision.
- Credit `4032` by total ONP and `4071` by total AFP.
- Credit `4111` by total net pay after all discounts.
- Credit `4151` by total CTS plus gratification provisions.
- Additional discounts require a configured liability account; no additional discount may be calculated without that mapping.

### VoucherRegistered - Venta

- One Draft entry uses `origen = Comprobante` and `id_origen = id_comprobante`.
- Debit `1212` by voucher total.
- Credit `7011` by taxable plus exempt base.
- Credit `40111` by IGV.

### VoucherRegistered - Compra

- Debit `6011` by taxable plus exempt base.
- Debit `40114` by IGV.
- Credit `4212` by voucher total.

### Nota de Credito

- The note requires an original voucher reference with the same movement; amounts remain positive.
- Venta note: Credit `1212` by total; Debit `7011` by taxable plus exempt base; Debit `40111` by IGV.
- Compra note: Credit `6011` by taxable plus exempt base; Credit `40114` by IGV; Debit `4212` by total.
- The referenced entry is not edited or reversed; the note owns its own source-unique Draft reduction entry.

### Nota de Debito

- The note requires an original voucher reference with the same movement.
- It increases the transaction and therefore uses the ordinary Venta or Compra mapping above.

### Source Correction

- Updating an unposted voucher updates its source Draft atomically.
- Annulment cancels an unposted source Draft.
- If the source entry is Posted, annulment creates a linked reversal Draft in a selected Open period; the original remains Posted.
- Payroll Finalized entries are not updated because Finalized payroll correction is out of scope.

## Reporting Rules

- Balance sheet uses posted ledger lines grouped by Asset, Liability, and Equity accounts.
- Income statement uses posted ledger lines grouped by Income, Cost, and Expense accounts.
- Draft and cancelled entries are excluded from financial reports.
