using System.Drawing;

namespace ERP.WinForms.Theming;

public sealed class ThemeManager
{
    private AppThemeMode _currentMode = AppThemeMode.Light;

    public AppThemeMode CurrentMode => _currentMode;

    public ThemePalette CurrentPalette => ResolvePalette(_currentMode);

    public void SetTheme(AppThemeMode mode)
    {
        _currentMode = mode;
    }

    public AppThemeMode ToggleLightDark()
    {
        _currentMode = _currentMode == AppThemeMode.Dark
            ? AppThemeMode.Light
            : AppThemeMode.Dark;

        return _currentMode;
    }

    private static ThemePalette ResolvePalette(AppThemeMode mode)
    {
        // System mode resolves to Light until OS appearance sync is wired in a settings layer.
        return mode == AppThemeMode.Dark ? DarkPalette : LightPalette;
    }

    public static ThemePalette LightPalette { get; } = new(
        AppBackground: FromHex("#F4F6F8"),
        Surface: FromHex("#FFFFFF"),
        SurfaceSubtle: FromHex("#F4F6F8"),
        SurfaceElevated: FromHex("#FFFFFF"),
        Border: FromHex("#B7BDC7"),
        BorderStrong: FromHex("#4B5563"),
        TextPrimary: FromHex("#1B1D21"),
        TextSecondary: FromHex("#4B5563"),
        TextMuted: FromHex("#4B5563"),
        Primary: FromHex("#1F5C99"),
        PrimaryHover: FromHex("#0A66C2"),
        Focus: FromHex("#0A66C2"),
        Success: FromHex("#1F6B3A"),
        Warning: FromHex("#7A4D00"),
        Danger: FromHex("#B42318"),
        Information: FromHex("#2457A7"),
        SelectionBackground: FromHex("#E6F0FA"),
        GridRowHover: FromHex("#F4F6F8"),
        GridRowSelected: FromHex("#E6F0FA"),
        InputBackground: FromHex("#FFFFFF"),
        InputBorder: FromHex("#B7BDC7"),
        InputDisabledBackground: FromHex("#F4F6F8"),
        InputDisabledText: FromHex("#4B5563"));

    public static ThemePalette DarkPalette { get; } = new(
        AppBackground: FromHex("#0F1115"),
        Surface: FromHex("#151922"),
        SurfaceSubtle: FromHex("#1E2430"),
        SurfaceElevated: FromHex("#242B38"),
        Border: FromHex("#3A4352"),
        BorderStrong: FromHex("#596273"),
        TextPrimary: FromHex("#F3F6FA"),
        TextSecondary: FromHex("#C5CBD5"),
        TextMuted: FromHex("#8E97A6"),
        Primary: FromHex("#8AB4F8"),
        PrimaryHover: FromHex("#A7C7FF"),
        Focus: FromHex("#7CB7FF"),
        Success: FromHex("#7DDC8A"),
        Warning: FromHex("#F2B84B"),
        Danger: FromHex("#FF8A80"),
        Information: FromHex("#8AB4F8"),
        SelectionBackground: FromHex("#203A5F"),
        GridRowHover: FromHex("#202633"),
        GridRowSelected: FromHex("#203A5F"),
        InputBackground: FromHex("#10141C"),
        InputBorder: FromHex("#4A5364"),
        InputDisabledBackground: FromHex("#1A1F2A"),
        InputDisabledText: FromHex("#8E97A6"));

    private static Color FromHex(string hex)
    {
        return ColorTranslator.FromHtml(hex);
    }
}
