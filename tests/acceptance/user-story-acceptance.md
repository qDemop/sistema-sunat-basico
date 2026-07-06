# User Story Acceptance Test Specifications

## US-001 Authentication and System Access

| Scenario | Given | When | Then |
|---|---|---|---|
| Valid login | Active user with valid password | User submits credentials | System returns JWT and opens dashboard. |
| Invalid login | User enters wrong credentials | User submits login | System shows error and stays on login. |
| Persistent lockout | Same username has two persisted consecutive failures | A third invalid login occurs | Account is blocked for 15 minutes, the attempt and lockout are persisted, and the public error remains generic. |
| Logout revocation | Authenticated session has an unexpired JWT with `jti` | User logs out | Current `jti` is persisted in `TOKEN_REVOCATION`; reuse is rejected until expiration. |
| Role-based dashboard | User has role Contador | Login succeeds | Only Accounting SUNAT and permitted modules are shown. |
| Protected request | User has JWT | User opens a protected module | Request includes `Authorization: Bearer <token>`. |

## US-002 Employee Registration

| Scenario | Given | When | Then |
|---|---|---|---|
| Create valid employee | Administrator RRHH has permission | Valid employee form is submitted | Employee is created and appears in list. |
| Duplicate DNI | Existing employee has same DNI | Form is submitted | System rejects the record with field error. |
| Invalid salary | Salary is zero or negative | Form is submitted | System rejects the record with salary error. |
| Underage employee | Birth date implies age under 18 | Form is submitted | API and database reject the employee consistently. |
| Logical deactivation | Employee exists | Administrator deactivates employee | Employee becomes inactive without physical deletion. |

## US-003 Monthly Payroll Calculation

| Scenario | Given | When | Then |
|---|---|---|---|
| Calculate period | Active employees exist | Administrator calculates `2026-05` | Payroll results are persisted and displayed. |
| Department assignment | Active employee belongs to `DEPARTAMENTO` | Payroll is calculated | Payroll results include department data for filtering. |
| Overtime input | Approved `HORAS_EXTRA` exists for employee and period | Payroll is calculated | Gross salary includes overtime by configured hour ranges. |
| Draft recalculation | Existing payroll for period has status `Draft` | Administrator recalculates period | Payroll values are updated atomically. |
| Finalized recalculation blocked | Existing payroll for period has status `Finalized` | Administrator recalculates period | System rejects recalculation. |
| Effective AFP deduction | Employee has AFP and one Active version is effective at period end | Payroll is calculated | Deduction uses that percentage and `DETALLE_PLANILLA` stores its version ID and applied rate. |
| Effective ONP deduction | Employee has ONP and one Active version is effective at period end | Payroll is calculated | Deduction uses that percentage and `DETALLE_PLANILLA` stores its version ID and applied rate. |
| Cash and provisions | Employee gross cash remuneration is S/ 1,200.00 | Payroll is calculated | Gratification provision is S/ 200.00, CTS provision is S/ 116.67, neither increases cash gross or net, and total cost includes both provisions. |
| Payroll totals | Period calculation completes | Results are displayed | Cash gross, employee deductions, net cash pay, CTS/gratification provisions, and total cost are shown separately. |
| Finalization identity | Draft payroll is valid | Authorized user finalizes it | Period becomes Finalized and stores finalizing user, timestamp and exactly one source-linked Draft entry. |
| Draft cancellation | Payroll period is Draft | Authorized user cancels it | Period becomes terminal Cancelled and remains auditable. |
| Payslip export | Persisted payroll results are Draft or Finalized | User exports PDF/ZIP or Excel | Export matches persisted results; Cancelled or missing payroll returns typed conflict/not-found. |

## US-004 Accounting Voucher Registration

| Scenario | Given | When | Then |
|---|---|---|---|
| Register taxable voucher | Accountant enters taxable operation | Base amount and issue date are entered | IGV is calculated from the active `CONFIG_TRIBUTARIA_VERSION`. |
| Movement type required | Accountant omits Compra/Venta movement | User saves voucher | System rejects voucher with movement type error. |
| Invalid RUC | RUC is not 11 digits | User saves voucher | System rejects voucher with RUC error. |
| Duplicate voucher | Same type, movement, series, and number already exist | User saves voucher | System rejects duplicate voucher. |
| Zero-total voucher | Both taxable and exempt bases are zero | User saves voucher | System rejects it before creating a source journal entry. |
| Credit note reference/mapping | Code 07 references a Registrado voucher of the same movement | User saves | Note stores the reference and creates an inverse reduction Draft without changing the original. |
| Debit note reference/mapping | Code 08 references a Registrado voucher of the same movement | User saves | Note stores the reference and creates an ordinary increase Draft without changing the original. |
| Successful save and mapping | Voucher is valid | User saves | Voucher appears as Registrado and exactly one deterministic source-linked Draft entry is created. |
| Computed validation | Voucher is entered or updated | Validation runs | Validation result is returned without creating a `Validado` voucher state. |
| Annul posted source | Registrado voucher has a Posted source entry | User annuls with an Open correction period | Voucher becomes Anulado and a linked reversal Draft is created; original entry remains Posted. |

## US-005 Purchase and Sales Book Generation

| Scenario | Given | When | Then |
|---|---|---|---|
| Generate Sales Book | Registered sales vouchers exist | Accountant generates sales book | Book preview includes SUNAT columns and totals. |
| Generate Purchase Book | Registered purchase vouchers exist | Accountant generates purchase book | Book preview includes purchase vouchers and totals. |
| Period filter | Vouchers exist in multiple periods | User selects one period | Only selected-period vouchers are included. |
| Book versioning | Existing book version exists for period/type | Accountant generates book again | New `LIBRO_CONTABLE.version` is created and vouchers are linked through `COMPROBANTE_LIBRO`. |
| Validation command separation | Period/type is selected | User validates then confirms generation | GET validation performs no mutation; POST generation reruns validation and creates the next version only when nonblocking. |
| Format and tax traceability | Active effective format exists and vouchers use different tax versions | Book is generated | Book stores its format version and each bridge row stores that voucher's tax-version snapshot. |
| Stable generation set | Eligible vouchers are locked while generation runs | Concurrent edit/annul is attempted | Header totals and all eleven bridge snapshot fields come from one stable set; concurrent mutation waits or is reflected only in a later version. |
| Export book | Generated book exists | User exports PDF/Excel | Export action is available. |

## US-006 Financial Report Visualization

| Scenario | Given | When | Then |
|---|---|---|---|
| Dashboard KPIs | Manager logs in | Dashboard loads | Financial KPIs are visible. |
| Receivable/payable formulas | Posted entries affect accounts 1212 and 4212 | Dashboard loads | Accounts receivable/payable equal their documented signed balances through period end. |
| Balance sheet | Posted ledger entries exist | Manager opens balance sheet | Assets, liabilities, and equity are shown from posted ledger entries. |
| Income statement | Posted ledger entries exist | Manager opens income statement | Income, costs, expenses, and net result are shown from posted ledger entries. |
| Period change | Reports are visible | Manager changes period | Values update for selected period. |
| Export report | Report is visible | Manager exports PDF/Excel | Export action is available. |
| Reproducible export | Report and filters are visible | Manager exports | Immutable snapshot header/lines store actor, filters, source cutoff, values, format and result before file delivery. |
