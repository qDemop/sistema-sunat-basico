# Test Specifications

This folder contains test specifications only. It does not contain executable automated tests.

These specifications validate the six canonical modules (Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration) for the .NET 10 / Visual Studio Community 2026 specification-first baseline.

Test scope:

- Acceptance criteria for extracted user stories.
- Acceptance criteria for all 64 documented in-scope/out-of-scope workflows.
- P1 cross-layer consistency cases in `acceptance/p1-consistency-acceptance.md`; these do not change the 64 workflow IDs.
- Non-functional validation criteria.
- Requirement and end-to-end workflow traceability.

Canonical P0 evidence:

- `acceptance/p0-lifecycle-acceptance.md`
- `traceability/workflow-traceability.md`
- `../docs/audit-event-catalog.md`

Future implementation may convert these specifications into unit, integration, UI, performance, and acceptance tests.
