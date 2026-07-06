# Module Specifications

## Module Map

| Module | Spec Folder | Primary Requirements |
|---|---|---|
| Authentication | `specs/authentication/` | RF-008, RF-ELI-01, RNF-002. |
| Payroll | `specs/payroll/` | RF-001, RF-002, RF-003 plus `Departamento` and `HorasExtra` decisions. |
| Accounting SUNAT | `specs/accounting-sunat/` | RF-004, RF-005, RF-006 plus Compra/Venta and book versioning decisions. |
| General Ledger | `specs/general-ledger/` | RF-015, RF-019 plus chart, posting, cancellation and reversal decisions. |
| Reports | `specs/reports/` | RF-007 plus ledger-based financial statements. |
| Administration | `specs/administration/` | RF-008, RF-EDI-05, versioned tax configuration, operational requirements. |

## Authentication Module

### Responsibilities

- Login with username and password.
- BCrypt password verification.
- JWT generation and validation.
- Persistent failed-attempt lockout and JWT `jti` revocation on logout.
- Role-based module visibility for Sprint 1.

### Inputs

- Username.
- Password.
- Role assigned through Administration.

### Outputs

- JWT token.
- User summary.
- Allowed modules from role.
- Authentication/authorization errors.

### Acceptance Highlights

- Valid credentials open dashboard.
- Invalid credentials remain on login.
- Protected endpoints require token.
- Dashboard modules match role.
- Three failed attempts block login for 15 minutes and successful login resets the counter.
- Logout revokes the current token until its expiration.
- Sprint 1 authorization is role-based only.

### Dependencies

- `USUARIO`.
- `ROL`.
- `TOKEN_REVOCATION`.
- `LOGIN_ATTEMPT`.
- JWT provider.
- Audit log.

## Payroll Module

### Responsibilities

- `Departamento` catalog.
- Employee CRUD.
- Employee search, filters, department assignment, and logical deactivation.
- `HorasExtra` registration by employee and period.
- Period-level payroll calculation owned by `PERIODO_PLANILLA`.
- Effective-dated AFP/ONP, overtime, cash gross, discounts, net pay, and monthly CTS/gratification provision calculation.
- Payroll finalization and Draft cancellation by period.
- Payslip PDF and payroll Excel export.

### Inputs

- `Departamento` data.
- Employee personal and labor data.
- Payroll period.
- `HorasExtra` hours for first two hours and later hours.

### Outputs

- `Departamento` list.
- Employee list/detail.
- `HorasExtra` records.
- Draft/finalized/cancelled payroll-period aggregate with employee result rows.
- Payroll totals.
- Payslip PDF.
- Payroll Excel.

### Acceptance Highlights

- DNI is unique.
- Salary is positive.
- `Departamento` filter is available for reports and payroll views.
- `HorasExtra` values cannot be negative.
- AFP/ONP use the Active effective pension-rate version and store the applied version.
- Gratification and CTS values are monthly provisions excluded from cash gross and net pay; legal payment/deposit execution is out of scope.
- Payroll recalculation is allowed only while status is `Draft`.
- Finalized payroll cannot be overwritten.
- Payroll for 100 employees completes within 30 seconds.

### Dependencies

- `DEPARTAMENTO`.
- `EMPLEADO`.
- `TIPO_DESCUENTO`.
- `HORAS_EXTRA`.
- `PERIODO_PLANILLA`.
- `PLANILLA`.
- `DETALLE_PLANILLA`.
- `CONFIG_DESCUENTO_PREVISIONAL_VERSION`.
- `sp_calcular_planilla(periodo, actor)`.
- Reports module consumes payroll outputs.
- Ledger module may consume finalized payroll for accounting entries.

## Accounting SUNAT Module

### Responsibilities

- Register purchase and sales vouchers.
- Require Compra/Venta discriminator.
- Validate document type and document number.
- Validate duplicate voucher identity by type, movement, series, and number.
- Resolve active tax configuration version.
- Calculate IGV.
- Generate versioned Purchase Book and Sales Book.
- Validate book eligibility before generation.
- Resolve and persist the effective SUNAT format version.
- Link vouchers to generated books through bridge table.
- Export books to PDF and Excel.
- Support configurable SUNAT rates and formats.

### Inputs

- Movement type: Compra or Venta.
- Voucher type.
- Series and number.
- Date.
- Document type and document number.
- Business name.
- Taxable and exempt bases.
- Operation type.
- Period.

### Outputs

- Voucher records with applied tax version and persisted state Registrado/Anulado.
- Voucher list by filters.
- Generated book preview and version.
- Book-voucher links.
- Book totals.
- PDF/Excel exports.

### Acceptance Highlights

- Movement type is required.
- RUC is 11 digits when document type is RUC.
- DNI is 8 digits when document type is DNI.
- Duplicate vouchers are rejected by type, movement, series, and number.
- IGV uses persisted active tax configuration version.
- Purchase books include only Compra vouchers.
- Sales books include only Venta vouchers.
- Book generation creates a new version and bridge links.
- Each bridge row snapshots amounts and the voucher tax version; the book stores its format version and generator.
- Voucher validation result is computed; `Validado` is not a persisted voucher state.
- Book generation for 1,000 vouchers completes within 60 seconds.

### Dependencies

- `COMPROBANTE`.
- `TIPO_COMPROBANTE`.
- `CONFIG_TRIBUTARIA_VERSION`.
- `CONFIG_SUNAT_FORMATO`.
- `LIBRO_CONTABLE`.
- `TIPO_LIBRO`.
- `COMPROBANTE_LIBRO`.
- `sp_generar_libro(tipo, periodo, actor)`.
- Administration module for tax configuration.
- Ledger module for accounting entries.
- Reports module consumes accounting and ledger outputs.

## General Ledger Module

### Responsibilities

- Maintain accounting periods.
- Maintain chart of accounts.
- Create draft journal entries.
- Validate debit/credit balance.
- Post balanced journal entries.
- Cancel Draft journal entries and reverse Posted entries through linked Draft adjustments.
- Close Open accounting periods; reopening is out of scope.
- Create deterministic Draft entries from finalized payroll and registered vouchers.
- Provide ledger data for balance sheet and income statement.

### Inputs

- Accounting period.
- Account data.
- Journal entry header.
- Debit/credit lines.
- Source module/entity references from payroll or voucher operations.

### Outputs

- Accounting periods.
- Chart of accounts.
- Draft/posted/cancelled journal entries.
- Ledger balances for reports.

### Acceptance Highlights

- Account code is unique.
- Journal entry belongs to an open accounting period.
- Posted entries must balance debits and credits.
- Posted entries cannot be edited.
- Entry date must be inside the selected Open period.
- Source identity is unique and posting remains explicit.
- Balance sheet and income statement use posted ledger entries.

### Dependencies

- `PERIODO_CONTABLE`.
- `CUENTA_CONTABLE`.
- `ASIENTO_CONTABLE`.
- `DETALLE_ASIENTO`.
- Payroll and Accounting SUNAT source events.
- Initial chart of accounts and source-event mapping in `specs/general-ledger/business-rules.md`.

## Reports Module

### Responsibilities

- Dashboard KPIs.
- Balance sheet from ledger entries.
- Income statement from ledger entries.
- Consolidated payroll report from payroll records.
- Period and department filters.
- PDF and Excel exports.
- Reproducible export snapshots and export history.

### Inputs

- Accounting period or date range.
- `Departamento` filter.
- Report type.

### Outputs

- KPI cards.
- Balance sheet.
- Income statement.
- Consolidated payroll.
- Charts and exports.

### Acceptance Highlights

- Report values change when period changes.
- `Departamento` filter is backed by `DEPARTAMENTO`.
- Balance sheet shows assets, liabilities, and equity from ledger accounts.
- Income statement shows income, costs, expenses, and net result from ledger accounts.
- Exported values match displayed values.
- Accounts receivable use account `1212`; accounts payable use `4212`; all financial values use Posted entries only.
- Reports are restricted to Gerente Financiero and Administrador Sistema.

### Dependencies

- Payroll records.
- `Departamento` records.
- Voucher records.
- Generated books.
- Ledger entries and accounts.
- Reporting read models or snapshots.
- `REPORTE_SNAPSHOT` and `REPORTE_LINEA` for exports.

## Administration Module

### Responsibilities

- User management.
- Assignment of the four predefined roles; role creation/deletion is out of scope.
- Password reset.
- Versioned tax configuration.
- Versioned SUNAT format configuration.
- Audit log review.
- Backup and migration operational tracking is `IMPLEMENTATION PENDING`; no current table/API is claimed.
- Pension-rate version administration.

### Inputs

- User data.
- Role data.
- Tax configuration version values.
- SUNAT format version values.
- Date filters for audit.

### Outputs

- User lists.
- Role lists.
- Updated versioned configuration.
- Audit logs.
- Backup status is `IMPLEMENTATION PENDING`; operational backup/restore instructions remain external.

### Acceptance Highlights

- Only Administrador Sistema can manage users and roles.
- Role names are unique.
- User reactivation, lockout, logout revocation, and audit actor identity are explicit.
- Sprint 1 RBAC is role-only.
- Tax configuration versions cannot overlap for the same code and effective dates.
- Vouchers and books preserve applied configuration version.
- Tax configuration changes are audited.
- Database access follows least privilege.

### Dependencies

- `USUARIO`.
- `ROL`.
- `CONFIG_TRIBUTARIA_VERSION`.
- `CONFIG_DESCUENTO_PREVISIONAL_VERSION`.
- `CONFIG_SUNAT_FORMATO`.
- `AUDIT_LOG`.

## Cross-Module Integration Matrix

| Producer | Consumer | Data |
|---|---|---|
| Authentication | All modules | User identity, role, token. |
| Administration | Authentication | Users and roles. |
| Administration | Accounting SUNAT | Versioned tax rates, SUNAT codes, book formats. |
| Payroll | General Ledger | One source-linked Draft entry created from a finalized payroll period. |
| Payroll | Reports | Payroll totals, employee costs, consolidated payroll, department filters. |
| Accounting SUNAT | General Ledger | One source-linked Draft entry per registered voucher; annulment cancels Draft or creates reversal Draft. |
| Accounting SUNAT | Reports | IGV, voucher totals, book totals. |
| General Ledger | Reports | Balance sheet and income statement source data. |
| All modules | Audit | Sensitive operation events. |
