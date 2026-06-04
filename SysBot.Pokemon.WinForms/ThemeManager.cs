using SysBot.Pokemon.WinForms;
using System.Collections.Generic;
using System.Drawing;

public static class ThemeManager
{
    public static Dictionary<string, ThemeColors> ThemePresets { get; } = new()
    {
        ["Classic"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(31, 30, 68),
            Shadow = Color.FromArgb(20, 19, 57),
            Hover = Color.FromArgb(31, 30, 68),
            ForeColor = Color.White

        },

        ["Diet Classic"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(41, 40, 78),
            Shadow = Color.FromArgb(30, 29, 67),
            Hover = Color.FromArgb(41, 40, 78),
            ForeColor = Color.White

        },

        ["In the Shadows"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(20, 19, 57),
            Shadow = Color.FromArgb(15, 15, 45),
            Hover = Color.FromArgb(20, 19, 57),
            ForeColor = Color.White

        },

        ["Darkest Nights"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(15, 15, 45),
            Shadow = Color.FromArgb(11, 11, 41),
            Hover = Color.FromArgb(15, 15, 45),
            ForeColor = Color.White

        },

        ["Singularity I"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(31, 30, 68),
            Shadow = Color.FromArgb(31, 30, 68),
            Hover = Color.FromArgb(31, 30, 68),
            ForeColor = Color.White

        },

        ["Singularity II"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(20, 19, 57),
            Shadow = Color.FromArgb(20, 19, 57),
            Hover = Color.FromArgb(20, 19, 57),
            ForeColor = Color.White

        },

        ["Singularity III"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(15, 15, 45),
            Shadow = Color.FromArgb(15, 15, 45),
            Hover = Color.FromArgb(15, 15, 45),
            ForeColor = Color.White

        },

        ["Deep Purple Vibe"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(25, 0, 77),
            Shadow = Color.FromArgb(9, 0, 51),
            Hover = Color.FromArgb(25, 0, 77),
            ForeColor = Color.White

        },

        ["Indigo Shroud"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(31, 30, 68),
            Shadow = Color.FromArgb(42, 0, 128),
            Hover = Color.FromArgb(31, 30, 68),
            ForeColor = Color.White

        },

        ["Night Wave"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(28, 24, 70),
            Shadow = Color.FromArgb(15, 12, 55),
            Hover = Color.FromArgb(28, 24, 70),
            ForeColor = Color.White

        },

        ["Violet Mist"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(35, 28, 85),
            Shadow = Color.FromArgb(20, 15, 60),
            Hover = Color.FromArgb(35, 28, 85),
            ForeColor = Color.White

        },

        ["Midnight Shore"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(25, 30, 70),
            Shadow = Color.FromArgb(15, 15, 50),
            Hover = Color.FromArgb(25, 30, 70),
            ForeColor = Color.White

        },

        ["Cosmic Purple"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(45, 20, 90),
            Shadow = Color.FromArgb(20, 10, 50),
            Hover = Color.FromArgb(45, 20, 90),
            ForeColor = Color.White

        },

        ["Lavender Dream"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(180, 160, 220),
            Shadow = Color.FromArgb(110, 90, 170),
            Hover = Color.FromArgb(180, 160, 220),
            ForeColor = Color.White
        },

        ["Stellar Night"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(30, 25, 75),
            Shadow = Color.FromArgb(12, 10, 45),
            Hover = Color.FromArgb(30, 25, 75),
            ForeColor = Color.White

        },

        ["Nightfall"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(15, 10, 50),
            Shadow = Color.FromArgb(5, 5, 25),
            Hover = Color.FromArgb(15, 10, 50),
            ForeColor = Color.White

        },

        ["Blackout Borders"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(31, 30, 68),
            Shadow = Color.FromArgb(0, 0, 0),
            Hover = Color.FromArgb(31, 30, 68),
            ForeColor = Color.White

        },

        ["Blackout"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(0, 0, 0),
            Shadow = Color.FromArgb(7, 7, 7),
            Hover = Color.FromArgb(0, 0, 0),
            ForeColor = Color.White

        },

        ["Dark Fade Out"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(32, 32, 32),
            Shadow = Color.FromArgb(96, 96, 96),
            Hover = Color.FromArgb(0, 0, 0),
            ForeColor = Color.White

        },

        ["Silver & Cold"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(64, 64, 64),
            Shadow = Color.FromArgb(0, 153, 153),
            Hover = Color.FromArgb(32, 32, 32),
            ForeColor = Color.White

        },

        ["Frostbite"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(120, 180, 220),
            Shadow = Color.FromArgb(180, 220, 255),
            Hover = Color.FromArgb(120, 180, 220),
            ForeColor = Color.White
        },

        ["Midnight Frost"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(20, 25, 55),
            Shadow = Color.FromArgb(10, 10, 30),
            Hover = Color.FromArgb(20, 25, 55),
            ForeColor = Color.White
        },

        ["Winter Void"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(12, 18, 45),
            Shadow = Color.FromArgb(90, 110, 150),
            Hover = Color.FromArgb(12, 18, 45),
            ForeColor = Color.White
        },

        ["Stormbreaker"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(8, 10, 30),
            Shadow = Color.FromArgb(40, 0, 60),
            Hover = Color.FromArgb(8, 10, 30),
            ForeColor = Color.White
        },

        ["Lunar Abyss"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(10, 10, 28),
            Shadow = Color.FromArgb(5, 5, 18),
            Hover = Color.FromArgb(10, 10, 28),
            ForeColor = Color.White
        },

        ["Neon Abyss"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(0, 0, 40),
            Shadow = Color.FromArgb(0, 0, 20),
            Hover = Color.FromArgb(0, 0, 40),
            ForeColor = Color.White
        },

        ["Blue Eclipse"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(18, 22, 60),
            Shadow = Color.FromArgb(0, 20, 70),
            Hover = Color.FromArgb(18, 22, 60),
            ForeColor = Color.White
        },

        ["Galaxy Violet"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(50, 0, 80),
            Shadow = Color.FromArgb(20, 0, 40),
            Hover = Color.FromArgb(50, 0, 80),
            ForeColor = Color.White
        },

        ["Ghostline Noir"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(5, 5, 5),
            Shadow = Color.FromArgb(30, 35, 50),
            Hover = Color.FromArgb(5, 5, 5),
            ForeColor = Color.White
        },

        ["WTF? :O"] = new ThemeColors
        {
            PanelBase = Color.FromArgb(69, 99, 14),
            Shadow = Color.FromArgb(150, 4, 100),
            Hover = Color.FromArgb(0, 150, 120),
            ForeColor = Color.White

        },
    };

    public static string CurrentThemeName { get; private set; } = "Classic";

    // 🆕 Easy access to current theme colors
    public static ThemeColors CurrentColors => ThemePresets[CurrentThemeName];

    public static void ApplyTheme(Main form, string themeName)
    {
        if (!ThemePresets.TryGetValue(themeName, out var colors))
            return;

        CurrentThemeName = themeName;

        // PANEL: Main stays untouched
        form.panelMain.BackColor = Color.FromArgb(10, 10, 40);

        // PANEL: Primary base
        form.panelLeftSide.BackColor = colors.PanelBase;
        form.panelTitleBar.BackColor = colors.PanelBase;

        // SHADOW PANELS + Borders
        form.shadowPanelTop.BackColor = colors.Shadow;
        form.shadowPanelLeft.BackColor = colors.Shadow;
        form.panel1.BackColor = colors.Shadow;
        form.panel2.BackColor = colors.Shadow;
        form.panel3.BackColor = colors.Shadow;
        form.panel4.BackColor = colors.Shadow;
        form.panel5.BackColor = colors.Shadow;
        form.panel6.BackColor = colors.Shadow;

        // BUTTONS — start with PanelBase color
        form.btnBots.BackColor = colors.PanelBase;
        form.btnHub.BackColor = colors.PanelBase;
        form.btnLogs.BackColor = colors.PanelBase;

        form.btnBots.ForeColor = colors.ForeColor;
        form.btnHub.ForeColor = colors.ForeColor;
        form.btnLogs.ForeColor = colors.ForeColor;
        form.lblTitle.ForeColor = colors.ForeColor;

        // Reapply hover animations using new theme colors
        form.SetupThemeAwareButtons();

        // Cascade the theme into the child forms (Bots/Hub/Logs) and their controls
        form.RefreshChildThemes();
    }

    public static ThemeColors? GetCurrentColors()
        => ThemePresets.TryGetValue(CurrentThemeName, out var colors) ? colors : null;

    // ──────────────────────────────────────────────────────────────────────
    //  Color helpers — used to derive the secondary surface colors so every
    //  preset themes the child forms without having to declare extra colors.
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lightens (factor &gt; 0, toward white) or darkens (factor &lt; 0, toward
    /// black) a color. <paramref name="factor"/> is a 0–1 ratio.
    /// </summary>
    public static Color Shade(Color c, float factor)
    {
        if (factor < 0)
        {
            float f = 1f + factor; // e.g. -0.30 -> scale channels by 0.70
            return Color.FromArgb(c.A, Clamp(c.R * f), Clamp(c.G * f), Clamp(c.B * f));
        }

        return Color.FromArgb(c.A,
            Clamp(c.R + (255 - c.R) * factor),
            Clamp(c.G + (255 - c.G) * factor),
            Clamp(c.B + (255 - c.B) * factor));
    }

    /// <summary>Linearly blends <paramref name="a"/> toward <paramref name="b"/> by <paramref name="t"/> (0–1).</summary>
    public static Color Blend(Color a, Color b, float t)
        => Color.FromArgb(
            Clamp(a.R + (b.R - a.R) * t),
            Clamp(a.G + (b.G - a.G) * t),
            Clamp(a.B + (b.B - a.B) * t));

    private static int Clamp(float v) => (int)(v < 0 ? 0 : v > 255 ? 255 : v);
}

// Add Hover support to ThemeColors
public class ThemeColors
{
    public Color PanelBase { get; set; }
    public Color Shadow { get; set; }
    public Color Hover { get; set; }
    public Color ForeColor { get; set; }

    // ── Derived semantic colors ───────────────────────────────────────────
    // Computed from the base palette so the child forms (Bots/Hub/Logs and the
    // bot controllers) follow each theme automatically.

    /// <summary>Deepest surface — logs view, property-grid view, text input backgrounds.</summary>
    public Color DeepBackground => ThemeManager.Shade(Shadow, -0.30f);

    /// <summary>Background for nested controls (input boxes) and the bot controller body.</summary>
    public Color ControlBackground => Shadow;

    /// <summary>
    /// Background of the bot-list panel — a touch lighter than the entries that sit
    /// on it, so the controllers read as slightly darker cards against the panel.
    /// Derived from <see cref="Shadow"/> (not PanelBase) so the contrast holds for
    /// every preset, including ones where PanelBase and Shadow are equal.
    /// </summary>
    public Color ListBackground => ThemeManager.Shade(Shadow, 0.05f);

    /// <summary>Lighter pop for hovered/selected list items.</summary>
    public Color Highlight => ThemeManager.Shade(PanelBase, 0.20f);

    /// <summary>Subtle border/divider color.</summary>
    public Color Border => ThemeManager.Shade(Shadow, -0.15f);

    /// <summary>Dimmed foreground for secondary labels.</summary>
    public Color SecondaryForeColor => ThemeManager.Blend(ForeColor, PanelBase, 0.45f);
}
