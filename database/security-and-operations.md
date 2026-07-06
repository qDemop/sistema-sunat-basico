# Database Security and Operations Specification

## Security Principles

- Apply least privilege for application database roles.
- Protect database connections using SSL/TLS.
- Encrypt sensitive data at rest according to deployment capabilities.
- Never store plaintext passwords.
- Store user passwords as BCrypt hashes.

## PostgreSQL Roles

| Role | Purpose | Access |
|---|---|---|
| rol_app_read | Read-only application queries | SELECT on approved tables/views. |
| rol_app_write | Application writes | SELECT plus explicit column-level INSERT/UPDATE grants for mutable data; initial state columns are omitted so defaults create Draft/Open/Active-master records, and lifecycle changes execute audited procedures. No direct DML on audit or generated books and no UPDATE on snapshots, effective configuration, payroll aggregates, or journal state. |
| rol_app_admin | Administrative operations | Controlled DDL/migration and maintenance rights. |

`database/security.sql` is the executable grant baseline. Application login roles remain separate from these PostgreSQL roles.

## Authentication Persistence

- `USUARIO.intentos_fallidos` and `bloqueado_hasta` enforce three-attempt/15-minute lockout.
- `LOGIN_ATTEMPT` records every attempt with correlation ID.
- `TOKEN_REVOCATION` makes logout authoritative until JWT expiration.

## Audit Actor Contract

- Authenticated sensitive operations pass actor user and role from validated JWT context, never request payload.
- Procedures record actor, role, entity, result, correlation ID, and structured event data.
- Unauthenticated login failures may have a null actor/role but retain submitted username in the login-attempt record.

## Backup and Recovery

- Full backup: weekly.
- Incremental backup: daily.
- Restore procedure: documented and periodically tested.
- Backup scope: database schema, data, roles, procedures, configuration tables.

## Availability

- Target availability: 99.5% during working hours, Monday-Friday 08:00-18:00.
- Monitor slow queries using PostgreSQL facilities such as `pg_stat_statements`.
- Optimize indexes and plans for payroll, voucher, and book generation queries.

## Data Retention

- Store at least 5 years of payroll and voucher history.
- Design for future period-based partitioning and historical archiving.

## Audit

Each sensitive business operation must capture:

- User ID.
- Role.
- Date and time.
- Operation type.
- Affected entity.
- Result.
- Correlation ID where applicable.

Immutable tables also have UPDATE/DELETE blocking triggers. `rol_app_write` writes audit only through the `SECURITY DEFINER` `audit.fn_registrar_evento`, has INSERT-only access to report snapshots/lines, no direct DML on generated books/bridge rows, and no direct UPDATE on versioned configuration or payroll lifecycle tables. Column-level INSERT grants exclude lifecycle state/actor metadata for users, departments, employees, overtime, configuration, vouchers, accounting periods, accounts and journal headers.

## Exact Application-Writer Boundary

- Full-row INSERT is limited to login attempts, token revocations, journal lines, and report snapshot/line records whose checks/triggers govern all state.
- Column INSERT creates users/departments/employees/accounts as Active by default; overtime/configuration as Draft; vouchers as Registrado; periods as Open; manual journal headers as Draft.
- Direct UPDATE is limited to mutable user/department/employee data, unlocked voucher business columns, permitted account columns, Draft journal header columns, and Draft journal lines.
- No operational application role receives table DELETE except Draft journal-line deletion; table triggers verify the parent remains Draft.
- Routine execution is granted after PUBLIC routine execution is revoked; lifecycle procedures validate the actor's application role and correlation contract.
