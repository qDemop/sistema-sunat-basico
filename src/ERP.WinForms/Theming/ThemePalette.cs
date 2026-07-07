using System.Drawing;

namespace ERP.WinForms.Theming;

public sealed record ThemePalette(
    Color AppBackground,
    Color Surface,
    Color SurfaceSubtle,
    Color SurfaceElevated,
    Color Border,
    Color BorderStrong,
    Color TextPrimary,
    Color TextSecondary,
    Color TextMuted,
    Color Primary,
    Color PrimaryHover,
    Color Focus,
    Color Success,
    Color Warning,
    Color Danger,
    Color Information,
    Color SelectionBackground,
    Color GridRowHover,
    Color GridRowSelected,
    Color InputBackground,
    Color InputBorder,
    Color InputDisabledBackground,
    Color InputDisabledText);
