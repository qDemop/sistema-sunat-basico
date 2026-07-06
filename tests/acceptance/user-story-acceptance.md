# User Story Acceptance Test Specifications

> Sprint 0 remediation: stable scenario IDs (`US-NNN-SC-NN`) added to all scenarios; US-007 section added for the implicit User and Role Administration story.

## US-001 Authentication and System Access

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-001-SC-01 | Valid login | Active user with valid password | User submits credentials | System returns JWT and opens dashboard. |
| US-001-SC-02 | Invalid login | User enters wrong credentials | User submits login | System shows error and stays on login. |
| US-001-SC-03 | Persistent lockout | Same username has two persisted consecutive failures | A third invalid login occurs | Account is blocked for 15 minutes, the attempt and lockout are persisted, and the public error remains generic. |
| US-001-SC-04 | Logout revocation | Authenticated session has an unexpired JWT with `jti` | User logs out | Current `jti` is persisted in `TOKEN_REVOCATION`; reuse is rejected until expiration. |
| US-001-SC-05 | Role-based dashboard | User has role Contador | Login succeeds | Only Accounting SUNAT and permitted modules are shown. |
| US-001-SC-06 | Protected request | User has JWT | User opens a protected module | Request includes `Authorization: Bearer <token>`. |

## US-002 Employee Registration

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-002-SC-01 | Create valid employee | Administrator RRHH has permission | Valid employee form is submitted | Employee is created and appears in list. |
| US-002-SC-02 | Duplicate DNI | Existing employee has same DNI | Form is submitted | System rejects the record with field error. |
| US-002-SC-03 | Invalid salary | Salary is zero or negative | Form is submitted | System rejects the record with salary error. |
| US-002-SC-04 | Underage employee | Birth date implies age under 18 | Form is submitted | API and database reject the employee consistently. |
| US-002-SC-05 | Logical deactivation | Employee exists | Administrator deactivates employee | Employee becomes inactive without physical deletion. |

## US-003 Monthly Payroll Calculation

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-003-SC-01 | Calculate period | Active employees exist | Administrator calculates `2026-05` | Payroll results are persisted and displayed. |
| US-003-SC-02 | Department assignment | Active employee belongs to `DEPARTAMENTO` | Payroll is calculated | Payroll results include department data for filtering. |
| US-003-SC-03 | Overtime input | Approved `HORAS_EXTRA` exists for employee and period | Payroll is calculated | Gross salary includes overtime by configured hour ranges. |
| US-003-SC-04 | Draft recalculation | Existing payroll for period has status `Draft` | Administrator recalculates period | Payroll values are updated atomically. |
| US-003-SC-05 | Finalized recalculation blocked | Existing payroll for period has status `Finalized` | Administrator recalculates period | System rejects recalculation. |
| US-003-SC-06 | Effective AFP deduction | Employee has AFP and one Active version is effective at period end | Payroll is calculated | Deduction uses that percentage and `DETALLE_PLANILLA` stores its version ID and applied rate. |
| US-003-SC-07 | Effective ONP deduction | Employee has ONP and one Active version is effective at period end | Payroll is calculated | Deduction uses that percentage and `DETALLE_PLANILLA` stores its version ID and applied rate. |
| US-003-SC-08 | Cash and provisions | Employee gross cash remuneration is S/ 1,200.00 | Payroll is calculated | Gratification provision is S/ 200.00, CTS provision is S/ 116.67, neither increases cash gross or net, and total cost includes both provisions. |
| US-003-SC-09 | Finalization identity | Draft payroll is valid | Authorized user finalizes it | Period becomes Finalized and stores finalizing user, timestamp and exactly one source-linked Draft entry. |
| US-003-SC-10 | Draft cancellation | Payroll period is Draft | Authorized user cancels it | Period becomes terminal Cancelled and remains auditable. |
| US-003-SC-11 | Payslip export | Persisted payroll results are Draft or Finalized | User exports PDF/ZIP or Excel | Export matches persisted results; Cancelled or missing payroll returns typed conflict/not-found. |

## US-004 Accounting Voucher Registration

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-004-SC-01 | Register taxable voucher | Accountant enters taxable operation | Base amount and issue date are entered | IGV is calculated from the active `CONFIG_TRIBUTARIA_VERSION`. |
| US-004-SC-02 | Movement type required | Accountant omits Compra/Venta movement | User saves voucher | System rejects voucher with movement type error. |
| US-004-SC-03 | Invalid RUC | RUC is not 11 digits | User saves voucher | System rejects voucher with RUC error. |
| US-004-SC-04 | Duplicate voucher | Same type, movement, series, and number already exist | User saves voucher | System rejects duplicate voucher. |
| US-004-SC-05 | Zero-total voucher | Both taxable and exempt bases are zero | User saves voucher | System rejects it before creating a source journal entry. |
| US-004-SC-06 | Credit note reference/mapping | Code 07 references a Registrado voucher of the same movement | User saves | Note stores the reference and creates an inverse reduction Draft without changing the original. |
| US-004-SC-07 | Debit note reference/mapping | Code 08 references a Registrado voucher of the same movement | User saves | Note stores the reference and creates an ordinary increase Draft without changing the original. |
| US-004-SC-08 | Successful save and mapping | Voucher is valid | User saves | Voucher appears as Registrado and exactly one deterministic source-linked Draft entry is created. |
| US-004-SC-09 | Computed validation | Voucher is entered or updated | Validation runs | Validation result is returned without creating a `Validado` voucher state. |
| US-004-SC-10 | Annul posted source | Registrado voucher has a Posted source entry | User annuls with an Open correction period | Voucher becomes Anulado and a linked reversal Draft is created; original entry remains Posted. |

## US-005 Purchase and Sales Book Generation

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-005-SC-01 | Generate Sales Book | Registered sales vouchers exist | Accountant generates sales book | Book preview includes SUNAT columns and totals. |
| US-005-SC-02 | Generate Purchase Book | Registered purchase vouchers exist | Accountant generates purchase book | Book preview includes purchase vouchers and totals. |
| US-005-SC-03 | Period filter | Vouchers exist in multiple periods | User selects one period | Only selected-period vouchers are included. |
| US-005-SC-04 | Book versioning | Existing book version exists for period/type | Accountant generates book again | New `LIBRO_CONTABLE.version` is created and vouchers are linked through `COMPROBANTE_LIBRO`. |
| US-005-SC-05 | Validation command separation | Period/type is selected | User validates then confirms generation | GET validation performs no mutation; POST generation reruns validation and creates the next version only when nonblocking. |
| US-005-SC-06 | Format and tax traceability | Active effective format exists and vouchers use different tax versions | Book is generated | Book stores its format version and each bridge row stores that voucher's tax-version snapshot. |
| US-005-SC-07 | Stable generation set | Eligible vouchers are locked while generation runs | Concurrent edit/annul is attempted | Header totals and all eleven bridge snapshot fields come from one stable set; concurrent mutation waits or is reflected only in a later version. |
| US-005-SC-08 | Export book | Generated book exists | User exports PDF/Excel | Export action is available. |

## US-006 Financial Report Visualization

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-006-SC-01 | Dashboard KPIs | Manager logs in | Dashboard loads | Financial KPIs are visible. |
| US-006-SC-02 | Receivable/payable formulas | Posted entries affect accounts 1212 and 4212 | Dashboard loads | Accounts receivable/payable equal their documented signed balances through period end. |
| US-006-SC-03 | Balance sheet | Posted ledger entries exist | Manager opens balance sheet | Assets, liabilities, and equity are shown from posted ledger entries. |
| US-006-SC-04 | Income statement | Posted ledger entries exist | Manager opens income statement | Income, costs, expenses, and net result are shown from posted ledger entries. |
| US-006-SC-05 | Period change | Reports are visible | Manager changes period | Values update for selected period. |
| US-006-SC-06 | Export report | Report is visible | Manager exports PDF/Excel | Export action is available. |
| US-006-SC-07 | Reproducible export | Report and filters are visible | Manager exports | Immutable snapshot header/lines store actor, filters, source cutoff, values, format and result before file delivery. |

## US-007 User and Role Administration (implicit)

> Extracted from `docs/user-stories.md:144-148` (Implicit Extracted User Need). Sprint 0 remediation adds this section to close the user-story acceptance gap.

| Scenario ID | Scenario | Given | When | Then |
|---|---|---|---|---|
| US-007-SC-01 | Create user | Administrador Sistema is authenticated | A new user form is submitted with username, name, and role | User is created with BCrypt-hashed password and appears in the user list; `USER_CREATED` audit event is persisted. |
| US-007-SC-02 | Assign role | An existing active user exists | Administrador Sistema changes the user's role | User's role is updated; `USER_UPDATED` audit event records the changed field; the user's next session reflects the new role's module visibility. |
| US-007-SC-03 | Deactivate user | An active user exists | Administrador Sistema deactivates the user | User becomes inactive; `USER_DEACTIVATED` audit event is persisted; the user's active JWT is not auto-revoked but renewal is blocked. |
| US-007-SC-04 | Reactivate user | A previously deactivated user exists | Administrador Sistema reactivates the user | User becomes active; `USER_REACTIVATED` audit event is persisted; the user can log in again. |
| US-007-SC-05 | Reset password | An active or inactive user exists | Administrador Sistema triggers password reset | A temporary password is set; `USER_PASSWORD_RESET` audit event records only the result (no credential material); the user must change on next login. |
| US-007-SC-06 | List roles | Four predefined roles exist | Administrador Sistema opens role list | The four roles (Administrador RRHH, Contador, Gerente Financiero, Administrador Sistema) are shown with their module matrix; role creation/deletion is not available. |
