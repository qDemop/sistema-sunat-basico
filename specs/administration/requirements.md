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
