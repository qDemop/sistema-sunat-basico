# Spec Driven Development Index

This folder contains the project-level specifications extracted from:

- `Entregable.pdf`
- `BITACORA -ERP.pdf`

Generated scope:

- Product vision and source analysis
- Functional and non-functional requirements
- User stories and acceptance criteria
- Architecture specification
- Glossary and domain language

The repository is specification-first. `src/` contains the five Clean Architecture projects and compile-ready API/WinForms startup scaffolding only; business features are not implemented.

## Canonical Technical Baseline

- C# with .NET 10; ASP.NET Core 10; WinForms targets `net10.0-windows`.
- Visual Studio Community 2026 (18.6).
- PostgreSQL 16, JWT, Clean Architecture and CQRS.
- Solution projects: `ERP.Domain`, `ERP.Application`, `ERP.Infrastructure`, `ERP.API`, `ERP.WinForms`.
- In-scope modules: Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration.

## SDD Structure

- `docs/`: project vision, requirements, architecture, glossary.
- `specs/`: module specifications for Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, and Administration.
- `database/`: conceptual and logical database specifications.
- `tests/`: acceptance and non-functional test specifications.

## Architecture Documents

- `sdd-architecture.md`: complete SDD architecture index.
- `domain-model.md`: bounded contexts, aggregates, value objects, services, events, and rules.
- `database-design.md`: logical schema, constraints, indexes, transactions, and security design.
- `api-contracts.md`: consolidated API contract map.
- `module-specifications.md`: module responsibilities, inputs, outputs, dependencies, and acceptance highlights.
- `implementation-roadmap.md`: sprint roadmap, dependencies, risks, and architecture Definition of Done.

## Source Traceability

The specifications reference the PDFs by document and page range when a requirement or story is extracted from them. Current architecture refinements and executable project files supersede historical technology baselines.

## Canonical Source of Truth

Current implementation guidance comes from `requirements.md`, `p0-decisions.md`, `domain-model.md`, `database-design.md`, canonical module specifications/OpenAPI, executable database contracts, UX governance, and traceability/acceptance specifications.

The following are historical, non-canonical reports and must not guide implementation: `gap-analysis.md`, `consistency-report.md`, `p0-remediation-report.md`, `p1-remediation-report.md`, and `net10-migration-report.md`. They preserve prior findings or migration evidence only.
