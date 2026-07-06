# Seed Data Contract

`database/seeds.sql` is bootstrap data for an empty PostgreSQL 16 database. It is idempotent and must not mutate effective configuration history on re-execution.

| Catalog | Seed values |
|---|---|
| Application roles | `Administrador RRHH`, `Contador`, `Gerente Financiero`, `Administrador Sistema`, active with levels 2, 3, 1 and 5. |
| Pension types | Active `AFP` and `ONP`. |
| Pension configuration | Version 1 effective 2026-01-01: AFP 10.00%, ONP 13.00%, Active. Existing version rows are preserved. |
| Departments | Active Recursos Humanos, Contabilidad, Gerencia and Tecnologia. |
| Tax configuration | `IGV` version 1, 18.00%, effective 2026-01-01, Active. Existing version row is preserved. |
| Voucher types | Factura 01, Recibo por Honorarios 02, Boleta 03, Nota de Credito 07, Nota de Debito 08, Guia de Remision 09 with the declared IGV flags. |
| Book types | Active Compras and Ventas. |
| SUNAT formats | Compras and Ventas format `2026.1`, effective 2026-01-01, Active, with the exact eleven governed tokens. Existing version rows are preserved. |
| Initial chart | Codes 1011, 1212, 40111, 40114, 4032, 4071, 4111, 4151, 4212, 6011, 6211, 6292, 6293 and 7011 with types/natures from `specs/general-ledger/business-rules.md`. |
| Initial accounting period | `2026-01`, Open, 2026-01-01 through 2026-01-31; existing row is preserved. |

Versioned configuration conflicts use `DO NOTHING` because Active/Closed versions are immutable. Corrections require a new Draft version through the Administration contract; bootstrap reruns never rewrite historical rates or formats.
