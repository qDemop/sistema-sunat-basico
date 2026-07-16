# Accessibility Guidelines

## Accessibility Objective

ERP.WinForms must be usable for daily enterprise work by people with different visual, motor, cognitive, and assistive technology needs. Accessibility is a productivity requirement, not a secondary compliance layer.

The target is WCAG 2.1 AA intent adapted to a Windows desktop ERP.

The measurable source of truth is `design-system.md`: normal text 4.5:1, large text/non-text/focus 3:1, focus ring 2 px, pointer target 32 x 32 px, adjacent target gap 8 px, and essential-content support through 200% Windows scaling.

All accessible names, descriptions, errors, status announcements, and shortcuts use Peruvian Spanish. The canonical shortcut set is defined in `keyboard-shortcuts.md`.

## Keyboard Access

All frequent workflows must be possible by keyboard:

- Login.
- Module navigation.
- Search.
- Grid navigation and selection.
- Form completion.
- Save/cancel.
- Payroll calculation.
- Voucher registration.
- SUNAT generation.
- Report filtering and export.

Rules:

- Focus order follows visual and business order.
- Focus is always visible.
- Focus never moves unexpectedly after validation.
- Modal or transient surfaces return focus to the triggering context when closed.
- Keyboard traps are not allowed.
- `F6` cycles through sidebar, workspace header, filters, content, summary, and contextual panel.
- Collapsing the sidebar preserves focus and does not change destination order.
- Breadcrumbs are keyboard reachable but do not interrupt the primary tab sequence.

## Screen Reader and Labels

Every meaningful element must have a clear accessible name:

- Fields use persistent labels.
- Icon-only actions expose action names.
- Status indicators include text.
- Errors identify the affected field or object.
- Tables expose row and column meaning.
- Financial totals are announced with their labels and currency context.

Avoid relying on placeholder text as the only label.

## Color and Contrast

- Text and critical UI indicators must meet AA contrast in both light and dark themes.
- Normal text must meet at least 4.5:1 contrast against its background.
- Large text, meaningful icons, focus indicators, and non-text UI components must meet at least 3:1 contrast.
- Borders must not carry critical meaning unless they meet at least 3:1 contrast against adjacent colors.
- Error, warning, success, and selected states must use text, icon, shape, or position in addition to color.
- Focus indicators must be visible against light, dark, neutral, selected, and elevated surfaces.
- Keyboard users must be able to identify focus clearly at every step.
- Sprint 1 WinForms theming applies the Focus token where native controls expose reliable styling, such as themed buttons; other standard controls retain native Windows focus behavior until custom 2 px focus-ring support is added.
- Disabled states must remain understandable and explainable while clearly inactive.
- Windows high-contrast preferences must preserve identity, selection, focus, validation, and totals.
- Theme support must not disable or bypass Windows high-contrast compatibility.

## Text and Readability

- Use plain business language.
- Avoid dense paragraphs in operational screens.
- Keep labels short but specific.
- Do not use all caps for long labels or messages.
- Support readable scaling without clipping important content.
- Support system text and display scaling without hiding the dominant action, current period, validation summary, or financial totals.
- At narrow supported layouts, secondary actions may group, but labels and keyboard access remain available.

## Data Tables

Accessible grids must support:

- Keyboard row navigation.
- Clear selected row state.
- Announced column headers.
- Sort state visibility.
- Active filter summary.
- Row status in text.
- Totals outside the scrolling row area when possible.
- Group headers announce label, row count, expanded/collapsed state, and subtotal when present.
- Frozen and hidden columns remain understandable through the `Columnas` summary.
- Bulk selection announces selected, eligible, and excluded counts.

Financial values must be unambiguous for screen reader users: label, currency, amount, and sign where relevant.

## Forms

Accessible form standards:

- Required fields are indicated textually.
- Errors are connected to fields.
- Error summary appears after failed save when multiple errors exist.
- Users can move to the first error.
- Date and period fields have clear expected format.
- Read-only calculated fields are identified as calculated.
- Sensitive bank values are not announced in full when the active role only requires masked display.

## Cognitive Load

Reduce avoidable mental effort:

- One main task per screen.
- Stable navigation placement.
- Consistent action names.
- Visible current period and filters.
- Clear draft/finalized/generated states.
- Confirmation before irreversible operations.
- Recovery instructions for errors.

## Motion and Timing

- Avoid unnecessary animation.
- Do not rely on timed messages for critical information.
- Session expiration warnings must give users enough time to respond.
- Long operations must communicate progress or working state.
- Async results announce `En proceso`, `Completado`, `Completado parcialmente`, `No completado`, or `Resultado desconocido` without forcing focus away from the user's current context.
- Reduced-motion preferences suppress nonessential transitions while preserving state feedback.

## Custom Chrome Accessibility Gate

Custom chrome is not active in this phase. Before a future adoption, the complete parity checklist in `layout-specifications.md` must pass with keyboard-only, screen-reader, high-contrast, light-theme, dark-theme, per-monitor DPI, and multiple-monitor verification. `Alt+Space`, the system menu, title-bar double click, drag, resize, minimize, maximize/restore, close, and Windows Snap/Snap Layouts when technically possible are acceptance requirements, not optional polish.

## Charts and Non-Text Content

- Every chart has a title, period, units, text summary, and equivalent tabular data.
- Trends are not conveyed by color alone; direction and magnitude are available as text.
- KPI values include label, value, period, comparison context, and data freshness.

## Accessibility by Module

| Module | Accessibility Priority |
|---|---|
| Login | Clear labels, error explanation, keyboard submit, no ambiguous lockout messaging. |
| Dashboard | KPI labels and values accessible; alerts not color-only. |
| Employees | Fast keyboard entry, field validation, readable employee grid. |
| Payroll | Period/status clarity, progress feedback, totals announced with labels. |
| Accounting | Voucher entry order, calculated IGV clarity, duplicate errors. |
| SUNAT | Exact table headers, version status, export availability. |
| Reports | Chart alternatives through tables/totals, filter summary, export labels. |
| Administration | Confirmation and audit context for risky actions. |

## Accessibility Acceptance Checklist

- All frequent workflows are keyboard-operable.
- Focus is visible and predictable.
- Fields and actions have accessible names.
- Color is never the only state indicator.
- Errors are specific and field-linked.
- Tables expose headers, row state, and totals.
- Text remains readable from 1280x720 to 1920x1080.
- Long operations and session states are announced clearly.
- Sidebar, breadcrumbs, active grouping, local selection, and export summaries are fully keyboard and screen-reader operable. Saved views and cross-page bulk actions remain `IMPLEMENTATION PENDING` and hidden.

## Required Verification Matrix

Each release candidate must verify at least one screen of every archetype included in its scope, plus all high-risk screens included in that scope.

Items not yet implemented or not included in the release scope do not block that release.

| Archetype / Screen | Keyboard Only | Screen Reader | High Contrast | 200% Scaling | Async Announcement |
|---|---:|---:|---:|---:|---:|
| Login AUT-01 | Required | Required | Required | Required | Login/error result. |
| Shell and Inicio DASH-01 | Required | Required | Required | Required | KPI/work-queue refresh. |
| Standard form EMP-02 | Required | Required | Required | Required | Save/error summary. |
| Bulk form EMP-07 | Required | Required | Required | Required | Partial result. |
| Financial grid PAY-02 | Required | Required | Required | Required | Calculation state. |
| Journal entry ACC-08 | Required | Required | Required | Required | Balance/post result. |
| SUNAT process SUN-02 | Required | Required | Required | Required | Validation/generation/export state. |
| Financial report REP-02 | Required | Required | Required | Required | Refresh/export state. |
| Custom chrome prototype | Required | Required | Required | Required on each tested monitor | Window and Snap state where technically possible. |

Acceptance requires no keyboard trap, visible focus at every step, correct accessible names, announced headers/status/totals, no clipped dominant action or error, no information conveyed only through color, and reduced-motion behavior that preserves functional feedback.
