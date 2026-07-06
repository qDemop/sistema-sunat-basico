# Authentication Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Tables

### USUARIO

| Column | Rule |
|---|---|
| id_usuario | Primary key. |
| username | Required, unique, at least 3 alphanumeric characters. |
| password_hash | Required, BCrypt hash. |
| nombre_completo | Required. |
| id_rol | Required FK to `ROL`. |
| ultimo_acceso | Updated after successful login. |
| activo | Controls whether user can authenticate. |
| intentos_fallidos | Required, default 0. |
| bloqueado_hasta | Optional timestamp; authentication is rejected while it is in the future. |

### ROL

| Column | Rule |
|---|---|
| id_rol | Primary key. |
| nombre | Required, unique. |
| descripcion | Optional. |
| nivel_acceso | Numeric access level. |

## Expected Roles

- Administrador RRHH.
- Contador.
- Gerente Financiero.
- Administrador Sistema.

## Constraints

- `USUARIO.username` unique case-insensitively through the normalized `lower(username)` index; authentication and failed-attempt lookup use the same normalization.
- `USUARIO.id_rol` references `ROL.id_rol`.
- `USUARIO.activo` default true.
- Role names must be unique.
- `USUARIO.intentos_fallidos >= 0`.

## Indexes

- `USUARIO(username)`.
- `USUARIO(id_rol)`.
- `ROL(nombre)`.

## Sprint 1 RBAC Decision

Sprint 1 uses role-based RBAC only. The canonical authentication schema contains `USUARIO` and `ROL`.

Permission tables such as `PERMISO` and `ROL_PERMISO` are deferred and must not be treated as Sprint 1 requirements.

## Required Security Tables

These tables are required by the active P0 security baseline:

- `AUDIT_LOG`: actor user, actor role snapshot, correlation ID, result, entity, event data, and timestamp.
- `LOGIN_ATTEMPT`: username, resolved user when known, timestamp, success, source IP when available, and correlation ID.
- `TOKEN_REVOCATION`: unique JWT `jti`, user, revocation timestamp, expiration, reason, and correlation ID.

`TOKEN_REVOCATION` is authoritative for Sprint 1; Redis and refresh tokens are out of scope.

## Atomic Login and Logout Contract

1. Login resolves and locks `USUARIO` by normalized username in one transaction.
2. Every submitted login inserts one `LOGIN_ATTEMPT`, including unknown usernames with null `id_usuario`.
3. If an existing account has `bloqueado_hasta > now()`, credentials are not checked; the attempt/result is recorded as blocked and the public response remains generic.
4. An invalid credential increments `intentos_fallidos`; the third consecutive failure sets `bloqueado_hasta = now() + interval '15 minutes'` atomically and records the lockout audit event.
5. A successful credential check sets `intentos_fallidos = 0`, clears `bloqueado_hasta`, updates `ultimo_acceso`, records the successful attempt/audit, and only then issues the JWT.
6. Logout inserts the current JWT `jti`, authenticated user, expiration and correlation ID into `TOKEN_REVOCATION` in one transaction. Repeating logout for the same `jti` is idempotent and does not extend its expiration.

The API command boundary supplies correlation and resolved actor context. Passwords and raw JWTs are never persisted in attempt, revocation, or audit records.
