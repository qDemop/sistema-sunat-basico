# P1 Cross-Layer Consistency Acceptance

These specification-only cases verify P1 alignment. They do not add workflows, application code, or business scope and do not replace `WF-AT-001` through `WF-AT-064`.

| ID | Consistency case | Given | When | Then |
|---|---|---|---|---|
| P1-AT-001 | Schema dictionary parity | `database/schema.sql` and `database/schema.md` are inspected | Tables and direct columns are compared | All 26 tables and every column name appear once in the exact Markdown contract; FK/index/trigger targets resolve. |
| P1-AT-002 | Applied salary snapshot | Employee salary is S/ X when payroll is calculated | Employee master salary later changes | Existing payroll response/report still uses `PLANILLA.salario_base_aplicado = X`. |
| P1-AT-003 | Employee validation parity | Name is blank/invalid, employee is under 18, department/type is inactive, or account is malformed | API/database save is attempted | OpenAPI/business validation and SQL constraints/triggers reject the same invalid employee. |
| P1-AT-004 | Overtime registration guard | Employee is inactive or any payroll aggregate exists for the period | Overtime insert/register is attempted | SQL/API reject registration; lifecycle commands cannot bypass the aggregate guard. |
| P1-AT-005 | Account hierarchy and Posted-use guard | Account parent is inactive/descendant or account has a Posted line | Account hierarchy/classification mutation is attempted | Cycle/inactive parent is rejected; Posted use protects code/type/nature/parent while name/logical state remain governed. |
| P1-AT-006 | Active account line | Draft entry exists and selected account is inactive | Line insert/update is attempted | Database and API validation reject the line. |
| P1-AT-007 | Lifecycle metadata | Period/entry is Open/Draft then Closed/Posted | Lifecycle command succeeds | Closing/posting actor and timestamp are null before and required after the transition; response DTO exposes both. |
| P1-AT-008 | Governed configuration | Tax code is not IGV or format JSON has extra/missing/duplicate tokens | Draft creation is attempted | OpenAPI and SQL reject the request consistently. |
| P1-AT-009 | SUNAT header semantics | Book contains taxable and exempt bases | Stable-set generation runs | `total_base_imponible`/`totalBaseImponible` sums taxable base only; row snapshots retain exempt base and general total. |
| P1-AT-010 | Report snapshot file invariant | Export snapshot result is Generated | Snapshot is inserted | Nonempty file reference is required; Failed may retain no file and history response reports availability. |
| P1-AT-011 | Active detail/list routes | PAY-01, ADM-02/09, ACC-05/09 load | UI requests canonical data | `listPayrollPeriods`, `getUser`, `getAuditEvent`, `getAccount`, and `getJournalEntry` provide typed, role-protected responses. |
| P1-AT-012 | Pending UX boundary | Unsupported auxiliary screen/control is evaluated | Navigation/actions render | Backup status, bulk overtime, persistent direct-export histories, saved views, report comparison and global search remain hidden and are not reported as COMPLETE links. |
| P1-AT-013 | Role/action parity | Each OpenAPI operation and UX action is inspected | Role sets are compared | Payroll is RRHH/Admin, Accounting SUNAT is Contador/Admin, Reports is Gerente/Admin, Administration is Admin, and General Ledger preserves read/write split. |

