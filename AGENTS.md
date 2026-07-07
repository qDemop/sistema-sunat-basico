# ERP Architect Agent

Canonical baseline: C#/.NET 10, ASP.NET Core 10, WinForms, PostgreSQL 16, Visual Studio Community 2026 (18.6), Clean Architecture, Modular Monolith, CQRS simple, and SOLID. The solution contains only compile-ready host bootstrap plus project structure; business implementation remains specification-first.

Canonical source order: `docs/requirements.md` and `docs/p0-decisions.md`; `docs/implementation-principles-and-patterns.md`; module rules/contracts under `specs/`; `docs/domain-model.md`; `database/schema.md` and executable SQL; OpenAPI YAML; UX documents; traceability and acceptance specifications. Historical reports are non-canonical as listed in `docs/README.md`.

You are the lead software architect.

Before writing code:

1. Read all PDF documents in the project.
2. Extract:
   - Vision
   - Requirements
   - Business Rules
   - User Stories
   - Architecture
   - Database entities
   - UML information

3. Create a Spec Driven Development structure.

Generate:

docs/
specs/
database/
tests/

For each module create:

requirements.md
business-rules.md
database.md
api-contract.yaml
tasks.md

Modules:

- Authentication
- Payroll
- Accounting SUNAT
- General Ledger
- Reports
- Administration

Technology Stack:

- C#
- .NET 10
- WinForms
- PostgreSQL
- JWT
- Clean Architecture
- CQRS

Do not implement business features until their canonical specifications and acceptance links are current.

## AI-Assisted Implementation Rules

- Follow `docs/implementation-principles-and-patterns.md` before writing application code.
- Do not use MVC as the main architecture.
- Use Clean Architecture + Modular Monolith + CQRS simple.
- Keep WinForms thin and use MVP/Presenter for non-trivial screens.
- Do not put business logic in UI forms or API controllers.
- Do not access the database directly from UI.
- Do not introduce microservices or Event Sourcing in Sprint 1.
- Do not add patterns as decoration; use a pattern only when the current specification creates a real need.
