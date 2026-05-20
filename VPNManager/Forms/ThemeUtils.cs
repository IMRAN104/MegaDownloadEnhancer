using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace VPNManager.Forms
{
    public enum ThemeMode
    {
        System,
        Light,
        Dark
    }

    public static class ThemeUtils
    {
        public static bool IsSystemDarkMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int intValue)
                    return intValue == 0;
            }
            catch
            {
            }
            return false;
        }

        public static void ApplyTheme(Form form, ThemeMode mode)
        {
            var isDark = mode == ThemeMode.Dark;

            form.BackColor = isDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(240, 240, 240);
            form.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;

            foreach (Control c in form.Controls)
            {
                ApplyThemeToControl(c, mode);
            }
        }

        public static void ApplyThemeToControl(Control control, ThemeMode mode)
        {
            if (control is Form) return;
            var isDark = mode == ThemeMode.Dark;
            ApplyThemeByType(control, isDark);

            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, mode);
            }
        }

        private static void ApplyThemeByType(Control control, bool isDark)
        {
            if (control is GroupBox gb)
            {
                gb.BackColor = isDark ? Color.FromArgb(45, 45, 45) : Color.White;
                gb.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
            }
            else if (control is Label l)
            {
                if (l.Font?.Bold == true && l.Font.Size >= 14)
                    l.ForeColor = isDark ? Color.FromArgb(100, 180, 255) : Color.FromArgb(0, 120, 215);
                else
                    l.ForeColor = isDark ? Color.FromArgb(200, 200, 200) : SystemColors.ControlText;
                l.BackColor = Color.Transparent;
            }
            else if (control is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
            }
            else if (control is TextBox tb)
            {
                tb.BackColor = isDark ? Color.FromArgb(55, 55, 55) : Color.White;
                tb.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
            }
            else if (control is ComboBox cb)
            {
                cb.BackColor = isDark ? Color.FromArgb(55, 55, 55) : Color.White;
                cb.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
            }
            else if (control is NumericUpDown nud)
            {
                nud.BackColor = isDark ? Color.FromArgb(55, 55, 55) : Color.White;
                nud.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
            }
            else if (control is CheckBox chk)
            {
                chk.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
                chk.BackColor = Color.Transparent;
            }
            else if (control is TabPage tp)
            {
                tp.BackColor = isDark ? Color.FromArgb(45, 45, 45) : Color.White;
                tp.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.Black;
            }
            else if (control is TabControl tc)
            {
                tc.BackColor = isDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(240, 240, 240);
            }
            else if (control is Panel p)
            {
                p.BackColor = control.BackColor;
            }
            else if (control is StatusStrip ss)
            {
                ss.BackColor = isDark ? Color.FromArgb(45, 45, 45) : SystemColors.Control;
                ss.ForeColor = isDark ? Color.FromArgb(200, 200, 200) : Color.Black;
            }
        }
    }
}
