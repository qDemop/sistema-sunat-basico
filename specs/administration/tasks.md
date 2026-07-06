# Administration Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 4.

## Cross-Module Dependencies

- **Depends on**: Authentication (user is created here but authn handled by Authentication module).
- **Blocks**: Accounting SUNAT (active tax config), Payroll (active pension config), Administration audit consumed by all modules.

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| ADM-T01 | Domain entities: `Usuario` admin view, `Rol`, config version entities read models + audit log view DTOs | Domain | 1 | - | 4 |
| ADM-T02 | `UsuarioCommand` + handlers: create, update role, reset password, activate/deactivate, reactivate | Command | 3 | ADM-T01 | 4 |
| ADM-T03 | `UsuarioQuery` + handlers: list (paginated), detail, search | Query | 2 | ADM-T02 | 4 |
| ADM-T04 | `RolQuery` + handler: list 4 predefined roles with module matrix | Query | 1 | ADM-T01 | 4 |
| ADM-T05 | `ConfigTributariaVersionCommand` + handlers: create (Draft), activate, close; no-overlap guard | Command | 2 | ADM-T01 | 4 |
| ADM-T06 | `ConfigDescuentoPrevisionalVersionCommand` + handlers: create, activate, close | Command | 1 | ADM-T01 | 4 |
| ADM-T07 | `ConfigSunatFormatoCommand` + handlers: create, activate, close; 11-token validation | Command | 2 | ADM-T01 | 4 |
| ADM-T08 | `AuditLogQuery` + handler: list with date/actor/entity filters | Query | 1 | ADM-T01 | 4 |
| ADM-T09 | `AdministrationController`: users, roles, config versions, audit endpoints | API | 2 | ADM-T02..ADM-T08 | 4 |
| ADM-T10 | FluentValidation: `UsuarioCommandValidator`, config version validators | Validation | 1 | ADM-T02, ADM-T05, ADM-T07 | 4 |
| ADM-T11 | Integration tests: user CRUD, role assignment, config lifecycle (no overlap), audit retrieval | Test | 2 | ADM-T09 | 4 |
| ADM-T12 | WinForms Administration forms: users, roles, tax config, pension config, SUNAT format, audit log | UI | 3 | ADM-T09 | 4 |

**Sprint 4 total**: 19 SP. Exit criteria per `docs/implementation-roadmap.md`: user/role management, versioned config lifecycle, audit log review, DB least-privilege.

## Out of Scope (Sprint 4)

- `BACKUP_LOG` and `MIGRATION_LOG` tables remain `IMPLEMENTATION PENDING`; operational backup/restore is external.
- Audit-log retention policy and migration rollback template are deferred to post-MVP.

## Spec Closure (pre-implementation)

- [x] Define final Sprint 1 role-based RBAC model.
- [x] Define versioned tax configuration fields and effective-date behavior.
- [~] Define audit-log retention policy. → Deferred (post-MVP).
- [~] Define backup and restore runbooks. → Deferred (operational, external).
- [x] Define executable database role grants and denied operations.
- [~] Define migration rollback documentation template. → Deferred (post-MVP).
- [x] Review administration API contract before implementation. (Closed Sprint 0.)
