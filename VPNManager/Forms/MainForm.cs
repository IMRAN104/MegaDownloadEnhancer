using System;
using System.Drawing;
using System.Windows.Forms;
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

        // UI Controls
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

        public MainForm()
        {
            _settings = AppSettings.Load();
            _vpnService = new VpnService(_settings);
            _megaService = new MegaService(_settings);

            InitializeComponent();
            InitializeTrayIcon();

            _refreshTimer = new Timer();
            _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
            _refreshTimer.Tick += RefreshTimer_Tick;

            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
            Resize += MainForm_Resize;

            // Show first-time setup dialog if needed
            if (string.IsNullOrEmpty(_settings.VpnName))
            {
                ShowFirstTimeSetup();
            }

            UpdateVpnStatus();
            UpdateMegaStatus();
        }

        private void InitializeComponent()
        {
            // Form
            this.Text = "VPN Manager";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F);

            // Title Label
            lblTitle = new Label
            {
                Text = "VPN Auto-Manager",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // VPN Status Group
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
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.Red
            };

            pnlVpnIndicator = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(120, 34),
                BackColor = Color.Red,
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
                Location = new Point(20, 85),
                ForeColor = Color.Gray
            };

            lblCycleStartTime = new Label
            {
                Text = "Cycle: Not Started",
                AutoSize = true,
                Location = new Point(300, 60),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9F)
            };

            grpVpnStatus.Controls.AddRange(new Control[] {
                lblVpnStatusTitle, lblVpnStatus, pnlVpnIndicator,
                lblVpnConnectionName, lblVpnLastUpdate, lblCycleStartTime
            });

            // Mega Status Group
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
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.Red
            };

            pnlMegaIndicator = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(120, 34),
                BackColor = Color.Red,
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
                Location = new Point(20, 85),
                ForeColor = Color.Gray
            };

            grpMegaStatus.Controls.AddRange(new Control[] {
                lblMegaStatusTitle, lblMegaStatus, pnlMegaIndicator,
                lblMegaProcessId, lblMegaLastUpdate
            });

            // Controls Group
            grpControls = CreateGroupBox("Controls", 20, 350, 550, 80);

            btnToggleCycle = CreateButton("Start Cycle", 20, 25, 140, 35, Color.FromArgb(0, 180, 0));
            btnToggleCycle.Click += BtnToggleCycle_Click;

            btnSettings = CreateButton("Settings", 180, 25, 100, 35, Color.FromArgb(100, 100, 100));
            btnSettings.Click += BtnSettings_Click;

            btnRefresh = CreateButton("Refresh Now", 300, 25, 120, 35, Color.FromArgb(0, 120, 215));
            btnRefresh.Click += BtnRefresh_Click;

            grpControls.Controls.AddRange(new Control[] { btnToggleCycle, btnSettings, btnRefresh });

            // Status Strip
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Ready" };
            lblDateTime = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblDateTime });

            // Add controls to form
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
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 0),
                BackColor = Color.White
            };
        }

        private Button CreateButton(string text, int x, int y, int width, int height, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backColor,
                ForeColor = Color.White,
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
            trayContextMenu.Items.Add("Exit", null, TrayExit_Click);

            trayIcon = new NotifyIcon
            {
                Text = "VPN Manager",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayContextMenu,
                Visible = false
            };

            trayIcon.DoubleClick += (s, e) => this.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_settings.StartMinimized && _settings.MinimizeToTray)
            {
                this.WindowState = FormWindowState.Minimized;
            }

            _refreshTimer.Start();
            UpdateButtonStates();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_settings.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.Visible = true;
            }
            else
            {
                _refreshTimer.Stop();
                trayIcon.Visible = false;
                _vpnService.StopVpnCycle();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && _settings.MinimizeToTray)
            {
                this.Hide();
                trayIcon.Visible = true;
            }
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            UpdateVpnStatus();
            UpdateMegaStatus();
            lblDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void UpdateVpnStatus()
        {
            try
            {
                var status = _vpnService.GetCurrentStatus(_settings.VpnName);

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

        private async void BtnToggleCycle_Click(object? sender, EventArgs e)
        {
            if (_vpnService.IsRunning)
            {
                // Stop the cycle
                try
                {
                    lblStatus.Text = "Stopping VPN cycle...";
                    _vpnService.StopVpnCycle();
                    _cycleStartTime = null;
                    lblStatus.Text = "VPN cycle stopped";
                    UpdateButtonStates();

                    // Wait for VPN to disconnect
                    await System.Threading.Tasks.Task.Delay(3000);

                    // Force immediate status update (MEGAsync is NOT restarted on stop)
                    UpdateVpnStatus();
                    UpdateMegaStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to stop VPN cycle: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    lblStatus.Text = "Failed to stop";
                }
            }
            else
            {
                // Start the cycle
                try
                {
                    // Validate settings before starting
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
                            BtnSettings_Click(sender, e);
                        }
                        return;
                    }

                    lblStatus.Text = "Starting VPN cycle...";
                    _vpnService.StartVpnCycle(_settings);
                    _cycleStartTime = DateTime.Now;
                    lblStatus.Text = "VPN cycle running";
                    UpdateButtonStates();

                    // Wait for VPN to connect (PowerShell script handles this)
                    await System.Threading.Tasks.Task.Delay(3000);

                    // Restart MEGAsync after VPN connects
                    _megaService.RestartMegasync();

                    // Wait a bit for MEGAsync to start
                    await System.Threading.Tasks.Task.Delay(2000);

                    // Force immediate status update
                    UpdateVpnStatus();
                    UpdateMegaStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to start VPN cycle: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    lblStatus.Text = "Failed to start";
                }
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_settings);
            if (settingsForm.ShowDialog(this) == DialogResult.OK)
            {
                _settings.Save();
                _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
                UpdateVpnStatus();
                UpdateButtonStates();
                lblStatus.Text = "Settings saved";
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            UpdateVpnStatus();
            UpdateMegaStatus();
            lblStatus.Text = "Status refreshed";
        }

        // Tray icon event handlers
        private void TrayShow_Click(object? sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
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

        private void TrayExit_Click(object? sender, EventArgs e)
        {
            _settings.MinimizeToTray = false; // Override to allow closing
            this.Close();
        }

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

            return null; // No error
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
                var settingsForm = new SettingsForm(_settings);
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings.Save();
                    _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
                    UpdateVpnStatus();
                    UpdateButtonStates();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Dispose();
                trayIcon?.Dispose();
                trayContextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
