# Reports Business Rules

## Dashboard Rules

- Dashboard values are calculated for the selected period.
- Default period is the current month.
- KPIs must refresh when the period changes.
- Users only see report modules permitted by role.
- All financial KPIs use Posted journal lines. Payroll KPIs use a Finalized `PERIODO_PLANILLA` only.
- `cuentasPorCobrar` is the Debit-nature signed balance of account `1212` through period end.
- `cuentasPorPagar` is the Credit-nature signed balance of account `4212` through period end.
- `igvPorPagar = creditos(40111) - debitos(40114)`; a negative value is reported as IGV credit.
- `ingresosTotales` is Credit minus Debit for Income accounts in the period.
- `costosGastos` is Debit minus Credit for Cost and Expense accounts in the period.
- `utilidadNeta = ingresosTotales - costosGastos`.
- `margenUtilidad = utilidadNeta / ingresosTotales * 100` when income is positive, otherwise 0.
- `costoPlanilla = total_bruto + provision_gratificacion + provision_cts`; `netoPlanilla = total_neto`.

## Balance Sheet Rules

- Balance sheet groups data into assets, liabilities, and equity.
- Values are calculated for a selected date or period end.
- Asset balances are Debit minus Credit; Liability and Equity balances are Credit minus Debit, cumulatively through period end.
- The report must expose enough detail to support strategic decision-making.
- Balance sheet values must be derived from posted `DETALLE_ASIENTO` lines grouped through `CUENTA_CONTABLE`.
- Draft journal entries are excluded.

## Income Statement Rules

- Income statement groups income, costs, expenses, and net result.
- Net utility is income minus costs and expenses.
- Margin is derived from net utility and income when income is greater than zero.
- Income statement values must be derived from posted `DETALLE_ASIENTO` lines grouped through `CUENTA_CONTABLE`.

## Consolidated Payroll Report Rules

- Payroll report totals must match Finalized payroll records.
- Report includes cash gross, discounts, provision CTS, provision gratification, total cost, and net pay.
- Period filters must be applied consistently across all totals.
- Department filters use the `PLANILLA.id_departamento` snapshot so later employee transfers do not change finalized history.

## Export Rules

- PDF export is intended for distribution.
- Excel export is intended for analysis.
- Exported report values must match the UI view and selected filters.
- Export creates immutable `REPORTE_SNAPSHOT` and `REPORTE_LINEA` records before the file is returned.
- Snapshot status is `Generated` or `Failed`; retry creates a new snapshot and never overwrites history.
