# Administration Business Rules

## User Management Rules

- Only Administrador Sistema can create, edit, deactivate, reactivate, or reset users.
- User deactivation is logical.
- Reset password action must require confirmation.
- Role assignment changes take effect on next authentication.

## Role Rules

- Role names are unique.
- Predefined roles must exist before production use.
- The predefined roles cannot be created, renamed, or deleted through the Sprint 1 API.
- Administrador Sistema has full access.
- Role changes must be audited.
- Sprint 1 uses role-based RBAC only. Permission catalogs are deferred.

## Configuration Rules

- Tax-rate configuration must include code, version, `tasa_igv`, effective start date, and optional effective end date.
- Historical transactions keep the rate used when recorded.
- SUNAT code and format changes must be traceable.
- Tax configuration versions for the same code must not overlap in effective date ranges.
- Tax, pension-rate, and SUNAT-format versions start Draft, can be activated once, and can later be closed. Active/Closed versions are immutable.
- Activation rejects missing dates, overlaps, or unsupported configuration codes/book types.
- Activation and closing execute only through audited SQL procedures. Direct application-role UPDATE is denied; triggers permit only Draft -> Active and Active -> Closed without changing business fields.
- SUNAT format structure uses the governed eleven-token `columns` contract; token order controls export order and no unknown, missing, or duplicate token is accepted.

## Operational Rules

- Backups must be scheduled and restoration steps documented.
- Slow queries must be monitored and reviewed.
- Database migrations require rollback documentation.
- Logs must retain actor user, actor role snapshot, timestamp, operation, entity, result, correlation ID, and event data.
