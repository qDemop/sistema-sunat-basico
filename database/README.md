# Database Artifacts

This folder contains canonical PostgreSQL 16 contracts for the specification-first .NET 10 / Visual Studio Community 2026 baseline. They support Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, and Administration without defining application business code.

Files:

- `schema.md`: entities, attributes, relationships, constraints, and indexes.
- `normalization.md`: normalization process from unnormalized table to BCNF.
- `procedures.md`: stored procedure specifications.
- `functions.md`: complete helper, resolver, calculation, audit, and integrity-trigger function contracts.
- `indexes.md`: exact secondary and lifecycle-integrity index inventory.
- `seeds.md`: exact bootstrap catalogs and idempotence rules.
- `security-and-operations.md`: roles, privileges, backups, availability, and operational requirements.
- `security.sql`: executable least-privilege role and grant baseline.
- `schema.sql`: PostgreSQL schemas, tables, constraints, and foreign keys.
- `indexes.sql`: PostgreSQL index definitions.
- `functions.sql`: helper and calculation functions.
- `procedures.sql`: stored procedure implementations.
- `seeds.sql`: base catalog seed data.

Execution order for a new database: `schema.sql`, `indexes.sql`, `functions.sql`, `procedures.sql`, `seeds.sql`, then `security.sql`. Re-execution of `seeds.sql` preserves existing versioned configuration rows.
