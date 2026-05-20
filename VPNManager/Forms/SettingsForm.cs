using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VPNManager.Models;
using VPNManager.Services;

namespace VPNManager.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly VpnService _vpnService;

        // UI Controls
        private TabControl tabControl;
        private TabPage tabVpn;
        private TabPage tabGeneral;
        private TabPage tabAppearance;

        // VPN Tab
        private ComboBox cmbVpnName;
        private Label lblVpnName;
        private Button btnRefreshVpns;
        private TextBox txtUsername;
        private Label lblUsername;
        private TextBox txtPassword;
        private Label lblPassword;
        private NumericUpDown numCycleDuration;
        private Label lblCycleDuration;
        private NumericUpDown numMaxRetries;
        private Label lblMaxRetries;
        private CheckBox chkUseSavedCredentials;

        // General Tab
        private CheckBox chkMinimizeToTray;
        private CheckBox chkStartMinimized;
        private CheckBox chkAutoStart;
        private CheckBox chkProcessMonitoringEnabled;
        private NumericUpDown numRefreshInterval;
        private Label lblRefreshInterval;
        private TextBox txtProcessName;
        private Label lblProcessName;
        private TextBox txtProcessDisplayName;
        private Label lblProcessDisplayName;

        // Appearance Tab
        private ComboBox cmbTheme;
        private Label lblTheme;

        // Buttons
        private Button btnOk;
        private Button btnCancel;
        private Button btnApply;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            _vpnService = new VpnService(settings);

            InitializeComponent();
            LoadSettings();
            ThemeUtils.ApplyThemeToControl(tabControl, _settings.ThemeMode);
            ThemeUtils.ApplyThemeToControl(tabVpn, _settings.ThemeMode);
            ThemeUtils.ApplyThemeToControl(tabGeneral, _settings.ThemeMode);
            ThemeUtils.ApplyThemeToControl(tabAppearance, _settings.ThemeMode);
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(550, 450);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            tabControl = new TabControl
            {
                Location = new Point(12, 12),
                Size = new Size(510, 340),
                Font = new Font("Segoe UI", 9F)
            };

            tabVpn = new TabPage("VPN Settings");
            CreateVpnTab();

            tabGeneral = new TabPage("General");
            CreateGeneralTab();

            tabAppearance = new TabPage("Appearance");
            CreateAppearanceTab();

            tabControl.TabPages.Add(tabVpn);
            tabControl.TabPages.Add(tabGeneral);
            tabControl.TabPages.Add(tabAppearance);

            btnOk = CreateButton("OK", 330, 365, 80, 30);
            btnOk.Click += BtnOk_Click;

            btnApply = CreateButton("Apply", 240, 365, 80, 30);
            btnApply.Click += BtnApply_Click;

            btnCancel = CreateButton("Cancel", 420, 365, 80, 30);
            btnCancel.Click += BtnCancel_Click;

            this.Controls.Add(tabControl);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnCancel);
        }

        private void CreateVpnTab()
        {
            var yPosition = 20;
            var labelWidth = 150;
            var controlWidth = 280;

            lblVpnName = new Label
            {
                Text = "VPN Connection Name:",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            cmbVpnName = new ComboBox
            {
                Location = new Point(180, yPosition - 2),
                Size = new Size(controlWidth - 30, 25),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            btnRefreshVpns = new Button
            {
                Text = "↻",
                Location = new Point(430, yPosition - 3),
                Size = new Size(30, 26),
                Tag = "Refresh VPN list"
            };
            btnRefreshVpns.Click += BtnRefreshVpns_Click;

            yPosition += 35;

            chkUseSavedCredentials = new CheckBox
            {
                Text = "Use saved credentials from Windows",
                Location = new Point(20, yPosition),
                Size = new Size(400, 24),
                Checked = true
            };
            chkUseSavedCredentials.CheckedChanged += ChkUseSavedCredentials_CheckedChanged;

            yPosition += 30;

            lblUsername = new Label
            {
                Text = "Username (optional):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            txtUsername = new TextBox
            {
                Location = new Point(180, yPosition - 2),
                Size = new Size(controlWidth, 25),
                PlaceholderText = "user@domain.com"
            };

            yPosition += 35;

            lblPassword = new Label
            {
                Text = "Password (optional):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            txtPassword = new TextBox
            {
                Location = new Point(180, yPosition - 2),
                Size = new Size(controlWidth, 25),
                PasswordChar = '●',
                PlaceholderText = "Leave empty to use saved credentials"
            };

            yPosition += 35;

            lblCycleDuration = new Label
            {
                Text = "Cycle Duration (minutes):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            numCycleDuration = new NumericUpDown
            {
                Location = new Point(180, yPosition - 2),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 1440,
                Value = 10
            };

            yPosition += 35;

            lblMaxRetries = new Label
            {
                Text = "Max Connection Retries:",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            numMaxRetries = new NumericUpDown
            {
                Location = new Point(180, yPosition - 2),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10,
                Value = 3
            };

            yPosition += 40;
            var lblInfo = new Label
            {
                Text = "💡 Leave username/password empty to use credentials saved in Windows VPN settings.",
                Location = new Point(20, yPosition),
                Size = new Size(460, 40),
                ForeColor = Color.FromArgb(0, 100, 200),
                Font = new Font("Segoe UI", 8F)
            };

            tabVpn.Controls.Add(lblVpnName);
            tabVpn.Controls.Add(cmbVpnName);
            tabVpn.Controls.Add(btnRefreshVpns);
            tabVpn.Controls.Add(chkUseSavedCredentials);
            tabVpn.Controls.Add(lblUsername);
            tabVpn.Controls.Add(txtUsername);
            tabVpn.Controls.Add(lblPassword);
            tabVpn.Controls.Add(txtPassword);
            tabVpn.Controls.Add(lblCycleDuration);
            tabVpn.Controls.Add(numCycleDuration);
            tabVpn.Controls.Add(lblMaxRetries);
            tabVpn.Controls.Add(numMaxRetries);
            tabVpn.Controls.Add(lblInfo);
        }

        private void CreateGeneralTab()
        {
            var yPosition = 20;
            var labelWidth = 180;
            var controlWidth = 100;

            chkMinimizeToTray = new CheckBox
            {
                Text = "Minimize to system tray",
                Location = new Point(20, yPosition),
                Size = new Size(400, 24)
            };

            yPosition += 30;

            chkStartMinimized = new CheckBox
            {
                Text = "Start application minimized",
                Location = new Point(20, yPosition),
                Size = new Size(400, 24)
            };

            yPosition += 30;

            chkAutoStart = new CheckBox
            {
                Text = "Auto-start with Windows",
                Location = new Point(20, yPosition),
                Size = new Size(400, 24)
            };

            yPosition += 35;

            lblRefreshInterval = new Label
            {
                Text = "Status Refresh Interval (seconds):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            numRefreshInterval = new NumericUpDown
            {
                Location = new Point(250, yPosition - 2),
                Size = new Size(controlWidth, 25),
                Minimum = 0.5m,
                Maximum = 60,
                Increment = 0.5m,
                DecimalPlaces = 1,
                Value = 1
            };

            yPosition += 35;

            chkProcessMonitoringEnabled = new CheckBox
            {
                Text = "Enable process monitoring",
                Location = new Point(20, yPosition),
                Size = new Size(400, 24),
                Checked = true
            };

            yPosition += 35;

            lblProcessName = new Label
            {
                Text = "Process Name (e.g., MEGAsync):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            txtProcessName = new TextBox
            {
                Location = new Point(250, yPosition - 2),
                Size = new Size(220, 25),
                Text = "MEGAsync"
            };

            yPosition += 35;

            lblProcessDisplayName = new Label
            {
                Text = "Display Name (e.g., MEGAsync):",
                Location = new Point(20, yPosition),
                Size = new Size(labelWidth, 20)
            };

            txtProcessDisplayName = new TextBox
            {
                Location = new Point(250, yPosition - 2),
                Size = new Size(220, 25),
                Text = "MEGAsync"
            };

            yPosition += 50;

            var lblInfo = new Label
            {
                Text = "⚙️ These settings control the application behavior and UI refresh rate.\n\n" +
                       "• Refresh interval: How often to check status (0.5-60 seconds)\n" +
                       "• Process monitoring: Monitor custom process (e.g., MEGAsync)\n" +
                       "• Auto-detection: Searches AppData\\Local, Program Files, Program Files (x86)",
                Location = new Point(20, yPosition),
                Size = new Size(460, 120),
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8F)
            };

            tabGeneral.Controls.Add(chkMinimizeToTray);
            tabGeneral.Controls.Add(chkStartMinimized);
            tabGeneral.Controls.Add(chkAutoStart);
            tabGeneral.Controls.Add(lblRefreshInterval);
            tabGeneral.Controls.Add(numRefreshInterval);
            tabGeneral.Controls.Add(chkProcessMonitoringEnabled);
            tabGeneral.Controls.Add(lblProcessName);
            tabGeneral.Controls.Add(txtProcessName);
            tabGeneral.Controls.Add(lblProcessDisplayName);
            tabGeneral.Controls.Add(txtProcessDisplayName);
            tabGeneral.Controls.Add(lblInfo);
        }

        private void CreateAppearanceTab()
        {
            var yPosition = 20;

            lblTheme = new Label
            {
                Text = "Theme:",
                Location = new Point(20, yPosition),
                Size = new Size(80, 20)
            };

            cmbTheme = new ComboBox
            {
                Location = new Point(120, yPosition - 2),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTheme.Items.AddRange(new[] { "System (default)", "Light", "Dark" });

            yPosition += 50;
            var lblInfo = new Label
            {
                Text = "🎨 Changes apply after clicking OK or Apply.\n" +
                       "System mode follows your Windows theme setting.",
                Location = new Point(20, yPosition),
                Size = new Size(460, 50),
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8F)
            };

            tabAppearance.Controls.Add(lblTheme);
            tabAppearance.Controls.Add(cmbTheme);
            tabAppearance.Controls.Add(lblInfo);
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                DialogResult = DialogResult.None,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
        }

        private void LoadSettings()
        {
            RefreshVpnList();

            cmbVpnName.Text = _settings.VpnName;
            txtUsername.Text = _settings.Username;
            txtPassword.Text = _settings.Password;
            numCycleDuration.Value = _settings.CycleDurationMinutes;
            numMaxRetries.Value = _settings.MaxRetries;

            chkUseSavedCredentials.Checked = string.IsNullOrEmpty(_settings.Username) &&
                                               string.IsNullOrEmpty(_settings.Password);
            ChkUseSavedCredentials_CheckedChanged(null, EventArgs.Empty);

            chkMinimizeToTray.Checked = _settings.MinimizeToTray;
            chkStartMinimized.Checked = _settings.StartMinimized;
            chkAutoStart.Checked = _settings.AutoStart;
            numRefreshInterval.Value = _settings.StatusRefreshIntervalSeconds;
            chkProcessMonitoringEnabled.Checked = _settings.ProcessMonitoringEnabled;
            txtProcessName.Text = _settings.MonitoredProcessName;
            txtProcessDisplayName.Text = _settings.MonitoredProcessDisplayName;

            cmbTheme.SelectedIndex = (int)_settings.ThemeMode;
        }

        private void RefreshVpnList()
        {
            try
            {
                cmbVpnName.Items.Clear();
                cmbVpnName.Items.Add("CloudflareWARP");

                var vpns = _vpnService.GetAvailableVpns();
                foreach (var vpn in vpns)
                {
                    if (!cmbVpnName.Items.Contains(vpn))
                    {
                        cmbVpnName.Items.Add(vpn);
                    }
                }

                cmbVpnName.Items.Add("(Manual Entry)");

                if (cmbVpnName.Items.Count == 0)
                {
                    cmbVpnName.Items.Add("No VPN connections found");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to retrieve VPN list: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnRefreshVpns_Click(object? sender, EventArgs e)
        {
            var selectedVpn = cmbVpnName.Text;
            RefreshVpnList();
            cmbVpnName.Text = selectedVpn;
        }

        private void ChkUseSavedCredentials_CheckedChanged(object? sender, EventArgs e)
        {
            var useSaved = chkUseSavedCredentials.Checked;
            txtUsername.Enabled = !useSaved;
            txtPassword.Enabled = !useSaved;

            if (useSaved)
            {
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }
        }

        private void SaveSettings()
        {
            _settings.VpnName = cmbVpnName.Text;

            if (chkUseSavedCredentials.Checked)
            {
                _settings.Username = string.Empty;
                _settings.Password = string.Empty;
            }
            else
            {
                _settings.Username = txtUsername.Text;
                _settings.Password = txtPassword.Text;
            }

            _settings.CycleDurationMinutes = (int)numCycleDuration.Value;
            _settings.MaxRetries = (int)numMaxRetries.Value;

            _settings.MinimizeToTray = chkMinimizeToTray.Checked;
            _settings.StartMinimized = chkStartMinimized.Checked;
            _settings.AutoStart = chkAutoStart.Checked;
            _settings.StatusRefreshIntervalSeconds = (int)Math.Round(numRefreshInterval.Value, MidpointRounding.AwayFromZero);
            _settings.ProcessMonitoringEnabled = chkProcessMonitoringEnabled.Checked;
            _settings.MonitoredProcessName = txtProcessName.Text.Trim();
            _settings.MonitoredProcessDisplayName = txtProcessDisplayName.Text.Trim();

            _settings.ThemeMode = (ThemeMode)cmbTheme.SelectedIndex;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            SaveSettings();
            _settings.Save();
            MessageBox.Show(
                "Settings saved successfully!",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
