# Design System

## Design System Purpose

The ERP design system defines the shared visual and behavioral language for all UX artifacts. It adopts Apple HIG ideas of clarity, deference, consistency, and depth, while preserving the density and speed expected from enterprise desktop software.

This is a UX specification. It defines semantic design decisions and interaction expectations, not implementation details or control classes.

All screens consume one reusable UI Foundation: semantic tokens, layout primitives, and shared visual components. Custom window chrome, if later adopted, is a separate reusable shell concern; it must not be duplicated by Forms or coupled to business workflows.

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

## Appearance Modes

Light mode is the default Sprint 1 baseline. Dark mode is an officially supported theme for prolonged desktop work, accessibility comfort, and reduced visual fatigue. System mode is recognized as a user preference target, but OS synchronization remains a future enhancement until settings and Windows appearance integration are introduced.

Appearance rules:

- Themes use semantic tokens only; screens and controls must not hardcode visual colors.
- Dark mode preserves clarity, hierarchy, deference to content, and low visual noise.
- Color is never the only state indicator; labels, icons, shape, or position must also communicate state.
- Foreground/background pairs must meet WCAG AA contrast: 4.5:1 for normal text and 3:1 for large text, icons, focus indicators, and non-text UI components.
- Borders must not carry critical meaning unless their contrast is at least 3:1 against adjacent colors.
- Disabled states must remain readable while clearly inactive.
- Focus rings must remain visible in both light and dark mode.
- Destructive actions must remain clearly distinguishable in both themes through Danger color plus text, icon, or confirmation language.
- Windows high-contrast mode may override color values but must preserve role, focus, selection, status, error, and totals.

### Canonical Light Theme Tokens

The baseline appearance is light and follows Windows system high-contrast overrides when enabled. Existing Sprint 0 light values remain canonical.

| Token | Value | Required Use |
|---|---|---|
| App background | `#F4F6F8` | Window background outside primary work surfaces. |
| Surface | `#FFFFFF` | Main content, forms, tables. |
| Surface subtle | `#F4F6F8` | Header, alternate grouping, read-only region. |
| Surface elevated | `#FFFFFF` | Panels, flyouts, contextual surfaces. |
| Border | `#B7BDC7` | Neutral boundaries that do not carry critical meaning. |
| Border strong | `#4B5563` | Meaningful boundaries requiring stronger separation. |
| Text primary | `#1B1D21` | Body, labels, table values. |
| Text secondary | `#4B5563` | Metadata and secondary explanations. |
| Text muted | `#4B5563` | Low-emphasis helper text that remains readable. |
| Primary | `#1F5C99` | Dominant action, selected navigation indicator, active link. |
| Primary hover | `#0A66C2` | Hover or emphasized primary action state. |
| Focus | `#0A66C2` | Keyboard focus ring only. |
| Success | `#1F6B3A` | Completed/valid state with text or icon. |
| Warning | `#7A4D00` | Warning/attention state with text or icon. |
| Danger | `#B42318` | Error, annulled, destructive consequence. |
| Information | `#2457A7` | Informational state. |
| Selection background | `#E6F0FA` | Selected row, selected navigation item, selected option background. |
| Grid row hover | `#F4F6F8` | Non-selected row hover state. |
| Grid row selected | `#E6F0FA` | Selected grid row background. |
| Input background | `#FFFFFF` | Text entry and selection controls. |
| Input border | `#B7BDC7` | Input boundary. |
| Input disabled background | `#F4F6F8` | Disabled text entry and selection controls. |
| Input disabled text | `#4B5563` | Disabled value text. |

State colors are used for text/icons on light surfaces and must not be used as the only indicator. Large filled color areas require a separately verified foreground pair.

### Canonical Dark Theme Tokens

Dark theme uses the same semantic roles as light mode. These values are the initial baseline and require contrast verification when paired with new foreground/background combinations.

| Token | Value | Required Use |
|---|---|---|
| App background | `#0F1115` | Window background outside primary work surfaces. |
| Surface | `#151922` | Main content, forms, tables. |
| Surface subtle | `#1E2430` | Header, alternate grouping, read-only region. |
| Surface elevated | `#242B38` | Panels, flyouts, contextual surfaces. |
| Border | `#3A4352` | Neutral boundaries that do not carry critical meaning. |
| Border strong | `#596273` | Meaningful boundaries requiring stronger separation. |
| Text primary | `#F3F6FA` | Body, labels, table values. |
| Text secondary | `#C5CBD5` | Metadata and secondary explanations. |
| Text muted | `#8E97A6` | Low-emphasis helper text that remains readable. |
| Primary | `#8AB4F8` | Dominant action, selected navigation indicator, active link. |
| Primary hover | `#A7C7FF` | Hover or emphasized primary action state. |
| Focus | `#7CB7FF` | Keyboard focus ring only. |
| Success | `#7DDC8A` | Completed/valid state with text or icon. |
| Warning | `#F2B84B` | Warning/attention state with text or icon. |
| Danger | `#FF8A80` | Error, annulled, destructive consequence. |
| Information | `#8AB4F8` | Informational state. |
| Selection background | `#203A5F` | Selected row, selected navigation item, selected option background. |
| Grid row hover | `#202633` | Non-selected row hover state. |
| Grid row selected | `#203A5F` | Selected grid row background. |
| Input background | `#10141C` | Text entry and selection controls. |
| Input border | `#4A5364` | Input boundary. |
| Input disabled background | `#1A1F2A` | Disabled text entry and selection controls. |
| Input disabled text | `#8E97A6` | Disabled value text. |

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

### Shape and Elevation Measurements

| Element | Radius | Elevation rule |
|---|---:|---|
| Text-entry and selection controls | 6 px | No shadow; use border and focus ring. |
| Buttons | 6 px | No shadow in resting state. |
| Cards and panels | 8 px | At most one subtle elevation level: a 1 px border plus `Elevation-1`. |
| Dialogs and transient surfaces | 10 px | May use `Elevation-1`; no higher elevation is approved. |

Exceptions require an explicit UX rationale and must preserve focus, contrast, target size, and Windows affordances.

`Elevation-1` is `offset-x: 0 px`, `offset-y: 2 px`, `blur: 8 px`, and `spread: 0 px`. Its shadow color is `#1B1D21` at 12% opacity in the light theme and `#000000` at 24% opacity in the dark theme. It is the only approved shadow token.

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

| Motion type | Duration | Acceptance rule |
|---|---:|---|
| Disclosure, hover, or state transition | 120-200 ms | Must communicate a state change without moving focus or content unexpectedly. |
| Reduced-motion preference | 0 ms for nonessential motion | Preserve the final state, progress, and status feedback without animation. |
| Long operation | No looping decorative animation | Show operation name and progress or a working state within 200 ms; never hide the current context. |

### Visual Acceptance Measurements

- Essential text, commands, current period, totals, validation summaries, and primary actions must remain visible without clipping at supported window sizes and at Windows text/display scaling through 200%.
- Long labels and messages wrap or expand vertically; they must not be truncated when they contain validation, legal, financial, or recovery information. Truncation is allowed only for secondary text with a full accessible name and tooltip.
- Light and dark themes use the same semantic roles, and every newly introduced foreground/background pair is contrast-verified before approval.
- Resize testing covers 1280x720, 1366x768, and 1920x1080 logical layouts, including sidebar collapse and preservation of focus, selected record, and dominant action.

## Enterprise Design Constraints

- Support daily use at 1280x720 through 1920x1080 without hiding primary actions.
- Maintain readable tables for large datasets.
- Prioritize keyboard access and predictable focus movement.
- Preserve financial precision and alignment.
- Keep visual complexity lower than data complexity.
- Avoid relying on color alone for state, priority, or errors.
