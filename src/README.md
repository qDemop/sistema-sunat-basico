# Source Structure

Clean Architecture project layout:

- `ERP.Domain`: domain entities, value objects, enums, events, and domain rules.
- `ERP.Application`: use cases, CQRS commands/queries, DTOs, interfaces, and application contracts.
- `ERP.Infrastructure`: persistence, external services, exports, identity providers, and implementation adapters.
- `ERP.API`: ASP.NET Core Web API host and composition root.
- `ERP.WinForms`: Windows Forms desktop client and composition root.

Technical baseline: .NET 10 with Visual Studio Community 2026 (18.6). API and WinForms contain compile-ready startup scaffolding only. Business logic and application features have not been implemented; canonical behavior remains owned by `docs/`, `specs/`, `database/`, and `tests/`.

The six in-scope functional modules are Authentication, Payroll, Accounting SUNAT, General Ledger, Reports, and Administration.
