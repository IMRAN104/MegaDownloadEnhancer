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

        // Layout panels
        private TableLayoutPanel _mainLayout;
        private Panel _headerPanel;
        private Panel _contentPanel;
        private Panel _footerPanel;

        // Header
        private Label _lblAppTitle;
        private Label _lblSubtitle;

        // Status cards
        private Panel _cardVpn;
        private Panel _cardMega;
        private Label _lblVpnLabel;
        private Label _lblVpnValue;
        private Label _lblVpnDetail;
        private Panel _dotVpn;
        private Label _lblMegaLabel;
        private Label _lblMegaValue;
        private Label _lblMegaDetail;
        private Panel _dotMega;

        // Controls
        private Panel _cardControls;
        private Button _btnToggle;
        private Button _btnSettings;
        private Button _btnRefresh;
        private Label _lblCycleInfo;

        // Footer
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private ToolStripStatusLabel _lblFooterRight;
        private Label _lblVersion;
        private LinkLabel _lnkAbout;

        private DateTime? _cycleStartTime;
        private ContextMenuStrip _trayContextMenu;
        private NotifyIcon _trayIcon;
        private bool _exiting;
        private bool _toggling;
        private VpnStatus _lastVpnStatus;

        private static readonly Color Accent = Color.FromArgb(0, 120, 215);
        private static readonly Color AccentDark = Color.FromArgb(0, 90, 180);
        private static readonly Color Success = Color.FromArgb(16, 185, 129);
        private static readonly Color Danger = Color.FromArgb(239, 68, 68);
        private static readonly Color Warning = Color.FromArgb(245, 158, 11);
        private static readonly Color CardBg = Color.White;
        private static readonly Color CardBgDark = Color.FromArgb(40, 40, 40);

        public MainForm()
        {
            _settings = AppSettings.Load();
            _vpnService = new VpnService(_settings);
            _megaService = new MegaService(_settings);
            _lastVpnStatus = new VpnStatus { IsConnected = false };

            InitializeComponent();
            InitializeTrayIcon();
            ApplyTheme();

            _refreshTimer = new Timer { Interval = _settings.StatusRefreshIntervalSeconds * 1000 };
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
                _settings.ThemeMode = ThemeUtils.IsSystemDarkMode() ? ThemeMode.Dark : ThemeMode.Light;

            var isDark = _settings.ThemeMode == ThemeMode.Dark;
            var bg = isDark ? Color.FromArgb(24, 24, 27) : Color.FromArgb(248, 250, 252);
            var cardBg = isDark ? CardBgDark : CardBg;
            var fg = isDark ? Color.FromArgb(228, 228, 231) : Color.FromArgb(24, 24, 27);
            var fgSubtle = isDark ? Color.FromArgb(161, 161, 170) : Color.FromArgb(113, 113, 122);

            BackColor = bg;
            ForeColor = fg;

            _mainLayout.BackColor = bg;
            _headerPanel.BackColor = bg;
            _contentPanel.BackColor = bg;

            _lblAppTitle.ForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
            _lblSubtitle.ForeColor = fgSubtle;

            _cardVpn.BackColor = cardBg;
            _cardMega.BackColor = cardBg;
            _cardControls.BackColor = cardBg;
            _lblCycleInfo.ForeColor = fgSubtle;

            // Style cards with subtle border
            foreach (var card in new[] { _cardVpn, _cardMega, _cardControls })
            {
                card.BackColor = cardBg;
            }

            _lblVersion.ForeColor = fgSubtle;
            _lnkAbout.LinkColor = isDark ? Color.FromArgb(96, 165, 250) : Accent;

            _footerPanel.BackColor = isDark ? Color.FromArgb(30, 30, 33) : Color.FromArgb(241, 245, 249);
            _statusStrip.BackColor = isDark ? Color.FromArgb(30, 30, 33) : Color.FromArgb(241, 245, 249);
            _statusStrip.ForeColor = fgSubtle;
        }

        public void ReapplyTheme()
        {
            ApplyTheme();
            foreach (Control c in this.Controls)
                ThemeUtils.ApplyThemeToControl(c, _settings.ThemeMode);
        }
        #endregion

        #region Initialization
        private void InitializeComponent()
        {
            Text = "Mega Download Enhancer";
            Size = new Size(720, 520);
            MinimumSize = new Size(640, 480);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            Icon = SystemIcons.Application;

            // ── Main layout ──
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            Controls.Add(_mainLayout);

            // ── Header ──
            _headerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 4) };
            _mainLayout.Controls.Add(_headerPanel, 0, 0);

            _lblAppTitle = new Label
            {
                Text = "Mega Download Enhancer",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 12)
            };

            _lblSubtitle = new Label
            {
                Text = "VPN Auto-Cycler & MEGAsync Companion",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(24, 42),
                ForeColor = Color.FromArgb(113, 113, 122)
            };
            _headerPanel.Controls.Add(_lblAppTitle);
            _headerPanel.Controls.Add(_lblSubtitle);

            // ── Content area ──
            _contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 8, 24, 8) };
            _mainLayout.Controls.Add(_contentPanel, 0, 1);

            // Status cards
            _cardVpn = CreateCard("VPN", "Disconnected", "Connection: —", 0, 0, ref _dotVpn, ref _lblVpnLabel, ref _lblVpnValue, ref _lblVpnDetail);
            _cardMega = CreateCard("MEGAsync", "Not Running", "Process: —", 248, 0, ref _dotMega, ref _lblMegaLabel, ref _lblMegaValue, ref _lblMegaDetail);
            _contentPanel.Controls.Add(_cardVpn);
            _contentPanel.Controls.Add(_cardMega);

            // Controls card
            _cardControls = new Panel
            {
                Location = new Point(0, 148),
                Size = new Size(490, 60),
                BackColor = CardBg,
                Padding = new Padding(16, 12, 16, 12)
            };
            _contentPanel.Controls.Add(_cardControls);

            _btnToggle = new Button
            {
                Text = "Start Cycle",
                Size = new Size(140, 36),
                Location = new Point(16, 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Success,
                ForeColor = Color.White
            };
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.Click += BtnToggleCycle_Click;

            _btnSettings = new Button
            {
                Text = "Settings",
                Size = new Size(90, 36),
                Location = new Point(168, 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White
            };
            _btnSettings.FlatAppearance.BorderSize = 0;
            _btnSettings.Click += BtnSettings_Click;

            _btnRefresh = new Button
            {
                Text = "⟳ Refresh",
                Size = new Size(100, 36),
                Location = new Point(270, 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F),
                BackColor = Accent,
                ForeColor = Color.White
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += BtnRefresh_Click;

            _cardControls.Controls.Add(_btnToggle);
            _cardControls.Controls.Add(_btnSettings);
            _cardControls.Controls.Add(_btnRefresh);

            // Cycle info
            _lblCycleInfo = new Label
            {
                Text = "Cycle: Not Running",
                AutoSize = true,
                Location = new Point(0, 216),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(113, 113, 122)
            };
            _contentPanel.Controls.Add(_lblCycleInfo);

            // Version + About
            _lblVersion = new Label
            {
                Text = $"v{Application.ProductVersion}",
                AutoSize = true,
                Location = new Point(0, 240),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(161, 161, 170)
            };
            _contentPanel.Controls.Add(_lblVersion);

            _lnkAbout = new LinkLabel
            {
                Text = "About",
                AutoSize = true,
                Location = new Point(70, 240),
                Font = new Font("Segoe UI", 8F),
                LinkColor = Accent
            };
            _lnkAbout.Click += (s, e) => new AboutDialog().ShowDialog(this);
            _contentPanel.Controls.Add(_lnkAbout);

            // Anchor content for resizing
            _cardVpn.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _cardMega.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _cardControls.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _lblCycleInfo.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            _lnkAbout.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            // ── Footer ──
            _footerPanel = new Panel { Dock = DockStyle.Fill };
            _mainLayout.Controls.Add(_footerPanel, 0, 2);

            _statusStrip = new StatusStrip { Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden };
            _lblStatus = new ToolStripStatusLabel { Text = "Ready", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _lblFooterRight = new ToolStripStatusLabel { Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), TextAlign = ContentAlignment.MiddleRight };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblFooterRight });
            _footerPanel.Controls.Add(_statusStrip);
        }

        private Panel CreateCard(string label, string value, string detail, int x, int y,
            ref Panel dot, ref Label labelCtrl, ref Label valueCtrl, ref Label detailCtrl)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(236, 136),
                BackColor = CardBg,
                Padding = new Padding(16, 14, 16, 14)
            };

            // Colored top bar
            var bar = new Panel
            {
                Size = new Size(236, 3),
                Location = new Point(0, 0),
                BackColor = Accent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            card.Controls.Add(bar);

            // Status dot
            dot = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(16, 20),
                BackColor = Danger
            };

            labelCtrl = new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(32, 18),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(113, 113, 122)
            };

            valueCtrl = new Label
            {
                Text = value,
                AutoSize = true,
                Location = new Point(16, 56),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Danger
            };

            detailCtrl = new Label
            {
                Text = detail,
                AutoSize = true,
                Location = new Point(16, 100),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(161, 161, 170)
            };

            card.Controls.Add(dot);
            card.Controls.Add(labelCtrl);
            card.Controls.Add(valueCtrl);
            card.Controls.Add(detailCtrl);
            return card;
        }

        private void InitializeTrayIcon()
        {
            _trayContextMenu = new ContextMenuStrip();
            _trayContextMenu.Items.Add("Show", null, TrayShow_Click);
            _trayContextMenu.Items.Add("Start VPN", null, TrayStart_Click);
            _trayContextMenu.Items.Add("Stop VPN", null, TrayStop_Click);
            _trayContextMenu.Items.Add(new ToolStripSeparator());
            _trayContextMenu.Items.Add("Auto-start with Windows", null, TrayAutoStart_Click);
            _trayContextMenu.Items.Add(new ToolStripSeparator());
            _trayContextMenu.Items.Add("About", null, (s, e) => new AboutDialog().ShowDialog(this));
            _trayContextMenu.Items.Add("Exit", null, TrayExit_Click);

            _trayIcon = new NotifyIcon
            {
                Text = "Mega Download Enhancer",
                Icon = SystemIcons.Application,
                ContextMenuStrip = _trayContextMenu,
                Visible = true
            };

            _trayIcon.DoubleClick += (s, e) =>
            {
                _exiting = false;
                Show();
                WindowState = FormWindowState.Normal;
            };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (_settings.StartMinimized)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
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
                Hide();
                if (_vpnService.IsRunning)
                    _trayIcon.ShowBalloonTip(3000, "Mega Download Enhancer", "VPN cycle still running in background", ToolTipIcon.Info);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized) Hide();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Stretch controls card to fill available width
            if (_cardControls != null && _contentPanel != null)
            {
                _cardControls.Width = _contentPanel.ClientSize.Width - 24;
            }
        }
        #endregion

        #region Shutdown
        public void Shutdown()
        {
            _exiting = true;
            _refreshTimer.Stop();
            _trayIcon.Visible = false;
            _vpnService.StopVpnCycle();
            _vpnService.Dispose();
            Close();
        }
        #endregion

        #region Refresh
        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            await UpdateVpnStatusAsync();
            UpdateMegaStatus();
            _lblFooterRight.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        #region Status Updates
        private async Task UpdateVpnStatusAsync()
        {
            try
            {
                var status = await _vpnService.GetCurrentStatusAsync(_settings.VpnName);
                _lblVpnValue.Text = status.IsConnected ? "Connected" : "Disconnected";
                _lblVpnValue.ForeColor = status.IsConnected ? Success : Danger;
                _dotVpn.BackColor = status.IsConnected ? Success : Danger;
                _lblVpnDetail.Text = !string.IsNullOrEmpty(_settings.VpnName)
                    ? $"Connection: {_settings.VpnName}  •  {DateTime.Now:HH:mm:ss}"
                    : "Connection: Not Configured";

                if (_vpnService.IsRunning && _lastVpnStatus.IsConnected != status.IsConnected)
                {
                    var state = status.IsConnected ? "connected" : "disconnected";
                    _trayIcon.ShowBalloonTip(3000, "Mega Download Enhancer", $"VPN {state}", ToolTipIcon.Info);
                }
                _lastVpnStatus = status;
            }
            catch (Exception ex)
            {
                _lblVpnValue.Text = "Error";
                _lblVpnValue.ForeColor = Warning;
                _dotVpn.BackColor = Warning;
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateMegaStatus()
        {
            try
            {
                var status = _megaService.GetCurrentStatus();
                if (status.IsRunning)
                {
                    _lblMegaValue.Text = status.IsSyncing ? "Syncing" : "Running";
                    _lblMegaValue.ForeColor = status.IsSyncing ? Accent : Success;
                    _dotMega.BackColor = status.IsSyncing ? Accent : Success;
                    _lblMegaDetail.Text = $"{_settings.MonitoredProcessDisplayName} (PID: {status.ProcessId})  •  {DateTime.Now:HH:mm:ss}";
                }
                else
                {
                    _lblMegaValue.Text = "Not Running";
                    _lblMegaValue.ForeColor = Danger;
                    _dotMega.BackColor = Danger;
                    _lblMegaDetail.Text = $"{_settings.MonitoredProcessDisplayName} — Not Running  •  {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                _lblMegaValue.Text = "Error";
                _lblMegaValue.ForeColor = Warning;
                _dotMega.BackColor = Warning;
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateButtonStates()
        {
            var isRunning = _vpnService.IsRunning;
            _btnToggle.Enabled = !string.IsNullOrEmpty(_settings.VpnName);
            _btnToggle.Text = isRunning ? "■ Stop Cycle" : "▶ Start Cycle";
            _btnToggle.BackColor = isRunning ? Danger : Success;

            if (_cycleStartTime.HasValue && isRunning)
            {
                _lblCycleInfo.Text = $"Cycle active since {_cycleStartTime.Value:HH:mm:ss}  •  Toggle every {_settings.CycleDurationMinutes} min";
                _lblCycleInfo.ForeColor = Success;
            }
            else
            {
                _lblCycleInfo.Text = "Cycle: Not Running";
                _lblCycleInfo.ForeColor = Color.FromArgb(113, 113, 122);
            }
        }
        #endregion

        #region Button Handlers
        private async void BtnToggleCycle_Click(object? sender, EventArgs e)
        {
            if (_toggling) return;
            _toggling = true;

            try
            {
                if (_vpnService.IsRunning)
                {
                    _lblStatus.Text = "Stopping VPN cycle...";
                    _vpnService.StopVpnCycle();
                    _cycleStartTime = null;
                    _lblStatus.Text = "Cycle stopped";
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
                            $"{validationError}\n\nOpen Settings now?",
                            "Configuration Required",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        if (result == DialogResult.Yes) ShowSettings();
                        return;
                    }

                    _lblStatus.Text = "Starting VPN cycle...";
                    _vpnService.StartVpnCycle(_settings);
                    _cycleStartTime = DateTime.Now;
                    _lblStatus.Text = "Cycle running";
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
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Operation failed";
            }
            finally { _toggling = false; }
        }

        private void BtnSettings_Click(object? sender, EventArgs e) => ShowSettings();
        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            BeginInvoke(new Action(async () => await UpdateVpnStatusAsync()));
            UpdateMegaStatus();
            _lblStatus.Text = "Refreshed";
        }

        private void ShowSettings()
        {
            var f = new SettingsForm(_settings);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                _settings.Save();
                _refreshTimer.Interval = _settings.StatusRefreshIntervalSeconds * 1000;
                ApplyAutoStart();
                BeginInvoke(new Action(() => ReapplyTheme()));
                BeginInvoke(new Action(async () => await UpdateVpnStatusAsync()));
                UpdateButtonStates();
                _lblStatus.Text = "Settings saved";
            }
        }
        #endregion

        #region Tray
        private void TrayShow_Click(object? s, EventArgs e) { _exiting = false; Show(); WindowState = FormWindowState.Normal; }
        private void TrayStart_Click(object? s, EventArgs e) { if (!_vpnService.IsRunning) BtnToggleCycle_Click(s, e); }
        private void TrayStop_Click(object? s, EventArgs e) { if (_vpnService.IsRunning) BtnToggleCycle_Click(s, e); }
        private void TrayAutoStart_Click(object? s, EventArgs e)
        {
            _settings.AutoStart = !_settings.AutoStart;
            _settings.Save();
            ApplyAutoStart();
            UpdateTrayAutoStartCheck();
            _trayIcon.ShowBalloonTip(2000, "Mega Download Enhancer",
                _settings.AutoStart ? "Will auto-start with Windows" : "Auto-start disabled", ToolTipIcon.Info);
        }
        private void TrayExit_Click(object? s, EventArgs e) => Shutdown();
        #endregion

        #region Auto-start
        private void ApplyAutoStart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (_settings.AutoStart)
                    key?.SetValue("MegaDownloadEnhancer", $"\"{Application.ExecutablePath}\"");
                else
                    key?.DeleteValue("MegaDownloadEnhancer", false);
            }
            catch { }
        }

        private void UpdateTrayAutoStartCheck()
        {
            foreach (ToolStripItem item in _trayContextMenu.Items)
            {
                var t = item.Text ?? "";
                if (t.StartsWith("Auto-start") || t.StartsWith("✓ Auto-start") || t.StartsWith("  Auto-start"))
                {
                    item.Text = _settings.AutoStart ? "✓ Auto-start with Windows" : "  Auto-start with Windows";
                    break;
                }
            }
        }
        #endregion

        #region Validation & First-time
        private string? ValidateSettings()
        {
            if (string.IsNullOrEmpty(_settings.VpnName))
                return "VPN name is not configured.\nPlease select a VPN connection in Settings.";
            if (!_vpnService.IsVpnAvailable(_settings.VpnName))
                return $"VPN connection '{_settings.VpnName}' is not available.\n\nFor WARP: Ensure 'warp-cli.exe' is in your PATH.\nFor Windows VPN: Check your Windows VPN settings.";
            return null;
        }

        private void ShowFirstTimeSetup()
        {
            var result = MessageBox.Show(
                "Welcome to Mega Download Enhancer!\n\n" +
                "Configure your VPN connection to get started:\n" +
                "• VPN connection (CloudflareWARP or Windows VPN)\n" +
                "• Cycle duration & refresh interval\n" +
                "• MEGAsync process monitoring\n\n" +
                "Open Settings now?",
                "First-Time Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes) ShowSettings();
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Dispose();
                _trayIcon?.Dispose();
                _trayContextMenu?.Dispose();
                _vpnService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
