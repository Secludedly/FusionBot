using System;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public partial class HubForm : Form
    {
        private readonly object _hubConfig;

        public HubForm(object selectedObject)
        {
            InitializeComponent();

            _hubConfig = selectedObject;
            PG_Hub.SelectedObject = _hubConfig;

            ApplyTheme();

            // Optional: Auto-save on close
            this.FormClosed += (_, _) =>
            {
                PG_Hub.Refresh(); // Apply changes
                Main.Instance?.SaveCurrentConfig();
            };
        }

        /// <summary>
        /// Recolors the property grid and form to the currently selected theme.
        /// The category splitter and selection accents are intentionally left as
        /// fixed pops so settings groups stay easy to scan across every theme.
        /// </summary>
        public void ApplyTheme()
        {
            var colors = ThemeManager.CurrentColors;

            BackColor = colors.PanelBase;

            PG_Hub.BackColor = colors.PanelBase;
            PG_Hub.ViewBackColor = colors.DeepBackground;
            PG_Hub.CommandsBackColor = colors.DeepBackground;
            PG_Hub.HelpBackColor = colors.PanelBase;
            PG_Hub.LineColor = colors.Border;

            PG_Hub.CategoryForeColor = colors.ForeColor;
            PG_Hub.ViewForeColor = colors.ForeColor;
            PG_Hub.HelpForeColor = colors.ForeColor;
            PG_Hub.DisabledItemForeColor = colors.SecondaryForeColor;
        }
    }
}

