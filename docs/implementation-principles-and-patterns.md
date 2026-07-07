# Implementation Principles and Pattern Governance

This document is canonical guidance for AI-assisted and human implementation during Sprint 1 and later. It governs how implementation choices must be made after the specification-first baseline is approved.

The goal is to keep the ERP simple, testable, maintainable, and aligned with Clean Architecture. Patterns are approved only when they solve a real business or technical problem. They must not be added as decoration.

## Purpose

This guide defines mandatory coding principles, approved architecture patterns, presentation rules, security expectations, testing expectations, and anti-patterns for the ERP implementation.

It applies to every implementation contributor, including human developers and AI assistants such as Codex/OpenCode. It does not replace module specifications, database contracts, OpenAPI contracts, UX specifications, or acceptance tests. It constrains implementation so those specifications are delivered consistently.

## Mandatory Principles

| Principle | Required meaning | ERP application |
|---|---|---|
| SOLID | Classes, interfaces, handlers, services, and adapters must have clear responsibilities and stable dependencies. | No god services, no business logic in forms or controllers, small focused interfaces, Application depends on abstractions instead of Infrastructure. |
| Separation of Concerns | UI, API, Application, Domain, Infrastructure, security, persistence, and reporting responsibilities must remain separated. | WinForms renders and captures user intent, API exposes endpoints, Application coordinates use cases, Domain protects business rules, Infrastructure implements external details. |
| DRY | Avoid duplicating business knowledge, formulas, authorization rules, and validation rules. | Payroll, tax, ledger, SUNAT, and permission rules must have one authoritative implementation path and matching tests. |
| KISS | Prefer the simplest implementation that satisfies the current specification and acceptance tests. | Do not add framework-heavy abstractions when a focused handler, service, or adapter is enough. |
| YAGNI | Do not implement speculative features or abstractions for future scope. | No fine-grained permissions, microservices, event sourcing, generic repositories, or configuration engines unless approved by current specs. |
| Fail Fast | Invalid input, invalid state, invalid configuration, and unauthorized actions must fail before mutation. | Validate payroll period state, open accounting period, balanced journal entries, active tax versions, and role access before saving. |
| Least Privilege | Users and components receive only the permissions and dependencies they need. | RBAC is role-only for Sprint 1, sensitive actions require explicit authorization, and UI must not decide permissions independently. |
| Testability First | Business rules must be designed so they can be tested before UI complexity is added. | Payroll, tax, ledger, lifecycle, SUNAT book, and report rules need focused unit, integration, contract, or golden master tests. |

Mandatory examples:

- No business logic in WinForms.
- No business logic in API controllers.
- No direct database access from WinForms.
- Application must depend on abstractions, not Infrastructure implementations.
- Interfaces must be small and focused.
- Avoid god classes and god services.
- Avoid premature abstractions.

## Architecture

Clean Architecture is the main architecture. Modular Monolith is the system style for Sprint 1. CQRS simple is the use-case organization model.

The canonical flow is:

```text
WinForms / API
-> Application / CQRS handlers
-> Domain
-> Infrastructure / PostgreSQL
```

Architecture decisions:

- Clean Architecture is the main architecture.
- Modular Monolith is the development and deployment style for Sprint 1.
- CQRS simple separates mutations from reads through commands, queries, and handlers.
- Moderate tactical DDD is approved for business rules with real invariants.
- Microservices are out of scope for Sprint 1.
- Event Sourcing is out of scope for Sprint 1.
- MVC is not the main architecture. MVC may be referenced only as a web or presentation pattern where applicable, not as the primary system architecture.
- Presentation patterns may be used where appropriate, but they must not replace the canonical Clean Architecture flow.

## Presentation Layer

WinForms must use MVP / Presenter Pattern where screens become non-trivial.

Presentation rules:

- Forms must remain thin.
- Forms may handle UI events, rendering, visual feedback, and control state only.
- Forms must not calculate payroll, taxes, accounting entries, SUNAT books, report values, or permissions.
- Forms must delegate use cases to presenters or application-facing services.
- Forms must use the canonical theme system and semantic tokens.
- No hardcoded colors in future screens.
- No direct database access from WinForms.

Use a presenter when a screen coordinates non-trivial state, validation feedback, command invocation, data loading, or multiple UI interactions. Keep trivial forms simple until the screen behavior justifies a presenter.

## API Layer

Controllers must be thin entry points.

API rules:

- Controllers must not contain business rules.
- Controllers delegate mutations to Application commands and reads to Application queries.
- Controllers enforce authentication and authorization entry points.
- Controllers return structured success and error responses.
- Controllers must not access PostgreSQL, Dapper, repositories, or Infrastructure implementations directly.
- Controllers must preserve correlation and audit context for sensitive operations.

## Application Layer

The Application layer owns use-case orchestration through CQRS simple.

Application rules:

- Use Commands for mutations.
- Use Queries for reads.
- Use Handlers for use cases.
- Use DTOs for external data transfer.
- Use Validators for input and use-case validation.
- Authorization checks must happen before mutations.
- Application must depend on abstractions.
- Application must not depend directly on Infrastructure.
- Handlers coordinate work but must not become god services.
- Transaction boundaries must be explicit for critical operations.

Examples:

- `FinalizePayrollCommand` validates actor role and payroll state before mutation.
- `GetPayrollByPeriodQuery` reads payroll data without changing state.
- `RegisterVoucherCommand` resolves tax configuration and records audit context through abstractions.

## Domain Layer

The Domain layer uses moderate tactical DDD. Use DDD where the business has rules, invariants, lifecycle, or language that must be protected. Avoid DDD overengineering for simple data transfer or CRUD-only cases.

Approved tactical DDD elements:

- Entities for objects with identity and lifecycle.
- Value Objects for immutable concepts such as period, money, document identity, and account code when they carry validation or behavior.
- Aggregates for consistency boundaries such as payroll period, journal entry, or SUNAT book version.
- Domain Services for business rules that do not naturally belong to one entity or value object.
- Domain Events only for important business events.
- Invariants that must always hold before and after mutations.

Recommended domain patterns:

| Pattern | Approved ERP use |
|---|---|
| Strategy | Variable business rules such as AFP/ONP, IGV, PDF/Excel export provider selection, and effective tax versions. |
| State | Lifecycle rules such as Payroll Draft/Finalized, Accounting Period Open/Closed, Journal Entry Draft/Posted/Reversed, and SUNAT book generated/exported. |
| Specification | Reusable rules such as period must be open, journal entry must be balanced, payroll must be Draft, and user must have permission. |
| Domain Events | Important business events such as payroll finalized, voucher registered, journal entry posted, SUNAT book generated, and sensitive actions requiring audit. |

Do not force every table into a rich aggregate. Do not create domain events for every property change. Do not hide simple rules behind unnecessary abstractions.

## Infrastructure Layer

Infrastructure implements Application abstractions and isolates technical details.

Infrastructure rules:

- Infrastructure implements Application ports and contracts.
- Use Adapter/Ports for JWT, BCrypt, PDF, Excel, audit, PostgreSQL, and external services.
- Use Repository only where it clarifies persistence around aggregates or business operations.
- Do not create a generic repository for everything by default.
- Use Unit of Work for critical transactions.
- Use Dependency Injection for composition.
- Do not use manual singletons.
- Do not use a service locator as a shortcut around proper dependencies.
- Business logic in stored procedures is forbidden unless explicitly specified by canonical requirements.

Repository and Unit of Work must support clarity and transaction safety. They are not mandatory for every entity.

## Security

Security is part of the architecture, not an afterthought.

Security rules:

- RBAC is the Sprint 1 authorization model.
- Sensitive operations require action-level authorization.
- Least privilege is the default.
- Sensitive actions require audit trail.
- Never store plaintext passwords.
- Never hardcode secrets.
- Roles and permissions must not be scattered across UI code.
- Authorization must be enforced server-side before mutation.

Sensitive operations include:

- Finalize payroll.
- Recalculate payroll.
- Register voucher.
- Annul voucher.
- Post journal entry.
- Reverse journal entry.
- Close accounting period.
- Generate/export SUNAT book.
- Manage users and roles.

## Testing

Tests must protect business rules before adding UI complexity.

Testing rules:

- Unit tests for payroll, tax, ledger, and lifecycle rules.
- Integration tests for API and database flows.
- Contract tests for OpenAPI behavior.
- Golden Master tests for payroll calculations, SUNAT books, and financial reports.
- Authorization and audit behavior must be covered for sensitive operations.
- Critical rule changes require matching test updates.

Testing priorities:

1. Domain invariants and calculations.
2. Application handlers and authorization before mutation.
3. Database transaction flows and persistence contracts.
4. API contracts and structured errors.
5. WinForms behavior after business rules are protected.

## Approved Pattern Matrix

| Pattern/principle | Approved use | When to use | When not to use |
|---|---|---|---|
| SOLID | Mandatory coding principle. | Always for classes, handlers, services, interfaces, and adapters. | Never use as an excuse for speculative abstractions. |
| Clean Architecture | Primary architecture. | Always for layer boundaries and dependency direction. | Do not bypass it with UI-to-database or controller-to-database access. |
| Modular Monolith | Sprint 1 system style. | Use module boundaries for Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, and Administration. | Do not split into microservices during Sprint 1. |
| CQRS | Simple use-case organization. | Use commands for mutations and queries for reads. | Do not create complex CQRS infrastructure without current need. |
| MVP / Presenter | WinForms presentation pattern. | Use when screens become non-trivial. | Do not add presenters for tiny screens with no behavior. |
| Moderate DDD | Business rule modeling. | Use for aggregates, invariants, lifecycle, and domain language. | Do not wrap every CRUD table in excessive DDD structure. |
| Repository | Persistence abstraction around aggregates or business operations. | Use when it improves clarity or isolates persistence. | Do not create a generic repository for every entity by default. |
| Unit of Work | Transaction coordination. | Use for critical operations spanning multiple changes. | Do not add when one atomic persistence call is enough. |
| Strategy | Variable algorithms or providers. | Use for AFP/ONP, IGV variants, export providers, and tax-version behavior. | Do not use when a simple conditional is clearer and stable. |
| State | Lifecycle-dependent behavior. | Use for payroll, accounting period, journal entry, and SUNAT book lifecycles. | Do not use for simple labels with no behavior. |
| Specification | Reusable business predicates. | Use for open period, balanced journal, Draft payroll, permission, and eligibility rules. | Do not use for one-off trivial checks. |
| Adapter | External or technical integration isolation. | Use for JWT, BCrypt, PDF, Excel, audit, PostgreSQL, and external services. | Do not wrap internal code only to add indirection. |
| Domain Events | Important business events. | Use when another process must react or audit a meaningful event. | Do not emit events for every property update. |
| Factory | Complex object creation. | Use when creation requires rules, variants, or valid combinations. | Do not use for simple constructors or DTO creation. |
| MVC | Not primary architecture. | May be referenced only as a web/presentation pattern where applicable. | Do not select MVC as the main architecture for this ERP. |
| Microservices | Not approved for Sprint 1. | Reconsider only after clear scaling, team, and deployment needs exist. | Do not introduce during Sprint 1. |
| Event Sourcing | Not approved for Sprint 1. | Reconsider only if full historical event reconstruction becomes a requirement. | Do not introduce for normal audit or CRUD history. |

## Anti-Patterns Forbidden

The following anti-patterns are forbidden unless a future approved specification explicitly changes this governance:

- God Service.
- Fat Controller.
- Fat Form.
- Anemic everything with no domain rules.
- Direct SQL from WinForms.
- Business logic in stored procedures unless explicitly specified.
- Generic repository for every entity by default.
- Manual singleton service locator.
- Hardcoded roles or permissions scattered across UI.
- Hardcoded colors in future UI screens.
- Premature microservices.
- Pattern decoration without real need.

## AI Implementation Rules

Codex/OpenCode and any AI-assisted implementation must follow these rules:

- Do not add patterns just for decoration.
- Declare why a pattern is used when introducing it.
- Keep implementation minimal and aligned with current specs.
- Prefer simple explicit code over speculative abstractions.
- Do not modify unrelated layers.
- Every business rule must have or receive tests.
- Every sensitive operation must enforce authorization and audit.
- Do not use MVC as the main architecture.
- Do not implement microservices or Event Sourcing in Sprint 1.
- Do not add business features before canonical specifications and acceptance links are current.
