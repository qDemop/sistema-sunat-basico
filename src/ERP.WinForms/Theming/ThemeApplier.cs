using System.Drawing;
using System.Runtime.CompilerServices;

namespace ERP.WinForms.Theming;

public static class ThemeApplier
{
    private static readonly ConditionalWeakTable<Button, ButtonThemeState> ButtonThemeStates = new();

    public static void Apply(Control root, ThemePalette palette)
    {
        ApplyControl(root, palette);

        foreach (Control child in root.Controls)
        {
            Apply(child, palette);
        }
    }

    private static void ApplyControl(Control control, ThemePalette palette)
    {
        switch (control)
        {
            case Form form:
                form.BackColor = palette.AppBackground;
                form.ForeColor = palette.TextPrimary;
                break;

            case Panel panel:
                panel.BackColor = palette.Surface;
                panel.ForeColor = palette.TextPrimary;
                break;

            case Label label:
                label.BackColor = Color.Transparent;
                label.ForeColor = label.Enabled ? palette.TextPrimary : palette.TextMuted;
                break;

            case Button button:
                ApplyButton(button, palette);
                break;

            case TextBox textBox:
                ApplyTextBox(textBox, palette);
                break;

            case ComboBox comboBox:
                ApplyComboBox(comboBox, palette);
                break;

            case DataGridView grid:
                ApplyDataGridView(grid, palette);
                break;

            case MenuStrip menuStrip:
                ApplyToolStrip(menuStrip, palette);
                break;

            case ToolStrip toolStrip:
                ApplyToolStrip(toolStrip, palette);
                break;

            default:
                control.BackColor = palette.Surface;
                control.ForeColor = control.Enabled ? palette.TextPrimary : palette.TextMuted;
                break;
        }
    }

    private static void ApplyButton(Button button, ThemePalette palette)
    {
        var state = ButtonThemeStates.GetValue(button, RegisterButtonFocusHandlers);
        state.Palette = palette;

        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = palette.Primary;
        button.ForeColor = palette.Surface;
        button.FlatAppearance.BorderColor = button.Focused ? palette.Focus : palette.Primary;
        button.FlatAppearance.BorderSize = button.Focused ? 2 : 1;
        button.FlatAppearance.MouseOverBackColor = palette.PrimaryHover;
        button.FlatAppearance.MouseDownBackColor = palette.PrimaryHover;
        button.UseVisualStyleBackColor = false;

        if (!button.Enabled)
        {
            button.BackColor = palette.InputDisabledBackground;
            button.ForeColor = palette.InputDisabledText;
            button.FlatAppearance.BorderColor = palette.Border;
            button.FlatAppearance.BorderSize = 1;
        }
    }

    private static ButtonThemeState RegisterButtonFocusHandlers(Button button)
    {
        var state = new ButtonThemeState();
        button.GotFocus += (_, _) => ApplyButtonFocusState(button, state.Palette, hasFocus: true);
        button.LostFocus += (_, _) => ApplyButtonFocusState(button, state.Palette, hasFocus: false);
        return state;
    }

    private static void ApplyButtonFocusState(Button button, ThemePalette? palette, bool hasFocus)
    {
        if (palette is null || !button.Enabled)
        {
            return;
        }

        button.FlatAppearance.BorderColor = hasFocus ? palette.Focus : palette.Primary;
        button.FlatAppearance.BorderSize = hasFocus ? 2 : 1;
        button.Invalidate();
    }

    private static void ApplyTextBox(TextBox textBox, ThemePalette palette)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = textBox.Enabled ? palette.InputBackground : palette.InputDisabledBackground;
        textBox.ForeColor = textBox.Enabled ? palette.TextPrimary : palette.InputDisabledText;
    }

    private static void ApplyComboBox(ComboBox comboBox, ThemePalette palette)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = comboBox.Enabled ? palette.InputBackground : palette.InputDisabledBackground;
        comboBox.ForeColor = comboBox.Enabled ? palette.TextPrimary : palette.InputDisabledText;
    }

    private static void ApplyDataGridView(DataGridView grid, ThemePalette palette)
    {
        grid.BackgroundColor = palette.Surface;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.GridColor = palette.Border;
        grid.EnableHeadersVisualStyles = false;
        grid.DefaultCellStyle.BackColor = palette.Surface;
        grid.DefaultCellStyle.ForeColor = palette.TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = palette.GridRowSelected;
        grid.DefaultCellStyle.SelectionForeColor = palette.TextPrimary;
        grid.AlternatingRowsDefaultCellStyle.BackColor = palette.SurfaceSubtle;
        grid.ColumnHeadersDefaultCellStyle.BackColor = palette.SurfaceSubtle;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = palette.TextPrimary;
        grid.RowHeadersDefaultCellStyle.BackColor = palette.SurfaceSubtle;
        grid.RowHeadersDefaultCellStyle.ForeColor = palette.TextSecondary;
    }

    private static void ApplyToolStrip(ToolStrip toolStrip, ThemePalette palette)
    {
        toolStrip.BackColor = palette.SurfaceSubtle;
        toolStrip.ForeColor = palette.TextPrimary;
        toolStrip.RenderMode = ToolStripRenderMode.System;

        foreach (ToolStripItem item in toolStrip.Items)
        {
            item.BackColor = palette.SurfaceSubtle;
            item.ForeColor = item.Enabled ? palette.TextPrimary : palette.TextMuted;
        }
    }

    private sealed class ButtonThemeState
    {
        public ThemePalette? Palette { get; set; }
    }
}
