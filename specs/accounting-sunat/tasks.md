# Accounting SUNAT Tasks

> Spec closure is tracked in `requirements.md`, `business-rules.md`, `database.md`, and `api-contract.yaml`. This file is the implementation backlog aligned with `docs/implementation-roadmap.md` Sprint 2.

## Cross-Module Dependencies

- **Depends on**: Authentication (actor, audit), Administration (active `CONFIG_TRIBUTARIA_VERSION` and `CONFIG_SUNAT_FORMATO`).
- **Blocks**: General Ledger (registered voucher creates source-linked Draft entry), Reports (IGV, voucher/book totals).

## Implementation Backlog

| ID | Task | Type | SP | Dependencies | Sprint |
|---|---|---|---|---|---|
| ACC-T01 | Domain entities: `TipoComprobante`, `Comprobante`, `TipoLibro`, `LibroContable`, `ComprobanteLibro`, `ConfigTributariaVersion`, `ConfigSunatFormato` + enums | Domain | 3 | - | 2 |
| ACC-T02 | `RegistrarComprobanteCommand` + handler: validate, dedup (type/movement/series/number), resolve tax version by issue date, IGV calc, persist, create ledger Draft, audit | Command | 3 | ACC-T01 | 2 |
| ACC-T03 | `AnularComprobanteCommand` + handler: state Registrado->Anulado, cancel/reverse ledger Draft | Command | 1 | ACC-T02 | 2 |
| ACC-T04 | `ComprobanteQuery` + handlers: list with filters (period, movement, type, RUC), detail | Query | 2 | ACC-T02 | 2 |
| ACC-T05 | `ValidarLibroQuery` + handler: returns `Valida`/`ConObservaciones`/`Bloqueada` (non-mutating) | Query | 2 | ACC-T04 | 2 |
| ACC-T06 | `GenerarLibroCommand` + handler: calls `sp_generar_libro(tipo, periodo, aceptar_observaciones, actor, correlation)`; next immutable version; bridge snapshots | Command | 3 | ACC-T05 | 2 |
| ACC-T07 | `LibroQuery` + handlers: versions list, preview, export | Query | 2 | ACC-T06 | 2 |
| ACC-T08 | `AccountingController`: vouchers + books endpoints | API | 2 | ACC-T02..ACC-T07 | 2 |
| ACC-T09 | FluentValidation: `ComprobanteRequestValidator` (Gravada/Exonerada/Inafecta base rules, RUC 11 / DNI 8, nota 07/08 reference) | Validation | 2 | ACC-T02 | 2 |
| ACC-T10 | Book PDF/Excel export service (layout defined during implementation) | Infra | 2 | ACC-T07 | 2 |
| ACC-T11 | Integration tests: voucher CRUD, IGV calc, duplicate reject, book validation, book generation (1000 vouchers perf), annulment | Test | 3 | ACC-T08 | 2 |
| ACC-T12 | WinForms Accounting forms: voucher registration, voucher list, book generation, book preview | UI | 3 | ACC-T08 | 2 |

**Sprint 2 total**: 28 SP. Exit criteria per `docs/implementation-roadmap.md`: Compra/Venta discriminator, IGV with applied tax version, immutable book versions, 1000 vouchers in 60s.

## Spec Closure (pre-implementation)

- [x] Confirm complete voucher type catalog and SUNAT codes. (Closed Sprint 0: Factura 01, Boleta 03, Nota Credito 07, Nota Debito 08, Guia Remision 09, Recibo por Honorarios 02.)
- [x] Define exact purchase/sales classification rule as `tipoMovimiento`.
- [x] Define generated-book versioning policy using `LIBRO_CONTABLE.version` and `COMPROBANTE_LIBRO`.
- [~] Specify PDF and Excel layout for SUNAT books. → Deferred to ACC-T10.
- [x] Define versioned configuration table for tax rates and SUNAT formats.
- [x] Define audit events for voucher validation, mutation, book generation and export.
- [x] Review accounting API contract before implementation. (Closed Sprint 0: `ComprobanteRequest.anyOf` fixed.)
