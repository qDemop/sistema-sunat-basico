# Authentication Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 1.

## Cross-Module Dependencies

- **Depends on**: none (foundation module).
- **Blocks**: Payroll, Accounting SUNAT, General Ledger, Reports, Administration (all require JWT identity and role).

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| AUTH-T01 | Domain entities: `Usuario`, `Rol`, `LoginAttempt`, `TokenRevocation` + role enum | Domain | 2 | - | 1 |
| AUTH-T02 | `LoginCommand` + handler: BCrypt verify, lockout (3 attempts / 15 min), JWT issue with `jti`, audit event | Command | 3 | AUTH-T01 | 1 |
| AUTH-T03 | `LogoutCommand` + handler: `jti` revocation in `TOKEN_REVOCATION`, audit event | Command | 1 | AUTH-T02 | 1 |
| AUTH-T04 | `GetCurrentUserQuery` + handler: returns `UserSession` + visible `modules` from role matrix | Query | 1 | AUTH-T02 | 1 |
| AUTH-T05 | `AuthController`: `POST /api/auth/login` (200/401/423), `POST /api/auth/logout` (204), `GET /api/auth/me` | API | 1 | AUTH-T02, AUTH-T03, AUTH-T04 | 1 |
| AUTH-T06 | FluentValidation: `LoginCommandValidator` (username pattern, password min length) | Validation | 1 | AUTH-T02 | 1 |
| AUTH-T07 | JWT bearer auth wiring + role policy handlers (4 roles) + correlation middleware | Infra | 2 | AUTH-T02 | 1 |
| AUTH-T08 | Repository: `UsuarioRepository`, `TokenRevocationRepository` (Dapper + Npgsql) | Infra | 2 | AUTH-T01 | 1 |
| AUTH-T09 | Integration tests: valid login, invalid password, lockout after 3 attempts, logout revokes token, `me` returns correct modules | Test | 2 | AUTH-T05 | 1 |
| AUTH-T10 | WinForms `LoginForm` + `SessionContext` (in-memory token storage, module visibility) | UI | 2 | AUTH-T05 | 1 |

**Sprint 1 total**: 19 SP. Exit criteria per `docs/implementation-roadmap.md`: role-based auth, lockout, JWT revocation, protected endpoints.

## Spec Closure (pre-implementation)

- [x] Define Sprint 1 access model as role-based RBAC only.
- [x] Specify token expiration and logout policy.
- [x] Specify account lockout persistence model.
- [x] Define user administration validation messages.
- [x] Define audit fields for login, logout, failed login, and role changes.
- [x] Convert P0 lifecycle acceptance criteria into test specifications.
- [x] Review OpenAPI contract with Product Owner before implementation. (Closed Sprint 0: enum `modules` reconciled with `administration/api-contract.yaml`.)
