# Audit Event Acceptance Test Specifications

> Sprint 0 remediation: new file closing the RF-022 audit-comprehensiveness gap. Each acceptance row verifies that a sensitive command emits the correct event from `docs/audit-event-catalog.md` with the mandatory envelope (actor user, actor role, correlation ID, result, entity, timestamp, sanitized detail).

This file is specification-only. It does not contain executable tests. Criteria are designed to be automatable via API integration tests that assert `audit.audit_log` rows after each command.

## Mandatory Envelope Assertions (apply to every AUD-AT row)

| Field | Assertion |
|---|---|
| `id_usuario_actor` | Present and resolves to the authenticated JWT subject for all authenticated commands; NULL only for unauthenticated login failures. |
| `rol_actor` | Matches the role claim from the validated JWT; never sourced from a command DTO. |
| `correlation_id` | Matches the `X-Correlation-ID` request header or the server-generated value; retained on Failure. |
| `resultado` | `Success`, `Failure`, or `Blocked`; `Blocked` reserved for lockout or policy denial. |
| `entidad` / `entidad_id` | Matches the affected entity table name and row ID when known. |
| `fecha_evento` | Lima-aware TIMESTAMPTZ; within 5 seconds of the command completion. |
| `detalle` JSON | Contains the required detail fields per catalog; never contains passwords, JWTs, bank account numbers, or hashes. |

## Authentication Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-001 | `AUTH_LOGIN_SUCCEEDED` | Active user with valid password | Login succeeds | `audit_log` row persisted with `AUTH_LOGIN_SUCCEEDED`, actor user, role, `jti` hash, correlation ID, `Success`. |
| AUD-AT-002 | `AUTH_LOGIN_FAILED` | User submits invalid password | Login fails | Row persisted with `AUTH_LOGIN_FAILED`, username/actor when known, attempt count, correlation ID, `Failure`. |
| AUD-AT-003 | `AUTH_ACCOUNT_LOCKED` | Two consecutive failures exist | Third invalid login occurs | Row persisted with `AUTH_ACCOUNT_LOCKED`, `bloqueado_hasta` in detail, `Blocked` result. |
| AUD-AT-004 | `AUTH_LOGOUT` | Authenticated session with unexpired JWT | User logs out | Row persisted with `AUTH_LOGOUT`, revoked `jti` hash, expiration, `Success`. |

## Administration Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-005 | `USER_CREATED` | Admin creates user | User creation succeeds | Row with `USER_CREATED`, assigned role in detail, `Success`. |
| AUD-AT-006 | `USER_UPDATED` | Admin updates user role | Update succeeds | Row with `USER_UPDATED`, changed field names (no secrets), `Success`. |
| AUD-AT-007 | `USER_DEACTIVATED` | Active user exists | Admin deactivates user | Row with `USER_DEACTIVATED`, previous state, `Success`. |
| AUD-AT-008 | `USER_REACTIVATED` | Inactive user exists | Admin reactivates user | Row with `USER_REACTIVATED`, previous state, `Success`. |
| AUD-AT-009 | `USER_PASSWORD_RESET` | User exists | Admin resets password | Row with `USER_PASSWORD_RESET`, result only (no credential material), `Success`. |
| AUD-AT-010 | `TAX_CONFIG_CREATED` | Admin creates tax Draft | Creation succeeds | Row with `TAX_CONFIG_CREATED`, code, version, effective dates, `Success`. |
| AUD-AT-011 | `TAX_CONFIG_ACTIVATED` | Draft tax version exists | Admin activates | Row with `TAX_CONFIG_ACTIVATED`, code and version, `Success`. |
| AUD-AT-012 | `TAX_CONFIG_CLOSED` | Active tax version exists | Admin closes | Row with `TAX_CONFIG_CLOSED`, code and version, `Success`. |
| AUD-AT-013 | `PENSION_CONFIG_CREATED` | Admin creates pension Draft | Creation succeeds | Row with `PENSION_CONFIG_CREATED`, regime, version, effective dates, `Success`. |
| AUD-AT-014 | `PENSION_CONFIG_ACTIVATED` | Draft pension version exists | Admin activates | Row with `PENSION_CONFIG_ACTIVATED`, regime and version, `Success`. |
| AUD-AT-015 | `PENSION_CONFIG_CLOSED` | Active pension version exists | Admin closes | Row with `PENSION_CONFIG_CLOSED`, regime and version, `Success`. |
| AUD-AT-016 | `SUNAT_FORMAT_CREATED` | Admin creates format Draft | Creation succeeds | Row with `SUNAT_FORMAT_CREATED`, book type, version, effective dates, `Success`. |
| AUD-AT-017 | `SUNAT_FORMAT_ACTIVATED` | Draft format exists | Admin activates | Row with `SUNAT_FORMAT_ACTIVATED`, book type and version, `Success`. |
| AUD-AT-018 | `SUNAT_FORMAT_CLOSED` | Active format exists | Admin closes | Row with `SUNAT_FORMAT_CLOSED`, book type and version, `Success`. |

## Payroll Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-019 | `DEPARTMENT_CREATED` | RRHH creates department | Creation succeeds | Row with `DEPARTMENT_CREATED`, name, `Success`. |
| AUD-AT-020 | `DEPARTMENT_UPDATED` | Department exists | Update succeeds | Row with `DEPARTMENT_UPDATED`, changed field names, `Success`. |
| AUD-AT-021 | `DEPARTMENT_DEACTIVATED` | Active department exists | Deactivation succeeds | Row with `DEPARTMENT_DEACTIVATED`, previous state, `Success`. |
| AUD-AT-022 | `DEPARTMENT_REACTIVATED` | Inactive department exists | Reactivation succeeds | Row with `DEPARTMENT_REACTIVATED`, previous state, `Success`. |
| AUD-AT-023 | `EMPLOYEE_CREATED` | RRHH creates employee | Creation succeeds | Row with `EMPLOYEE_CREATED`, employee ID and department (no bank data), `Success`. |
| AUD-AT-024 | `EMPLOYEE_UPDATED` | Employee exists | Update succeeds | Row with `EMPLOYEE_UPDATED`, changed field names (no salary/bank values), `Success`. |
| AUD-AT-025 | `EMPLOYEE_DEACTIVATED` | Active employee exists | Deactivation succeeds | Row with `EMPLOYEE_DEACTIVATED`, previous state, `Success`. |
| AUD-AT-026 | `EMPLOYEE_REACTIVATED` | Inactive employee exists | Reactivation succeeds | Row with `EMPLOYEE_REACTIVATED`, previous state, `Success`. |
| AUD-AT-027 | `OVERTIME_REGISTERED` | RRHH registers overtime | Registration succeeds | Row with `OVERTIME_REGISTERED`, employee, period, hour bands, `Success`. |
| AUD-AT-028 | `APROBAR_HORAS_EXTRA` | Draft overtime exists | Approval succeeds | Row with `APROBAR_HORAS_EXTRA`, prior/new state, `Success`. |
| AUD-AT-029 | `CANCELAR_HORAS_EXTRA` | Draft/Approved overtime exists | Cancellation succeeds | Row with `CANCELAR_HORAS_EXTRA`, prior/new state, `Success`. |
| AUD-AT-030 | `CALCULAR_PLANILLA` | Active employees and approved overtime exist | Payroll calculation succeeds | Row with `CALCULAR_PLANILLA`, period, recalculation flag, employee count, totals, `Success`. |
| AUD-AT-031 | `FINALIZAR_PLANILLA` | Draft payroll exists | Finalization succeeds | Row with `FINALIZAR_PLANILLA`, period, generated Draft entry ID, `Success`. |
| AUD-AT-032 | `CANCELAR_PLANILLA` | Draft payroll exists | Cancellation succeeds | Row with `CANCELAR_PLANILLA`, period, `Success`. |

## Accounting SUNAT Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-033 | `VOUCHER_REGISTERED` | Accountant registers voucher | Registration succeeds | Row with `VOUCHER_REGISTERED`, movement, type, series/number, Draft entry ID, `Success`. |
| AUD-AT-034 | `VOUCHER_UPDATED` | Registrado unlocked voucher exists | Update succeeds | Row with `VOUCHER_UPDATED`, changed field names, Draft entry ID, `Success`. |
| AUD-AT-035 | `ANULAR_COMPROBANTE` | Registrado voucher exists | Annulment succeeds | Row with `ANULAR_COMPROBANTE`, source entry, cancellation/reversal result, `Success`. |
| AUD-AT-036 | `VALIDAR_LIBRO` | Period/type selected | Validation query runs | Row with `VALIDAR_LIBRO`, validation state, observation codes, `Success` (audited even though non-mutating). |
| AUD-AT-037 | `GENERAR_LIBRO` | Eligible vouchers exist | Book generation succeeds | Row with `GENERAR_LIBRO`, period, type, version, format version, row count, `Success`. |
| AUD-AT-038 | `EXPORTAR_LIBRO` | Generated book exists | Export succeeds | Row with `EXPORTAR_LIBRO`, exact version, format, `Success`. |

## General Ledger Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-039 | `ACCOUNT_CREATED` | Contador creates account | Creation succeeds | Row with `ACCOUNT_CREATED`, code, type, `Success`. |
| AUD-AT-040 | `ACCOUNT_UPDATED` | Account exists | Update succeeds | Row with `ACCOUNT_UPDATED`, changed field names, `Success`. |
| AUD-AT-041 | `ACCOUNT_DEACTIVATED` | Active account exists | Deactivation succeeds | Row with `ACCOUNT_DEACTIVATED`, previous state, `Success`. |
| AUD-AT-042 | `ACCOUNT_REACTIVATED` | Inactive account exists | Reactivation succeeds | Row with `ACCOUNT_REACTIVATED`, previous state, `Success`. |
| AUD-AT-043 | `PERIOD_CREATED` | Contador creates period | Creation succeeds | Row with `PERIOD_CREATED`, period, dates, `Success`. |
| AUD-AT-044 | `SINCRONIZAR_COMPROBANTE` | Voucher registered | Source mapping runs | Row with `SINCRONIZAR_COMPROBANTE`, voucher ID, `Success`. |
| AUD-AT-045 | `JOURNAL_CREATED` | Contador creates manual Draft | Creation succeeds | Row with `JOURNAL_CREATED`, period, source, totals, `Success`. |
| AUD-AT-046 | `JOURNAL_UPDATED` | Draft entry exists | Update succeeds | Row with `JOURNAL_UPDATED`, changed header/line count, totals, `Success`. |
| AUD-AT-047 | `POSTEAR_ASIENTO` | Balanced Draft exists | Posting succeeds | Row with `POSTEAR_ASIENTO`, debit and credit totals, `Success`. |
| AUD-AT-048 | `CANCELAR_ASIENTO` | Draft entry exists | Cancellation succeeds | Row with `CANCELAR_ASIENTO`, prior/new state, `Success`. |
| AUD-AT-049 | `REVERTIR_ASIENTO` | Posted entry exists | Reversal requested | Row with `REVERTIR_ASIENTO`, original and reversal Draft IDs, `Success`. |
| AUD-AT-050 | `CERRAR_PERIODO` | Open period exists | Closing succeeds | Row with `CERRAR_PERIODO`, period, `Success`. |

## Reports Audit Acceptance

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-051 | `REPORT_EXPORTED` | Report visible with filters | Export succeeds | Row with `REPORT_EXPORTED`, report type, format, filters, cutoff, `Success`. |

## Failure and Blocked Path Assertions

| ID | Event code | Given | When | Then |
|---|---|---|---|---|
| AUD-AT-052 | Any sensitive command | Command transaction rolls back | Failure boundary | API command boundary records `Failure` in a new transaction with the same correlation ID and sanitized error code; no partial audit row from the rolled-back procedure. |
| AUD-AT-053 | `AUTH_ACCOUNT_LOCKED` / policy denial | Lockout or role policy denies command | Blocked path | Row with `Blocked` result, `AUTH_ACCOUNT_LOCKED` or relevant event code, correlation ID. |

## Coverage Summary

- 53 audit acceptance rows covering all 40+ events from `docs/audit-event-catalog.md`.
- Every sensitive command across 6 modules has at least one `Success` audit assertion.
- Failure and Blocked paths are covered by AUD-AT-052 and AUD-AT-053.
- Read-only navigation, sorting, and pagination are explicitly NOT audited (per catalog boundaries).
