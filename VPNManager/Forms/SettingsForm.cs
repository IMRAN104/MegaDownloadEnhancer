using System;
using System.Drawing;
using System.Windows.Forms;
using VPNManager.Models;
using VPNManager.Services;

namespace VPNManager.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly VpnService _vpnService;

        private TabControl _tabControl;
        private TabPage _tabVpn;
        private TabPage _tabGeneral;
        private TabPage _tabAppearance;
        private TabPage _tabAbout;

        // VPN
        private ComboBox _cmbVpnName;
        private Label _lblVpnName;
        private Button _btnRefreshVpns;
        private TextBox _txtUsername;
        private Label _lblUsername;
        private TextBox _txtPassword;
        private Label _lblPassword;
        private NumericUpDown _numCycleDuration;
        private Label _lblCycleDuration;
        private NumericUpDown _numMaxRetries;
        private Label _lblMaxRetries;
        private CheckBox _chkUseSavedCredentials;

        // General
        private CheckBox _chkMinimizeToTray;
        private CheckBox _chkStartMinimized;
        private CheckBox _chkAutoStart;
        private CheckBox _chkProcessMonitoringEnabled;
        private NumericUpDown _numRefreshInterval;
        private Label _lblRefreshInterval;
        private TextBox _txtProcessName;
        private Label _lblProcessName;
        private TextBox _txtProcessDisplayName;
        private Label _lblProcessDisplayName;

        // Appearance
        private ComboBox _cmbTheme;
        private Label _lblTheme;

        // Buttons
        private Button _btnOk;
        private Button _btnApply;
        private Button _btnCancel;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            _vpnService = new VpnService(settings);

            InitializeComponent();
            LoadSettings();
            ThemeUtils.ApplyTheme(this, _settings.ThemeMode);
        }

        private void InitializeComponent()
        {
            Text = "Settings — Mega Download Enhancer";
            Size = new Size(580, 460);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(520, 420);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(248, 250, 252);

            _tabControl = new TabControl
            {
                Location = new Point(16, 16),
                Size = new Size(540, 340),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            _tabVpn = new TabPage("VPN");
            BuildVpnTab();
            _tabGeneral = new TabPage("General");
            BuildGeneralTab();
            _tabAppearance = new TabPage("Appearance");
            BuildAppearanceTab();
            _tabAbout = new TabPage("About");
            BuildAboutTab();

            _tabControl.TabPages.Add(_tabVpn);
            _tabControl.TabPages.Add(_tabGeneral);
            _tabControl.TabPages.Add(_tabAppearance);
            _tabControl.TabPages.Add(_tabAbout);

            _btnOk = CreateButton("OK", DialogResult.OK);
            _btnApply = CreateButton("Apply", DialogResult.None);
            _btnCancel = CreateButton("Cancel", DialogResult.Cancel);

            // Position buttons at bottom-right
            _btnCancel.Location = new Point(476, 370);
            _btnOk.Location = new Point(394, 370);
            _btnApply.Location = new Point(308, 370);
            _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            _btnOk.Click += (s, e) => { SaveSettings(); DialogResult = DialogResult.OK; Close(); };
            _btnApply.Click += (s, e) => { SaveSettings(); _settings.Save(); MessageBox.Show("Settings saved!", "Mega Download Enhancer", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(_tabControl);
            Controls.Add(_btnOk);
            Controls.Add(_btnApply);
            Controls.Add(_btnCancel);
        }

        private Button CreateButton(string text, DialogResult result)
        {
            return new Button
            {
                Text = text,
                Size = new Size(80, 32),
                DialogResult = result,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private void BuildVpnTab()
        {
            var y = 16;
            var lw = 150;
            var cw = 260;

            _lblVpnName = Label("VPN Connection:", 16, y);
            _cmbVpnName = new ComboBox
            {
                Location = new Point(lw, y - 2),
                Size = new Size(cw - 34, 26),
                DropDownStyle = ComboBoxStyle.DropDown,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            _btnRefreshVpns = new Button { Text = "↻", Location = new Point(lw + cw - 30, y - 2), Size = new Size(30, 26) };
            _btnRefreshVpns.Click += (s, e) => { var sel = _cmbVpnName.Text; RefreshVpnList(); _cmbVpnName.Text = sel; };

            y += 40;
            _chkUseSavedCredentials = new CheckBox { Text = "Use saved Windows credentials", Location = new Point(16, y), Size = new Size(460, 24), Checked = true };
            _chkUseSavedCredentials.CheckedChanged += (s, e) =>
            {
                var useSaved = _chkUseSavedCredentials.Checked;
                _txtUsername.Enabled = !useSaved;
                _txtPassword.Enabled = !useSaved;
                if (useSaved) { _txtUsername.Text = ""; _txtPassword.Text = ""; }
            };

            y += 32;
            _lblUsername = Label("Username:", 32, y);
            _txtUsername = TextBox("user@domain.com", lw, y);

            y += 34;
            _lblPassword = Label("Password:", 32, y);
            _txtPassword = TextBox("Leave empty to use saved credentials", lw, y);
            _txtPassword.PasswordChar = '●';

            y += 34;
            _lblCycleDuration = Label("Cycle Duration (min):", 16, y);
            _numCycleDuration = new NumericUpDown { Location = new Point(lw, y - 2), Size = new Size(80, 26), Minimum = 1, Maximum = 1440, Value = 10 };

            y += 34;
            _lblMaxRetries = Label("Max Retries:", 16, y);
            _numMaxRetries = new NumericUpDown { Location = new Point(lw, y - 2), Size = new Size(80, 26), Minimum = 1, Maximum = 10, Value = 3 };

            var info = Label("💡 Leave credentials empty to use Windows saved VPN credentials.", 16, y + 40);
            info.ForeColor = Color.FromArgb(0, 120, 215);
            info.Font = new Font("Segoe UI", 8F);
            info.AutoSize = true;

            _tabVpn.Controls.AddRange(new Control[] { _lblVpnName, _cmbVpnName, _btnRefreshVpns, _chkUseSavedCredentials,
                _lblUsername, _txtUsername, _lblPassword, _txtPassword, _lblCycleDuration, _numCycleDuration, _lblMaxRetries, _numMaxRetries, info });
        }

        private void BuildGeneralTab()
        {
            var y = 16;
            var lw = 200;

            _chkMinimizeToTray = new CheckBox { Text = "Minimize to system tray", Location = new Point(16, y), Size = new Size(480, 24) };
            y += 32;
            _chkStartMinimized = new CheckBox { Text = "Start minimized", Location = new Point(16, y), Size = new Size(480, 24) };
            y += 32;
            _chkAutoStart = new CheckBox { Text = "Auto-start with Windows", Location = new Point(16, y), Size = new Size(480, 24) };

            y += 40;
            _lblRefreshInterval = Label("Status Refresh Interval (s):", 16, y);
            _numRefreshInterval = new NumericUpDown { Location = new Point(lw, y - 2), Size = new Size(80, 26), Minimum = 0.5m, Maximum = 60, Increment = 0.5m, DecimalPlaces = 1, Value = 1 };

            y += 36;
            _chkProcessMonitoringEnabled = new CheckBox { Text = "Enable process monitoring", Location = new Point(16, y), Size = new Size(480, 24), Checked = true };

            y += 34;
            _lblProcessName = Label("Process Name:", 32, y);
            _txtProcessName = TextBox("MEGAsync", lw, y);

            y += 34;
            _lblProcessDisplayName = Label("Display Name:", 32, y);
            _txtProcessDisplayName = TextBox("MEGAsync", lw, y);

            _tabGeneral.Controls.AddRange(new Control[] { _chkMinimizeToTray, _chkStartMinimized, _chkAutoStart,
                _lblRefreshInterval, _numRefreshInterval, _chkProcessMonitoringEnabled,
                _lblProcessName, _txtProcessName, _lblProcessDisplayName, _txtProcessDisplayName });
        }

        private void BuildAppearanceTab()
        {
            var y = 16;
            _lblTheme = Label("Theme:", 16, y);
            _cmbTheme = new ComboBox
            {
                Location = new Point(120, y - 2),
                Size = new Size(220, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbTheme.Items.AddRange(new[] { "System (follows Windows)", "Light", "Dark" });

            var info = Label("🎨 Theme changes apply after OK or Apply.", 16, y + 50);
            info.ForeColor = Color.FromArgb(100, 116, 139);
            info.Font = new Font("Segoe UI", 8F);
            info.AutoSize = true;

            _tabAppearance.Controls.Add(_lblTheme);
            _tabAppearance.Controls.Add(_cmbTheme);
            _tabAppearance.Controls.Add(info);
        }

        private void BuildAboutTab()
        {
            var title = new Label
            {
                Text = "Mega Download Enhancer",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            var version = new Label
            {
                Text = $"Version {Application.ProductVersion}",
                AutoSize = true,
                Location = new Point(20, 50),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            var desc = new Label
            {
                Text = "VPN Auto-Cycler for seamless MEGA.nz downloads.\nAutomatically rotates Cloudflare WARP to bypass\nfree-tier daily transfer limits.",
                AutoSize = true,
                Location = new Point(20, 80),
                ForeColor = Color.FromArgb(63, 63, 70),
                MaximumSize = new Size(480, 0)
            };

            var copyright = new Label
            {
                Text = "© 2026 Imran Rahman. All rights reserved.",
                AutoSize = true,
                Location = new Point(20, 140),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(161, 161, 170)
            };

            var repo = new LinkLabel
            {
                Text = "View on GitHub",
                AutoSize = true,
                Location = new Point(20, 165),
                Font = new Font("Segoe UI", 8F),
                LinkColor = Color.FromArgb(0, 120, 215)
            };
            repo.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/IMRAN104/MegaDownloadEnhancer",
                UseShellExecute = true
            });

            _tabAbout.Controls.Add(title);
            _tabAbout.Controls.Add(version);
            _tabAbout.Controls.Add(desc);
            _tabAbout.Controls.Add(copyright);
            _tabAbout.Controls.Add(repo);
        }

        private static Label Label(string text, int x, int y) => new Label { Text = text, Location = new Point(x, y), AutoSize = true };
        private static TextBox TextBox(string placeholder, int x, int y) => new TextBox
        {
            Location = new Point(x, y - 2),
            Size = new Size(260, 26),
            PlaceholderText = placeholder,
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
        };

        private void LoadSettings()
        {
            RefreshVpnList();
            _cmbVpnName.Text = _settings.VpnName;
            _txtUsername.Text = _settings.Username;
            _txtPassword.Text = _settings.Password;
            _numCycleDuration.Value = _settings.CycleDurationMinutes;
            _numMaxRetries.Value = _settings.MaxRetries;
            _chkUseSavedCredentials.Checked = string.IsNullOrEmpty(_settings.Username) && string.IsNullOrEmpty(_settings.Password);
            _chkMinimizeToTray.Checked = _settings.MinimizeToTray;
            _chkStartMinimized.Checked = _settings.StartMinimized;
            _chkAutoStart.Checked = _settings.AutoStart;
            _numRefreshInterval.Value = _settings.StatusRefreshIntervalSeconds;
            _chkProcessMonitoringEnabled.Checked = _settings.ProcessMonitoringEnabled;
            _txtProcessName.Text = _settings.MonitoredProcessName;
            _txtProcessDisplayName.Text = _settings.MonitoredProcessDisplayName;
            _cmbTheme.SelectedIndex = (int)_settings.ThemeMode;
        }

        private void RefreshVpnList()
        {
            try
            {
                _cmbVpnName.Items.Clear();
                _cmbVpnName.Items.Add("CloudflareWARP");
                foreach (var vpn in _vpnService.GetAvailableVpns())
                    if (!_cmbVpnName.Items.Contains(vpn)) _cmbVpnName.Items.Add(vpn);
                _cmbVpnName.Items.Add("(Manual Entry)");
                if (_cmbVpnName.Items.Count == 0) _cmbVpnName.Items.Add("No VPN connections found");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve VPN list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            _settings.VpnName = _cmbVpnName.Text;
            if (_chkUseSavedCredentials.Checked) { _settings.Username = ""; _settings.Password = ""; }
            else { _settings.Username = _txtUsername.Text; _settings.Password = _txtPassword.Text; }
            _settings.CycleDurationMinutes = (int)_numCycleDuration.Value;
            _settings.MaxRetries = (int)_numMaxRetries.Value;
            _settings.MinimizeToTray = _chkMinimizeToTray.Checked;
            _settings.StartMinimized = _chkStartMinimized.Checked;
            _settings.AutoStart = _chkAutoStart.Checked;
            _settings.StatusRefreshIntervalSeconds = (int)Math.Round(_numRefreshInterval.Value, MidpointRounding.AwayFromZero);
            _settings.ProcessMonitoringEnabled = _chkProcessMonitoringEnabled.Checked;
            _settings.MonitoredProcessName = _txtProcessName.Text.Trim();
            _settings.MonitoredProcessDisplayName = _txtProcessDisplayName.Text.Trim();
            _settings.ThemeMode = (ThemeMode)_cmbTheme.SelectedIndex;
        }
    }
}
