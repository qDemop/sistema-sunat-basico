# End-to-End Workflow Traceability

This matrix closes RF-024 for the 64 certification workflows. `COMPLETE` means every applicable specification link below exists; it never means application code exists.

## Evidence Resolution

| Matrix evidence | Canonical resolution |
|---|---|
| Requirement / rule | Requirement ID in `docs/requirements.md` plus the named module's `requirements.md` and `business-rules.md`. |
| UX | Screen/journey IDs resolve through `docs/ui/screen-inventory.md`, `forms.md`, `data-grids.md`, `states.md`, and `ux-traceability.md`. Entries marked `IMPLEMENTATION PENDING` are excluded from a COMPLETE workflow. |
| API operation | Exact `operationId` in `specs/*/api-contract.yaml`; its request, response, errors and `x-roles` are part of the link. |
| Domain | Named entity/aggregate resolves to `docs/domain-model.md` and the module specification. |
| Database | Named table/constraint/routine resolves to `database/schema.sql`, `functions.sql`, `procedures.sql` and their Markdown contracts. |
| Role / permission | The operation's `x-roles` must match the action row in `docs/ui/ux-traceability.md` and the four-role matrix in `specs/authentication/requirements.md`. |
| Audit | Exact event name or explicitly justified N/A resolves to `docs/audit-event-catalog.md`; correlation behavior is part of sensitive-command evidence. |
| Test | Exact `WF-AT-*` row in `tests/acceptance/p0-lifecycle-acceptance.md`. |

If any applicable resolution is absent, the row must be `PARTIAL`, `AMBIGUOUS`, `OUT OF SCOPE`, or `IMPLEMENTATION PENDING`, not `COMPLETE`.

## Authentication and Administration

| ID | Workflow | Requirement / rule | UX | API operation | Domain / database | Audit | Test | Status |
|---|---|---|---|---|---|---|---|---|
| WF-001 | Login | RF-008, RF-ELI-01; Authentication | AUT-01, J-01 | `login` | Usuario; `identity.usuario`, `login_attempt` | `AUTH_LOGIN_SUCCEEDED` | WF-AT-001 | COMPLETE |
| WF-002 | Logout | RF-008, RF-022; Authentication | Global session action | `logout` | TokenRevocation; `token_revocation` | `AUTH_LOGOUT` | WF-AT-002 | COMPLETE |
| WF-003 | Failed login/lockout | RF-008, RF-022; Authentication | AUT-01 states | `login` 401/423 | Usuario; `usuario`, `login_attempt` | `AUTH_LOGIN_FAILED`, `AUTH_ACCOUNT_LOCKED` | WF-AT-003 | COMPLETE |
| WF-004 | User creation | RF-008, RF-022; Administration | ADM-01/02, J-09 | `createUser` | Usuario/Rol; `usuario`, `rol` | `USER_CREATED` | WF-AT-004 | COMPLETE |
| WF-005 | User deactivate/reactivate | RF-017, RF-022; Administration | ADM-02 | `deactivateUser`, `reactivateUser` | Usuario; `usuario.activo` | `USER_DEACTIVATED`, `USER_REACTIVATED` | WF-AT-005 | COMPLETE |
| WF-006 | Role assignment | RF-008, RF-022; role-only rule | ADM-02/04 | `updateUser`, `listPredefinedRoles` | Usuario -> predefined Rol | `USER_UPDATED` | WF-AT-006 | COMPLETE |
| WF-007 | Department creation | RF-010, RF-022; Payroll | EMP-04/05, J-02 | `createDepartamento` | Departamento; `departamento` | `DEPARTMENT_CREATED` | WF-AT-007 | COMPLETE |
| WF-008 | Department update | RF-010, RF-022; Payroll | EMP-05 | `updateDepartamento` | Departamento; `departamento` | `DEPARTMENT_UPDATED` | WF-AT-008 | COMPLETE |
| WF-009 | Department deactivate/reactivate | RF-010, RF-017; Payroll | EMP-04/05 | `deactivateDepartamento`, `reactivateDepartamento` | Departamento; `departamento.activo` | `DEPARTMENT_DEACTIVATED`, `DEPARTMENT_REACTIVATED` | WF-AT-009 | COMPLETE |
| WF-010 | Tax/pension version creation | RF-006, RF-018, RF-022; Administration | ADM-05/06, J-10 | `createTaxVersion`, `createPensionVersion` | Tax/Pension Draft version tables | `TAX_CONFIG_CREATED`, `PENSION_CONFIG_CREATED` | WF-AT-010 | COMPLETE |
| WF-011 | Tax/pension version activation | RF-006, RF-018, RF-022; Administration | ADM-06 `Activar version` | `activateTaxVersion`, `activatePensionVersion` | Draft -> Active; exclusion + immutable trigger; audited activate procedures | `TAX_CONFIG_ACTIVATED`, `PENSION_CONFIG_ACTIVATED` | WF-AT-011 | COMPLETE |
| WF-012 | Tax/pension version closing | RF-006, RF-018, RF-022; Administration | ADM-06 `Cerrar version` | `closeTaxVersion`, `closePensionVersion` | Active -> Closed; immutable trigger; audited close procedures | `TAX_CONFIG_CLOSED`, `PENSION_CONFIG_CLOSED` | WF-AT-012 | COMPLETE |

## Payroll

| ID | Workflow | Requirement / rule | UX | API operation | Domain / database | Audit | Test | Status |
|---|---|---|---|---|---|---|---|---|
| WF-013 | Employee creation | RF-001; Payroll | EMP-01/02, J-02 | `createEmpleado` | Empleado; `empleado` | `EMPLOYEE_CREATED` | WF-AT-013 | COMPLETE |
| WF-014 | Employee update | RF-001; Payroll | EMP-02/03 | `updateEmpleado` | Empleado; `empleado` | `EMPLOYEE_UPDATED` | WF-AT-014 | COMPLETE |
| WF-015 | Employee deactivate/reactivate | RF-001, RF-017; Payroll | EMP-03 | `deactivateEmpleado`, `reactivateEmpleado` | Empleado; `empleado.activo` | `EMPLOYEE_DEACTIVATED`, `EMPLOYEE_REACTIVATED` | WF-AT-015 | COMPLETE |
| WF-016 | Register overtime | RF-011; Payroll | EMP-06/07, J-03 | `createHorasExtra` | HorasExtra; `horas_extra` Draft | `OVERTIME_REGISTERED` | WF-AT-016 | COMPLETE |
| WF-017 | Approve overtime | RF-011; Payroll | EMP-07 | `approveHorasExtra` | Draft -> Approved only when no PeriodoPlanilla exists; guarded procedure | `APROBAR_HORAS_EXTRA` | WF-AT-017 | COMPLETE |
| WF-018 | Cancel overtime | RF-011; Payroll | EMP-06/07 | `cancelHorasExtra` | Draft/Approved -> Cancelled; procedure | `CANCELAR_HORAS_EXTRA` | WF-AT-018 | COMPLETE |
| WF-019 | Calculate payroll | RF-002, RF-018; Payroll | PAY-01/02, J-04 | `listPayrollPeriods`, `calculatePayroll` | PeriodoPlanilla; period/result/detail tables; procedure | `CALCULAR_PLANILLA` | WF-AT-019 | COMPLETE |
| WF-020 | Recalculate payroll | RF-002, RF-016; Draft-only rule | PAY-02 | `calculatePayroll` with existing Draft | Same aggregate; atomic result replacement | `CALCULAR_PLANILLA` with recalculation detail | WF-AT-020 | COMPLETE |
| WF-021 | Finalize payroll | RF-016, RF-019; Payroll/GL mapping | PAY-02 | `finalizePayroll` | Draft -> Finalized; finalizer; source Draft | `FINALIZAR_PLANILLA` | WF-AT-021 | COMPLETE |
| WF-022 | Cancel Draft payroll | RF-016; Payroll | PAY-02 | `cancelPayroll` | Draft -> Cancelled; procedure | `CANCELAR_PLANILLA` | WF-AT-022 | COMPLETE |
| WF-023 | Payroll audit trail | RF-022; audit catalog | Command result in PAY-02; audit review in ADM-09 (Admin only) | `calculatePayroll`, `finalizePayroll`, `cancelPayroll`, `listAuditEvents`, `getAuditEvent` | `audit.audit_log`; `fn_registrar_evento` | `CALCULAR_PLANILLA`, `FINALIZAR_PLANILLA`, `CANCELAR_PLANILLA` | WF-AT-023 | COMPLETE |
| WF-024 | Payroll to GL | RF-019; GL source mapping | PAY-02 -> ACC-09 | `finalizePayroll`, later `postJournalEntry` | PeriodoPlanilla -> Asiento/Detalle; unique source | `FINALIZAR_PLANILLA`, `POSTEAR_ASIENTO` | WF-AT-024 | COMPLETE |

## Accounting SUNAT

| ID | Workflow | Requirement / rule | UX | API operation | Domain / database | Audit | Test | Status |
|---|---|---|---|---|---|---|---|---|---|
| WF-025 | Register voucher | RF-004, RF-019; Accounting SUNAT/GL | ACC-01/02, J-05 | `createComprobante` | Comprobante; tax version; note self-reference trigger; source Draft | `VOUCHER_REGISTERED`, `SINCRONIZAR_COMPROBANTE` | WF-AT-025 | COMPLETE |
| WF-026 | Update voucher | RF-004; unlocked-source rule | ACC-02/03 | `updateComprobante` | Registrado + unlocked; source Draft update | `VOUCHER_UPDATED`, sync event | WF-AT-026 | COMPLETE |
| WF-027 | Validate voucher | RF-004; computed validation rule | ACC-02 states | `createComprobante`, `updateComprobante` typed validation errors | Comprobante rules/checks; no Validado state | `VOUCHER_REGISTERED`, `VOUCHER_UPDATED` with Success/Failure result | WF-AT-027 | COMPLETE |
| WF-028 | Annul voucher | RF-004, RF-019; correction rule | ACC-03 | `annulComprobante` | Registrado -> Anulado; cancel/reversal procedure | `ANULAR_COMPROBANTE` | WF-AT-028 | COMPLETE |
| WF-029 | Generate Purchase Book | RF-005, RF-020; SUNAT | SUN-01/02, J-07 | `validateSunatBook`, `generateSunatBook` Compras | Locked voucher set; governed format; complete immutable bridge snapshots | `VALIDAR_LIBRO`, `GENERAR_LIBRO` | WF-AT-029 | COMPLETE |
| WF-030 | Generate Sales Book | RF-005, RF-020; SUNAT | SUN-01/02, J-07 | `validateSunatBook`, `generateSunatBook` Ventas | Locked voucher set; governed format; complete immutable bridge snapshots | `VALIDAR_LIBRO`, `GENERAR_LIBRO` | WF-AT-030 | COMPLETE |
| WF-031 | Replace book version | RF-005; immutable version rule | SUN-02/03/04 | `generateSunatBook` | Unique period/type/version; no overwrite | `GENERAR_LIBRO` | WF-AT-031 | COMPLETE |
| WF-032 | Book immutability | RF-005; immutable version rule | SUN-04 | Read-only `getSunatBook`; no mutation endpoint | Book/bridge immutable trigger and denied UPDATE | `GENERAR_LIBRO`, `EXPORTAR_LIBRO`; no mutation event exists | WF-AT-032 | COMPLETE |
| WF-033 | Included voucher later changed | RF-005; bridge snapshot rule | ACC-03/SUN-04 | `updateComprobante` blocked; `annulComprobante`; later `generateSunatBook` | lock trigger; historical bridge retained | `ANULAR_COMPROBANTE`, `GENERAR_LIBRO` | WF-AT-033 | COMPLETE |
| WF-034 | SUNAT format versioning | RF-006, RF-020; Administration/SUNAT | ADM-07/08 explicit create/activate/close | `createSunatFormatVersion`, `activateSunatFormatVersion`, `closeSunatFormatVersion` | Governed JSON; immutable trigger; audited activate/close procedures | `SUNAT_FORMAT_CREATED`, `SUNAT_FORMAT_ACTIVATED`, `SUNAT_FORMAT_CLOSED` | WF-AT-034 | COMPLETE |
| WF-035 | Voucher to GL | RF-019; GL source mapping including notes | ACC-02/03 -> ACC-09 | `createComprobante`, `updateComprobante`, `annulComprobante`, `postJournalEntry`, `reverseJournalEntry` | Ordinary/debit increase; credit-note inverse reduction; source-unique entry | `SINCRONIZAR_COMPROBANTE`, `ANULAR_COMPROBANTE`, `POSTEAR_ASIENTO`, `REVERTIR_ASIENTO` | WF-AT-035 | COMPLETE |

## General Ledger

| ID | Workflow | Requirement / rule | UX | API operation | Domain / database | Audit | Test | Status |
|---|---|---|---|---|---|---|---|---|---|
| WF-036 | Create account | RF-015; General Ledger | ACC-04/05, J-06 | `createAccount`, `getAccount` | CuentaContable; `cuenta_contable` | `ACCOUNT_CREATED` | WF-AT-036 | COMPLETE |
| WF-037 | Update account | RF-015; account immutability rule | ACC-05 | `getAccount`, `updateAccount` | CuentaContable/Post-use guards | `ACCOUNT_UPDATED` | WF-AT-037 | COMPLETE |
| WF-038 | Account deactivate/reactivate | RF-015, RF-017 | ACC-04/05 | `deactivateAccount`, `reactivateAccount` | `cuenta_contable.activo`; line guard | `ACCOUNT_DEACTIVATED`, `ACCOUNT_REACTIVATED` | WF-AT-038 | COMPLETE |
| WF-039 | Create accounting period | RF-015; period rules | ACC-06 | `createAccountingPeriod` | PeriodoContable Open | `PERIOD_CREATED` | WF-AT-039 | COMPLETE |
| WF-040 | Close accounting period | RF-015; period rules | ACC-06 | `closeAccountingPeriod` | Open row lock -> Closed; entry trigger serializes concurrent create/post | `CERRAR_PERIODO` | WF-AT-040 | COMPLETE |
| WF-041 | Reopen accounting period | Explicit P0 out-of-scope decision | No action shown | No endpoint | Closed terminal; corrections in Open period | N/A | WF-AT-041 | OUT OF SCOPE |
| WF-042 | Create journal entry | RF-015; journal rules | ACC-07/08/09 | `createJournalEntry`, `getJournalEntry` | AsientoContable Draft + lines | `JOURNAL_CREATED` | WF-AT-042 | COMPLETE |
| WF-043 | Post journal entry | RF-015; balance/open-period rules | ACC-08/09 | `postJournalEntry` | Draft -> Posted; procedure | `POSTEAR_ASIENTO` | WF-AT-043 | COMPLETE |
| WF-044 | Cancel Draft journal | RF-015; journal lifecycle | ACC-08/09 | `cancelJournalEntry` | Draft -> Cancelled; procedure | `CANCELAR_ASIENTO` | WF-AT-044 | COMPLETE |
| WF-045 | Reverse Posted journal | RF-015; correction rule | ACC-09 | `reverseJournalEntry` | Original row lock + unique active-reversal index; linked Draft | `REVERTIR_ASIENTO` | WF-AT-045 | COMPLETE |
| WF-046 | Validate debit/credit balance | RF-015; journal invariant | ACC-08 | `postJournalEntry` 409 | line checks + procedure totals | `POSTEAR_ASIENTO` with Failure result and correlation | WF-AT-046 | COMPLETE |
| WF-047 | Validate journal date/period | RF-015; period invariant | ACC-08 | `createJournalEntry`, `updateJournalEntry`, `postJournalEntry` typed errors | period FK/date/Open checks | `JOURNAL_CREATED`, `JOURNAL_UPDATED`, `POSTEAR_ASIENTO` with Failure result | WF-AT-047 | COMPLETE |

## Reports and UX Governance

| ID | Workflow | Requirement / rule | UX | API operation | Domain / database | Audit | Test | Status |
|---|---|---|---|---|---|---|---|---|
| WF-048 | Balance sheet | RF-007, RF-021; Reports | REP-02, J-08 | `getBalanceSheet` | Posted ledger cumulative source | N/A for read; export is WF-052 `REPORT_EXPORTED` | WF-AT-048 | COMPLETE |
| WF-049 | Income statement | RF-007, RF-021; Reports | REP-03 | `getIncomeStatement` | Posted ledger range source | N/A for read; export is WF-052 `REPORT_EXPORTED` | WF-AT-049 | COMPLETE |
| WF-050 | Payroll report | RF-007, RF-021; Reports | REP-04 | `getConsolidatedPayroll` | Finalized payroll + department snapshot | N/A for read; export is WF-052 `REPORT_EXPORTED` | WF-AT-050 | COMPLETE |
| WF-051 | SUNAT report | RF-005, RF-020; SUNAT | SUN-04/05 | `getSunatBook`, `exportSunatBookPdf`, `exportSunatBookExcel` | Stored immutable book/bridge | `EXPORTAR_LIBRO` | WF-AT-051 | COMPLETE |
| WF-052 | Export report | RF-LUI-05, RF-021; Reports | REP-02-05 | `createReportExport`, `listReportExports`, `downloadReportExport` | Insert-only grants + immutable triggers on ReporteSnapshot/Linea | `REPORT_EXPORTED` | WF-AT-052 | COMPLETE |
| WF-053 | Report permission filtering | RF-008; role-only matrix | REP screens and PAY-02 | `getDashboardKpis`, `getBalanceSheet`, `getIncomeStatement`, `getConsolidatedPayroll`, `createReportExport`, `listReportExports`, `downloadReportExport` | JWT role policy; no permission catalog | N/A; fine-grained permission events are explicitly out of scope | WF-AT-053 | COMPLETE |
| WF-054 | Dashboard navigation | RF-LUI-01/04; dashboard.md | DASH-01 | RRHH `getPayrollDashboard`; Contador `getAccountingOperationalDashboard`; Gerente `getDashboardKpis` | Explicit role-specific sources | N/A | WF-AT-054 | COMPLETE |
| WF-055 | Sidebar navigation | RF-LUI-01; navigation.md | Global shell | N/A | Role matrix | N/A | WF-AT-055 | COMPLETE |
| WF-056 | Breadcrumb behavior | RF-LUI-01; navigation.md | All list/detail/forms | N/A | N/A | N/A | WF-AT-056 | COMPLETE |
| WF-057 | Modal behavior | RF-LUI-02; forms.md/state rules | Lifecycle screens | `deactivateUser`, `finalizePayroll`, `annulComprobante`, `postJournalEntry`, `generateSunatBook` | Existing aggregate guards | `USER_DEACTIVATED`, `FINALIZAR_PLANILLA`, `ANULAR_COMPROBANTE`, `POSTEAR_ASIENTO`, `GENERAR_LIBRO` | WF-AT-057 | COMPLETE |
| WF-058 | Data-grid filtering | RF-LUI-03; data-grids.md | Supported canonical grids only | `listUsers`, `listTaxVersions`, `listPensionVersions`, `listSunatFormatVersions`, `listAuditEvents`, `listDepartamentos`, `listEmpleados`, `listHorasExtra`, `listPayrollPeriods`, `listComprobantes`, `listSunatBooks`, `listAccountingPeriods`, `listAccounts`, `listJournalEntries`, `listReportExports` | Declared read projections; saved views/bulk pending | N/A | WF-AT-058 | COMPLETE |
| WF-059 | Data-grid sorting | RF-LUI-03; data-grids.md | Supported canonical grids only | `listUsers`, `listTaxVersions`, `listPensionVersions`, `listSunatFormatVersions`, `listAuditEvents`, `listDepartamentos`, `listEmpleados`, `listHorasExtra`, `listPayrollPeriods`, `listComprobantes`, `listSunatBooks`, `listAccountingPeriods`, `listAccounts`, `listJournalEntries`, `listReportExports` using `sortBy` and `sortDirection` | Stable read order; saved views pending | N/A | WF-AT-059 | COMPLETE |
| WF-060 | Data-grid pagination | RF-LUI-03, RF-ELI-07 | Supported canonical grids only | `listUsers`, `listTaxVersions`, `listPensionVersions`, `listSunatFormatVersions`, `listAuditEvents`, `listDepartamentos`, `listEmpleados`, `listHorasExtra`, `listPayrollPeriods`, `listComprobantes`, `listSunatBooks`, `listAccountingPeriods`, `listAccounts`, `listJournalEntries`, `listReportExports` using `page`, `pageSize`, and `pageInfo` | Paged read projections; cross-page bulk pending | N/A | WF-AT-060 | COMPLETE |
| WF-061 | Keyboard shortcuts | RF-LUI-01, RF-LUI-02; keyboard-shortcuts.md | Documented contexts | `createEmpleado`, `createComprobante`, `calculatePayroll`, `postJournalEntry`, `createReportExport` | Corresponding command guards | `EMPLOYEE_CREATED`, `VOUCHER_REGISTERED`, `CALCULAR_PLANILLA`, `POSTEAR_ASIENTO`, `REPORT_EXPORTED` | WF-AT-061 | COMPLETE |
| WF-062 | Empty/loading/error states | RF-ELI-07; states.md | All data/process screens | Shared `ErrorResponse`; `calculatePayroll`, `finalizePayroll`, `postJournalEntry`, `generateSunatBook` | No duplicate mutation on unknown outcome | `CALCULAR_PLANILLA`, `FINALIZAR_PLANILLA`, `POSTEAR_ASIENTO`, `GENERAR_LIBRO` retain correlation on Failure | WF-AT-062 | COMPLETE |
| WF-063 | Accessibility requirements | RF-LUI-01, RF-LUI-02, RF-LUI-03; accessibility.md | Representative inventory | N/A | N/A | N/A | WF-AT-063 | COMPLETE |
| WF-064 | Form validation behavior | RF-LUI-02; forms.md | Canonical forms | `createEmpleado`, `updateEmpleado`, `createComprobante`, `updateComprobante`, `createTaxVersion`, `createJournalEntry`, `updateJournalEntry` typed 400/409 errors | Constraints/invariants | `EMPLOYEE_CREATED`, `EMPLOYEE_UPDATED`, `VOUCHER_REGISTERED`, `VOUCHER_UPDATED`, `TAX_CONFIG_CREATED`, `JOURNAL_CREATED`, `JOURNAL_UPDATED` with Failure result | WF-AT-064 | COMPLETE |

## Coverage Result

- 63 in-scope workflows are `COMPLETE` at specification level.
- WF-041 is explicitly `OUT OF SCOPE` and has a documented correction alternative; no missing implementation link is hidden.
- Auxiliary UX capabilities explicitly marked `IMPLEMENTATION PENDING` are not counted as completed workflow links and do not alter the 64 certification workflows.
- Application implementation remains pending by design and is not evidence of a documentation gap.
