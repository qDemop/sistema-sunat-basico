# Requirements Traceability Matrix

> Sprint 0 remediation: expanded from 8 explicit rows to all 72 requirement IDs. ALV family (requirements-management meta-tool) is explicitly OUT OF SCOPE for the MVP ERP.

## User Stories to Requirements

| Story | Requirement IDs | Modules |
|---|---|---|
| US-001 Authentication and System Access | RF-008, RF-ELI-01, RNF-002, RNF-ELI-02 | Authentication, Administration |
| US-002 Employee Registration | RF-001, RF-010, RF-ELI-02, RF-LUI-02, RF-LUI-03 | Payroll |
| US-003 Monthly Payroll Calculation | RF-002, RF-003, RF-011, RF-016, RF-ELI-03, RF-EDI-02, RNF-001, RNF-EDI-01 | Payroll, Reports |
| US-004 Accounting Voucher Registration | RF-004, RF-006, RF-ELI-04, RF-EDI-03 | Accounting SUNAT |
| US-005 Purchase and Sales Book Generation | RF-005, RF-006, RF-ELI-05, RF-EDI-02 | Accounting SUNAT |
| US-006 Financial Report Visualization | RF-007, RF-015, RF-LUI-04, RF-LUI-05 | Reports, General Ledger |
| US-007 User and Role Administration (implicit) | RF-008, RF-017, RF-EDI-05, RNF-002, RNF-ALV-05 | Administration, Authentication |

## Full Requirement Traceability Matrix

### Consolidated Functional Requirements

| ID | Spec file | DB table(s) | API operationId(s) | Acceptance test |
|---|---|---|---|---|
| RF-001 | `specs/payroll/requirements.md` PAY-FR-001..003 | `empleado` | `createEmpleado`, `updateEmpleado`, `getEmpleado`, `listEmpleados`, `deactivateEmpleado`, `reactivateEmpleado` | WF-AT-013, WF-AT-014, WF-AT-015, US-002-SC-01..05 |
| RF-002 | `specs/payroll/requirements.md` PAY-FR-004..008, 015 | `periodo_planilla`, `planilla`, `detalle_planilla`, `config_descuento_previsional_version` | `calculatePayroll` | WF-AT-019, WF-AT-020, US-003-SC-01..09 |
| RF-003 | `specs/payroll/requirements.md` PAY-FR-009 | `planilla`, `detalle_planilla` | `exportPayslipPdf`, `exportPayrollExcel` | US-003-SC-11 |
| RF-004 | `specs/accounting-sunat/requirements.md` ACC-FR-001..004, 012, 013, 017, 018 | `comprobante`, `tipo_comprobante` | `createComprobante`, `updateComprobante`, `annulComprobante` | WF-AT-025..028, WF-AT-035, US-004-SC-01..10 |
| RF-005 | `specs/accounting-sunat/requirements.md` ACC-FR-006..008, 010, 011, 014..016, 019 | `libro_contable`, `comprobante_libro`, `tipo_libro` | `validateSunatBook`, `generateSunatBook`, `getSunatBook` | WF-AT-029..033, US-005-SC-01..08 |
| RF-006 | `specs/accounting-sunat/requirements.md` ACC-FR-003, 009; `specs/administration/requirements.md` ADM-FR-006, 012 | `config_tributaria_version`, `config_sunat_formato` | `createTaxVersion`, `activateTaxVersion`, `closeTaxVersion`, `createSunatFormatVersion`, `activateSunatFormatVersion`, `closeSunatFormatVersion` | WF-AT-010..012, WF-AT-034 |
| RF-007 | `specs/reports/requirements.md` REP-FR-001..009 | `reporte_snapshot`, `reporte_linea` | `getDashboardKpis`, `getBalanceSheet`, `getIncomeStatement`, `getConsolidatedPayroll` | WF-AT-048..050, US-006-SC-01..07 |
| RF-008 | `specs/authentication/requirements.md` AUTH-FR-001..013; `specs/administration/requirements.md` ADM-FR-001..005, 011 | `usuario`, `rol`, `login_attempt`, `token_revocation` | `login`, `logout`, `getCurrentUser`, `createUser`, `updateUser`, `deactivateUser`, `reactivateUser`, `listPredefinedRoles` | WF-AT-001..006, US-001-SC-01..06, US-007-SC-01..06 |
| RF-010 | `specs/payroll/requirements.md` PAY-FR-011 | `departamento` | `createDepartamento`, `updateDepartamento`, `listDepartamentos`, `deactivateDepartamento`, `reactivateDepartamento` | WF-AT-007..009 |
| RF-011 | `specs/payroll/requirements.md` PAY-FR-012 | `horas_extra` | `createHorasExtra`, `approveHorasExtra`, `cancelHorasExtra` | WF-AT-016..018, US-003-SC-03 |
| RF-015 | `specs/general-ledger/requirements.md` GL-FR-001..011 | `cuenta_contable`, `periodo_contable`, `asiento_contable`, `detalle_asiento` | `createAccount`, `updateAccount`, `createAccountingPeriod`, `closeAccountingPeriod`, `createJournalEntry`, `postJournalEntry`, `cancelJournalEntry`, `reverseJournalEntry` | WF-AT-036..047 |
| RF-016 | `specs/payroll/requirements.md` PAY-FR-013 | `periodo_planilla` | `calculatePayroll`, `finalizePayroll`, `cancelPayroll` | WF-AT-020..022, US-003-SC-04, 05, 10 |
| RF-017 | `specs/payroll/requirements.md` PAY-FR-014; `specs/general-ledger/requirements.md` GL-FR-008; `specs/administration/requirements.md` ADM-FR-001 | `empleado`, `departamento`, `cuenta_contable`, `usuario` | `reactivateEmpleado`, `reactivateDepartamento`, `reactivateAccount`, `reactivateUser` | WF-AT-005, 009, 015, 038 |
| RF-018 | `specs/payroll/requirements.md` PAY-FR-015; `specs/administration/requirements.md` ADM-FR-006 | `config_descuento_previsional_version` | `createPensionVersion`, `activatePensionVersion`, `closePensionVersion` | WF-AT-010..012, US-003-SC-06, 07 |
| RF-019 | `specs/payroll/requirements.md` PAY-FR-016; `specs/accounting-sunat/requirements.md` ACC-FR-017; `specs/general-ledger/requirements.md` GL-FR-012 | `asiento_contable`, `detalle_asiento` | `finalizePayroll`, `createComprobante`, `postJournalEntry` | WF-AT-024, 035, US-003-SC-09, US-004-SC-08 |
| RF-020 | `specs/accounting-sunat/requirements.md` ACC-FR-015, 016, 019 | `config_sunat_formato`, `libro_contable`, `comprobante_libro` | `validateSunatBook`, `generateSunatBook` | WF-AT-029..034, US-005-SC-05, 06 |
| RF-021 | `specs/reports/requirements.md` REP-FR-010, 011 | `reporte_snapshot`, `reporte_linea` | `createReportExport`, `listReportExports`, `downloadReportExport` | WF-AT-052, US-006-SC-07 |
| RF-022 | `docs/audit-event-catalog.md`; all module `business-rules.md` audit sections | `audit_log` | `listAuditEvents`, `getAuditEvent` | WF-AT-001..064 (sensitive mutations); `tests/acceptance/audit-acceptance.md` AUD-AT-001..040 |
| RF-023 | `specs/*/api-contract.yaml` (6 files); `specs/shared-api-components.yaml` | n/a (contract requirement) | All operationIds across 6 contracts | `tests/non-functional/non-functional-tests.md` (contract validation checklist) |
| RF-024 | `tests/traceability/workflow-traceability.md` | n/a (traceability requirement) | n/a | WF-AT-001..064 |

### Requirements by Role: Analysis and Documentation (OUT OF SCOPE)

| ID | Status | Rationale |
|---|---|---|
| RF-ALV-01 | OUT OF SCOPE | Requirements-management meta-tool (stakeholder interviews). Not part of the 6 ERP modules. The MVP uses `docs/requirements.md` as the canonical catalog, not a built-in requirements tool. |
| RF-ALV-02 | OUT OF SCOPE | User-story redaction tool. Same rationale. |
| RF-ALV-03 | OUT OF SCOPE | Backlog management tool. Same rationale. |
| RF-ALV-04 | OUT OF SCOPE | Requirement validation tool. Same rationale. |
| RF-ALV-05 | OUT OF SCOPE | SRS generation tool. Same rationale. |

> Decision: RF-ALV-01..05 describe a requirements-management application, not ERP business features. They are OUT OF SCOPE for the MVP. Reinstating any of them requires a new scope decision in `docs/p0-decisions.md`.

### Requirements by Role: WinForms UI

| ID | Spec file | UX docs | Acceptance test |
|---|---|---|---|
| RF-LUI-01 | `specs/authentication/requirements.md` AUTH-FR-007 | `docs/ui/navigation.md`, `docs/ui/screen-inventory.md` | WF-AT-054, 055 (dashboard/sidebar navigation) |
| RF-LUI-02 | n/a (cross-module) | `docs/ui/forms.md`, `docs/ui/states.md` | WF-AT-064 (form validation behavior) |
| RF-LUI-03 | n/a (cross-module) | `docs/ui/data-grids.md` | WF-AT-058, 059, 060 (grid filter/sort/pagination) |
| RF-LUI-04 | `specs/reports/requirements.md` REP-FR-001 | `docs/ui/dashboard.md` | WF-AT-054 (dashboard KPIs) |
| RF-LUI-05 | `specs/reports/requirements.md` REP-FR-008 | `docs/ui/forms.md` export actions | WF-AT-052 (export report) |

### Requirements by Role: Backend API

| ID | Spec file | API operationId(s) | Acceptance test |
|---|---|---|---|
| RF-ELI-01 | `specs/authentication/api-contract.yaml` | `login` | WF-AT-001, US-001-SC-01 |
| RF-ELI-02 | `specs/payroll/api-contract.yaml` | `createEmpleado`, `updateEmpleado`, `getEmpleado`, `listEmpleados`, `deactivateEmpleado` | WF-AT-013..015, US-002-SC-01..05 |
| RF-ELI-03 | `specs/payroll/api-contract.yaml` | `calculatePayroll`, `finalizePayroll`, `cancelPayroll` | WF-AT-019..022, US-003-SC-01..10 |
| RF-ELI-04 | `specs/accounting-sunat/api-contract.yaml` | `createComprobante`, `updateComprobante`, `annulComprobante`, `listComprobantes` | WF-AT-025..028, US-004-SC-01..10 |
| RF-ELI-05 | `specs/accounting-sunat/api-contract.yaml` | `validateSunatBook`, `generateSunatBook`, `getSunatBook`, `listSunatBooks` | WF-AT-029..033, US-005-SC-01..08 |
| RF-ELI-06 | `specs/general-ledger/api-contract.yaml` | `createAccount`, `updateAccount`, `createAccountingPeriod`, `closeAccountingPeriod`, `createJournalEntry`, `postJournalEntry`, `cancelJournalEntry`, `reverseJournalEntry` | WF-AT-036..047 |
| RF-ELI-07 | `specs/shared-api-components.yaml` | All paged/sorted operations | WF-AT-058..060 (pagination/sort), `tests/non-functional/non-functional-tests.md` |

### Requirements by Role: Database

| ID | Spec file | DB artifact(s) | Acceptance test |
|---|---|---|---|
| RF-EDI-01 | `docs/database-design.md`, `database/normalization.md` | `database/schema.sql` (BCNF schema) | `tests/acceptance/p1-consistency-acceptance.md` P1-AT-001 (schema parity) |
| RF-EDI-02 | `database/procedures.md` | `database/procedures.sql` (`sp_calcular_planilla`, `sp_generar_libro`, `sp_postear_asiento`, etc.) | WF-AT-019, 029, 030, 043 |
| RF-EDI-03 | `docs/database-design.md` Core Constraints | `database/schema.sql` (FK, CHECK, UNIQUE, NOT NULL) | P1-AT-001 (schema parity), P1-AT-003 (validation parity) |
| RF-EDI-04 | `database/indexes.md` | `database/indexes.sql` (36 indexes) | `tests/non-functional/non-functional-tests.md` (performance) |
| RF-EDI-05 | `database/security-and-operations.md` | `database/security.sql` (3 NOLOGIN roles, grants) | P1-AT-008 (role/action parity), `tests/non-functional/non-functional-tests.md` (DB least privilege) |
| RF-EDI-06 | `specs/administration/requirements.md` ADM-FR-006 | `database/schema.sql` (`config_tributaria_version`, `config_sunat_formato`) | WF-AT-010..012, 034 |
| RF-EDI-07 | `specs/general-ledger/requirements.md` GL-FR-001..004 | `database/schema.sql` (`cuenta_contable`, `periodo_contable`, `asiento_contable`, `detalle_asiento`) | WF-AT-036..047 |
| RF-EDI-08 | `specs/payroll/requirements.md` PAY-FR-005, 007 | `database/schema.sql` (`periodo_planilla`, `planilla`, `detalle_planilla`) | WF-AT-019..022 |
| RF-EDI-09 | `specs/authentication/requirements.md` AUTH-FR-013; `specs/administration/requirements.md` ADM-FR-013 | `database/schema.sql` (`login_attempt`, `token_revocation`, `audit_log`) | WF-AT-001..003, `tests/acceptance/audit-acceptance.md` |
| RF-EDI-10 | `specs/reports/requirements.md` REP-FR-011 | `database/schema.sql` (`reporte_snapshot`, `reporte_linea`) | WF-AT-052, P1-AT-007 (report snapshot invariant) |

### Consolidated Non-Functional Requirements

| ID | Spec file | Acceptance test |
|---|---|---|
| RNF-001 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (payroll 100 emp 30s) |
| RNF-002 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (BCrypt, TLS, JWT); at-rest encryption deferred |
| RNF-003 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (UI 200ms, 3 interactions) |
| RNF-004 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (Windows 10/11, .NET 10) |
| RNF-005 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (modular architecture); coverage target deferred |

### Additional Non-Functional Requirements

#### Analysis and Documentation (OUT OF SCOPE)

| ID | Status | Rationale |
|---|---|---|
| RNF-ALV-01 | OUT OF SCOPE | 100% bidirectional traceability is satisfied by this matrix, not by a built-in tool. |
| RNF-ALV-02 | OUT OF SCOPE | SRS generation tool. Not part of ERP. |
| RNF-ALV-03 | OUT OF SCOPE | Conflict detection tool. Not part of ERP. |
| RNF-ALV-04 | OUT OF SCOPE | Backlog propagation tool. Not part of ERP. |
| RNF-ALV-05 | OUT OF SCOPE | Requirement-editing role policy. Not part of ERP. |

> Decision: RNF-ALV-01..05 describe non-functional concerns of a requirements-management tool, not the ERP itself. RNF-ALV-01 (100% traceability) is satisfied by this expanded matrix and `workflow-traceability.md`. The rest are OUT OF SCOPE.

#### WinForms UI

| ID | Spec file | Acceptance test |
|---|---|---|
| RNF-LUI-01 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (dashboard <3s) — pending explicit row |
| RNF-LUI-02 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (UI 200ms) |
| RNF-LUI-03 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (1280x720..1920x1080) |
| RNF-LUI-04 | `docs/requirements.md` | `docs/ui/design-system.md` (visual consistency) — pending acceptance row |
| RNF-LUI-05 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (keyboard, labels) — WCAG 2.1 AA conformance pending explicit row |

#### Backend API

| ID | Spec file | Acceptance test |
|---|---|---|
| RNF-ELI-01 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (API p95 500ms) |
| RNF-ELI-02 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (JWT required, 401) |
| RNF-ELI-03 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (correlation ID in errors) |
| RNF-ELI-04 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (50 concurrent, 800ms) |
| RNF-ELI-05 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (Swagger/OpenAPI) |

#### Database

| ID | Spec file | Acceptance test |
|---|---|---|
| RNF-EDI-01 | `docs/requirements.md` | WF-AT-019 (payroll 30s/100emp) |
| RNF-EDI-02 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (99.5% availability) — pending explicit row |
| RNF-EDI-03 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (TLS/SSL); at-rest encryption pending |
| RNF-EDI-04 | `docs/requirements.md` | `tests/non-functional/non-functional-tests.md` (5 years history) — pending explicit row |
| RNF-EDI-05 | `docs/requirements.md` | Versioned migrations pending acceptance row |

## P0 Cross-Cutting Requirements

| Requirement | Primary evidence | Acceptance evidence |
|---|---|---|
| RF-017 Reactivacion Logica | Module requirements, OpenAPI lifecycle commands, UX action matrix | WF-AT-005, 009, 015, 038 |
| RF-018 Tasas Previsionales Versionadas | Payroll rules/database, administration contracts | WF-AT-019, 020 |
| RF-019 Contabilizacion de Origenes | General Ledger source mapping and SQL procedure contracts | WF-AT-024, 035 |
| RF-020 Validacion y Formato SUNAT | Accounting SUNAT rules, OpenAPI and SUNAT states | WF-AT-027, 029-034 |
| RF-021 Fuentes de Reportes y Snapshots | Reports rules/database/API | WF-AT-048-053 |
| RF-022 Auditoria de Operaciones Sensibles | `docs/audit-event-catalog.md`, audit schema/function | `tests/acceptance/audit-acceptance.md` AUD-AT-001..040; WF-AT-001..064 where mutation is sensitive |
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
| RF-ALV-01 to RF-ALV-05 | `Entregable.pdf`, pages 23-24. OUT OF SCOPE for MVP. |
| RF-LUI-01 to RF-LUI-05 | `Entregable.pdf`, pages 25-27. |
| RF-ELI-01 to RF-ELI-07 | `Entregable.pdf`, pages 29-30. |
| RF-EDI-01 to RF-EDI-10 | `Entregable.pdf`, pages 32-34. |
| RNF consolidated | `Entregable.pdf`, pages 36-37 and 87-88. |
| RNF by role | `Entregable.pdf`, pages 24-36. RNF-ALV OUT OF SCOPE for MVP. |
| User stories US-001 to US-006 | `Entregable.pdf`, pages 17-22. |
| US-007 (implicit Admin) | Extracted from `docs/user-stories.md:144-148`. |
| Database schema and BCNF | `Entregable.pdf`, pages 70-84. |
| ERP module scope | `BITACORA -ERP.pdf`, pages 1-2. |

## Coverage Summary

- Total requirement IDs: 72 (20 consolidated FR + 5 RF-ALV + 5 RF-LUI + 7 RF-ELI + 10 RF-EDI + 5 RNF + 5 RNF-ALV + 5 RNF-LUI + 5 RNF-ELI + 5 RNF-EDI)
- Explicitly traced: 62 (all non-ALV)
- OUT OF SCOPE: 10 (RF-ALV-01..05, RNF-ALV-01..05) — requirements-management meta-tool, not ERP
- Pending acceptance rows: RNF-LUI-01/04/05, RNF-EDI-02/04/05 (deferred to non-functional tests expansion post-Sprint-0)
