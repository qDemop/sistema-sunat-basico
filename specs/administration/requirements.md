# Administration Requirements

Source: `BITACORA -ERP.pdf`, page 1; `Entregable.pdf`, pages 11, 13, 32-36, 70, 98-106.

## Purpose

Manage users, roles, configuration, database operations, logs, and tax-rule maintenance.

## Actors

- Administrador Sistema.
- Arquitecto / Administrador de Base de Datos.
- DBA Operacional.

## Functional Requirements

| ID | Requirement |
|---|---|
| ADM-FR-001 | The system must allow creating, modifying, deactivating, and reactivating user accounts. |
| ADM-FR-002 | The system must allow assigning roles to users. |
| ADM-FR-003 | The system must allow viewing users with username, full name, role, last access, and active/inactive state. |
| ADM-FR-004 | The system must allow resetting user passwords. |
| ADM-FR-005 | The system must list the four predefined roles and assign one to each user; role creation and deletion are out of Sprint 1 scope. |
| ADM-FR-006 | The system must maintain Draft/Active/Closed versions for IGV, pension deductions, and SUNAT formats with non-overlapping effective dates and audit data. |
| ADM-FR-007 | The system must expose logs of access and activity. |
| ADM-FR-008 | The system must support backup and restore procedures as operational specifications. |
| ADM-FR-009 | The database must define least-privilege PostgreSQL roles. |
| ADM-FR-010 | Schema changes must be managed by versioned migrations with rollback procedures. |
| ADM-FR-011 | Sprint 1 access control must use role-based RBAC only. |
| ADM-FR-012 | The system must activate and close tax and SUNAT-format versions through explicit lifecycle commands; Active and Closed versions are immutable. |
| ADM-FR-013 | Authentication and sensitive mutation audit must preserve actor user, actor role, correlation ID, result, and affected entity. |

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| ADM-NFR-001 | Only Administrador Sistema may manage users and roles. |
| ADM-NFR-002 | Database availability target is 99.5% during working hours. |
| ADM-NFR-003 | Sensitive operations must be audited. |
| ADM-NFR-004 | At least 5 years of operational data must be supported. |
| ADM-NFR-005 | Database role privileges must follow least privilege. |
| ADM-NFR-006 | Tax configuration versions for the same code must not overlap by effective date range. |

## Traceability Cross-References

### Consolidated Requirements (see `docs/requirements.md`)

| Module FR/NFR | Consolidated ID |
|---|---|
| ADM-FR-001..005 | RF-008 (RBAC), RF-017 (Reactivacion) |
| ADM-FR-006 | RF-006 (Normativa Tributaria), RF-018 (Tasas Previsionales) |
| ADM-FR-007, 013 | RF-022 (Auditoria) |
| ADM-FR-008 | Operational, out of scope (IMPLEMENTATION PENDING) |
| ADM-FR-009 | RF-EDI-05 (Database roles) |
| ADM-FR-010 | RNF-EDI-05 (Versioned migrations) |
| ADM-FR-011 | RF-008 (role-only RBAC) |
| ADM-FR-012 | RF-006, RF-018 (config lifecycle) |
| ADM-NFR-001 | RF-022 (admin-only) |
| ADM-NFR-002 | RNF-EDI-02 (99.5% availability) |
| ADM-NFR-003 | RF-022 (audited) |
| ADM-NFR-004 | RNF-EDI-04 (5 years) |
| ADM-NFR-005 | RF-EDI-05 (least privilege) |
| ADM-NFR-006 | RF-006 (no overlap) |

### User Stories and Acceptance Tests

Implicit user story (User and Role Administration, `docs/user-stories.md`): no dedicated US section; covered by administration workflows below.

| Workflows | Acceptance test IDs |
|---|---|
| WF-004 User creation, WF-005 User deactivate/reactivate, WF-006 Role assignment | WF-AT-004, WF-AT-005, WF-AT-006 |
| WF-010 Tax/pension version creation, WF-011 Activation, WF-012 Closing | WF-AT-010, WF-AT-011, WF-AT-012 |
| WF-034 SUNAT format versioning (cross-ref with Accounting SUNAT) | WF-AT-034 |

Full workflow detail: `tests/traceability/workflow-traceability.md` (Authentication and Administration table).
