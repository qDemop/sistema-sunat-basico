# User Stories

The following user stories are extracted from the initial product backlog in `Entregable.pdf`, pages 17-22. Acceptance criteria are derived from the documented flows.

## US-001: Authentication and System Access

As a system user, I want to authenticate with secure credentials so that I can access the modules assigned to my role.

### Flow

1. User opens the WinForms application and sees the login screen.
2. User enters username and password.
3. Frontend sends `POST /api/auth/login`.
4. Backend searches `USUARIO` and verifies password hash with BCrypt.
5. Backend returns signed JWT with ID, name, role, and expiration.
6. Frontend stores token in memory and uses `Authorization: Bearer <token>`.
7. Dashboard shows modules according to role.
8. Invalid credentials show "Usuario o contrasena incorrectos".

### Acceptance Criteria

- Valid credentials return a JWT and dashboard access.
- Invalid credentials do not navigate away from login.
- Visible modules must match user role access rules.
- All subsequent protected requests include the JWT.

## US-002: Employee Registration

As an Administrador RRHH, I want to register a new employee so that the personnel payroll stays updated.

### Flow

1. Administrator logs in with JWT.
2. Dashboard shows available modules.
3. Administrator opens Payroll and selects Employees.
4. System lists employees with search/filter options.
5. Administrator opens New Employee form.
6. Administrator enters DNI, names, surnames, birth date, hire date, job, base salary, discount type, bank, and account number.
7. Frontend sends `POST /api/empleados`.
8. Backend validates unique DNI, format, and salary greater than zero.
9. Backend inserts the employee into `EMPLEADO`.
10. UI confirms successful registration and refreshes the list.

### Acceptance Criteria

- Employee cannot be created with duplicate DNI.
- Salary must be greater than zero.
- Invalid fields show specific error messages.
- Successful creation returns employee ID and refreshes the employee list.

## US-003: Monthly Payroll Calculation

As an Administrador RRHH, I want to execute the automatic monthly payroll calculation so that I obtain earnings, discounts, and net pay for all employees.

### Flow

1. Administrator opens Payroll Calculation.
2. System shows period selector, active employees, and Calculate button.
3. Administrator selects a period such as `2026-05`.
4. Frontend sends calculation request with selected period.
5. Backend executes `sp_calcular_planilla(periodo)`.
6. Procedure creates or recalculates the Draft `PERIODO_PLANILLA`, resolves each employee's effective AFP/ONP version, and calculates cash gross, deductions, net pay, and monthly CTS/gratification provisions.
7. Procedure inserts period lifecycle in `PERIODO_PLANILLA` and employee results in `PLANILLA`/`DETALLE_PLANILLA`.
8. Backend returns employee-level results.
9. UI shows table and totals.
10. Administrator may export PDF payslips as a ZIP and export payroll to Excel.

### Acceptance Criteria

- Calculation includes all active employees for the selected period.
- AFP/ONP use the Active persisted version effective at period end and retain the applied version.
- Net pay equals gross salary minus discounts.
- CTS and gratification are monthly provisions shown separately and excluded from cash gross/net; legal payment/deposit execution is out of scope.
- Draft periods can be recalculated, finalized, or cancelled; terminal periods cannot reopen.
- Results are persisted and displayed with totals.
- Payslip PDF export is available after calculation.

## US-004: Accounting Voucher Registration

As a Contador, I want to register a purchase or sales voucher so that the accounting record stays updated and SUNAT books can be generated.

### Flow

1. Accountant logs in and opens Accounting SUNAT.
2. System shows Comprobantes, Libro Compras, Libro Ventas, and Reports.
3. Accountant selects New Voucher.
4. System shows type, series, number, issue date, document type, document number, business name, taxable base, and operation type.
5. System calculates IGV using the Active persisted tax version effective on the issue date.
6. Accountant saves.
7. System validates 11-digit RUC format and duplicate series/number.
8. System saves the Registrado voucher and creates one source-linked Draft journal entry using the Compra/Venta mapping.

### Acceptance Criteria

- RUC must be 11 digits for RUC documents.
- Duplicate voucher series and number are rejected.
- IGV is automatically calculated for taxable operations.
- Saved voucher is visible in the period list.

## US-005: Purchase and Sales Book Generation

As a Contador, I want to generate the Purchase Book and Sales Book for a period so that I comply with SUNAT tax obligations.

### Flow

1. Accountant opens Accounting SUNAT and selects Generate Books.
2. System shows options for Purchase Book and Sales Book with a period selector.
3. Accountant selects period/type and runs SUNAT validation.
4. When validation is not Bloqueada, the accountant confirms `POST /api/libros`; the system queries registered matching vouchers.
5. System structures data using SUNAT fields: period, RUC, voucher type, series, number, date, taxable base, exempt base, IGV, total.
6. System groups by voucher type and calculates group totals.
7. System resolves the Active SUNAT format, creates the next immutable version, snapshots voucher tax versions, and previews totals.
8. Accountant exports to PDF or Excel; optional direct SUNAT submission remains future scope.
9. Same flow applies for Purchase Book using purchase vouchers.

### Acceptance Criteria

- Only registered vouchers for the selected period are included.
- Book preview uses required SUNAT columns.
- Totals by group and general totals are calculated.
- PDF and Excel export options are available.

## US-006: Financial Report Visualization

As a Gerente Financiero, I want to view the balance sheet and income statement so that I can make informed strategic decisions.

### Flow

1. Financial Manager logs in and is redirected to dashboard.
2. Dashboard shows payroll total, sales income, sales cost, gross profit, IGV payable, accounts receivable, and accounts payable.
3. Manager selects Balance Sheet.
4. System shows assets, liabilities, and equity.
5. Manager changes report period.
6. System updates balance/income values from Posted ledger entries and payroll consolidation from Finalized payroll-period results.
7. Manager exports to PDF or Excel.

### Acceptance Criteria

- Report data changes when period changes.
- Balance sheet groups assets, liabilities, and equity.
- Dashboard exposes financial KPIs.
- PDF and Excel export actions are available.

## Extracted Implicit User Need: User and Role Administration

The PDFs do not define this as a numbered backlog story, but RF-008 and the administration wireframe require it.

As an Administrador Sistema, I need to create, modify, deactivate, and assign roles to user accounts so that system access is controlled by responsibility.
