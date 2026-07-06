# Source Analysis

## Analyzed Documents

| Source | Pages | Extracted Themes |
|---|---:|---|
| `BITACORA -ERP.pdf` | 2 | ERP objective, team roles, C#, PostgreSQL, financial/accounting modules, payroll, SUNAT books, reports. |
| `Entregable.pdf` | 106 | Sprint 0 documentation, product vision, user stories, functional requirements, non-functional requirements, UML descriptions, database normalization, architecture, wireframes. |

## Project Identity

- Project name: Sistema ERP - Modulo de Gestion Financiera y Contable.
- Project code: ERP-GFC-001-2026.
- Institution: Universidad Nacional de Juliaca.
- Course: Taller de Procesos ERP / ERP Process Workshop.
- Methodology: Scrum, 4-week sprints.
- Sprint 0 focus: analysis, requirements, UML, initial architecture, database model.

## Extracted Product Scope

The PDFs define an ERP module for Peruvian financial and accounting management with these major capabilities:

- Payroll management for employee data, monthly payroll calculation, AFP/ONP discounts, CTS, gratifications, and PDF payslips.
- SUNAT accounting for purchase/sales vouchers, IGV calculation, electronic Purchase Book and Sales Book generation.
- Financial reports for balance sheet, income statement, consolidated payroll, KPIs, and export to PDF/Excel.
- Authentication and role-based access using JWT, BCrypt password hashing, and predefined roles.
- Administration for users, roles, role-based access, configuration, backups, logs, and tax-rate updates.

## Explicit Roles

| Role | Responsibility |
|---|---|
| Administrador RRHH | Employee management, payroll processing, payslip generation. |
| Contador | Voucher registration, IGV calculation, SUNAT books, accounting compliance. |
| Gerente Financiero | Financial dashboards, balance sheet, income statement, exportable reports. |
| Administrador Sistema | Users, roles, role-based access, security, logs, backup, configuration. |
| Product Owner | Product vision, backlog priority, deliverable validation. |
| Analista de Requisitos | Requirement elicitation, backlog, user stories, SRS documentation. |
| Frontend Developer | WinForms UI, forms, dashboards, usability and accessibility. |
| Backend Developer | Business logic, REST services, JWT, database integration. |
| Arquitecto / DBA | PostgreSQL schema, normalization, procedures, indexes, security, backups. |

## Source Page Map

| Topic | Source |
|---|---|
| ERP objective, roles, module list | `BITACORA -ERP.pdf`, pages 1-2 |
| Introduction, methods, objectives | `Entregable.pdf`, pages 6-8 |
| Product vision and modules | `Entregable.pdf`, pages 8-11 |
| Actors and system model | `Entregable.pdf`, pages 11-13 |
| Payroll/accounting formulas | `Entregable.pdf`, pages 13-16 |
| Initial backlog and user stories | `Entregable.pdf`, pages 17-22 |
| Requirements by role | `Entregable.pdf`, pages 23-36 |
| Consolidated requirements | `Entregable.pdf`, pages 36-37 and 86-88 |
| Architecture and components | `Entregable.pdf`, pages 48-58 and 88-92 |
| UI states and wireframes | `Entregable.pdf`, pages 59-67 and 98-105 |
| Database model and normalization | `Entregable.pdf`, pages 68-84 |
| Technical acceptance criteria | `Entregable.pdf`, page 92 |
