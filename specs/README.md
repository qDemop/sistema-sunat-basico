# Module Specifications

Canonical baseline: .NET 10 / Visual Studio Community 2026 (18.6), with six in-scope modules and specification-first business implementation.

Each module follows the same Spec Driven Development file set:

- `requirements.md`: functional and non-functional requirements.
- `business-rules.md`: domain rules, calculations, validations, invariants.
- `database.md`: entities, relationships, constraints, indexes, and persistence notes.
- `api-contract.yaml`: API specification only, not application code.
- `tasks.md`: implementation planning tasks for later development.

## Modules

Canonical module names: Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration.

- `authentication/`
- `payroll/`
- `accounting-sunat/`
- `general-ledger/`
- `reports/`
- `administration/`

No application code is included in this directory. These module contracts, together with project requirements, database contracts, UX governance and acceptance traceability, are canonical implementation inputs.
