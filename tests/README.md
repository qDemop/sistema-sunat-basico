# Test Specifications

This folder contains test specifications only. It does not contain executable automated tests.

These specifications validate the six canonical modules (Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, Administration) for the .NET 10 / Visual Studio Community 2026 specification-first baseline.

Test scope:

- Acceptance criteria for extracted user stories (US-001..US-007 with stable scenario IDs `US-NNN-SC-NN`).
- Acceptance criteria for all 64 documented in-scope/out-of-scope workflows.
- Audit event acceptance matrix covering all 40+ events from `docs/audit-event-catalog.md` (`acceptance/audit-acceptance.md`).
- P1 cross-layer consistency cases in `acceptance/p1-consistency-acceptance.md`; these do not change the 64 workflow IDs.
- Non-functional validation criteria.
- Requirement and end-to-end workflow traceability (72 requirement IDs traced; ALV family OUT OF SCOPE).

Canonical P0 evidence:

- `acceptance/p0-lifecycle-acceptance.md`
- `acceptance/audit-acceptance.md`
- `traceability/workflow-traceability.md`
- `../docs/audit-event-catalog.md`

Future implementation may convert these specifications into unit, integration, UI, performance, and acceptance tests.
