using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using VPNManager.Models;
using VPNManager.Services;

namespace VPNManager.Forms
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly VpnService _vpnService;
        private readonly MegaService _megaService;
        private readonly Timer _refreshTimer;

        private Label lblTitle;
        private GroupBox grpVpnStatus;
        private Label lblVpnStatusTitle;
        private Label lblVpnStatus;
        private Label lblVpnConnectionName;
        private Label lblVpnLastUpdate;
        private Label lblCycleStartTime;
        private Panel pnlVpnIndicator;
        private GroupBox grpMegaStatus;
        private Label lblMegaStatusTitle;
        private Label lblMegaStatus;
        private Label lblMegaProcessId;
        private Label lblMegaLastUpdate;
        private Panel pnlMegaIndicator;
        private GroupBox grpControls;
        private Button btnToggleCycle;
        private Button btnSettings;
        private Button btnRefresh;
        private DateTime? _cycleStartTime;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblDateTime;
        private ContextMenuStrip trayContextMenu;
        private NotifyIcon trayIcon;
        private bool _exiting;
        private bool _toggling;
        private VpnStatus _lastVpnStatus;

        public MainForm()
        {
            _settings = AppSettings.Load();
            _vpnService = new VpnService(_settings);
            _megaService = new MegaService(_settings);
            _lastVpnStatus = new VpnStatus { IsConnected = false };

            InitializeComponent();
            InitializeTrayIcon();
            ApplyTheme();

            _refreshTimer = new Timer();
            _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
            _refreshTimer.Tick += RefreshTimer_Tick;

            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
            Resize += MainForm_Resize;

            if (string.IsNullOrEmpty(_settings.VpnName))
            {
                BeginInvoke(new Action(() => ShowFirstTimeSetup()));
            }

            BeginInvoke(new Action(async () =>
            {
                await UpdateVpnStatusAsync();
                UpdateMegaStatus();
            }));

            ApplyAutoStart();
        }

        #region Theme
        private void ApplyTheme()
        {
            if (_settings.ThemeMode == ThemeMode.System)
            {
                _settings.ThemeMode = ThemeUtils.IsSystemDarkMode() ? ThemeMode.Dark : ThemeMode.Light;
            }
            ThemeUtils.ApplyTheme(this, _settings.ThemeMode);
        }

        public void ReapplyTheme()
        {
            ApplyTheme();
            foreach (Control c in this.Controls)
                ThemeUtils.ApplyThemeToControl(c, _settings.ThemeMode);
            foreach (Control c in grpVpnStatus.Controls)
                ThemeUtils.ApplyThemeToControl(c, _settings.ThemeMode);
            foreach (Control c in grpMegaStatus.Controls)
                ThemeUtils.ApplyThemeToControl(c, _settings.ThemeMode);
            foreach (Control c in grpControls.Controls)
                ThemeUtils.ApplyThemeToControl(c, _settings.ThemeMode);
        }
        #endregion

        #region Initialization
        private void InitializeComponent()
        {
            this.Text = "VPN Manager";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);
            this.Font = new Font("Segoe UI", 9F);

            lblTitle = new Label
            {
                Text = "VPN Auto-Manager",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            grpVpnStatus = CreateGroupBox("VPN Status", 20, 70, 550, 130);

            lblVpnStatusTitle = new Label
            {
                Text = "Current Status:",
                AutoSize = true,
                Location = new Point(20, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblVpnStatus = new Label
            {
                Text = "Disconnected",
                AutoSize = true,
                Location = new Point(140, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            pnlVpnIndicator = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(120, 34),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblVpnConnectionName = new Label
            {
                Text = "Connection: Not Configured",
                AutoSize = true,
                Location = new Point(20, 60)
            };

            lblVpnLastUpdate = new Label
            {
                Text = "Last Update: Never",
                AutoSize = true,
                Location = new Point(20, 85)
            };

            lblCycleStartTime = new Label
            {
                Text = "Cycle: Not Started",
                AutoSize = true,
                Location = new Point(300, 60)
            };

            grpVpnStatus.Controls.AddRange(new Control[] {
                lblVpnStatusTitle, lblVpnStatus, pnlVpnIndicator,
                lblVpnConnectionName, lblVpnLastUpdate, lblCycleStartTime
            });

            grpMegaStatus = CreateGroupBox("MEGAsync Status", 20, 210, 550, 130);

            lblMegaStatusTitle = new Label
            {
                Text = "Current Status:",
                AutoSize = true,
                Location = new Point(20, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblMegaStatus = new Label
            {
                Text = "Not Running",
                AutoSize = true,
                Location = new Point(140, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            pnlMegaIndicator = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(120, 34),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblMegaProcessId = new Label
            {
                Text = $"Process: {_settings.MonitoredProcessDisplayName} - Not Running",
                AutoSize = true,
                Location = new Point(20, 60)
            };

            lblMegaLastUpdate = new Label
            {
                Text = "Last Update: Never",
                AutoSize = true,
                Location = new Point(20, 85)
            };

            grpMegaStatus.Controls.AddRange(new Control[] {
                lblMegaStatusTitle, lblMegaStatus, pnlMegaIndicator,
                lblMegaProcessId, lblMegaLastUpdate
            });

            grpControls = CreateGroupBox("Controls", 20, 350, 550, 80);

            btnToggleCycle = CreateButton("Start Cycle", 20, 25, 140, 35);
            btnToggleCycle.Click += BtnToggleCycle_Click;

            btnSettings = CreateButton("Settings", 180, 25, 100, 35);
            btnSettings.Click += BtnSettings_Click;

            btnRefresh = CreateButton("Refresh Now", 300, 25, 120, 35);
            btnRefresh.Click += BtnRefresh_Click;

            grpControls.Controls.AddRange(new Control[] { btnToggleCycle, btnSettings, btnRefresh });

            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Ready" };
            lblDateTime = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblDateTime });

            this.Controls.Add(lblTitle);
            this.Controls.Add(grpVpnStatus);
            this.Controls.Add(grpMegaStatus);
            this.Controls.Add(grpControls);
            this.Controls.Add(statusStrip);
        }

        private GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
        {
            return new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private void InitializeTrayIcon()
        {
            trayContextMenu = new ContextMenuStrip();
            trayContextMenu.Items.Add("Show", null, TrayShow_Click);
            trayContextMenu.Items.Add("Start VPN", null, TrayStart_Click);
            trayContextMenu.Items.Add("Stop VPN", null, TrayStop_Click);
            trayContextMenu.Items.Add(new ToolStripSeparator());
            trayContextMenu.Items.Add("Auto-start with Windows", null, TrayAutoStart_Click);
            trayContextMenu.Items.Add(new ToolStripSeparator());
            trayContextMenu.Items.Add("Exit", null, TrayExit_Click);

            trayIcon = new NotifyIcon
            {
                Text = "VPN Manager",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayContextMenu,
                Visible = true
            };

            trayIcon.DoubleClick += (s, e) =>
            {
                _exiting = false;
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };
        }
        #endregion

        #region Form Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_settings.StartMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }

            _refreshTimer.Start();
            UpdateButtonStates();
            UpdateTrayAutoStartCheck();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_exiting)
            {
                e.Cancel = true;
                this.Hide();
                if (_vpnService.IsRunning)
                {
                    trayIcon.ShowBalloonTip(3000, "VPN Manager", "VPN cycle still running in background", ToolTipIcon.Info);
                }
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
        #endregion

        #region Shutdown
        public void Shutdown()
        {
            _exiting = true;
            _refreshTimer.Stop();
            trayIcon.Visible = false;
            _vpnService.StopVpnCycle();
            _vpnService.Dispose();
            this.Close();
        }
        #endregion

        #region Refresh loop
        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            await UpdateVpnStatusAsync();
            UpdateMegaStatus();
            lblDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        #region Status Updates
        private async Task UpdateVpnStatusAsync()
        {
            try
            {
                var status = await _vpnService.GetCurrentStatusAsync(_settings.VpnName);

                if (status.IsConnected)
                {
                    lblVpnStatus.Text = "Connected";
                    lblVpnStatus.ForeColor = Color.Green;
                    pnlVpnIndicator.BackColor = Color.Green;
                }
                else
                {
                    lblVpnStatus.Text = "Disconnected";
                    lblVpnStatus.ForeColor = Color.Red;
                    pnlVpnIndicator.BackColor = Color.Red;
                }

                lblVpnConnectionName.Text = !string.IsNullOrEmpty(_settings.VpnName)
                    ? $"Connection: {_settings.VpnName}"
                    : "Connection: Not Configured";

                lblVpnLastUpdate.Text = $"Last Update: {DateTime.Now:HH:mm:ss}";

                if (_vpnService.IsRunning && _lastVpnStatus.IsConnected != status.IsConnected)
                {
                    var state = status.IsConnected ? "connected" : "disconnected";
                    trayIcon.ShowBalloonTip(3000, "VPN Manager", $"VPN {state}", ToolTipIcon.Info);
                }
                _lastVpnStatus = status;
            }
            catch (Exception ex)
            {
                lblVpnStatus.Text = "Error";
                lblVpnStatus.ForeColor = Color.Orange;
                pnlVpnIndicator.BackColor = Color.Orange;
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateMegaStatus()
        {
            try
            {
                var status = _megaService.GetCurrentStatus();

                if (status.IsRunning)
                {
                    lblMegaStatus.Text = status.IsSyncing ? "Syncing" : "Running";
                    lblMegaStatus.ForeColor = status.IsSyncing ? Color.Blue : Color.Green;
                    pnlMegaIndicator.BackColor = status.IsSyncing ? Color.Blue : Color.Green;
                    lblMegaProcessId.Text = $"Process: {_settings.MonitoredProcessDisplayName} (PID: {status.ProcessId})";
                }
                else
                {
                    lblMegaStatus.Text = "Not Running";
                    lblMegaStatus.ForeColor = Color.Red;
                    pnlMegaIndicator.BackColor = Color.Red;
                    lblMegaProcessId.Text = $"Process: {_settings.MonitoredProcessDisplayName} - Not Running";
                }

                lblMegaLastUpdate.Text = $"Last Update: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                lblMegaStatus.Text = "Error";
                lblMegaStatus.ForeColor = Color.Orange;
                pnlMegaIndicator.BackColor = Color.Orange;
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateButtonStates()
        {
            var isRunning = _vpnService.IsRunning;
            btnToggleCycle.Enabled = !string.IsNullOrEmpty(_settings.VpnName);
            btnToggleCycle.Text = isRunning ? "Stop Cycle" : "Start Cycle";
            btnToggleCycle.BackColor = isRunning ? Color.FromArgb(200, 50, 50) : Color.FromArgb(0, 180, 0);
            btnToggleCycle.ForeColor = Color.White;

            if (_cycleStartTime.HasValue && isRunning)
            {
                lblCycleStartTime.Text = $"Cycle Started: {_cycleStartTime.Value:HH:mm:ss}";
                lblCycleStartTime.ForeColor = Color.Green;
            }
            else
            {
                lblCycleStartTime.Text = "Cycle: Not Running";
                lblCycleStartTime.ForeColor = Color.Gray;
            }
        }
        #endregion

        #region Button Handlers
        private async void BtnToggleCycle_Click(object? sender, EventArgs e)
        {
            if (_toggling)
                return;
            _toggling = true;

            try
            {
                if (_vpnService.IsRunning)
                {
                    lblStatus.Text = "Stopping VPN cycle...";
                    _vpnService.StopVpnCycle();
                    _cycleStartTime = null;
                    lblStatus.Text = "VPN cycle stopped";
                    UpdateButtonStates();
                    await Task.Delay(3000);
                    await UpdateVpnStatusAsync();
                    UpdateMegaStatus();
                }
                else
                {
                    var validationError = ValidateSettings();
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        var result = MessageBox.Show(
                            $"{validationError}\n\nWould you like to open Settings now?",
                            "Configuration Required",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.Yes)
                        {
                            ShowSettings();
                        }
                        return;
                    }

                    lblStatus.Text = "Starting VPN cycle...";
                    _vpnService.StartVpnCycle(_settings);
                    _cycleStartTime = DateTime.Now;
                    lblStatus.Text = "VPN cycle running";
                    UpdateButtonStates();
                    await Task.Delay(3000);
                    await _megaService.RestartMegasyncAsync();
                    await Task.Delay(2000);
                    await UpdateVpnStatusAsync();
                    UpdateMegaStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Operation failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                lblStatus.Text = "Operation failed";
            }
            finally
            {
                _toggling = false;
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            var settingsForm = new SettingsForm(_settings);
            if (settingsForm.ShowDialog(this) == DialogResult.OK)
            {
                _settings.Save();
                _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
                ApplyAutoStart();
                BeginInvoke(new Action(() => ReapplyTheme()));
                BeginInvoke(new Action(async () => await UpdateVpnStatusAsync()));
                UpdateButtonStates();
                lblStatus.Text = "Settings saved";
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            BeginInvoke(new Action(async () => await UpdateVpnStatusAsync()));
            UpdateMegaStatus();
            lblStatus.Text = "Status refreshed";
        }
        #endregion

        #region Tray Handlers
        private void TrayShow_Click(object? sender, EventArgs e)
        {
            _exiting = false;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void TrayStart_Click(object? sender, EventArgs e)
        {
            if (!_vpnService.IsRunning)
            {
                BtnToggleCycle_Click(sender, e);
            }
        }

        private void TrayStop_Click(object? sender, EventArgs e)
        {
            if (_vpnService.IsRunning)
            {
                BtnToggleCycle_Click(sender, e);
            }
        }

        private void TrayAutoStart_Click(object? sender, EventArgs e)
        {
            _settings.AutoStart = !_settings.AutoStart;
            _settings.Save();
            ApplyAutoStart();
            UpdateTrayAutoStartCheck();
            trayIcon.ShowBalloonTip(2000, "VPN Manager",
                _settings.AutoStart ? "Will auto-start with Windows" : "Auto-start disabled",
                ToolTipIcon.Info);
        }

        private void TrayExit_Click(object? sender, EventArgs e)
        {
            Shutdown();
        }
        #endregion

        #region Auto-start
        private void ApplyAutoStart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (_settings.AutoStart)
                {
                    var exePath = Application.ExecutablePath;
                    key?.SetValue("VPNManager", $"\"{exePath}\"");
                }
                else
                {
                    key?.DeleteValue("VPNManager", false);
                }
            }
            catch
            {
            }
        }

        private void UpdateTrayAutoStartCheck()
        {
            foreach (ToolStripItem item in trayContextMenu.Items)
            {
                if (item.Text?.StartsWith("Auto-start") == true || item.Text?.StartsWith("✓ Auto-start") == true || item.Text?.StartsWith("  Auto-start") == true)
                {
                    item.Text = _settings.AutoStart
                        ? "✓ Auto-start with Windows"
                        : "  Auto-start with Windows";
                    break;
                }
            }
        }
        #endregion

        #region Settings Validation & First-time Setup
        private string? ValidateSettings()
        {
            if (string.IsNullOrEmpty(_settings.VpnName))
            {
                return "VPN name is not configured.\nPlease select a VPN connection in Settings.";
            }

            if (!_vpnService.IsVpnAvailable(_settings.VpnName))
            {
                return $"VPN connection '{_settings.VpnName}' is not available on this system.\n\n" +
                       $"For WARP: Ensure 'warp-cli.exe' is in your PATH.\n" +
                       $"For Windows VPN: Check your Windows VPN settings.";
            }

            return null;
        }

        private void ShowFirstTimeSetup()
        {
            var result = MessageBox.Show(
                "Welcome to VPN Manager!\n\n" +
                "Before you can use this application, you need to configure:\n" +
                "• VPN connection (CloudflareWARP or Windows VPN)\n" +
                "• Status refresh interval\n" +
                "• Process monitoring settings (optional)\n\n" +
                "Would you like to open Settings now?",
                "First-Time Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                ShowSettings();
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Dispose();
                trayIcon?.Dispose();
                trayContextMenu?.Dispose();
                _vpnService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
