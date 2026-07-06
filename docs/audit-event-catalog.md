# Audit Event Catalog

This catalog is the canonical P0 contract for sensitive-operation audit events. It does not add a new module; events are persisted in `audit.audit_log` and queried through `GET /api/admin/audit`.

## Mandatory Envelope

Every event stores `modulo`, `operacion`, `entidad`, `entidad_id` when known, `id_usuario_actor` when known, `rol_actor` when known, `correlation_id`, `resultado`, Lima-aware timestamp, and non-secret JSON detail. Actor user and role come from the validated JWT or resolved login identity, never from a command DTO.

`resultado` is `Success`, `Failure`, or `Blocked`; `Blocked` is reserved for an enforced lockout or policy denial. A successful SQL lifecycle procedure records its event in the same transaction. If a command transaction rolls back, the API command boundary records `Failure` in a new transaction with the same correlation ID and sanitized error code; passwords, JWTs, bank account numbers, and hashes are never audit detail.

## Canonical Events

| Module | Operation code | Domain event or trigger | Entity | Required detail |
|---|---|---|---|---|
| Authentication | `AUTH_LOGIN_SUCCEEDED` | `UserAuthenticated` | `USUARIO` | Username, resulting role, `jti` hash/reference only. |
| Authentication | `AUTH_LOGIN_FAILED` | `UserLoginFailed` | `USUARIO` or username | Attempt count when identity is known. |
| Authentication | `AUTH_ACCOUNT_LOCKED` | Third consecutive failed login | `USUARIO` | `bloqueado_hasta`. |
| Authentication | `AUTH_LOGOUT` | `UserLoggedOut` | `TOKEN_REVOCATION` | Revoked `jti` hash/reference and expiration. |
| Administration | `USER_CREATED` | User creation | `USUARIO` | Assigned role. |
| Administration | `USER_UPDATED` | User profile/role update | `USUARIO` | Changed field names; no secrets. |
| Administration | `USER_DEACTIVATED` | User deactivation | `USUARIO` | Previous state. |
| Administration | `USER_REACTIVATED` | `UserReactivated` | `USUARIO` | Previous state. |
| Administration | `USER_PASSWORD_RESET` | Password reset | `USUARIO` | Result only; no credential material. |
| Administration | `TAX_CONFIG_CREATED` | Tax Draft creation | `CONFIG_TRIBUTARIA_VERSION` | Code, version, effective dates. |
| Administration | `TAX_CONFIG_ACTIVATED` | Draft -> Active | `CONFIG_TRIBUTARIA_VERSION` | Code and version. |
| Administration | `TAX_CONFIG_CLOSED` | Active -> Closed | `CONFIG_TRIBUTARIA_VERSION` | Code and version. |
| Administration | `PENSION_CONFIG_CREATED` | Pension Draft creation | `CONFIG_DESCUENTO_PREVISIONAL_VERSION` | Regime, version, effective dates. |
| Administration | `PENSION_CONFIG_ACTIVATED` | Draft -> Active | `CONFIG_DESCUENTO_PREVISIONAL_VERSION` | Regime and version. |
| Administration | `PENSION_CONFIG_CLOSED` | Active -> Closed | `CONFIG_DESCUENTO_PREVISIONAL_VERSION` | Regime and version. |
| Administration | `SUNAT_FORMAT_CREATED` | Format Draft creation | `CONFIG_SUNAT_FORMATO` | Book type, version, effective dates. |
| Administration | `SUNAT_FORMAT_ACTIVATED` | Draft -> Active | `CONFIG_SUNAT_FORMATO` | Book type and version. |
| Administration | `SUNAT_FORMAT_CLOSED` | Active -> Closed | `CONFIG_SUNAT_FORMATO` | Book type and version. |
| Payroll | `DEPARTMENT_CREATED` | `DepartamentoCreated` | `DEPARTAMENTO` | Name. |
| Payroll | `DEPARTMENT_UPDATED` | Department update | `DEPARTAMENTO` | Changed field names. |
| Payroll | `DEPARTMENT_DEACTIVATED` | Department deactivation | `DEPARTAMENTO` | Previous state. |
| Payroll | `DEPARTMENT_REACTIVATED` | Department reactivation | `DEPARTAMENTO` | Previous state. |
| Payroll | `EMPLOYEE_CREATED` | `EmployeeRegistered` | `EMPLEADO` | Employee ID and department; no bank data. |
| Payroll | `EMPLOYEE_UPDATED` | Employee update | `EMPLEADO` | Changed field names; no salary/bank values. |
| Payroll | `EMPLOYEE_DEACTIVATED` | `EmployeeDeactivated` | `EMPLEADO` | Previous state. |
| Payroll | `EMPLOYEE_REACTIVATED` | Employee reactivation | `EMPLEADO` | Previous state. |
| Payroll | `OVERTIME_REGISTERED` | `OvertimeRegistered` | `HORAS_EXTRA` | Employee, period and hour bands. |
| Payroll | `APROBAR_HORAS_EXTRA` | `OvertimeApproved` | `HORAS_EXTRA` | Prior/new state. |
| Payroll | `CANCELAR_HORAS_EXTRA` | `OvertimeCancelled` | `HORAS_EXTRA` | Prior/new state. |
| Payroll | `CALCULAR_PLANILLA` | `PayrollDraftComputed` | `PERIODO_PLANILLA` | Period, recalculation flag, employee count and totals. |
| Payroll | `FINALIZAR_PLANILLA` | `PayrollFinalized` | `PERIODO_PLANILLA` | Period and generated Draft entry ID. |
| Payroll | `CANCELAR_PLANILLA` | `PayrollDraftCancelled` | `PERIODO_PLANILLA` | Period. |
| AccountingSUNAT | `VOUCHER_REGISTERED` | `VoucherRegistered` | `COMPROBANTE` | Movement, type, series/number and Draft entry ID. |
| AccountingSUNAT | `VOUCHER_UPDATED` | Unlocked voucher update | `COMPROBANTE` | Changed field names and Draft entry ID. |
| AccountingSUNAT | `ANULAR_COMPROBANTE` | `VoucherAnnulled` | `COMPROBANTE` | Source entry and cancellation/reversal result. |
| AccountingSUNAT | `VALIDAR_LIBRO` | Explicit validation query | Period/type | Validation state and observation codes. |
| AccountingSUNAT | `GENERAR_LIBRO` | `AccountingBookGenerated` | `LIBRO_CONTABLE` | Period, type, version, format version and row count. |
| AccountingSUNAT | `EXPORTAR_LIBRO` | Book export | `LIBRO_CONTABLE` | Exact version and format. |
| GeneralLedger | `ACCOUNT_CREATED` | Account creation | `CUENTA_CONTABLE` | Code and type. |
| GeneralLedger | `ACCOUNT_UPDATED` | Account update | `CUENTA_CONTABLE` | Changed field names. |
| GeneralLedger | `ACCOUNT_DEACTIVATED` | Account deactivation | `CUENTA_CONTABLE` | Previous state. |
| GeneralLedger | `ACCOUNT_REACTIVATED` | Account reactivation | `CUENTA_CONTABLE` | Previous state. |
| GeneralLedger | `PERIOD_CREATED` | Period creation | `PERIODO_CONTABLE` | Period and dates. |
| GeneralLedger | `SINCRONIZAR_COMPROBANTE` | Voucher source mapping | `ASIENTO_CONTABLE` | Voucher ID. |
| GeneralLedger | `JOURNAL_CREATED` | Manual Draft creation | `ASIENTO_CONTABLE` | Period, source and totals. |
| GeneralLedger | `JOURNAL_UPDATED` | Draft update | `ASIENTO_CONTABLE` | Changed header/line count and totals. |
| GeneralLedger | `POSTEAR_ASIENTO` | `JournalEntryPosted` | `ASIENTO_CONTABLE` | Debit and credit totals. |
| GeneralLedger | `CANCELAR_ASIENTO` | Draft cancellation | `ASIENTO_CONTABLE` | Prior/new state. |
| GeneralLedger | `REVERTIR_ASIENTO` | `JournalEntryReversalRequested` | `ASIENTO_CONTABLE` | Original and reversal Draft IDs. |
| GeneralLedger | `CERRAR_PERIODO` | `AccountingPeriodClosed` | `PERIODO_CONTABLE` | Period. |
| Reports | `REPORT_EXPORTED` | Report snapshot generation | `REPORTE_SNAPSHOT` | Report type, format, filters, cutoff and result. |

## Explicit Boundaries

- Read-only navigation, sorting, and pagination are not sensitive audit events.
- `VALIDAR_LIBRO` is audited because it is legal-generation evidence even though it does not mutate business state.
- Fine-grained permission events, direct SUNAT submission, accounting-period reopen, and finalized-payroll correction are out of scope.
