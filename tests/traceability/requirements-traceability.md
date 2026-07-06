# Requirements Traceability Matrix

## User Stories to Requirements

| Story | Requirement IDs | Modules |
|---|---|---|
| US-001 Authentication and System Access | RF-008, RF-ELI-01, RNF-002, RNF-ELI-02 | Authentication, Administration |
| US-002 Employee Registration | RF-001, RF-010, RF-ELI-02, RF-LUI-02, RF-LUI-03 | Payroll |
| US-003 Monthly Payroll Calculation | RF-002, RF-003, RF-011, RF-016, RF-ELI-03, RF-EDI-02, RNF-001, RNF-EDI-01 | Payroll, Reports |
| US-004 Accounting Voucher Registration | RF-004, RF-006, RF-ELI-04, RF-EDI-03 | Accounting SUNAT |
| US-005 Purchase and Sales Book Generation | RF-005, RF-006, RF-ELI-05, RF-EDI-02 | Accounting SUNAT |
| US-006 Financial Report Visualization | RF-007, RF-015, RF-LUI-04, RF-LUI-05 | Reports, General Ledger |
| Implicit Admin Need | RF-008, RF-EDI-05, RNF-002, RNF-ALV-05 | Administration, Authentication |

## P0 Cross-Cutting Requirements

| Requirement | Primary evidence | Acceptance evidence |
|---|---|---|
| RF-017 Reactivacion Logica | Module requirements, OpenAPI lifecycle commands, UX action matrix | WF-AT-005, 009, 015, 038 |
| RF-018 Tasas Previsionales Versionadas | Payroll rules/database, administration contracts | WF-AT-019, 020 |
| RF-019 Contabilizacion de Origenes | General Ledger source mapping and SQL procedure contracts | WF-AT-024, 035 |
| RF-020 Validacion y Formato SUNAT | Accounting SUNAT rules, OpenAPI and SUNAT states | WF-AT-027, 029-034 |
| RF-021 Fuentes de Reportes y Snapshots | Reports rules/database/API | WF-AT-048-053 |
| RF-022 Auditoria de Operaciones Sensibles | `docs/audit-event-catalog.md`, audit schema/function | WF-AT-001-053 where mutation/validation is sensitive |
| RF-023 Contratos API Completos | Six module OpenAPI files and shared components | Contract validation checklist in non-functional tests |
| RF-024 Trazabilidad de Flujos | `tests/traceability/workflow-traceability.md` | WF-AT-001 through WF-AT-064 |

## Workflow Range Ownership

| Workflow range | Requirement/rule owner | Domain/database owner | API/role owner | UX owner | Acceptance owner |
|---|---|---|---|---|---|
| WF-001..006 | Authentication/Administration | Usuario, Rol, LoginAttempt, TokenRevocation, AuditLog | Authentication and Administration OpenAPI | AUT/ADM, J-01/J-09 | WF-AT-001..006 |
| WF-007..024 | Payroll/Administration/General Ledger | Payroll aggregate, employee/configuration snapshots and source journal | Payroll and Administration OpenAPI; RRHH/Admin policies | EMP/PAY, J-02..04/J-10 | WF-AT-007..024 |
| WF-025..035 | Accounting SUNAT/General Ledger | Voucher, tax/format versions, books/bridge and source journal | Accounting SUNAT OpenAPI; Contador/Admin policies | ACC/SUN, J-05/J-07 | WF-AT-025..035 |
| WF-036..047 | General Ledger | Account, period, journal and lines | General Ledger OpenAPI; read/write role split | ACC, J-06 | WF-AT-036..047 |
| WF-048..053 | Reports | Posted ledger, finalized payroll and immutable report snapshots | Reports OpenAPI; Gerente/Admin policies | REP, J-08 | WF-AT-048..053 |
| WF-054..064 | UX governance | Existing module read projections and command guards | Existing module operations only | Dashboard/navigation/forms/grids/states/accessibility | WF-AT-054..064 |

Exact per-workflow links, role/action checks, audit event and acceptance ID are authoritative in `workflow-traceability.md`.

## Requirements to Source Documents

| Requirement Set | Source |
|---|---|
| RF-001 to RF-016 | `Entregable.pdf`, pages 36-37 and 86-88 plus resolved SDD gap decisions. |
| RF-017 to RF-024 | P0 remediation decisions in `docs/p0-decisions.md`, required to make existing workflows implementable without adding scope. |
| RF-ALV-01 to RF-ALV-05 | `Entregable.pdf`, pages 23-24 |
| RF-LUI-01 to RF-LUI-05 | `Entregable.pdf`, pages 25-27 |
| RF-ELI-01 to RF-ELI-05 | `Entregable.pdf`, pages 29-30 |
| RF-EDI-01 to RF-EDI-05 | `Entregable.pdf`, pages 32-34 |
| RNF consolidated | `Entregable.pdf`, pages 36-37 and 87-88 |
| RNF by role | `Entregable.pdf`, pages 24-36 |
| User stories US-001 to US-006 | `Entregable.pdf`, pages 17-22 |
| Database schema and BCNF | `Entregable.pdf`, pages 70-84 |
| ERP module scope | `BITACORA -ERP.pdf`, pages 1-2 |
