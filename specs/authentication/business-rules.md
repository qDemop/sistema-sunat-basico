# Authentication Business Rules

## Credential Rules

- Username is required.
- Username must have at least 3 alphanumeric characters.
- Password is required.
- Password is compared only through BCrypt hash verification.
- Disabled users cannot authenticate.

## Token Rules

- JWT expiration default is 1 hour, configurable by environment.
- JWT claims must include user ID, full name, predefined role, `jti`, issued-at, and expiration.
- Expired tokens are invalid.
- A `jti` present in `TOKEN_REVOCATION` is invalid until its recorded expiration.
- Invalid tokens are rejected before the request reaches the protected use case.

## Failed Login Rules

- Failed attempts are counted per username.
- After 3 failed attempts, the account is blocked for 15 minutes.
- Failed-attempt count and `bloqueado_hasta` are persisted on `USUARIO`; every attempt is recorded in `LOGIN_ATTEMPT`.
- Successful login resets `intentos_fallidos` and `bloqueado_hasta`.
- Error messages must not reveal whether username or password was incorrect.

## Authorization Rules

- The dashboard must be generated from role access rules.
- Users cannot see unauthorized module menu entries.
- Direct attempts to access unauthorized endpoints must be denied.
- Administrador Sistema has full module access.
- Gerente Financiero has read-only access to General Ledger queries and no ledger command access.
- Administrador RRHH sees payroll totals in Payroll only and has no Reports module access.
- Sprint 1 uses role-based RBAC only. There is no permission catalog in the Sprint 1 domain or database model.
- Module access is derived directly from role: Administrador RRHH, Contador, Gerente Financiero, or Administrador Sistema.

## Session Rules

- Logout persists the current JWT `jti` in `TOKEN_REVOCATION` until expiration and then clears the in-memory token in the UI.
- Refresh tokens are out of Sprint 1 scope and are not part of the Sprint 1 API contract.
- Refresh-token rotation and multi-device session management are out of scope.

## Audit Rules

- Login success, login failure, lockout, logout, user lifecycle, and role assignment record actor user when known, actor role when known, correlation ID, result, and affected username/user ID.
- Client requests never supply the audit actor ID or actor role; both are taken from the validated JWT or resolved login identity.
