# Product Vision

## Vision Statement

The Sistema ERP - Modulo de Gestion Financiera y Contable is a Windows desktop ERP module built with C# and WinForms to automate payroll and SUNAT accounting processes for Peruvian organizations.

The system must provide accurate payroll calculation, compliant tax/accounting records, financial reporting, and controlled access by role. It must be documented, modular, testable, and ready for incremental Scrum delivery after Sprint 0.

## Business Objectives

- Automate monthly payroll calculations for active employees.
- Reduce manual accounting errors in IGV and SUNAT book generation.
- Provide management with timely financial dashboards and reports.
- Enforce secure access through users and role-based access rules.
- Maintain a normalized PostgreSQL database with referential integrity.
- Produce a specification baseline for development without generating application code yet.

## Product Modules

| Module | Business Goal |
|---|---|
| Authentication | Allow users to access only the modules authorized by their role. |
| Payroll | Manage employees and calculate payroll, benefits, discounts, and payslips. |
| Accounting SUNAT | Register vouchers, calculate IGV, and generate SUNAT Purchase/Sales books. |
| General Ledger | Maintain the chart of accounts, accounting periods, journal entries, posting and reversals. |
| Reports | Provide financial KPIs, balance sheet, income statement, and exports. |
| Administration | Manage users, roles, configuration, logs, and maintenance. |

## Success Criteria

- Payroll for 100 employees is calculated in 30 seconds or less.
- SUNAT books for 1,000 vouchers are generated in 60 seconds or less.
- Calculations are correct with a maximum tolerance of S/ 0.01 per employee.
- All protected operations require JWT authentication.
- Common user tasks can be completed in no more than 3 interactions from the main screen.
- The architecture allows modules to be replaced or extended independently.
