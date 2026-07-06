# User Journeys

## Journey Principles

Journeys are optimized for frequent use, low cognitive load, and safe completion of high-consequence work. Each journey defines the user's intent, minimum path, decision points, feedback, and failure handling.

Journey IDs are canonical in `ux-traceability.md`. Visible destinations and actions use Peruvian Spanish.

Canonical state names, allowed/invalid transitions, and correction behavior are defined only in `states.md`. Journeys must not imply a transition that `states.md` prohibits.

## J-01 - Iniciar Sesion y Entrar por Rol

| Item | Journey |
|---|---|
| Actor | Any authorized user. |
| Intent | Access only the modules assigned to the user's role. |
| Minimum path | Abrir ERP > ingresar credenciales > `Iniciar sesion` > `Inicio`. |
| Immediate feedback | Invalid credentials remain on Login with a field-level or form-level message. Successful login shows user name and role. |
| Critical UX rule | The user must not need to understand access rules; the visible navigation already reflects them. |
| Failure handling | Empty credentials, invalid credentials, inactive account, locked account, expired session. |

## J-02 - Registrar un Empleado

| Item | Journey |
|---|---|
| Actor | Administrador RRHH. |
| Intent | Add an employee so payroll data is complete. |
| Minimum path | `Inicio > Empleados > Nuevo empleado > Guardar empleado`. |
| Key decisions | DNI validity, department, employment dates, salary, pension regime, bank data. |
| Feedback | Inline validation while entering data; success message after save; new employee appears in filtered list. |
| Efficiency requirements | Search remains available before and after save; keyboard entry follows the form's natural business order. |
| Failure handling | Duplicate DNI, salary not positive, future hire date, employee under 18, invalid bank account, missing required fields. |

## J-02B - Editar o Desactivar un Empleado

| Item | Journey |
|---|---|
| Actor | Administrador RRHH. |
| Intent | Keep employee data accurate or remove employee from active operations. |
| Minimum path | `Empleados > buscar/seleccionar empleado > Editar` o `Desactivar`. |
| Key decisions | Whether the change affects payroll readiness for the current period. |
| Feedback | Changed fields are visible before save; deactivation requires confirmation and states downstream effect. |
| Failure handling | Employee locked by finalized payroll context, missing required corrections, restricted role. |

## J-03 - Registrar Horas Extra

| Item | Journey |
|---|---|
| Actor | Administrador RRHH. |
| Intent | Add approved overtime for a selected employee and period before payroll calculation. |
| Minimum path | `Empleados > Horas extra > seleccionar periodo > Registrar horas > Guardar`. |
| Key decisions | Period, employee, first two overtime hours, additional overtime hours. |
| Feedback | Period and employee identity remain visible; totals update after save. |
| Failure handling | Negative hours, duplicate employee-period record, closed or finalized payroll period. |

## J-04 - Calcular Planilla Mensual

| Item | Journey |
|---|---|
| Actor | Administrador RRHH. |
| Intent | Generate a draft payroll result for all active employees in a period. |
| Minimum path | `Inicio > Planillas > seleccionar periodo > Calcular planilla`. |
| Key decisions | Period, readiness of employee data, draft versus finalized status. |
| Feedback | Readiness checks before calculation, progress during calculation, result grid and totals after completion. |
| Efficiency requirements | Default to the latest Draft payroll period; if none exists, use the current month in America/Lima. Results expose export actions without additional navigation. |
| Failure handling | Period already finalized, no active employees, missing required employee data, calculation error. |

## J-04B - Revisar y Finalizar Planilla

| Item | Journey |
|---|---|
| Actor | Administrador RRHH. |
| Intent | Verify totals, export outputs, and lock payroll when ready. |
| Minimum path | `Planillas > periodo > revisar resultados y totales > Finalizar planilla`. |
| Key decisions | Whether totals are correct and whether recalculation should remain possible. |
| Feedback | Estado Borrador visible; `Finalizar planilla` explains that recalculation will be blocked. |
| Failure handling | Attempt to finalize incomplete or inconsistent result, export unavailable before calculation, unknown finalization outcome. A finalized payroll remains read-only; reopening and replacement are outside current scope. |

## J-05 - Registrar un Comprobante

| Item | Journey |
|---|---|
| Actor | Contador. |
| Intent | Record a purchase or sales document for accounting and SUNAT books. |
| Minimum path | `Inicio > Contabilidad > Nuevo comprobante > Guardar comprobante`. |
| Key decisions | Compra/Venta, document type, series, number, date, RUC/DNI, taxable base, operation type. |
| Feedback | IGV and total are visible as derived values; duplicate and format validations appear before save when possible. |
| Efficiency requirements | Data entry follows the physical voucher order. `Guardar y crear otro` retains only safe period/date/movement context and clears identity, party, and amounts. |
| Failure handling | Duplicate voucher, invalid RUC/DNI, negative amount, missing movement type, or missing effective tax configuration. |

## J-07 - Generar Libro SUNAT

| Item | Journey |
|---|---|
| Actor | Contador, including the Operador SUNAT persona. |
| Intent | Generate a versioned Purchase Book or Sales Book for a period. |
| Minimum path | `Inicio > SUNAT > seleccionar periodo y tipo de libro > Generar libro > Revisar/Exportar`. |
| Key decisions | Book type, period, validation warnings, and explicit confirmation to create the next version. Existing versions are never overwritten. |
| Feedback | Eligible/excluded voucher count, new sequential version, current/superseded relation, totals, validation and export state. |
| Failure handling | No eligible registered vouchers, missing configuration, blocking validation, unknown generation outcome, export failure. Replacement always creates a later immutable version. |

## J-08 - Revisar Reportes Financieros

| Item | Journey |
|---|---|
| Actor | Gerente Financiero. |
| Intent | Understand financial position and export evidence. |
| Minimum path | `Inicio > Reportes > elegir reporte o KPI > ajustar periodo > Exportar`. |
| Key decisions | Period, department, report type, level of detail. |
| Feedback | KPI values update with filter summary; report shows source period and last refresh. |
| Failure handling | No data for selected period, incomplete ledger, export unavailable due to missing report data. |

## J-09 - Administrar Usuarios y Roles

| Item | Journey |
|---|---|
| Actor | Administrador Sistema. |
| Intent | Create, modify, deactivate, or reset user access. |
| Minimum path | `Inicio > Administracion > Usuarios > seleccionar o crear usuario > Guardar usuario`. |
| Key decisions | Role, active/inactive state, password reset, access impact. |
| Feedback | User status and role are visible; risky changes are confirmed and audited. |
| Failure handling | Duplicate username, inactive role, required identity fields missing, restricted action. |

## J-10 - Cambiar Configuracion Tributaria o Formato SUNAT

| Item | Journey |
|---|---|
| Actor | Administrador Sistema. |
| Intent | Maintain versioned rules without breaking historical transactions. |
| Minimum path | `Administracion > configuracion > revisar version activa > Nueva version > Guardar version`. |
| Key decisions | Configuration type, effective date range, tax/pension rate or SUNAT format version, and reason for change. |
| Feedback | Draft, Active and Closed versions are visually separated; resolved consumers and effective dates are visible. |
| Failure handling | Overlapping date ranges, missing effective dates, invalid format structure, attempt to edit Active/Closed history. |

## J-06 - Mantener Contabilidad General

| Item | Journey |
|---|---|
| Actor | Contador or Administrador Sistema. |
| Intent | Maintain accounts, accounting periods, and balanced journal entries. |
| Minimum path | `Contabilidad > Plan de cuentas / Periodos contables / Asientos contables`. |
| Key decisions | Account hierarchy, open period, entry source, debit/credit balance, draft versus posted state. |
| Feedback | Debit and credit totals remain visible; `Contabilizar asiento` appears only when balanced and permitted. |
| Failure handling | Closed period, inactive account, imbalance, concurrent modification, unknown posting outcome. Draft may be cancelled; Posted is corrected only through a linked adjustment reversal in an Open period. |

## J-11 - Revisar Auditoria y Respaldos

| Item | Journey |
|---|---|
| Actor | Administrador Sistema. |
| Intent | Verify sensitive activity. Backup-status review is `IMPLEMENTATION PENDING`. |
| Minimum path | `Administracion > Registro de auditoria`; the backup destination is not active until a persistence/API contract exists. |
| Feedback | Filters, event result, timestamp in Lima time, affected object, and correlation reference remain visible. |
| Failure handling | No events, unavailable detail, restricted retention period. Backup status must not be simulated from undocumented data. |
