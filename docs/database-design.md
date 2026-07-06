# Database Design

## Architecture Decisions Applied

| Decision | Status |
|---|---|
| Role-based RBAC only for Sprint 1. | `ROL` and `USUARIO` are canonical. Permission tables are deferred. |
| Departamento entity. | Added to Payroll schema. |
| HorasExtra entity. | Added to Payroll schema. |
| Compra/Venta discriminator. | Added to Comprobante. |
| Accounting ledger. | Added `CUENTA_CONTABLE`, `PERIODO_CONTABLE`, `ASIENTO_CONTABLE`, `DETALLE_ASIENTO`. |
| Persisted and versioned tax configuration. | Added `CONFIG_TRIBUTARIA_VERSION` and applied-version references. |
| Payroll recalculation only while Draft. | Added status constraint and recalculation rule. |
| SUNAT book versioning through bridge table. | Added `COMPROBANTE_LIBRO`; removed direct book ownership from `COMPROBANTE`. |

## Design Goals

- PostgreSQL 16 relational design.
- BCNF-oriented normalization.
- Referential integrity with FK constraints.
- Transactional processing for payroll, SUNAT book generation, and ledger posting.
- Efficient lookup by period, employee, department, voucher, movement type, RUC/document number, date, username, and role.
- Least-privilege database access.
- At least 5 years of operational history.

## Logical Schemas

| Schema | Purpose |
|---|---|
| identity | Users, roles, authentication audit support. |
| payroll | Departments, employees, pension discount types, overtime, payroll, payroll detail. |
| accounting | Vouchers, voucher types, SUNAT books, book versions, ledger accounts, accounting periods, journal entries. |
| reporting | Report snapshots and read models derived from payroll/accounting. |
| admin | Versioned tax configuration and SUNAT format configuration. Backup/migration operational evidence is implementation pending and is not part of the current canonical schema. |
| audit | Cross-module audit records. |

## Entity Groups

### Identity

| Table | Purpose | Key Relationships |
|---|---|---|
| USUARIO | Stores application users and password hashes. | Many users reference one role. |
| ROL | Stores role names and access level. | One role grants module access for Sprint 1. |

Sprint 1 RBAC rule: permission tables are not part of the canonical Sprint 1 schema. Fine-grained permissions may be added after Sprint 1 through a separate migration.

### Payroll

| Table | Purpose | Key Relationships |
|---|---|---|
| DEPARTAMENTO | Organizational unit for employee/report filtering. | One department has many employees. |
| EMPLEADO | Employee master data. | References required `TIPO_DESCUENTO` and required `DEPARTAMENTO`. |
| TIPO_DESCUENTO | AFP/ONP regime catalog without mutable percentages. | Referenced by employees and pension versions. |
| CONFIG_DESCUENTO_PREVISIONAL_VERSION | Effective-dated AFP/ONP percentage versions. | Referenced by payroll detail as applied version. |
| HORAS_EXTRA | Overtime hours per employee and period. | References `EMPLEADO`; unique by employee and period. |
| PERIODO_PLANILLA | One payroll aggregate/lifecycle header per period. | Owns Draft/Finalized/Cancelled state and totals. |
| PLANILLA | One employee result within a payroll period. | References aggregate, employee, and department snapshot. |
| DETALLE_PLANILLA | Payroll calculated concepts. | References `PLANILLA`. |

### Accounting SUNAT

| Table | Purpose | Key Relationships |
|---|---|---|
| TIPO_COMPROBANTE | Voucher type catalog with SUNAT code. | Referenced by vouchers. |
| COMPROBANTE | Purchase/sales voucher records. | References voucher type, applied tax configuration version, and original voucher for codes 07/08. |
| TIPO_LIBRO | Purchase/Sales book catalog. | Referenced by books. |
| LIBRO_CONTABLE | Generated SUNAT book header with period/type/version, format, and generator. | References book type and format version. |
| COMPROBANTE_LIBRO | Immutable complete row snapshots. | Links vouchers, tax version and eleven governed SUNAT fields to generated books. |

### General Ledger

| Table | Purpose | Key Relationships |
|---|---|---|
| PERIODO_CONTABLE | Accounting period control. | Referenced by journal entries. |
| CUENTA_CONTABLE | Chart of accounts. | Self-references parent account when hierarchical. |
| ASIENTO_CONTABLE | Journal entry header. | References accounting period and optional source module/entity. |
| DETALLE_ASIENTO | Debit/credit lines. | References journal entry and account. |

### Administration and Audit

| Table | Purpose | Notes |
|---|---|---|
| CONFIG_TRIBUTARIA_VERSION | Versioned IGV configuration. | Stores code, percentage, version, effective date range. |
| CONFIG_SUNAT_FORMATO | Governed eleven-column SUNAT format with version and effective dates. | Token order controls export order without code changes. |
| EMPRESA | Own-company (emitter) identity for SUNAT voucher and book emission. | RUC 11 digits unique, razón social, domicilio fiscal, régimen. One active row enforced by partial unique index. Added Sprint 0 remediation. |
| AUDIT_LOG | Sensitive operation history. | Required for traceability. |
| BACKUP_LOG | `IMPLEMENTATION PENDING`; not present in `database/schema.sql` and not used by an in-scope P1 workflow. | Operational tracking is deferred without implying persistence exists. |
| MIGRATION_LOG | `IMPLEMENTATION PENDING`; not present in `database/schema.sql` and not used by an in-scope P1 workflow. | Migration tooling evidence is deferred. |
| LOGIN_ATTEMPT | Login success/failure evidence. | Supports persistent lockout. |
| TOKEN_REVOCATION | Revoked JWT identifiers until expiration. | Supports authoritative logout. |
| REPORTE_SNAPSHOT / REPORTE_LINEA | Reproducible report exports. | Store actor, filters, cutoff, totals, and immutable lines. |

## Relationships

| Relationship | Cardinality | Delete Policy | Update Policy |
|---|---|---|---|
| ROL to USUARIO | 1:N | Restrict | Cascade |
| USUARIO to LOGIN_ATTEMPT | 1:0..N optional resolution | Set Null | Cascade |
| USUARIO to TOKEN_REVOCATION | 1:N | Restrict | Cascade |
| DEPARTAMENTO to EMPLEADO | 1:N | Restrict | Cascade |
| TIPO_DESCUENTO to EMPLEADO | 1:N | Restrict | Cascade |
| EMPLEADO to HORAS_EXTRA | 1:N | Cascade | Cascade |
| USUARIO approver to HORAS_EXTRA | 1:0..N | Restrict | Cascade |
| TIPO_DESCUENTO to CONFIG_DESCUENTO_PREVISIONAL_VERSION | 1:N | Restrict | Cascade |
| PERIODO_PLANILLA to PLANILLA | 1:N | Cascade | Cascade |
| USUARIO finalizer to PERIODO_PLANILLA | 1:0..N | Restrict | Cascade |
| EMPLEADO to PLANILLA | 1:N | Restrict | Cascade |
| DEPARTAMENTO to PLANILLA | 1:N snapshots | Restrict | Cascade |
| PLANILLA to DETALLE_PLANILLA | 1:1 | Cascade | Cascade |
| TIPO_COMPROBANTE to COMPROBANTE | 1:N | Restrict | Cascade |
| CONFIG_DESCUENTO_PREVISIONAL_VERSION to DETALLE_PLANILLA | 1:N | Restrict | Cascade |
| CONFIG_TRIBUTARIA_VERSION to COMPROBANTE | 1:N | Restrict | Cascade |
| CONFIG_TRIBUTARIA_VERSION to COMPROBANTE_LIBRO | 1:N snapshots | Restrict | Cascade |
| TIPO_LIBRO to LIBRO_CONTABLE | 1:N | Restrict | Cascade |
| CONFIG_SUNAT_FORMATO to LIBRO_CONTABLE | 1:N | Restrict | Cascade |
| USUARIO generator to LIBRO_CONTABLE | 1:N | Restrict | Cascade |
| LIBRO_CONTABLE to COMPROBANTE_LIBRO | 1:N | Restrict | Cascade |
| COMPROBANTE to COMPROBANTE_LIBRO | 1:N | Restrict | Cascade |
| COMPROBANTE original to credit/debit notes | 1:N | Restrict | Cascade |
| PERIODO_CONTABLE to ASIENTO_CONTABLE | 1:N | Restrict | Cascade |
| USUARIO closer to PERIODO_CONTABLE | 1:0..N | Restrict | Cascade |
| ASIENTO_CONTABLE to DETALLE_ASIENTO | 1:N | Cascade | Cascade |
| CUENTA_CONTABLE to DETALLE_ASIENTO | 1:N | Restrict | Cascade |
| CUENTA_CONTABLE parent to CUENTA_CONTABLE | 1:0..N | Restrict | Cascade |
| ASIENTO_CONTABLE original to active reversal | 1:0..1 | Restrict | Cascade |
| USUARIO poster to ASIENTO_CONTABLE | 1:0..N | Restrict | Cascade |
| REPORTE_SNAPSHOT to REPORTE_LINEA | 1:N | Restrict | Cascade |
| DEPARTAMENTO to REPORTE_SNAPSHOT | 1:0..N optional filter | Restrict | Cascade |
| USUARIO to REPORTE_SNAPSHOT | 1:N | Restrict | Cascade |
| USUARIO to AUDIT_LOG | 1:0..N optional actor | Set Null | Cascade |

## Core Constraints

| Constraint | Applies To |
|---|---|
| Unique username | `USUARIO.username`. |
| Unique role name | `ROL.nombre`. |
| Unique department name | `DEPARTAMENTO.nombre`. |
| Unique DNI | `EMPLEADO.dni`. |
| Positive salary | `EMPLEADO.salario_base > 0`. |
| Valid period | `PERIODO_PLANILLA.periodo`, `HORAS_EXTRA.periodo`, `LIBRO_CONTABLE.periodo`, `PERIODO_CONTABLE.codigo`. |
| Unique overtime by employee/period | `HORAS_EXTRA(id_empleado, periodo)`. |
| Overtime hour values | `HORAS_EXTRA.horas_primeras_dos >= 0`, `HORAS_EXTRA.horas_posteriores >= 0`, and at least one value greater than 0. |
| Unique payroll period | `PERIODO_PLANILLA(periodo)`. |
| Unique employee result | `PLANILLA(id_periodo_planilla, id_empleado)`. |
| Applied salary snapshot | `PLANILLA.salario_base_aplicado > 0`. |
| Payroll recalculation guard | Existing payroll can be recalculated only if status is `Draft`. |
| Unique voucher identity | `COMPROBANTE(id_tipo_comp, tipo_movimiento, serie, numero)`. |
| Voucher movement type | `COMPROBANTE.tipo_movimiento IN ('Compra', 'Venta')`. |
| Positive voucher source | At least one voucher base is positive and derived `COMPROBANTE.total > 0`. |
| Required RUC/DNI formats | RUC 11 digits, DNI 8 digits according to document type. |
| Applied tax version required for taxable voucher | `COMPROBANTE.id_config_tributaria_version`. |
| Unique SUNAT book version | `LIBRO_CONTABLE(id_tipo_libro, periodo, version)`. |
| Bridge uniqueness | `COMPROBANTE_LIBRO(id_libro, id_comprobante)`. |
| Configuration immutability | Draft -> Active -> Closed only; Active/Closed fields cannot change and direct application UPDATE is denied. |
| Unique account code | `CUENTA_CONTABLE.codigo`. |
| Balanced posted entry | Sum of debit lines equals sum of credit lines before posting. |
| Non-overlapping tax versions | Same tax code cannot have overlapping effective date ranges. |
| IGV configuration code | `CONFIG_TRIBUTARIA_VERSION.codigo = 'IGV'`. |
| Lifecycle metadata | Closed periods require closing actor/time; Posted entries require posting actor/time; Generated report snapshots require a file reference. |

## Index Strategy

| Index Group | Columns | Purpose |
|---|---|---|
| Identity login | unique `lower(USUARIO.username)` plus `USUARIO(username)` | Case-insensitive identity and fast login lookup. |
| Employee lookup | `EMPLEADO(dni)`, `EMPLEADO(activo)`, `EMPLEADO(id_departamento)` | Employee search and department reports. |
| Overtime period | `HORAS_EXTRA(id_empleado, periodo)` | Payroll calculation. |
| Payroll period | `PERIODO_PLANILLA(periodo, estado)`, `PLANILLA(id_periodo_planilla, id_departamento)` | Payroll results and reports. |
| Voucher lookup | `COMPROBANTE(tipo_documento, numero_documento)`, `COMPROBANTE(fecha_emision)` | Voucher filters. |
| Voucher book generation | `COMPROBANTE(tipo_movimiento, fecha_emision, estado)` | SUNAT book generation. |
| Book lookup | `LIBRO_CONTABLE(periodo, id_tipo_libro, version)` | Book retrieval/versioning. |
| Ledger reports | `ASIENTO_CONTABLE(id_periodo_contable, estado)`, `DETALLE_ASIENTO(id_cuenta_contable)` | Balance sheet and income statement. |
| Tax config | `CONFIG_TRIBUTARIA_VERSION(codigo, version)`, effective date range | Applied tax lookup. |
| Audit | `AUDIT_LOG(fecha_evento)`, `AUDIT_LOG(id_usuario)`, `AUDIT_LOG(entidad, id_entidad)`, `AUDIT_LOG(correlation_id)` | Audit review. |

## Stored Procedure Specifications

| Procedure | Purpose | Requirement |
|---|---|---|
| `sp_calcular_planilla(periodo, actor, correlation)` | Calculate/recalculate one Draft period using effective pension versions. | <= 30 seconds for 100 employees. |
| `sp_finalizar_planilla` / `sp_cancelar_planilla` | Transition the period aggregate and create the payroll Draft entry on finalization. | Terminal-state guard. |
| `sp_generar_libro(tipo, periodo, aceptar_observaciones, actor, correlation)` | Validate and generate next immutable version using effective format and bridge snapshots. | <= 60 seconds for 1,000 vouchers. |
| `sp_postear_asiento`, `sp_cancelar_asiento`, `sp_revertir_asiento`, `sp_cerrar_periodo` | Implement the canonical ledger lifecycle. | Role, date, balance, state, and audit guards. |
| `sp_activar_*` / `sp_cerrar_*` configuration procedures | Implement audited tax, pension and SUNAT-format transitions. | No direct application UPDATE. |

## Transaction Boundaries

| Operation | Transaction Requirement |
|---|---|
| User creation | Save user, role assignment, and audit atomically. |
| Department creation/update | Save department and audit atomically. |
| Employee registration | Save employee and audit atomically. |
| Overtime registration | Save overtime record and audit atomically. |
| Payroll calculation | Process full period atomically; reject recalculation for non-Draft existing payroll. |
| Voucher registration | Resolve tax version, validate duplicate, calculate IGV, save voucher, create optional ledger draft, audit atomically. |
| Book generation | Select vouchers, create next book version, calculate totals, link vouchers through bridge, audit atomically. |
| Ledger posting | Validate balanced entry, post entry, audit atomically. |
| Tax configuration change | Save new version and audit atomically. |

## Security Design

| Role | Privilege Scope |
|---|---|
| `rol_app_read` | Read-only access to approved tables/views. |
| `rol_app_write` | Explicit mutable-column DML plus audited lifecycle-procedure execution; no UPDATE on audit, books, snapshots, effective configuration, payroll lifecycle, or journal state. |
| `rol_app_admin` | Controlled migration and administration access. |

Application RBAC for Sprint 1 is role-based:

- Administrador RRHH: Authentication, Payroll.
- Contador: Authentication, Accounting SUNAT, General Ledger entry preparation.
- Gerente Financiero: Authentication, Reports, and read-only General Ledger queries.
- Administrador Sistema: All modules.

## Closed Design Decisions

| Decision | Final Choice |
|---|---|
| Payroll recalculation | Existing payroll can be recalculated only while `PERIODO_PLANILLA.estado = Draft`. Finalized and Cancelled payroll periods are terminal; finalized correction/cancellation is out of scope. |
| Book generation versioning | Use `LIBRO_CONTABLE.version` plus `COMPROBANTE_LIBRO` bridge table. |
| Permission model | Sprint 1 uses role-only RBAC. Permission tables are deferred. |
| Report source | Balance sheet and income statement use ledger tables. Consolidated payroll uses payroll tables. |
| Tax and format traceability | Vouchers retain tax version; book bridge rows snapshot it; books retain the applied SUNAT format version. |
| Payroll rate ownership | Percentages live only in effective-dated `CONFIG_DESCUENTO_PREVISIONAL_VERSION`. |
| Reporting exports | Persist immutable `REPORTE_SNAPSHOT` and `REPORTE_LINEA`. |
