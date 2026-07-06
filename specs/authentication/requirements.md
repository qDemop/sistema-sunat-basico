# Authentication Requirements

Source: `Entregable.pdf`, pages 10-12, 17-18, 29, 36-37, 98-100.

## Purpose

Authenticate users securely and authorize module access according to role.

Sprint 1 uses role-based RBAC only. Fine-grained permissions are out of Sprint 1 scope.

## Actors

- Usuario del sistema.
- Administrador RRHH.
- Contador.
- Gerente Financiero.
- Administrador Sistema.

## Functional Requirements

| ID | Requirement |
|---|---|
| AUTH-FR-001 | The system must show a login screen with username and password fields. |
| AUTH-FR-002 | The system must validate user credentials against `USUARIO` in PostgreSQL. |
| AUTH-FR-003 | The system must verify passwords using BCrypt hashes. |
| AUTH-FR-004 | The system must generate a signed JWT after successful authentication. |
| AUTH-FR-005 | The JWT must include user ID, name, predefined role, `jti`, issued-at, and expiration. |
| AUTH-FR-006 | The UI must store the JWT in memory and send it as `Authorization: Bearer <token>` for protected requests. |
| AUTH-FR-007 | The dashboard must show only modules permitted by the authenticated role. |
| AUTH-FR-008 | Invalid credentials must show a clear error and keep the user on the login screen. |
| AUTH-FR-010 | The system must support predefined roles: Administrador RRHH, Contador, Gerente Financiero, Administrador Sistema. |
| AUTH-FR-011 | User and role administration must be owned by the Administration module, not by Authentication endpoints. |
| AUTH-FR-012 | Logout must revoke the current JWT `jti` until its expiration. |
| AUTH-FR-013 | A successful login must reset failed-attempt state; the third consecutive failure must block the account for 15 minutes. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| AUTH-NFR-001 | All API endpoints except login must require a valid JWT. |
| AUTH-NFR-002 | Requests without token or with expired token must return 401. |
| AUTH-NFR-003 | Stored passwords must never be plaintext. |
| AUTH-NFR-004 | Session and authorization errors must include structured messages and correlation IDs. |
| AUTH-NFR-005 | Login form must validate required fields before sending the request. |
| AUTH-NFR-006 | After 3 failed login attempts, the account must be blocked for 15 minutes. |
| AUTH-NFR-007 | Sprint 1 authorization decisions must be derived from the user's role only. |

## Role Access Matrix

| Module | Administrador RRHH | Contador | Gerente Financiero | Administrador Sistema |
|---|---:|---:|---:|---:|
| Payroll | Yes | No | No | Yes |
| Accounting SUNAT | No | Yes | No | Yes |
| General Ledger queries | No | Yes | Read only | Yes |
| General Ledger commands | No | Yes | No | Yes |
| Reports | No | No | Yes | Yes |
| Administration | No | No | No | Yes |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| AUTH-FR-001..008, 010..013 | RF-008 (Autenticacion JWT y RBAC) |
| AUTH-FR-013 (audit fields) | RF-022 (Auditoria) |
| AUTH-FR-011 (API contracts) | RF-023 (Contratos API) |
| AUTH-NFR-001..002 | RNF-002 (Seguridad), RNF-ELI-02 (JWT required) |
| AUTH-NFR-003 | RNF-002 (BCrypt) |
| AUTH-NFR-004 | RNF-ELI-03 (correlation ID) |
| AUTH-NFR-005 | RNF-003 (Usabilidad) |
| AUTH-NFR-006 | RF-008 (lockout) |
| AUTH-NFR-007 | RF-008 (role-only RBAC) |

### User Stories and Acceptance Tests

| User story | Workflows | Acceptance test IDs |
|---|---|---|
| US-001 (Valid login) | WF-001 Login, WF-002 Logout, WF-003 Failed login/lockout | WF-AT-001, WF-AT-002, WF-AT-003 |

Full workflow detail: `tests/traceability/workflow-traceability.md` (Authentication and Administration table) and `tests/acceptance/p0-lifecycle-acceptance.md`.
