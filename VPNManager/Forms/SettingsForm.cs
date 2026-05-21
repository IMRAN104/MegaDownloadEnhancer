using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using VPNManager.Models;
using VPNManager.Services;

namespace VPNManager.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly VpnService _vpnService;

        // ── Layout ───────────────────────────────────────────────
        private Panel _sidebar = null!;
        private Panel _contentArea = null!;
        private Panel _activeSection = null!;
        private string _currentSection = "VPN";

        // ── VPN controls ─────────────────────────────────────────
        private ComboBox _cmbVpnName = null!;
        private Button _btnRefreshVpns = null!;
        private NumericUpDown _numCycleDuration = null!;
        private NumericUpDown _numMaxRetries = null!;
        private CheckBox _chkUseSavedCreds = null!;
        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;

        // ── General controls ─────────────────────────────────────
        private CheckBox _chkMinimize = null!;
        private CheckBox _chkStartMinimized = null!;
        private CheckBox _chkAutoStart = null!;
        private NumericUpDown _numRefresh = null!;
        private CheckBox _chkMonitoring = null!;
        private TextBox _txtProcessName = null!;
        private TextBox _txtProcessDisplay = null!;

        // ── Appearance ───────────────────────────────────────────
        private ComboBox _cmbTheme = null!;

        // ── Palette ──────────────────────────────────────────────
        static readonly Color C_BG      = Color.FromArgb(7,   11,  18);
        static readonly Color C_SURFACE = Color.FromArgb(13,  20,  32);
        static readonly Color C_SURF2   = Color.FromArgb(18,  27,  44);
        static readonly Color C_BORDER  = Color.FromArgb(28,  40,  68);
        static readonly Color C_ACCENT  = Color.FromArgb(0,  207, 168);
        static readonly Color C_TEXT1   = Color.FromArgb(226, 235, 248);
        static readonly Color C_TEXT2   = Color.FromArgb(80,  105, 145);
        static readonly Color C_INPUT   = Color.FromArgb(16,  24,  38);

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            _vpnService = new VpnService(settings);
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Icon = AppIcon.Create(16);
            Text = "Settings — Mega Download Enhancer";
            ClientSize = new Size(560, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = C_BG;
            ForeColor = C_TEXT1;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;

            // ── Header ──────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = C_SURFACE };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using var titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);
                using var b = new SolidBrush(C_TEXT1);
                g.DrawString("Settings", titleFont, b, 20, 14);
                using var sep = new Pen(C_BORDER, 1f);
                g.DrawLine(sep, 0, 51, header.Width, 51);
                using var accentBar = new LinearGradientBrush(
                    new Point(0, 0), new Point(header.Width, 0),
                    C_ACCENT, Color.FromArgb(0, C_ACCENT));
                g.FillRectangle(accentBar, 0, 0, header.Width, 2);
            };

            // ── Sidebar ─────────────────────────────────────────
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 130,
                BackColor = C_SURFACE,
                Padding = new Padding(0, 8, 0, 0)
            };
            _sidebar.Paint += (s, e) =>
            {
                using var sep = new Pen(C_BORDER, 1f);
                e.Graphics.DrawLine(sep, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);
            };

            // Sidebar nav buttons
            var sections = new[] { "VPN", "General", "Appearance" };
            int navY = 12;
            foreach (var sec in sections)
            {
                var s = sec;
                var navBtn = new Label
                {
                    Text = s,
                    Location = new Point(0, navY),
                    Size = new Size(130, 36),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = C_TEXT2,
                    BackColor = Color.Transparent,
                    Tag = s
                };
                navBtn.Click += (_, _) => SwitchSection(s);
                navBtn.MouseEnter += (_, _) => { if ((string)navBtn.Tag! != _currentSection) navBtn.ForeColor = C_TEXT1; };
                navBtn.MouseLeave += (_, _) => { if ((string)navBtn.Tag! != _currentSection) navBtn.ForeColor = C_TEXT2; };
                navBtn.Paint += (_, pe) =>
                {
                    var active = (string)navBtn.Tag! == _currentSection;
                    if (active)
                    {
                        pe.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(20, 0, 207, 168)),
                            0, 0, navBtn.Width, navBtn.Height);
                        pe.Graphics.FillRectangle(new SolidBrush(C_ACCENT),
                            0, 8, 3, navBtn.Height - 16);
                        navBtn.ForeColor = C_ACCENT;
                    }
                    pe.Graphics.DrawString(navBtn.Text, navBtn.Font,
                        new SolidBrush(navBtn.ForeColor), new RectangleF(20, 0, navBtn.Width - 20, navBtn.Height),
                        new StringFormat { LineAlignment = StringAlignment.Center });
                };
                _sidebar.Controls.Add(navBtn);
                navY += 38;
            }

            // ── Content area ────────────────────────────────────
            _contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(24, 16, 24, 16)
            };

            // ── Footer buttons ───────────────────────────────────
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = C_SURFACE
            };
            footer.Paint += (s, e) =>
            {
                using var sep = new Pen(C_BORDER, 1f);
                e.Graphics.DrawLine(sep, 0, 0, footer.Width, 0);
            };

            var btnOk = MakeBtn("Save", C_ACCENT, Color.FromArgb(7, 11, 18));
            var btnCancel = MakeBtn("Cancel", C_SURF2, C_TEXT1);
            btnOk.Size = new Size(88, 34);
            btnCancel.Size = new Size(88, 34);
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Location = new Point(560 - 24 - 88, 12);
            btnCancel.Location = new Point(560 - 24 - 88 - 96, 12);

            btnOk.Click += (s, e) => { SaveSettings(); DialogResult = DialogResult.OK; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            footer.Controls.Add(btnOk);
            footer.Controls.Add(btnCancel);

            // ── Build section panels ─────────────────────────────
            _activeSection = BuildVpnSection();
            _contentArea.Controls.Add(_activeSection);

            Controls.Add(_contentArea);
            Controls.Add(_sidebar);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private void SwitchSection(string section)
        {
            _currentSection = section;
            _contentArea.Controls.Clear();

            _activeSection = section switch
            {
                "VPN"        => BuildVpnSection(),
                "General"    => BuildGeneralSection(),
                "Appearance" => BuildAppearanceSection(),
                _            => BuildVpnSection()
            };

            _contentArea.Controls.Add(_activeSection);
            _sidebar.Invalidate(true);
            LoadSettings();
        }

        // ── Section builders ─────────────────────────────────────

        private Panel BuildVpnSection()
        {
            var p = SectionPanel();
            int y = 0;

            SectionTitle(p, "VPN Connection", ref y);

            var row1 = FieldRow(p, "Connection", ref y);
            _cmbVpnName = new ComboBox
            {
                Location = new Point(0, 0),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor = C_INPUT,
                ForeColor = C_TEXT1,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            StyleCombo(_cmbVpnName);
            _btnRefreshVpns = MakeBtn("↻", C_SURF2, C_TEXT1);
            _btnRefreshVpns.Size = new Size(32, 26);
            _btnRefreshVpns.Location = new Point(226, 0);
            _btnRefreshVpns.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnRefreshVpns.Click += (s, e) => { var sel = _cmbVpnName.Text; RefreshVpnList(); _cmbVpnName.Text = sel; };
            row1.Controls.Add(_cmbVpnName);
            row1.Controls.Add(_btnRefreshVpns);

            SectionDivider(p, ref y);
            SectionTitle(p, "Cycle Settings", ref y);

            var row2 = FieldRow(p, "Cycle duration (min)", ref y);
            _numCycleDuration = MakeNumeric(1, 1440, 10, row2);

            var row3 = FieldRow(p, "Max retries", ref y);
            _numMaxRetries = MakeNumeric(1, 10, 3, row3);

            SectionDivider(p, ref y);
            SectionTitle(p, "Credentials", ref y);

            _chkUseSavedCreds = DarkCheck(p, "Use saved Windows credentials", ref y);
            _chkUseSavedCreds.CheckedChanged += (s, e) =>
            {
                bool saved = _chkUseSavedCreds.Checked;
                _txtUsername.Enabled = !saved;
                _txtPassword.Enabled = !saved;
            };

            var row4 = FieldRow(p, "Username", ref y);
            _txtUsername = DarkTextBox(row4, "user@domain.com");

            var row5 = FieldRow(p, "Password", ref y);
            _txtPassword = DarkTextBox(row5, "leave empty for saved creds");
            _txtPassword.PasswordChar = '●';

            var hint = new Label
            {
                Text = "Tip: Save credentials in Windows VPN settings — more secure than storing here.",
                Location = new Point(0, y + 4),
                Size = new Size(380, 32),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = C_TEXT2,
                BackColor = Color.Transparent
            };
            p.Controls.Add(hint);

            return p;
        }

        private Panel BuildGeneralSection()
        {
            var p = SectionPanel();
            int y = 0;

            SectionTitle(p, "Window Behavior", ref y);
            _chkMinimize      = DarkCheck(p, "Minimize to system tray on close", ref y);
            _chkStartMinimized = DarkCheck(p, "Start minimized",                  ref y);
            _chkAutoStart     = DarkCheck(p, "Auto-start with Windows",            ref y);

            SectionDivider(p, ref y);
            SectionTitle(p, "Status Polling", ref y);

            var rowRefresh = FieldRow(p, "Refresh interval (sec)", ref y);
            _numRefresh = MakeNumeric(1, 60, 1, rowRefresh);

            SectionDivider(p, ref y);
            SectionTitle(p, "Process Monitoring", ref y);
            _chkMonitoring = DarkCheck(p, "Enable process monitoring", ref y);

            var rowProc = FieldRow(p, "Process name", ref y);
            _txtProcessName = DarkTextBox(rowProc, "MEGAsync");

            var rowDisp = FieldRow(p, "Display name", ref y);
            _txtProcessDisplay = DarkTextBox(rowDisp, "MEGAsync");

            return p;
        }

        private Panel BuildAppearanceSection()
        {
            var p = SectionPanel();
            int y = 0;

            SectionTitle(p, "Theme", ref y);

            var row = FieldRow(p, "Color scheme", ref y);
            _cmbTheme = new ComboBox
            {
                Location = new Point(0, 0),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = C_INPUT,
                ForeColor = C_TEXT1,
                FlatStyle = FlatStyle.Flat
            };
            _cmbTheme.Items.AddRange(new[] { "System (Windows default)", "Light", "Dark" });
            StyleCombo(_cmbTheme);
            row.Controls.Add(_cmbTheme);

            var note = new Label
            {
                Text = "Theme changes apply after restarting the app.",
                Location = new Point(0, y + 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = C_TEXT2,
                BackColor = Color.Transparent
            };
            p.Controls.Add(note);

            return p;
        }

        // ── Load / Save ──────────────────────────────────────────

        private void LoadSettings()
        {
            if (_cmbVpnName != null)
            {
                RefreshVpnList();
                _cmbVpnName.Text = _settings.VpnName;
            }
            if (_numCycleDuration != null) _numCycleDuration.Value = _settings.CycleDurationMinutes;
            if (_numMaxRetries != null)    _numMaxRetries.Value    = _settings.MaxRetries;
            if (_chkUseSavedCreds != null)
            {
                _chkUseSavedCreds.Checked = string.IsNullOrEmpty(_settings.Username);
                _txtUsername.Text = _settings.Username;
                _txtPassword.Text = _settings.Password;
                _txtUsername.Enabled = !_chkUseSavedCreds.Checked;
                _txtPassword.Enabled = !_chkUseSavedCreds.Checked;
            }
            if (_chkMinimize != null)       _chkMinimize.Checked       = _settings.MinimizeToTray;
            if (_chkStartMinimized != null) _chkStartMinimized.Checked = _settings.StartMinimized;
            if (_chkAutoStart != null)      _chkAutoStart.Checked      = _settings.AutoStart;
            if (_numRefresh != null)        _numRefresh.Value           = _settings.StatusRefreshIntervalSeconds;
            if (_chkMonitoring != null)     _chkMonitoring.Checked     = _settings.ProcessMonitoringEnabled;
            if (_txtProcessName != null)    _txtProcessName.Text        = _settings.MonitoredProcessName;
            if (_txtProcessDisplay != null) _txtProcessDisplay.Text     = _settings.MonitoredProcessDisplayName;
            if (_cmbTheme != null)          _cmbTheme.SelectedIndex     = (int)_settings.ThemeMode;
        }

        private void SaveSettings()
        {
            if (_cmbVpnName != null)       _settings.VpnName = _cmbVpnName.Text;
            if (_numCycleDuration != null) _settings.CycleDurationMinutes = (int)_numCycleDuration.Value;
            if (_numMaxRetries != null)    _settings.MaxRetries = (int)_numMaxRetries.Value;
            if (_chkUseSavedCreds != null)
            {
                _settings.Username = _chkUseSavedCreds.Checked ? "" : _txtUsername.Text;
                _settings.Password = _chkUseSavedCreds.Checked ? "" : _txtPassword.Text;
            }
            if (_chkMinimize != null)       _settings.MinimizeToTray    = _chkMinimize.Checked;
            if (_chkStartMinimized != null) _settings.StartMinimized    = _chkStartMinimized.Checked;
            if (_chkAutoStart != null)      _settings.AutoStart         = _chkAutoStart.Checked;
            if (_numRefresh != null)        _settings.StatusRefreshIntervalSeconds = (int)_numRefresh.Value;
            if (_chkMonitoring != null)     _settings.ProcessMonitoringEnabled     = _chkMonitoring.Checked;
            if (_txtProcessName != null)    _settings.MonitoredProcessName         = _txtProcessName.Text.Trim();
            if (_txtProcessDisplay != null) _settings.MonitoredProcessDisplayName  = _txtProcessDisplay.Text.Trim();
            if (_cmbTheme != null)          _settings.ThemeMode = (ThemeMode)_cmbTheme.SelectedIndex;
        }

        private void RefreshVpnList()
        {
            if (_cmbVpnName == null) return;
            try
            {
                _cmbVpnName.Items.Clear();
                _cmbVpnName.Items.Add("CloudflareWARP");
                foreach (var vpn in _vpnService.GetAvailableVpns())
                    if (!_cmbVpnName.Items.Contains(vpn)) _cmbVpnName.Items.Add(vpn);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load VPN list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── UI Helpers ───────────────────────────────────────────

        static Panel SectionPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(4) };
            return p;
        }

        static void SectionTitle(Panel parent, string title, ref int y)
        {
            var lbl = new Label
            {
                Text = title.ToUpper(),
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 207, 168),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lbl);
            y += 24;
        }

        static void SectionDivider(Panel parent, ref int y)
        {
            var div = new Panel
            {
                Location = new Point(0, y + 4),
                Size = new Size(400, 1),
                BackColor = Color.FromArgb(28, 40, 68)
            };
            parent.Controls.Add(div);
            y += 18;
        }

        static Panel FieldRow(Panel parent, string labelText, ref int y)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(0, y + 4),
                Size = new Size(160, 22),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(160, 180, 210),
                BackColor = Color.Transparent
            };
            var row = new Panel
            {
                Location = new Point(162, y),
                Size = new Size(300, 28),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(row);
            y += 36;
            return row;
        }

        static CheckBox DarkCheck(Panel parent, string text, ref int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(160, 180, 210),
                BackColor = Color.Transparent
            };
            parent.Controls.Add(chk);
            y += 28;
            return chk;
        }

        static TextBox DarkTextBox(Panel parent, string placeholder)
        {
            var tb = new TextBox
            {
                Location = new Point(0, 0),
                Width = 230,
                BackColor = Color.FromArgb(16, 24, 38),
                ForeColor = Color.FromArgb(226, 235, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
                PlaceholderText = placeholder
            };
            parent.Controls.Add(tb);
            return tb;
        }

        static NumericUpDown MakeNumeric(int min, int max, int val, Panel parent)
        {
            var n = new NumericUpDown
            {
                Location = new Point(0, 0),
                Size = new Size(80, 26),
                Minimum = min,
                Maximum = max,
                Value = val,
                BackColor = Color.FromArgb(16, 24, 38),
                ForeColor = Color.FromArgb(226, 235, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };
            parent.Controls.Add(n);
            return n;
        }

        static void StyleCombo(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = Color.FromArgb(16, 24, 38);
            cb.ForeColor = Color.FromArgb(226, 235, 248);
        }

        static Button MakeBtn(string text, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(bg.A, Math.Min(255, bg.R + 20), Math.Min(255, bg.G + 20), Math.Min(255, bg.B + 20));
            return b;
        }
    }
}
