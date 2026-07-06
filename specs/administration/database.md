# Administration Database Specification

Exact columns, types, nullability, FK actions and CHECK constraints are owned by `database/schema.md` and `database/schema.sql`; this module file adds no alternate physical definition.

## Core Tables

- `USUARIO`
- `ROL`

## Operational Specifications

The active P0 baseline requires these persistence structures:

- `AUDIT_LOG`: sensitive operation records.
- `CONFIG_TRIBUTARIA_VERSION`: tax rates such as IGV with `codigo`, `version`, `tasa_igv`, effective dates, and status.
- `CONFIG_SUNAT_FORMATO`: SUNAT book format metadata and effective dates.
- `CONFIG_DESCUENTO_PREVISIONAL_VERSION`: AFP/ONP percentages, versions, effective dates, and status.
- `LOGIN_ATTEMPT`: authentication-attempt evidence.
- `TOKEN_REVOCATION`: revoked JWT `jti` values until expiration.
- `BACKUP_LOG`: `IMPLEMENTATION PENDING`; no table/API is part of the current canonical schema.
- `MIGRATION_LOG`: `IMPLEMENTATION PENDING`; no table/API is part of the current canonical schema.

## PostgreSQL Roles

| Role | Purpose |
|---|---|
| rol_app_read | Read-only operational access. |
| rol_app_write | Application read/write access without schema administration. |
| rol_app_admin | Migration and administration access. |

## Constraints

- `ROL.nombre` unique.
- `USUARIO.username` unique.
- Config entries must include code, version, tax rate, effective start date, and status.
- Active config rows for same rule and period must not conflict.
- Sprint 1 uses role-only RBAC; permission tables are deferred.
- Tax configuration versions for the same code must not overlap by effective date range.
- Tax configuration `codigo` is fixed to `IGV` in this scope.
- Pension-rate versions for the same type and Active SUNAT-format versions for the same book type must not overlap.
- Version triggers reject DELETE, any Closed update, and any Active update other than field-preserving Active -> Closed. Application write roles receive INSERT but no UPDATE on these version tables; audited `SECURITY DEFINER` procedures own transitions.
- `AUDIT_LOG.rol_actor` is required for every authenticated operation and retained even if the user is later deactivated; it may be null only for an unauthenticated login attempt whose identity cannot be resolved.

## Indexes

- `USUARIO(username)`.
- `USUARIO(id_rol)`.
- `ROL(nombre)`.
- Audit/config tables should index by date, user, entity, and operation type when implemented.
- `CONFIG_TRIBUTARIA_VERSION(codigo, version)`.
- `CONFIG_TRIBUTARIA_VERSION(codigo, fecha_inicio, fecha_fin)`.
