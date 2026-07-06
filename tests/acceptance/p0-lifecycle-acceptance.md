# P0 Workflow Acceptance Specifications

These specification-only scenarios cover the 64 workflows named by the implementation traceability certification. They do not prescribe test framework or application code.

## Authentication and Administration

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-001 | Login | Active user, valid BCrypt credential, no active block | Credentials are submitted | JWT contains `sub`, username, role, `jti`, issued-at and expiration; success is audited. |
| WF-AT-002 | Logout | Authenticated unexpired JWT | Logout is requested | Current `jti` is revoked until expiration, later reuse is denied, and actor/correlation are audited. |
| WF-AT-003 | Failed login/lockout | Same username has two consecutive persisted failures | Third invalid attempt occurs | Attempt is persisted, account blocks for 15 minutes, generic response is returned, and lockout is audited. |
| WF-AT-004 | User creation | Administrador Sistema supplies valid data and one predefined role | User is created | Active user is persisted with hashed credential and audit event; no custom role is created. |
| WF-AT-005 | User deactivation/reactivation | Existing user is Active then Inactive | Authorized lifecycle commands run | State changes logically in each direction and both operations are audited; physical deletion never occurs. |
| WF-AT-006 | Role assignment | Existing user and one of four predefined roles | Admin updates assignment | Next authentication carries the new role and audit records old/new role; client cannot supply audit actor. |
| WF-AT-007 | Department creation | Unique valid department name | RRHH/Admin creates department | Active department is persisted and audited. |
| WF-AT-008 | Department update | Active department exists | RRHH/Admin changes allowed fields | Allowed fields change, uniqueness remains valid, and changed field names are audited. |
| WF-AT-009 | Department deactivation/reactivation | Department can satisfy dependency guards | Authorized commands run | Logical state transitions occur, inactive assignment is prevented, and both actions are audited. |
| WF-AT-010 | Tax/pension version creation | No conflicting Draft identity | Admin creates IGV or AFP/ONP Draft version | Version, rate and dates persist as Draft and the matching created event is audited. |
| WF-AT-011 | Tax/pension version activation | Valid nonoverlapping Draft exists | Admin activates it through its command/procedure | Draft becomes Active, immutable fields remain unchanged and activation is audited; direct UPDATE or invalid-source activation is denied. |
| WF-AT-012 | Tax/pension version closing | Active version exists | Admin closes it through its command/procedure | Active becomes terminal Closed and history is unchanged/audited; direct UPDATE, repeat close and Closed edits are denied. |

## Payroll

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-013 | Employee creation | Valid employee, Active department, AFP/ONP regime | RRHH/Admin creates employee | Active employee persists with unique DNI and audit omits bank values. |
| WF-AT-014 | Employee update | Active employee exists | Allowed fields are updated | Employee changes persist; finalized payroll snapshots remain unchanged; changed field names are audited. |
| WF-AT-015 | Employee deactivation/reactivation | Employee exists | Authorized lifecycle commands run | Logical state changes in each direction and only Active employees enter later calculations. |
| WF-AT-016 | Register overtime | Active employee/period and positive hour band exists | RRHH/Admin registers overtime | Unique Draft overtime persists and is excluded from payroll until approved. |
| WF-AT-017 | Approve overtime | Overtime is Draft | Authorized approval runs before or after a payroll aggregate exists | Before payroll it becomes Approved and is audited; once any aggregate exists approval is rejected and state remains Draft. |
| WF-AT-018 | Cancel overtime | Overtime is Draft/Approved and no payroll Draft exists | Authorized cancellation runs | State becomes terminal Cancelled; payroll excludes it. |
| WF-AT-019 | Calculate payroll | Eligible employees, Approved overtime and one effective pension version per regime | Period calculation runs | One Draft `PERIODO_PLANILLA` and one result per employee persist atomically; response/result evidence includes salary, overtime, gross, applied pension type/version/rate, AFP or ONP amount, fixed-zero additional discounts, total deductions, net, both provisions, and total cost. |
| WF-AT-020 | Recalculate payroll | Period aggregate is Draft | Same period is calculated again | Existing results are atomically replaced/updated under same aggregate; Finalized/Cancelled would be rejected. |
| WF-AT-021 | Finalize payroll | Draft totals are complete and reviewed | Authorized finalization runs | Aggregate becomes Finalized with actor/time and exactly one deterministic payroll Draft entry is created. |
| WF-AT-022 | Cancel Draft payroll | Aggregate is Draft | Authorized cancellation runs | Aggregate becomes terminal Cancelled and remains auditable; no final output is created. |
| WF-AT-023 | Payroll audit trail | Any calculate/recalculate/finalize/cancel attempt occurs | Command succeeds or fails | Audit contains actor, role, correlation, result, aggregate ID/period and safe detail; failed transaction is logged after rollback. |
| WF-AT-024 | Payroll to General Ledger | Valid Draft payroll is finalized | Finalization commits | One source-unique balanced Draft maps gross/provisions/deductions/net to codes 6211, 6292, 6293, 4032, 4071, 4111 and 4151; posting remains explicit. |

## Accounting SUNAT

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-025 | Register voucher | Valid Compra/Venta voucher and effective IGV version | Contador/Admin saves an ordinary voucher or code 07/08 note | Registrado voucher stores applied tax version and one source Draft; notes require a same-movement original reference. |
| WF-AT-026 | Update voucher | Voucher is Registrado, unlocked, and source entry is Draft | Allowed fields change | Voucher and mapped Draft update atomically; book snapshots remain untouched. |
| WF-AT-027 | Validate voucher | Voucher input or stored Registrado voucher is checked | Validation executes | Typed field/business errors or valid result are returned without creating a `Validado` state. |
| WF-AT-028 | Annul voucher | Registrado voucher has Draft or Posted source | Authorized annulment runs | Voucher becomes Anulado; Draft source cancels or Posted source gets linked reversal Draft in Open period. |
| WF-AT-029 | Generate Purchase Book | Active governed Compras format and eligible Compra vouchers validate | POST generation locks and generates | Header and complete bridge rows use the same locked set and store format, tax snapshots, actor and totals. |
| WF-AT-030 | Generate Sales Book | Active governed Ventas format and eligible Venta vouchers validate | POST generation locks and generates | Header and complete bridge rows use the same locked set and store format, tax snapshots, actor and totals. |
| WF-AT-031 | Replace book version | Earlier version exists | Generation is repeated after validation | New sequential version is created; prior version remains immutable and is derived as superseded. |
| WF-AT-032 | Book immutability | Generated version exists | Update/delete is attempted | No mutation contract exists and database history remains unchanged. |
| WF-AT-033 | Included voucher later changed | Voucher is linked to generated book | Edit or annul is attempted | Edit is blocked; annulment preserves old snapshot and a later book version reflects current eligibility. |
| WF-AT-034 | SUNAT format versioning | Format Draft has exactly eleven unique governed tokens and is nonoverlapping | Admin activates/closes through explicit UI/API/SQL commands | Draft -> Active -> Closed holds, direct updates are denied, and generation consumes/stores exactly one locked Active format. |
| WF-AT-035 | Voucher to General Ledger | Ordinary voucher or referenced code 07/08 note is registered/updated/annulled | Source synchronization runs | Ordinary/debit-note mapping increases; credit-note mapping inversely reduces; cancellation/Posted reversal rules hold without changing the original. |

## General Ledger

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-036 | Create account | Unique valid code/type/nature and valid parent | Contador/Admin creates | Active account persists and is audited. |
| WF-AT-037 | Update account | Account is not protected by Posted use | Allowed fields change | Hierarchy and classification remain valid and update is audited. |
| WF-AT-038 | Account deactivate/reactivate | Account dependency guards pass | Authorized commands run | Logical state changes; inactive account cannot enter new lines. |
| WF-AT-039 | Create accounting period | Unique `YYYY-MM` and matching dates | Contador/Admin creates | Period persists Open and creation is audited. |
| WF-AT-040 | Close accounting period | Open period has no Draft entries or unknown posting | Close races with entry creation/posting | Period-row locks serialize operations; it becomes Closed with actor/time only when still eligible, and later mutations/posting are blocked. |
| WF-AT-041 | Reopen accounting period | Period is Closed | Reopen is sought | Workflow is explicitly out of scope: no UI/API command exists and correction uses an Open period. |
| WF-AT-042 | Create journal entry | Open period, valid active accounts and at least two lines | Contador/Admin saves | Source-unique Draft entry and lines persist; no posting occurs. |
| WF-AT-043 | Post journal entry | Draft is balanced, valid and in Open period | Post command runs | State becomes Posted with actor/time and immutable lines; event is audited. |
| WF-AT-044 | Cancel Draft journal | Entry is Draft | Cancel command runs | State becomes terminal Cancelled and event is audited. |
| WF-AT-045 | Reverse Posted journal | Posted entry and selected Open correction period exist | One or concurrent reverse commands run | Exactly one active linked adjustment Draft is created by row lock/unique constraint; original stays Posted. |
| WF-AT-046 | Validate debit/credit balance | Draft lines have unequal totals | Posting is attempted | Command returns conflict/validation error and state remains Draft. |
| WF-AT-047 | Validate journal date | Entry date is outside selected period or period Closed | Create/update/post is attempted | Command is rejected and no Posted entry is produced. |

## Reports

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-048 | Balance sheet | Posted lines exist through period end | Gerente/Admin queries | Cumulative signed Asset/Liability/Equity balances are returned; Draft/Cancelled lines are excluded. |
| WF-AT-049 | Income statement | Posted lines exist in requested period/range | Gerente/Admin queries | Income minus Cost/Expense and margin use documented formulas; Draft/Cancelled lines are excluded. |
| WF-AT-050 | Payroll report | Finalized period/results exist | Gerente/Admin queries with optional department | Stored department snapshots and cash/provision totals are used; bank data is absent. |
| WF-AT-051 | SUNAT report | Generated book version exists | Contador/Admin opens/exports selected version | Stored version rows/totals/format are returned; no report is regenerated by GET. |
| WF-AT-052 | Export report | Authorized report and filters are visible | Gerente/Admin requests PDF/Excel export then direct update/delete is attempted | Snapshot header/lines persist actor, filters, cutoff and values; grants/triggers reject later mutation. |
| WF-AT-053 | Report permission filtering | Users of all four roles authenticate | Navigation and report endpoints are accessed | Only Gerente/Admin access Reports; RRHH payroll totals stay in Payroll and Contador uses operational accounting/SUNAT views. |

## UX Governance

| Test | Workflow | Given | When | Then |
|---|---|---|---|---|
| WF-AT-054 | Dashboard navigation | Authenticated role opens Inicio | Dashboard loads by role | RRHH uses `/api/planilla/dashboard`, Contador `/api/comprobantes/dashboard`, Gerente `/api/reportes/dashboard`; only authorized cards/actions appear. |
| WF-AT-055 | Sidebar navigation | Role-specific session exists | Sidebar is rendered/used | Unauthorized modules are absent; selected module and target are stable. |
| WF-AT-056 | Breadcrumb behavior | User navigates list/detail/form | Route changes | Breadcrumb reflects documented hierarchy and returns without inventing a screen. |
| WF-AT-057 | Modal behavior | Destructive/lifecycle command needs confirmation | Modal opens/cancels/confirms | Focus is trapped/restored, consequence is explicit, and one command is submitted. |
| WF-AT-058 | Data grid filtering | Grid has data across periods/states | Filters change | Query parameters and visible filter summary match returned rows. |
| WF-AT-059 | Data grid sorting | Pageable grid has sortable columns | Sort changes | `sortBy`/`sortDirection` are sent and stable order is displayed. |
| WF-AT-060 | Data grid pagination | Collection exceeds page size | Page/page size changes | `pageInfo` and rows match requested page without losing filters/sort. |
| WF-AT-061 | Keyboard shortcuts | Focus is in documented context | Shortcut is used | Only documented action occurs; browser/system/text-entry shortcuts are not overridden. |
| WF-AT-062 | Empty/loading/error states | Query has no data, is pending, fails or outcome is unknown | State is rendered | Context remains visible and documented recovery action is offered without duplicate mutation. |
| WF-AT-063 | Accessibility requirements | Keyboard/screen-reader/high-contrast checks run | Representative screens are inspected | Focus, names, order, contrast, errors and status announcements meet `docs/ui/accessibility.md`. |
| WF-AT-064 | Form validation behavior | Invalid then valid form values are entered | Blur/submit occurs | Field errors map to API validation codes, focus reaches first error, and successful submit preserves lifecycle rules. |
