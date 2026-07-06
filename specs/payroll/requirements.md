# Payroll Requirements

Source: `BITACORA -ERP.pdf`, page 2; `Entregable.pdf`, pages 8-10, 11-16, 18-20, 36-37, 100-104.

## Purpose

Manage employees and calculate payroll according to Peruvian payroll rules defined in the PDFs.

## Actors

- Administrador RRHH.
- Administrador Sistema.

## Functional Requirements

| ID | Requirement |
|---|---|
| PAY-FR-001 | The system must register, edit, consult, search, filter, and logically deactivate employees. |
| PAY-FR-002 | Employee form must capture DNI, names, surnames, birth date, hire date, department, job, salary base, AFP/ONP, bank, and account number. |
| PAY-FR-003 | Employee list must show DNI, full name, department, job, salary base, discount type, hire date, and status. |
| PAY-FR-004 | The payroll calculation screen must allow selecting month and year. |
| PAY-FR-005 | The system must create one `PERIODO_PLANILLA` aggregate per month and calculate one result for every active employee. |
| PAY-FR-006 | The calculation must include cash gross, overtime, effective-dated AFP/ONP, monthly CTS and gratification provisions, and net pay. Additional discounts are persisted as S/ 0.00; their capture/configuration is out of scope. |
| PAY-FR-007 | The system must persist lifecycle state in `PERIODO_PLANILLA`, employee results in `PLANILLA`, and applied concepts/configuration in `DETALLE_PLANILLA`. |
| PAY-FR-008 | The UI must show cash gross, AFP/ONP, provision CTS, provision gratification, discounts, net pay, and period totals without presenting provisions as paid cash. |
| PAY-FR-009 | The system must export individual payslips to PDF and payroll to Excel. |
| PAY-FR-011 | The system must manage a `DEPARTAMENTO` catalog for employee assignment and report filtering. |
| PAY-FR-012 | The system must manage `HORAS_EXTRA` through Draft, Approved, and Cancelled states; only Approved records are calculated. |
| PAY-FR-013 | Payroll calculation creates a Draft period; recalculation is allowed only in Draft; Draft can become Finalized or Cancelled; terminal states cannot reopen. |
| PAY-FR-014 | Employee and department reactivation must be explicit and must restore eligibility only for future or recalculated Draft periods. |
| PAY-FR-015 | The system must resolve one Active effective-dated pension-rate version for the employee regime at period end and store that version with the result. |
| PAY-FR-016 | Finalizing a period must create exactly one source-linked balanced Draft journal entry using the approved payroll mapping. |
| PAY-FR-017 | `GET /api/planilla/dashboard` must provide RRHH/Admin operational dashboard counts, period state and payroll totals from Payroll sources only. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| PAY-NFR-001 | Payroll for 100 employees must calculate in 30 seconds or less. |
| PAY-NFR-002 | Calculation tolerance must be no greater than S/ 0.01 per employee. |
| PAY-NFR-003 | Employee list and result tables must support sorting, filtering, pagination, and totals. |
| PAY-NFR-004 | Common payroll tasks must be completable in no more than 3 interactions from the dashboard. |
| PAY-NFR-005 | Payroll operations must be auditable by user, date, period, and result. |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| PAY-FR-001..003 | RF-001 (Gestion de Empleados) |
| PAY-FR-004..008, 015 | RF-002 (Calculo de Planillas), RF-018 (Tasas Previsionales) |
| PAY-FR-009 | RF-003 (Reporte de Pago) |
| PAY-FR-011 | RF-010 (Gestion de Departamentos) |
| PAY-FR-012 | RF-011 (Registro de Horas Extra) |
| PAY-FR-013 | RF-016 (Ciclo de Periodo de Planilla) |
| PAY-FR-014 | RF-017 (Reactivacion Logica) |
| PAY-FR-016 | RF-019 (Contabilizacion de Origenes) |
| PAY-FR-017 | RF-023 (Contratos API - dashboard) |
| PAY-NFR-001 | RNF-001 (Rendimiento 30s/100emp), RNF-EDI-01 |
| PAY-NFR-003..004 | RNF-003 (Usabilidad) |
| PAY-NFR-005 | RF-022 (Auditoria) |

### User Stories and Acceptance Tests

| User story | Workflows | Acceptance test IDs |
|---|---|---|
| US-002 (Employee registration) | WF-013 Employee creation, WF-014 Employee update, WF-015 Employee deactivate/reactivate | WF-AT-013, WF-AT-014, WF-AT-015 |
| US-003 (Payroll calculation) | WF-016..018 Overtime, WF-019 Calculate, WF-020 Recalculate, WF-021 Finalize, WF-022 Cancel, WF-023 Audit, WF-024 Payroll-to-GL | WF-AT-016..WF-AT-024 |

Department workflows: WF-AT-007, WF-AT-008, WF-AT-009. Full workflow detail: `tests/traceability/workflow-traceability.md` (Payroll table).
