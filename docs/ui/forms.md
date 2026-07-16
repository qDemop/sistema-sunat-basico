# Form Standards

## Form Objective

Forms must support accurate, fast, keyboard-first entry while preventing silent business errors. The form experience should feel calm and structured, with the user's attention directed to the current business decision.

Visible labels use Peruvian Spanish. Form IDs below are referenced by `ux-traceability.md`; no wireframe may add, remove, or rename a field without updating this document.

## Form Anatomy

Each form should include:

- Title with mode: new, edit, review, or read-only.
- Object identity when editing.
- Status when relevant.
- Required-field convention.
- Grouped fields in business order.
- Inline validation.
- Save and cancel actions.
- Dirty-state warning when leaving with unsaved changes.

## Shared Form Visual Foundation

Forms use the shared UI Foundation and semantic tokens from `design-system.md`; they do not define independent colors, radii, heights, focus treatment, or window chrome. Standard text-entry/select targets are 36 px high, compact targets are 32 px only in high-volume contexts, and destructive or dominant commands are at least 36 px high. Labels, validation, recovery messages, and legal or financial values wrap or expand rather than clipping; exceptions require an explicit rationale and an accessible full value.

At 1280x720, 1366x768, and 1920x1080 layouts and through 200% Windows scaling, resizing must preserve the visible primary action, validation summary, current period where applicable, and financial totals. Secondary actions may group, but required fields and keyboard access remain available.

## Field Grouping

Group fields by user mental model, not database structure.

| Form | Recommended Groups |
|---|---|
| Employee | Identity, employment, compensation, pension, bank, status. |
| Overtime | Period, employee, approved hours, notes/status. |
| Payroll action | Period, readiness checks, calculation/finalization summary. |
| Voucher | Movement, document identity, party identity, amounts, tax, status. |
| User | Identity, role, account status, security actions. |
| Tax configuration | Version identity, effective dates, rates/codes, reason/history. |
| SUNAT format | Version identity, book type, effective dates, column/format summary. |

## Form Modes

| Mode | UX Rules |
|---|---|
| New | Empty state with required fields clear and primary action named for creation. |
| Edit | Existing values visible; changed fields are understandable before save. |
| Review | Read-only information with actions separated from content. |
| Locked | Explain why editing is unavailable, such as finalized payroll or historical tax version. |

## Validation Standards

Validation should happen early enough to prevent wasted work and late enough to avoid noisy interruption.

| Validation Type | UX Behavior |
|---|---|
| Required field | Indicate after field exit or save attempt. |
| Format | Indicate when a value is complete enough to evaluate. |
| Duplicate | Indicate after enough identity fields are present. |
| Business rule | Explain consequence and source object where possible. |
| Cross-field | Place message near the field group and highlight affected fields. |

## Required Fields

Required markers must be consistent. Do not rely on placeholder text as the only label or required indicator.

Required examples:

- DNI.
- Name and surname.
- Department.
- Hire date.
- Salary.
- Pension regime.
- Voucher movement.
- Voucher type, series, number.
- RUC/DNI when required.
- Period.
- Role.

## Financial Entry Standards

- Monetary values use Peruvian soles where applicable.
- Money displays two decimal places.
- Negative values are not allowed unless explicitly defined by the business process.
- Derived values such as IGV, total, net pay, and discounts are visually connected to their source inputs.
- Derived values must be marked as calculated or read-only in UX language.

## Date and Period Standards

- Payroll, SUNAT, and reports must show a period in `YYYY-MM` or an equivalent readable month-year label.
- Date ranges must show both start and end.
- Future dates are rejected where business rules forbid them.
- Periods `Finalizada` or `Cerrado` must be visibly distinct from editable periods.

## Save and Cancel

Rules:

- Save action labels should include the object: `Guardar empleado`, `Guardar comprobante`, `Guardar usuario`.
- Cancel returns to the source context without saving.
- If data changed, cancel or navigation away asks whether to discard changes.
- Successful save confirms completion and preserves the user's next logical workflow.
- Failed save preserves all user-entered data.

## Form Error Summary

When a save attempt fails with multiple errors:

- Show a concise error summary at the top of the form.
- Keep field-level messages in place.
- Allow users to move directly to the first error.
- Do not clear valid fields.

## High-Risk Form Actions

High-risk actions are not routine save actions:

- Deactivate employee.
- Deactivate user.
- Reset password.
- Finalize payroll.
- Annul voucher.
- Activate a new tax configuration version.

These actions require explicit confirmation and must describe impact.

Reversible deactivation should offer `Reactivar` from the detail or filtered inactive list. Finalization, posting, annulment, and version generation remain explicit irreversible or legally significant actions.

## Canonical Form Inventory

| Form ID | Screen | Canonical fields in focus order |
|---|---|---|
| FRM-AUT-01 | AUT-01 | Usuario; Contrasena. |
| FRM-ADM-01 | ADM-02 | Nombre de usuario; Nombres y apellidos; Contrasena temporal on create; Rol; Estado derived. Security actions are separate. |
| FRM-ADM-02 | ADM-06 | Tipo de configuracion; Codigo; Regimen previsional conditional; Version; Descripcion; Tasa; Vigente desde; Vigente hasta; Estado derived. |
| FRM-ADM-03 | ADM-08 | Tipo de libro; Version de formato; Definicion de estructura; Vigente desde; Vigente hasta; Estado derived. |
| FRM-EMP-01 | EMP-02 | DNI; Nombres; Apellidos; Fecha de nacimiento; Departamento; Cargo; Fecha de ingreso; Salario base; Regimen pensionario; Banco; Numero de cuenta. |
| FRM-EMP-02 | EMP-05 | Nombre del departamento; Descripcion; Estado. |
| FRM-EMP-03 | EMP-07 | Periodo; Empleado; Departamento derived; Primeras dos horas; Horas adicionales; Estado derived. |
| FRM-PAY-01 | PAY-02 | Periodo; Estado; Preparacion; Empleados elegibles; Alertas. Results are displayed in GRD-PAY-02. |
| FRM-ACC-01 | ACC-02 | Movimiento; Tipo de comprobante; Comprobante de referencia conditional; Serie; Numero; Fecha de emision; Tipo de documento; Numero de documento; Razon social; Tipo de operacion; Base gravada; Base exonerada; IGV derived; Total derived; Version tributaria derived. |
| FRM-ACC-02 | ACC-05 | Codigo de cuenta; Nombre; Tipo; Naturaleza; Cuenta padre; Estado derived. |
| FRM-ACC-03 | ACC-06 | Periodo; Fecha de inicio; Fecha de fin; Estado. |
| FRM-ACC-04 | ACC-08 | Periodo; Fecha; Descripcion; Origen; Lines: Cuenta, Descripcion, Debe, Haber. Totals derived. |
| FRM-SUN-01 | SUN-02 | Periodo; Tipo de libro; Version existente derived; Comprobantes elegibles derived; Exclusiones derived; Validacion derived. |
| FRM-REP-01 | REP-01/02/03/04 | Reporte derived by screen; Periodo or date range; Departamento only for payroll report; Alcance derived by role. |

FRM-EMP-03 multi-row save/partial-result behavior and FRM-REP-01 `Comparar con` are `IMPLEMENTATION PENDING`. The active contracts support one overtime row per request and one requested reporting period/range; these controls must remain hidden until corresponding API contracts exist.

## Authentication Field Definition - FRM-AUT-01

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Usuario | Yes | At least 3 alphanumeric characters; leading/trailing spaces ignored. | None. | Editable until submit. |
| Contrasena | Yes | Nonempty; validated without revealing which credential failed. | Usuario must be present. | Masked; never redisplayed, persisted in UI, or included in logs. Paste remains available. |

After three failed attempts the account is blocked for 15 minutes. The visible error remains generic. Password recovery is outside the current scope and no recovery link is displayed.

## User Field Definition - FRM-ADM-01

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Nombre de usuario | Yes | 3-50 alphanumeric characters; unique. | None. | Immutable after creation. |
| Nombres y apellidos | Yes | 1-200 characters; not blank after trimming. | None. | Editable while account exists. |
| Contrasena temporal | Create only | Nonempty and accepted by configured credential policy. | New user mode. | Masked; never shown after save. Password changes use Restablecer contrasena. |
| Rol | Yes | One active predefined role: Administrador RRHH, Contador, Gerente Financiero, Administrador Sistema. | Role catalog. | Change takes effect on next authentication and is audited. |
| Estado | Derived | Activo or Inactivo. | Account state. | Changed only through Desactivar/Reactivar. |
| Ultimo acceso | Derived | Lima timestamp. | Authentication history. | Read-only. |

## Tax and Pension Configuration Field Definition - FRM-ADM-02

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Tipo de configuracion | Yes | IGV or Regimen previsional. | None. | Immutable after first save. |
| Codigo | Yes | `IGV`, `AFP`, or `ONP`; unique together with Version. | Tipo de configuracion. | Immutable after first save. |
| Regimen previsional | Conditional | AFP or ONP; required only for Regimen previsional and must equal Codigo. | Tipo de configuracion. | Immutable after first save. |
| Version | Yes | Positive integer; unique for Codigo. Default is next available version. | Codigo. | Immutable after first save. |
| Descripcion | Conditional | Maximum 250 characters; available only for IGV. | Tipo de configuracion. | Hidden for pension configuration; editable only while Borrador. |
| Tasa | Yes | 0.00-100.00 percent; two decimals. Represents IGV or the employee pension deduction according to Codigo. | Codigo. | Editable only while Borrador. |
| Vigente desde | Yes | `dd/MM/yyyy`. | None. | Editable only while Borrador. |
| Vigente hasta | No | `dd/MM/yyyy`; not earlier than Vigente desde. | Vigente desde. | Editable only while Borrador. |
| Estado | Derived | Borrador, Activa, Cerrada. | Effective dates and activation action. | Read-only. |

`Activar version` is available for Draft and `Cerrar version` for Active, only to Administrador Sistema. Activation requires no overlapping range. Payroll resolves AFP/ONP by regime/period; vouchers resolve IGV by issue date. Active and Closed versions are immutable; corrections create a new Draft version.

## SUNAT Format Field Definition - FRM-ADM-03

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Tipo de libro | Yes | Compras or Ventas. | None. | Immutable after first save. |
| Version de formato | Yes | 1-20 characters; unique for Tipo de libro and validity. | Tipo de libro. | Immutable after first save. |
| Definicion de estructura | Yes | Governed format definition; must pass structural validation before activation. | Tipo de libro and version. | Editable only while Borrador and never shown as raw implementation metadata in normal read views. |
| Vigente desde | Yes | `dd/MM/yyyy`. | None. | Editable only while Borrador. |
| Vigente hasta | No | `dd/MM/yyyy`; not earlier than Vigente desde and no overlap. | Vigente desde. | Editable only while Borrador. |
| Estado | Derived | Borrador, Activa or Cerrada. | Explicit activation/closing action. | Read-only. |

`Activar formato` requires a structurally valid Draft and no overlapping Active version for the same book type. `Cerrar formato` is visible only for Active and ends future consumption without modifying generated books. Active and Closed versions are immutable; corrections create a new Draft format version.

## Department Field Definition - FRM-EMP-02

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Nombre del departamento | Yes | 1-100 characters; unique after trimming. | None. | Editable while Active. |
| Descripcion | No | Maximum 250 characters. | None. | Editable while Active. |
| Estado | Derived | Activo or Inactivo. | Status action. | Changed only through Desactivar/Reactivar; inactive departments cannot be assigned. |

## Overtime Field Definition - FRM-EMP-03

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Periodo | Yes | `YYYY-MM`; one record per employee/period. | Payroll period context. | Fixed for all rows in bulk entry. |
| Empleado | Yes | Active employee; searchable by DNI/name. | Periodo. | Immutable after first save. |
| Departamento | Derived | Current employee department. | Empleado. | Read-only. |
| Primeras dos horas | Yes | Decimal, default 0, nonnegative. | Empleado and Periodo. | Editable only in Borrador. |
| Horas adicionales | Yes | Decimal, default 0, nonnegative. | Empleado and Periodo. | Editable only in Borrador. |
| Estado | Derived | Borrador, Aprobada, Cancelada. | Lifecycle action. | Read-only. |

At least one hour value must be greater than 0. `Aprobar horas extra` locks values. Approved overtime may be cancelled only before a payroll Draft exists for the same period.

## Payroll Action Field Definition - FRM-PAY-01

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Periodo | Yes | `YYYY-MM`; valid month/year. | None. | Fixed while calculation is running. |
| Estado de planilla | Derived | Sin calcular, Borrador, Finalizada, Cancelada. | Existing payroll. | Read-only. |
| Empleados activos/elegibles | Derived | Count greater than 0 to calculate. | Employee records. | Read-only; opens filtered evidence. |
| Datos incompletos | Derived | Count and list of blocking employee errors. | Employee validation. | Read-only; opens correction path. |
| Horas extra aprobadas | Derived | Included count; Draft/Cancelled records excluded. | Overtime lifecycle. | Read-only. |
| Totales y alertas | Derived | Visible after calculation; gross earnings, employee deductions, net cash payment, employer CTS/gratification provisions and total payroll cost use two decimals. | GRD-PAY-02. | Read-only. |

Calculate is available only when no payroll exists or state is Draft. Finalize is available only for a successful Draft with no blocking readiness errors. Finalized and Cancelled records are read-only.

## Account Field Definition - FRM-ACC-02

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Codigo de cuenta | Yes | 1-20 characters; unique. | None. | Immutable after the account is referenced by a Posted entry. |
| Nombre | Yes | 1-150 characters; not blank after trimming. | None. | Editable while Active. |
| Tipo | Yes | Activo, Pasivo, Patrimonio, Ingreso, Costo, Gasto. | None. | Immutable after Posted use. |
| Naturaleza | Yes | Debe or Haber. | Tipo. | Immutable after Posted use. |
| Cuenta padre | No | Active account; cannot be self or descendant. | Compatible hierarchy. | Immutable after Posted use when hierarchy would change reporting. |
| Estado | Derived | Activa or Inactiva. | Status action. | Changed through Desactivar/Reactivar; inactive accounts cannot be used in new lines. |

## Accounting Period Field Definition - FRM-ACC-03

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Periodo | Yes | Unique `YYYY-MM`. | None. | Immutable after creation. |
| Fecha de inicio | Yes | `dd/MM/yyyy`. | Must correspond to Periodo. | Immutable after any entry exists. |
| Fecha de fin | Yes | `dd/MM/yyyy`; not earlier than start. | Fecha de inicio and Periodo. | Immutable after any entry exists. |
| Estado | Derived | Abierto or Cerrado. | Close action. | Closed is terminal; no Reopen. |

Close is available to Contador/Admin only when no Draft entries remain. Closed periods are read-only.

A newly saved valid period starts as Abierto. Cerrado is terminal in the current scope.

## Journal Entry Field Definition - FRM-ACC-04

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Periodo | Yes | Existing Open period. | None. | Immutable after first save. |
| Fecha | Yes | `dd/MM/yyyy` within selected period. | Periodo. | Editable only in Draft. |
| Descripcion | Yes | 1-250 characters. | None. | Editable only in Draft. |
| Origen | Derived | `Manual` for user-created entries. Comprobante, Planilla and Ajuste are assigned only by their domain commands. | Creation context. | Read-only. |
| Referencia de origen | Derived | Hidden for manual creation; shown read-only on source-generated entry detail. | Origen. | Never user-editable. |
| Cuenta | Each line | Active account. | Periodo and line. | Editable only in Draft. |
| Descripcion de linea | No | Maximum 250 characters. | Line. | Editable only in Draft. |
| Debe | Each line | Nonnegative, two decimals; exactly one of Debe/Haber is greater than 0. | Line. | Editable only in Draft. |
| Haber | Each line | Nonnegative, two decimals; exactly one of Debe/Haber is greater than 0. | Line. | Editable only in Draft. |
| Total debe/haber/diferencia | Derived | Two decimals; difference must be S/ 0.00 to post. | All lines. | Read-only. |
| Estado | Derived | Borrador, Contabilizado, Cancelado. | Lifecycle action. | Read-only. |

At least two valid lines are required. Posted entries are immutable; reversal creates a separate linked adjustment Draft.

## SUNAT Generation Field Definition - FRM-SUN-01

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Periodo | Yes | `YYYY-MM`. | None. | Fixed during validation/generation. |
| Tipo de libro | Yes | Compras or Ventas. | None. | Fixed during validation/generation. |
| Version vigente | Derived | Highest version or Sin version. | Periodo and type. | Read-only. |
| Comprobantes elegibles | Derived | Registered vouchers matching period/movement. | Periodo, type, vouchers. | Read-only; opens evidence. |
| Exclusiones | Derived | Count and reasons by voucher. | Validation. | Read-only; opens evidence. |
| Configuracion/formato aplicado | Derived | Active SUNAT format version and tax versions resolved for every eligible voucher. | Administration versions. | Read-only. |
| Estado de validacion | Derived | Pendiente, Valida, Con observaciones, Bloqueada. | All validations. | Read-only. |
| Totales | Derived | Base, exempt base, IGV, total; two decimals. | Eligible snapshot. | Read-only. |

Generation is available only in Valida or acknowledged Con observaciones state. Each generation creates a new immutable sequential version.

## Report Filter Field Definition - FRM-REP-01

| Label | Required | Format and Validation | Dependency | Edit Restriction |
|---|---:|---|---|---|
| Reporte | Derived | Panel financiero, Balance general, Estado de resultados, Planilla consolidada. | Current screen. | Read-only. |
| Periodo or Rango de fechas | Yes | `YYYY-MM` or inclusive `dd/MM/yyyy` range according to report. | Reporte. | Editable before refresh/export. |
| Departamento | No | Active/historical department list; available only for Planilla consolidada. | Reporte. | Hidden for other reports. |
| Comparar con | No | IMPLEMENTATION PENDING; hidden until an API comparison contract exists. | Primary period. | Not active. |
| Alcance | Derived | Authorized report dataset. | Role and report. | Read-only. |
| Ultima actualizacion | Derived | Lima timestamp and freshness state. | Loaded result. | Read-only. |

Only Gerente Financiero and Administrador Sistema access financial reports. Exports use the exact displayed filters and values.

Report-specific dependencies are fixed:

| Screen | Required Time Filter | Optional Filter | Result Scope |
|---|---|---|---|
| REP-01 Panel financiero | Period or inclusive date range shown in header. | None; comparison is `IMPLEMENTATION PENDING`. | Role-authorized financial KPIs. |
| REP-02 Balance general | Accounting period/cutoff date. | None; comparison is `IMPLEMENTATION PENDING`. | Posted Asset, Liability, Equity balances. |
| REP-03 Estado de resultados | Accounting period or inclusive date range. | None; comparison is `IMPLEMENTATION PENDING`. | Posted Income, Cost, Expense entries. |
| REP-04 Planilla consolidada | Payroll period `YYYY-MM`. | Departamento. | Persisted payroll values; no bank account data. |

## Employee Field Definition - FRM-EMP-01

| Label | Required | Format or Source | Edit Rule |
|---|---:|---|---|
| DNI | Yes | 8 numeric digits; unique. | Editable only before any payroll record exists for the employee; then read-only. |
| Nombres | Yes | At least 2 characters; letters, spaces, and valid name punctuation. | Editable. |
| Apellidos | Yes | At least 2 characters; letters, spaces, and valid name punctuation. | Editable. |
| Fecha de nacimiento | Yes | `dd/MM/yyyy`; minimum age 18. | Editable with validation. |
| Departamento | Yes | Active department catalog. | Editable for the employee; changes affect future or recalculated Draft payroll only and never alter Finalized results. |
| Cargo | Yes | 1-80 characters; governed free text. | Editable. |
| Fecha de ingreso | Yes | `dd/MM/yyyy`; not future. | Changes affect only future calculation/recalculation of Draft payroll; finalized payroll remains unchanged. |
| Salario base | Yes | `S/`, positive, two decimals. | Changes affect only future calculation/recalculation of Draft payroll; finalized payroll remains unchanged. |
| Regimen pensionario | Yes | AFP or ONP. | Changes affect only future calculation/recalculation of Draft payroll; finalized payroll remains unchanged. |
| Banco | Yes | 1-80 characters; suggested bank list with governed `Otro` entry. | Editable. |
| Numero de cuenta | Yes | 14-20 digits according to selected bank. | Mask in read-only contexts where role does not require full value. |
| Estado | Derived | Activo or Inactivo. | Changed through Desactivar/Reactivar, not ordinary save. |

Employee dependencies are fixed: Departamento must be Active; Regimen pensionario is AFP or ONP; Numero de cuenta validation depends on Banco; payroll-impacting changes affect only future or recalculated Draft payroll and never alter Finalized results.

## Voucher Field Definition - FRM-ACC-01

| Label | Required | Format or Source | Edit Rule |
|---|---:|---|---|
| Movimiento | Yes | Compra or Venta. | Required before document identity. |
| Tipo de comprobante | Yes | Governed voucher catalog. | Part of duplicate identity. |
| Comprobante de referencia | Conditional | Existing Registrado voucher with the same Compra/Venta movement. | Required only for Nota de Credito or Nota de Debito; hidden otherwise. |
| Serie | Yes | Uppercase normalized text. | Part of duplicate identity. |
| Numero | Yes | Normalized voucher number. | Part of duplicate identity. |
| Fecha de emision | Yes | `dd/MM/yyyy`. | Resolves tax version. |
| Tipo de documento | Yes | RUC, DNI, or allowed catalog value. | Controls number validation. |
| Numero de documento | Yes | RUC 11 digits; DNI 8 digits. | Searches matching party where available. |
| Razon social | Yes | Party legal or personal name. | Confirmed by user. |
| Tipo de operacion | Yes | Gravada, Exonerada, Inafecta, or governed value. | Controls amount requirements. |
| Base gravada | Conditional | `S/`, non-negative, two decimals; at least one base must be greater than zero. | Required for taxable operation. |
| Base exonerada | Conditional | `S/`, non-negative, two decimals; at least one base must be greater than zero. | Required for exempt/inaffected operation. |
| IGV | Derived | Active tax version applied to issue date. | Read-only and visibly calculated. |
| Total | Derived | Sum of applicable amounts. | Read-only and visibly calculated. |
| Version tributaria | Derived | Effective configuration identifier. | Read-only; visible in detail. |
| Estado | Derived | Registrado or Anulado. | Anulacion is a separate action. |

Voucher fields are editable only while Estado is Registrado and the voucher is not linked to a generated book version. Once linked, the record is read-only; correction uses Anular comprobante where permitted and a later SUNAT book version. Anulado is terminal and read-only. Validation is a computed result, never a persisted voucher state.

Voucher dependencies are fixed: Tipo de documento controls Numero de documento format; Fecha de emision resolves Version tributaria; Tipo de operacion controls required taxable/exempt bases; Movimiento plus Tipo, Serie, and Numero define duplicate identity; Base values and tax version derive IGV and Total.

## Privacy Classification

| Form / Fields | Classification | Visible To | UX Requirements |
|---|---|---|---|
| FRM-AUT-01 / Contrasena | Secret | Current user only during entry. | Mask, never log, never persist in UI, allow secure paste. |
| FRM-ADM-01 / Contrasena temporal | Secret | Administrador Sistema during creation/reset only. | Mask and never redisplay after save. |
| FRM-ADM-01 / identity, role, status | Restricted administration | Administrador Sistema. | Global search is `IMPLEMENTATION PENDING`; future contracts must not expose it to other roles. |
| FRM-EMP-01 / DNI, birth date | Restricted personal | RRHH, Administrador Sistema. | Full value only in authorized employee screens/exports. |
| FRM-EMP-01 / Salario base, pension | Restricted payroll | RRHH, Administrador Sistema. | Gerente sees only authorized report aggregates/results, not employee form. |
| FRM-EMP-01 / Banco, Numero de cuenta | Highly restricted payroll | RRHH, Administrador Sistema. | Mask outside edit/detail need; never include in general search, Dashboard, or financial reports. |
| FRM-EMP-03 / overtime | Restricted payroll | RRHH, Administrador Sistema. | Visible only in payroll context and audit. |
| FRM-PAY-01 / employee results | Restricted payroll | RRHH, Administrador Sistema. | Exports follow same role restriction. |
| FRM-ACC-01 / party and amounts | Restricted accounting | Contador, Administrador Sistema. | Not exposed to RRHH or general global-search results. |
| FRM-ACC-02/03/04 / balances and entries | Restricted accounting | Contador, Administrador Sistema; Gerente read-only where traced. | Gerente cannot edit, post, cancel, or reverse. |
| FRM-SUN-01 / books and exports | Restricted tax | Contador, Administrador Sistema. | Historical versions retain generator and timestamp. |
| FRM-REP-01 / financial reports | Restricted financial | Gerente Financiero, Administrador Sistema. | Export permissions mirror report access; no bank account data. |

## Normalization Rules

- `Periodo` always means `YYYY-MM` plus a readable month-year label.
- `Movimiento` always uses `Compra` or `Venta`.
- `Regimen pensionario` always uses `AFP` or `ONP`.
- `Estado` is not silently editable inside ordinary forms when a dedicated transition exists.
- Derived financial values remain visible, labeled `Calculado`, and excluded from the normal edit sequence.
- Wireframes and grids must reuse the exact labels and field order in this inventory.

## Form Acceptance Checklist

- Form title states mode and object.
- Required fields are visible before save.
- Tab order follows business order.
- Inline validation is specific and recoverable.
- Derived totals are visually connected to input fields.
- Save/cancel behavior is predictable.
- Unsaved changes are protected.
- Locked states explain why editing is unavailable.
