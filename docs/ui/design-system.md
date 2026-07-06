# Design System

## Design System Purpose

The ERP design system defines the shared visual and behavioral language for all UX artifacts. It adopts Apple HIG ideas of clarity, deference, consistency, and depth, while preserving the density and speed expected from enterprise desktop software.

This is a UX specification. It defines semantic design decisions and interaction expectations, not implementation details or control classes.

The visible language is Peruvian Spanish. Canonical screen labels come from `screen-inventory.md`; layout behavior comes from `layout-specifications.md`.

## Visual Personality

| Quality | Direction |
|---|---|
| Calm | Neutral surfaces, restrained emphasis, predictable hierarchy. |
| Precise | Aligned numbers, clear labels, consistent status language. |
| Professional | Enterprise productivity tone, no decorative illustrations in work areas. |
| Trustworthy | Visible system state, audit context, confirmation for high-risk actions. |
| Efficient | Dense but readable information, keyboard-friendly structure, minimal navigation distance. |

## Color Semantics

Use color to communicate state, not decoration. Text labels must remain the primary carrier of meaning.

| Semantic Role | Intended Use |
|---|---|
| Institutional primary | Primary action, selected navigation item, current focus area. |
| Neutral surface | Main background, panels, forms, tables. |
| Neutral text | Body copy, labels, secondary metadata. |
| Success | Completed calculation, generated book, saved record, active account. |
| Warning | Estado Borrador, validacion pendiente, sesion por vencer, configuracion por revisar. |
| Danger | Error, blocked action, failed validation, annulled or inactive state. |
| Information | Helpful note, system update, read-only context. |

Color must pass contrast requirements for normal text, large text, icons, borders that convey state, and focused elements.

### Canonical Color Tokens

The baseline appearance is light and follows Windows system high-contrast overrides when enabled.

| Token | Value | Required Use |
|---|---|---|
| Primary | `#1F5C99` | Dominant action, selected navigation indicator, active link. |
| Focus | `#0A66C2` | Keyboard focus ring only. |
| Text primary | `#1B1D21` | Body, labels, table values. |
| Text secondary | `#4B5563` | Metadata and secondary explanations. |
| Surface | `#FFFFFF` | Main content. |
| Surface subtle | `#F4F6F8` | Header, alternate grouping, read-only region. |
| Border | `#B7BDC7` | Neutral boundaries that do not carry state. |
| Success | `#1F6B3A` | Completed/valid state with text or icon. |
| Warning | `#7A4D00` | Warning/attention state with text or icon. |
| Danger | `#B42318` | Error, annulled, destructive consequence. |
| Information | `#2457A7` | Informational state. |

State colors are used for text/icons on light surfaces and must not be used as the only indicator. Large filled color areas require a separately verified foreground pair.

## Typography

The canonical family is `Segoe UI`, using the current Windows fallback only when unavailable. Font size never scales with viewport width.

| Token | Size / Line Height | Weight | Use |
|---|---|---|---|
| Display | 24 px / 32 px | Semibold | Login product name or exceptional top-level identity. |
| Screen title | 20 px / 28 px | Semibold | Current workspace title. |
| Section title | 16 px / 24 px | Semibold | Form/report sections. |
| Body | 14 px / 20 px | Regular | Default labels, values, commands, messages. |
| Body strong | 14 px / 20 px | Semibold | Totals, selected summary, important labels. |
| Grid compact | 13 px / 18 px | Regular | Dense operational grids only. |
| Caption | 12 px / 16 px | Regular | Metadata, timestamps, secondary help. |

Error text uses Body or Caption according to available space and never relies on color alone. Financial totals use Body strong with tabular numeric alignment.

## Spacing and Density

The application should feel organized, not sparse. Spacing must create scan paths and prevent accidental clicks while preserving data density.

### Spacing Tokens

| Token | Value | Use |
|---|---:|---|
| XXS | 4 px | Icon/text internal separation, compact metadata. |
| XS | 8 px | Related fields/actions, minimum adjacent target gap. |
| S | 12 px | Field label-to-field rhythm, grid toolbar groups. |
| M | 16 px | Standard content padding and form row gap. |
| L | 24 px | Section separation. |
| XL | 32 px | Major workspace separation. |
| XXL | 48 px | Login or exceptional empty-state spacing only. |

Only these values and their sums may define UX spacing. New arbitrary spacing values require a design-system revision.

| Area | Density Direction |
|---|---|
| Dashboard | Medium density with strong KPI hierarchy. |
| Lists and grids | High density with legible row height and visible totals. |
| Forms | Medium density with grouped fields and predictable tab order. |
| Reports | Medium-high density with emphasis on totals and drill-down. |
| Administration | Medium density with safety spacing around risky actions. |

### Density Measurements

| Element | Compact | Standard | Comfortable |
|---|---:|---:|---:|
| Grid row | 32 px | 36 px | 40 px |
| Grid/header row | 36 px | 40 px | 44 px |
| Text-entry/select target | 32 px | 36 px | 40 px |
| Command target | 32 px | 36 px | 40 px |

- Standard is the default for forms and general lists.
- Compact is allowed only for high-volume grids and never for destructive confirmations.
- Comfortable is available as a user preference for long-session readability.
- Density changes preserve content, selection, scroll position, and focus.

### Shell Measurements

| Element | Measurement |
|---|---:|
| Expanded sidebar | 240 px |
| Collapsed sidebar | 64 px |
| Top shell region | 48 px minimum |
| Workspace horizontal padding | 16 px at 1280-1365; 24 px at 1366+ |
| Context panel | 320-420 px when shown; must not reduce main content below 720 px |

## Layout Grammar

Use a consistent layout grammar across modules:

- Shell: global navigation, user/session context, active module.
- Workspace header: title, period/scope, primary action, status.
- Filter/search band: search first, common filters visible, advanced filters collapsed.
- Content body: table, form, preview, or report.
- Summary/footer: totals, count, selected records, export readiness.
- Detail area: secondary panel, disclosure section, or separate detail workspace.

## Iconography

Icons are supportive, not decorative. They should reinforce known actions:

- Search.
- Add.
- Edit.
- Save.
- Refresh.
- Export.
- Print/PDF.
- Warning.
- Success.
- Lock/session.
- User/role.
- Calendar/period.
- Filter.

Every icon-only action must have an accessible name and a visible tooltip or equivalent label on discovery.

## Language and Tone

Use short, operational language:

- Prefer "Guardar empleado" over generic "Aceptar".
- Prefer "Calcular planilla" over "Procesar".
- Prefer "Generar libro de ventas" over "Ejecutar".
- Prefer "RUC invalido: debe tener 11 digitos" over "Error".

Tone rules:

- Be specific.
- Avoid blame.
- State consequence before irreversible actions.
- Use the user's business terms.
- Avoid technical implementation language in the UI.

### Peruvian Spanish and Locale

- Use `Iniciar sesion`, `Inicio`, `Administracion`, `Empleados`, `Planillas`, `Contabilidad`, `SUNAT`, and `Reportes`.
- Use `Guardar`, `Guardar y crear otro`, `Cancelar`, `Actualizar`, `Exportar`, `Finalizar planilla`, `Generar libro`, and `Anular comprobante`.
- Use `S/ 1,234.56` for display in this SDD baseline, with two decimal places and an explicit currency symbol.
- Use `dd/MM/yyyy` for dates and `YYYY-MM` plus a readable month label for accounting periods.
- Use the `America/Lima` business timezone for user-visible timestamps and include the date when ambiguity is possible.
- Preserve uppercase domain abbreviations: DNI, RUC, IGV, AFP, ONP, CTS, SUNAT.
- Do not mix English states or commands into the user interface.

## Status Language

| Status | Meaning |
|---|---|
| Borrador | Editable and recalculable. |
| Finalizada | Locked for normal editing and ready for reporting/export. |
| Cancelada | No longer active for operational use. |
| Registrado | Saved and available for downstream workflows. |
| Validacion conforme | A computed validation result passed required checks; it is not a persisted voucher state. |
| Anulado | Preserved historically but excluded from normal active workflows. |
| Activo | Available for selection or operation. |
| Inactivo | Hidden from new operations but retained historically. |

## Hierarchy Standards

- One visually dominant action per workflow state. A screen may expose secondary actions without competing emphasis.
- Secondary actions are grouped near the primary action only when part of the same workflow.
- Risky actions are visually separated from routine actions.
- Totals are visually stronger than row values but weaker than blocking errors.
- `Borrador`/`Finalizada` state is visible before actions that depend on it.

## Desktop Shell Visual Rules

- The persistent sidebar is visually subordinate to workspace content.
- Expanded and collapsed sidebar states preserve order, selection, and accessible names.
- Breadcrumbs use lower emphasis than the screen title.
- Focus, hover, pressed, selected, disabled, loading, error, and read-only states are visually distinct without relying on color alone.
- Dense grids may use compact spacing, but forms and confirmation areas retain comfortable separation to prevent errors.

## Interaction State Tokens

### Focus

- Keyboard focus uses a 2 px Focus-color ring with 2 px visual separation from the element boundary.
- Focus indication maintains at least 3:1 contrast against adjacent colors.
- Focus is never represented by color change alone and is not removed while the element is active.
- Validation does not move focus until submit; submit moves to the first error and announces the error summary.

### Hover and Pressed

- Hover changes surface or border without moving or resizing content.
- Pressed state is visually stronger than hover and returns immediately after activation.
- Hover is supplementary; every action remains discoverable without pointer hover.

### Disabled and Read-Only

- Disabled means the action cannot currently execute. It remains labeled, is removed from normal activation, and exposes a concise reason.
- Read-only means the value can be selected/copied but not changed. It uses Surface subtle and a visible `Solo lectura` state when ambiguity exists.
- Disabled meaningful text maintains at least 3:1 contrast. A disabled state never hides required information.
- Security restrictions hide unauthorized actions entirely; lifecycle restrictions keep authorized actions visible but disabled with a reason.

### Error, Warning, and Success

- Error uses Danger color plus icon and field/action-specific text.
- Warning uses Warning color plus text describing consequence.
- Success uses Success color plus confirmation text; it never depends on a transient color flash.

## Accessibility Measurements

| Requirement | Minimum |
|---|---:|
| Normal text contrast | 4.5:1 |
| Large text contrast | 3:1 |
| Meaningful icon/boundary contrast | 3:1 |
| Focus indicator contrast | 3:1 |
| Focus ring thickness | 2 px |
| Pointer target | 32 x 32 px |
| Destructive/dominant command height | 36 px |
| Gap between adjacent pointer targets | 8 px |
| Supported Windows display/text scaling | Up to 200% without clipped essential content |

Large text follows the WCAG definition. High-contrast mode may replace color tokens but must preserve role, focus, selection, status, error, and totals.
- Current period must be visible on payroll, accounting, SUNAT, and report screens.

## Motion and Feedback

Motion, if used, must communicate change in state:

- Loading progress.
- Transition from list to detail.
- Disclosure of advanced filters or details.
- Completion or failure of an operation.

Avoid decorative motion. Long operations must show progress and allow users to keep context.

## Enterprise Design Constraints

- Support daily use at 1280x720 through 1920x1080 without hiding primary actions.
- Maintain readable tables for large datasets.
- Prioritize keyboard access and predictable focus movement.
- Preserve financial precision and alignment.
- Keep visual complexity lower than data complexity.
- Avoid relying on color alone for state, priority, or errors.
