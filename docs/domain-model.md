# Domain Model

## Architecture Decisions Applied

| Decision | Status |
|---|---|
| Sprint 1 access control uses role-based RBAC only. | Applied. Permissions are out of Sprint 1 scope. |
| Employee belongs to Departamento. | Applied. |
| Payroll can include HorasExtra. | Applied. |
| Comprobante has Compra/Venta discriminator. | Applied. |
| Financial reports use accounting ledger entities. | Applied. |
| Tax configuration is persisted and versioned. | Applied. |
| Payroll recalculation is allowed only while payroll status is Draft. | Applied. |
| SUNAT books use bridge-table versioning. | Applied. |
| Payroll lifecycle belongs to one period aggregate. | Applied through `PeriodoPlanilla`. |
| Pension deductions use effective-dated persisted versions. | Applied. |
| Source events create deterministic Draft journal entries. | Applied. |
| Report exports persist reproducible snapshots. | Applied. |

## Bounded Contexts

| Context | Responsibility | Main Actors |
|---|---|---|
| Identity and Access | Authentication, JWT session, users, roles, and role-based module access. | Usuario, Administrador Sistema. |
| Payroll | Employee records, departments, overtime, salary calculation, AFP/ONP, CTS, gratification, and payslips. | Administrador RRHH. |
| SUNAT Accounting | Vouchers, Compra/Venta classification, IGV calculation, purchase/sales books, and SUNAT compliance. | Contador. |
| General Ledger | Accounting periods, accounts, journal entries, and accounting entry details. | Contador, Gerente Financiero. |
| Financial Reporting | KPIs, balance sheet, income statement, consolidated payroll, and exports. | Gerente Financiero. |
| Administration | Users, roles, versioned tax configuration, SUNAT format configuration and audit. Backup/restore is an external operational specification; application tracking is implementation pending. | Administrador Sistema, DBA. |

## Aggregates

### User Account Aggregate

| Element | Description |
|---|---|
| Aggregate Root | UserAccount. |
| Entities | UserAccount, Role. |
| Value Objects | Username, PasswordHash, AccessLevel, JwtId, LockoutWindow. |
| Invariants | Username is unique; password is stored as hash; inactive or temporarily locked users cannot authenticate; each user has exactly one predefined role; a logged-out `jti` is rejected until expiration. |
| Sprint 1 Scope | Role-based RBAC only. No permission catalog is required for Sprint 1. |
| Source Requirements | RF-008, RF-ELI-01, authentication wireframes. |

### Departamento Aggregate

| Element | Description |
|---|---|
| Aggregate Root | Departamento. |
| Entities | Departamento. |
| Value Objects | NombreDepartamento. |
| Invariants | Departamento name is unique; inactive departments cannot be assigned to new employees. |
| Source Requirements | Report filters by department, employee organization needs. |

### Employee Aggregate

| Element | Description |
|---|---|
| Aggregate Root | Employee. |
| Entities | Employee, PensionDiscountType, ConfiguracionDescuentoPrevisionalVersion. |
| Value Objects | DNI, FullName, SalaryAmount, BankAccount, EmploymentDate. |
| Relationships | Employee belongs to one required Departamento and one required PensionDiscountType; payroll resolves an effective ConfiguracionDescuentoPrevisionalVersion for that type. |
| Invariants | DNI is unique; names/surnames use letters and spaces with at least two characters; employee is at least 18; salary is positive; hire date is not future; discount type is AFP or ONP; Active pension-rate versions for the same type cannot overlap. |
| Source Requirements | RF-001, US-002, employee wireframes. |

### HorasExtra Aggregate

| Element | Description |
|---|---|
| Aggregate Root | HorasExtra. |
| Entities | HorasExtra. |
| Value Objects | Period, HorasPrimerasDos, HorasPosteriores. |
| Invariants | HorasExtra belongs to one Active employee and one period; hours cannot be negative and their sum is positive; one overtime record per employee/period; registration, approval and cancellation are rejected once any PeriodoPlanilla exists for that period. |
| Source Requirements | Payroll formula references overtime and surcharge rates. |

### PeriodoPlanilla Aggregate

| Element | Description |
|---|---|
| Aggregate Root | PeriodoPlanilla. |
| Entities | PeriodoPlanilla, Planilla, DetallePlanilla. |
| Value Objects | Period, Money, PayrollTotals, PayrollStatus. |
| Identity | The aggregate is addressed by its unique `periodo` (`YYYY-MM`) in commands, procedures, exports, and audit events. |
| Invariants | One aggregate exists per period; it includes one result per active employee; each result snapshots `salario_base_aplicado` and department assignment; `total_bruto = salario_base_aplicado + horas_extra`; `total_neto = total_bruto - total_descuentos`; provisions do not change cash gross or net; persisted detail sums match aggregate totals. |
| Recalculation Rule | Calculation creates Draft. Recalculation is allowed only in Draft. Draft can become Finalized or Cancelled. Finalized and Cancelled are terminal. |
| Provision Rule | `provision_gratificacion = total_bruto / 6`; `provision_cts = (total_bruto + provision_gratificacion) / 12`. Both are monthly accounting provisions. Legal payment/deposit execution is out of scope. |
| Source Requirements | RF-002, RF-003, US-003. |

### Comprobante Aggregate

| Element | Description |
|---|---|
| Aggregate Root | Comprobante. |
| Entities | Comprobante, TipoComprobante. |
| Value Objects | DocumentIdentity, VoucherNumber, Series, IssueDate, TaxableBase, IGV, OperationType, MovementType. |
| MovementType | Required discriminator with values `Compra` and `Venta`. |
| Invariants | Voucher type, movement type, series, and number are unique together; RUC has 11 digits when document type is RUC; IGV uses the applied tax configuration version; bases are non-negative with at least one positive base and derived total greater than zero; credit/debit notes reference an original voucher of the same movement; persisted state is `Registrado` or `Anulado`; a voucher linked to any generated book version is immutable except for annulment. |
| Source Requirements | RF-004, RF-006, US-004. |

### LibroContable Aggregate

| Element | Description |
|---|---|
| Aggregate Root | LibroContable. |
| Entities | LibroContable, TipoLibro, ComprobanteLibro. |
| Value Objects | Period, BookTotals, SUNATCode, BookVersion, SUNATValidationResult. |
| Versioning Rule | A generated SUNAT book is versioned by period and type. Vouchers are linked through a bridge entity, not a direct FK. |
| Invariants | Book is generated by an authenticated actor for one period, type, format version, and sequential version; only registered vouchers matching Compra/Venta and period are included; bridge rows snapshot amounts and each voucher's applied tax version; generated versions are immutable. |
| Source Requirements | RF-005, US-005. |

### Ledger Aggregate

| Element | Description |
|---|---|
| Aggregate Root | AsientoContable. |
| Entities | CuentaContable, PeriodoContable, AsientoContable, DetalleAsiento. |
| Value Objects | AccountCode, DebitAmount, CreditAmount, AccountingPeriod, EntryStatus. |
| Invariants | A journal entry belongs to one accounting period; its date is within that period; lines use Active accounts; debits equal credits before posting; posted entries cannot be edited or cancelled; reversal creates a linked Draft adjustment in an Open period; non-adjustment source identity is unique; account code is unique; account hierarchy cannot cycle and Posted use protects code/type/nature/parent. |
| Source Requirements | RF-007 financial statements and GAP-001 resolution. |

### Financial Report Aggregate

| Element | Description |
|---|---|
| Aggregate Root | FinancialReport. |
| Entities | ReportSnapshot, ReportLine. |
| Value Objects | ReportPeriod, KPI, Money, Percentage. |
| Invariants | Balance sheet and income statement use Posted ledger entries only; consolidated payroll uses Finalized payroll and its department snapshot; export snapshots preserve actor, filters, source cutoff, totals, and lines; exported values match the snapshot. |
| Source Requirements | RF-007, US-006. |

### ConfiguracionTributaria Aggregate

| Element | Description |
|---|---|
| Aggregate Root | ConfiguracionTributariaVersion. |
| Entities | ConfiguracionTributariaVersion, ConfiguracionSunatFormato. |
| Value Objects | EffectiveDateRange, ConfigurationCode, TaxPercentage, Version. |
| Versioning Rule | Tax configuration and SUNAT format configuration are persisted with effective dates. Vouchers store the applied tax version; generated books store the applied format version; bridge rows retain each voucher tax version. |
| Invariants | Configuration state is Draft/Active/Closed; Active ranges must not overlap for the same code/type and date range; Active/Closed business fields are immutable; historical records preserve original rates/formats; changes are audited. |
| Source Requirements | RF-006, RF-EDI-05, administration requirements. |

## Value Objects

| Value Object | Rules |
|---|---|
| Period | Format `YYYY-MM`; used for payroll, SUNAT books, reports, and accounting periods. |
| Money | Decimal amount, two decimal precision. Accounting entries may use debit/credit amounts; operational totals remain non-negative. |
| DNI | Exactly 8 numeric digits. |
| DocumentIdentity | Document type plus document number; RUC requires 11 digits, DNI requires 8 digits. |
| Username | At least 3 alphanumeric characters; unique. |
| PasswordHash | BCrypt hash only; plaintext is forbidden. |
| JWTClaims | User ID, name, predefined role, `jti`, issued-at, expiration. |
| VoucherIdentity | Voucher type, movement type, series, and number. |
| MovementType | `Compra` or `Venta`. |
| TaxRateVersion | Percentage plus effective date range and version identity. |
| SalaryBase | Positive monthly salary in soles. |
| PayrollStatus | `Draft`, `Finalized`, `Cancelled`. |
| OvertimeStatus | `Draft`, `Approved`, `Cancelled`. |
| ConfigurationStatus | `Draft`, `Active`, `Closed`. |
| VoucherStatus | `Registrado`, `Anulado`. |
| SUNATValidationResult | `Valida`, `ConObservaciones`, `Bloqueada`; `Pendiente` is UI-only. |
| BookVersion | Positive integer per book type and period. |
| AccountCode | Unique accounting account code. |
| EntryStatus | `Draft`, `Posted`, `Cancelled`. |

## Domain Services

| Service | Responsibility |
|---|---|
| AuthenticationService | Validate credentials, apply 3-attempt/15-minute lockout, issue JWT with `jti`, and reject revoked tokens. |
| AuthorizationService | Determine module access by role only for Sprint 1. |
| PayrollCalculationService | Resolve the effective pension-rate version and calculate cash gross, deductions, net pay, and monthly CTS/gratification provisions for a PeriodoPlanilla. |
| VoucherTaxService | Resolve active tax configuration version, calculate IGV, and validate voucher totals. |
| SUNATBookService | Validate eligibility, resolve the effective SUNAT format, and build immutable versioned books using snapshot bridge links. |
| LedgerPostingService | Create one source-linked balanced Draft entry from PayrollFinalized or VoucherRegistered and create reversal Drafts for posted source corrections. |
| FinancialReportingService | Calculate defined KPIs, cumulative balance sheet, period income statement, and finalized payroll consolidation; create reproducible export snapshots. |
| AuditService | Record user, date, operation, entity, result, and correlation ID for sensitive actions. |

## Domain Events

| Event | Trigger | Consumers |
|---|---|---|
| UserAuthenticated | Valid login. | Audit, dashboard session. |
| UserLoginFailed | Invalid login. | Audit, lockout policy. |
| UserLoggedOut | Current JWT is revoked. | Token revocation, audit. |
| UserReactivated | Inactive user is reactivated. | Authentication, audit. |
| DepartamentoCreated | Departamento registered. | Audit, employee assignment. |
| EmployeeRegistered | New employee saved. | Audit, payroll readiness. |
| EmployeeDeactivated | Employee deactivated. | Audit, payroll inclusion rules. |
| OvertimeRegistered | Overtime recorded for employee and period. | Payroll calculation, audit. |
| OvertimeApproved | Draft overtime approved. | Payroll readiness, audit. |
| OvertimeCancelled | Draft/approved overtime cancelled before payroll Draft. | Payroll readiness, audit. |
| PayrollDraftComputed | Draft period payroll completed. | Reports, audit, export availability. |
| PayrollFinalized | Payroll marked as finalized. | Ledger posting, reports, audit. |
| PayrollDraftCancelled | Draft payroll period cancelled. | Audit. |
| VoucherRegistered | Voucher saved with Compra/Venta discriminator and tax version. | SUNAT books, ledger posting, reports, audit. |
| VoucherAnnulled | Voucher state changed to anulado. | SUNAT books, ledger posting, reports, audit. |
| AccountingBookGenerated | Versioned purchase or sales book generated. | Reports, audit, export availability. |
| JournalEntryPosted | Accounting entry posted. | Financial reports, audit. |
| JournalEntryReversalRequested | Posted entry requires correction. | Linked Draft adjustment, audit. |
| AccountingPeriodClosed | Open period closed. | Posting guard, audit. |
| ConfiguracionTributariaChanged | Tax rate or SUNAT format version changed. | Audit, validation of future transactions. |

## Key Domain Rules

| Rule ID | Rule |
|---|---|
| DM-RULE-001 | Sprint 1 authorization is role-based only. |
| DM-RULE-002 | Users can access only modules granted to their role. |
| DM-RULE-003 | AFP deduction uses the single Active AFP configuration version effective on the payroll period end date. |
| DM-RULE-004 | ONP deduction uses the single Active ONP configuration version effective on the payroll period end date. |
| DM-RULE-005 | Overtime uses 25% surcharge for the first two overtime hours and 35% for additional overtime hours. |
| DM-RULE-006 | Monthly gratification provision is `total_bruto / 6` and is excluded from cash gross and net pay. |
| DM-RULE-007 | Monthly CTS provision is `(total_bruto + provision_gratificacion) / 12` and is excluded from cash gross and net pay. |
| DM-RULE-008 | PeriodoPlanilla recalculation is allowed only in `Draft`; Finalized and Cancelled are terminal. |
| DM-RULE-009 | IGV is calculated from the persisted active tax configuration version applied to the voucher date. |
| DM-RULE-010 | Comprobante must specify `Compra` or `Venta`; SUNAT books filter by that discriminator. |
| DM-RULE-011 | SUNAT books are versioned and linked to vouchers through a bridge entity. |
| DM-RULE-012 | Posted accounting entries must balance total debits and credits. |
| DM-RULE-013 | Balance sheet and income statement derive from ledger entries, not directly from raw vouchers alone. |
| DM-RULE-014 | Payroll and accounting operations must be auditable. |
| DM-RULE-015 | Report exports must match displayed filtered values. |
| DM-RULE-016 | Configuration changes must preserve historical transaction meaning. |
| DM-RULE-017 | Three consecutive failed logins block the user for 15 minutes; successful login resets the counter. |
| DM-RULE-018 | Logout revokes the current JWT `jti` until expiration. |
| DM-RULE-019 | Gerente Financiero may query but never mutate General Ledger; Administrador RRHH has no Reports module access. |
| DM-RULE-020 | Voucher persisted state is Registrado or Anulado; SUNAT validation is a separate computed result. |
| DM-RULE-021 | Book generation is a command that always creates the next immutable version after successful validation. |
| DM-RULE-022 | Journal entry dates must fall within the selected Open period; Closed periods cannot reopen. |
| DM-RULE-023 | Posted entries are corrected only by a linked reversal Draft in an Open period. |
| DM-RULE-024 | Balance sheet, income statement, receivables, payables, IGV, and utility use the account/formula mapping in `specs/reports/business-rules.md`. |
