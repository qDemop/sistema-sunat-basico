# Payroll Business Rules

## Employee Validation Rules

- DNI is required, numeric, exactly 8 digits, and unique.
- Names and surnames are required, letters and spaces only, minimum 2 characters.
- Birth date is required and employee must be at least 18 years old.
- Hire date is required and cannot be future date.
- Salary base is required and must be greater than S/ 0.00.
- `DEPARTAMENTO` is required for employee assignment and payroll/report filtering.
- Discount type is required and must be AFP or ONP.
- Bank is required.
- Account number is required and must have 14 to 20 digits.

## Payroll Calculation Rules

- Payroll is calculated for a selected period in `YYYY-MM`.
- Only active employees are included.
- Gross salary starts from salary base.
- The PDF defines hourly rate as `salary_base / 240`.
- Overtime is stored in `HORAS_EXTRA` by employee and period using `horas_primeras_dos` and `horas_posteriores`.
- Overtime is based on hours worked and hourly rate; the PDF notes 25% surcharge for first two overtime hours and 35% for subsequent hours.
- AFP/ONP percentage is read from the single Active `CONFIG_DESCUENTO_PREVISIONAL_VERSION` effective on the last day of the period; absence or overlap is blocking.
- `total_bruto = salario_base + monto_horas_extra` and represents cash remuneration.
- `provision_gratificacion = total_bruto / 6`.
- `provision_cts = (total_bruto + provision_gratificacion) / 12`.
- Both provisions are accounting provisions and are excluded from `total_bruto` and `total_neto`.
- Legal July/December gratification payment and CTS deposit execution are out of scope.
- Total discounts equal AFP or ONP in the current scope; `descuentos_adicionales` is persisted as zero and configuration/capture is out of scope.
- Net pay is `gross_salary - total_discounts`.
- One `PeriodoPlanilla` owns the period state and starts as `Draft`.
- Existing results can be recalculated only while the period is `Draft`.
- Draft can transition to Finalized or Cancelled. Both are terminal; reopening or adjusting Finalized payroll is out of scope.
- Overtime is Approved before calculation. Registration, approval, or cancellation is allowed only before any payroll aggregate exists for that period.

## Persistence Rules

- Payroll lifecycle and totals are stored once in `PERIODO_PLANILLA`; employee results reference it.
- Payroll detail stores the applied pension configuration version, AFP/ONP amount, provision CTS, provision gratification, overtime, and additional discounts.
- `PLANILLA` stores the employee department assignment and applied base salary snapshots used by finalized reports and payroll-result responses.
- Payroll detail must store overtime amount used by the calculation.
- Recalculation policy must block changes unless the existing payroll is still `Draft`.

## Export Rules

- Payslip PDF export is available only after payroll has been calculated.
- Payroll Excel export must include all visible result columns and totals.
- Exported amounts must match persisted payroll results.
