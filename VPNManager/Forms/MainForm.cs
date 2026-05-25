using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using VPNManager.Models;
using VPNManager.Services;

namespace VPNManager.Forms
{
    public partial class MainForm : Form
    {
        // ── Services ─────────────────────────────────────────────
        private readonly AppSettings _settings;
        private readonly VpnService _vpnService;
        private readonly MegaService _megaService;

        // ── Timers ───────────────────────────────────────────────
        private readonly Timer _refreshTimer;
        private readonly Timer _pulseTimer;
        private readonly Timer _clockTimer;

        // ── UI State ─────────────────────────────────────────────
        private bool _pulseOn;
        private DateTime? _cycleStartTime;
        private bool _toggling;
        private VpnStatus _lastVpnStatus = new();
        private bool _vpnConnected;
        private bool _megaRunning;
        private bool _megaSyncing;

        // ── Controls ─────────────────────────────────────────────
        private Panel _headerPanel = null!;
        private Panel _vpnCard = null!;
        private Panel _megaCard = null!;
        private Panel _cycleCard = null!;
        private Panel _footerPanel = null!;
        private Button _btnToggle = null!;
        private Button _btnSettings = null!;
        private Button _btnRefresh = null!;
        private Label _lblFooterLeft = null!;
        private Label _lblFooterRight = null!;

        // ── Tray ─────────────────────────────────────────────────
        private NotifyIcon _trayIcon = null!;
        private ContextMenuStrip _trayMenu = null!;
        private bool _exiting;

        // ── Palette ──────────────────────────────────────────────
        static readonly Color C_BG = Color.FromArgb(7, 11, 18);
        static readonly Color C_SURFACE = Color.FromArgb(13, 20, 32);
        static readonly Color C_SURF2 = Color.FromArgb(18, 27, 44);
        static readonly Color C_BORDER = Color.FromArgb(28, 40, 68);
        static readonly Color C_ACCENT = Color.FromArgb(0, 207, 168);
        static readonly Color C_BLUE = Color.FromArgb(59, 130, 246);
        static readonly Color C_DANGER = Color.FromArgb(255, 58, 90);
        static readonly Color C_TEXT1 = Color.FromArgb(226, 235, 248);
        static readonly Color C_TEXT2 = Color.FromArgb(80, 105, 145);
        static readonly Color C_DIM = Color.FromArgb(24, 36, 58);

        public MainForm()
        {
            _settings = AppSettings.Load();
            _vpnService = new VpnService(_settings);
            _megaService = new MegaService(_settings);

            InitializeComponent();
            InitializeTrayIcon();

            _refreshTimer = new Timer { Interval = Math.Max(1000, _settings.StatusRefreshIntervalSeconds * 1000) };
            _refreshTimer.Tick += async (s, e) => { await UpdateVpnStatusAsync(); UpdateMegaStatus(); };

            _pulseTimer = new Timer { Interval = 650 };
            _pulseTimer.Tick += (s, e) =>
            {
                _pulseOn = !_pulseOn;
                _vpnCard?.Invalidate();
                _megaCard?.Invalidate();
            };

            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) =>
            {
                if (_lblFooterRight != null)
                    _lblFooterRight.Text = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");
                _cycleCard?.Invalidate();
            };

            // When PS script exits unexpectedly, reset UI to idle state
            _vpnService.StatusChanged += (s, status) =>
            {
                if (status.CurrentState == "Stopped")
                {
                    if (InvokeRequired)
                        BeginInvoke(() => { _cycleStartTime = null; UpdateButtonStates(); SetStatus("Cycle stopped"); });
                    else
                    { _cycleStartTime = null; UpdateButtonStates(); SetStatus("Cycle stopped"); }
                }
            };

            ApplyAutoStart();
            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
            Resize += (s, e) => { if (WindowState == FormWindowState.Minimized) Hide(); };
        }

        // ── Init ─────────────────────────────────────────────────

        private void InitializeComponent()
        {
            Icon = AppIcon.Create(32);
            Text = "Mega Download Enhancer";
            ClientSize = new Size(500, 560);
            MinimumSize = new Size(460, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = C_BG;
            ForeColor = C_TEXT1;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // ── Header ──────────────────────────────────────────
            _headerPanel = DB(new Panel { Dock = DockStyle.Top, Height = 72, BackColor = C_SURFACE });
            _headerPanel.Paint += PaintHeader;

            // ── Footer ──────────────────────────────────────────
            _footerPanel = DB(new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = C_SURFACE });
            _footerPanel.Paint += (s, e) =>
            {
                using var p = new Pen(C_BORDER, 1f);
                e.Graphics.DrawLine(p, 0, 0, _footerPanel.Width, 0);
            };

            _lblFooterLeft = new Label
            {
                Text = "Ready",
                AutoSize = true,
                Location = new Point(16, 8),
                Font = new Font("Segoe UI", 8F),
                ForeColor = C_TEXT2,
                BackColor = Color.Transparent
            };

            var ver = Application.ProductVersion;
            var cleanVer = ver.Contains('+') ? ver[..ver.IndexOf('+')] : ver;

            _lblFooterRight = new Label
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"),
                AutoSize = true,
                Location = new Point(290, 8),
                Font = new Font("Consolas", 7.5F),
                ForeColor = C_TEXT2,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var lblVer = new Label
            {
                Text = $"v{cleanVer}",
                AutoSize = true,
                Location = new Point(16, 8),
                Font = new Font("Consolas", 7.5F),
                ForeColor = C_TEXT2,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _footerPanel.Controls.Add(_lblFooterLeft);
            _footerPanel.Controls.Add(_lblFooterRight);
            _footerPanel.Controls.Add(lblVer);

            // ── Content ─────────────────────────────────────────
            var content = DB(new Panel { Dock = DockStyle.Fill, BackColor = C_BG });

            // VPN card
            _vpnCard = DB(new Panel { BackColor = Color.Transparent });
            _vpnCard.Paint += PaintVpnCard;

            // MEGA card
            _megaCard = DB(new Panel { BackColor = Color.Transparent });
            _megaCard.Paint += PaintMegaCard;

            // Cycle card
            _cycleCard = DB(new Panel { BackColor = Color.Transparent });
            _cycleCard.Paint += PaintCycleCard;
            _cycleCard.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // Buttons
            _btnToggle = MakeBtn("▶  Start Cycle", C_ACCENT, Color.FromArgb(7, 11, 18));
            _btnToggle.Size = new Size(148, 38);
            _btnToggle.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnToggle.Click += BtnToggle_Click;

            _btnSettings = MakeBtn("⚙  Settings", C_SURF2, C_TEXT1);
            _btnSettings.Size = new Size(102, 38);
            _btnSettings.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnSettings.Click += (s, e) => ShowSettings();

            _btnRefresh = MakeBtn("⟳  Refresh", C_SURF2, C_TEXT1);
            _btnRefresh.Size = new Size(102, 38);
            _btnRefresh.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnRefresh.Click += BtnRefresh_Click;

            content.Controls.Add(_vpnCard);
            content.Controls.Add(_megaCard);
            content.Controls.Add(_cycleCard);
            content.Controls.Add(_btnToggle);
            content.Controls.Add(_btnSettings);
            content.Controls.Add(_btnRefresh);

            Controls.Add(content);
            Controls.Add(_footerPanel);
            Controls.Add(_headerPanel);

            RepositionCards();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionCards();
        }

        private void RepositionCards()
        {
            if (_vpnCard == null) return;
            int cw = ClientSize.Width;
            const int margin = 16, gap = 10;
            int cardW = (cw - 2 * margin - gap) / 2;
            int cardH = 116;

            _vpnCard.SetBounds(margin, 16, cardW, cardH);
            _megaCard.SetBounds(margin + cardW + gap, 16, cardW, cardH);
            _cycleCard.SetBounds(margin, 148, cw - 2 * margin, 88);

            int btnY = 254;
            _btnToggle.Location = new Point(margin, btnY);
            _btnSettings.Location = new Point(margin + 148 + 8, btnY);
            _btnRefresh.Location = new Point(margin + 148 + 8 + 102 + 8, btnY);

            _lblFooterRight.Left = cw - _lblFooterRight.Width - 16;
        }

        private void InitializeTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();
            StyleContextMenu(_trayMenu);
            Add(_trayMenu, "Show", (s, e) => ShowWindow());
            Add(_trayMenu, "▶  Start Cycle", (s, e) => { if (!_vpnService.IsRunning) BtnToggle_Click(s, e); });
            Add(_trayMenu, "■  Stop Cycle", (s, e) => { if (_vpnService.IsRunning) BtnToggle_Click(s, e); });
            _trayMenu.Items.Add(new ToolStripSeparator());
            Add(_trayMenu, "    Auto-start with Windows", ToggleAutoStart);
            _trayMenu.Items.Add(new ToolStripSeparator());
            Add(_trayMenu, "Exit", (s, e) => Shutdown());

            _trayIcon = new NotifyIcon
            {
                Text = "Mega Download Enhancer",
                Icon = AppIcon.Create(16),
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            _trayIcon.DoubleClick += (s, e) => ShowWindow();
        }

        // ── Load & Close ─────────────────────────────────────────

        private void MainForm_Load(object? sender, EventArgs e)
        {
            if (_settings.StartMinimized) { WindowState = FormWindowState.Minimized; Hide(); }
            _refreshTimer.Start();
            _pulseTimer.Start();
            _clockTimer.Start();
            UpdateButtonStates();
            UpdateTrayAutoStartCheck();

            if (string.IsNullOrEmpty(_settings.VpnName))
                BeginInvoke(new Action(() => ShowFirstTimeSetup()));

            BeginInvoke(new Action(async () =>
            {
                await UpdateVpnStatusAsync();
                UpdateMegaStatus();
            }));
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_exiting)
            {
                e.Cancel = true;
                Hide();
                if (_vpnService.IsRunning)
                    _trayIcon.ShowBalloonTip(3000, "Mega Download Enhancer", "Still cycling in background", ToolTipIcon.Info);
            }
        }

        // ── Painting ─────────────────────────────────────────────

        private void PaintHeader(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = _headerPanel.Width, h = _headerPanel.Height;
            using var bg = new LinearGradientBrush(
                new Point(0, 0), new Point(w, h),
                C_SURFACE, Color.FromArgb(10, 16, 28));
            g.FillRectangle(bg, 0, 0, w, h);

            // Bottom separator
            using var sep = new Pen(C_BORDER, 1f);
            g.DrawLine(sep, 0, h - 1, w, h - 1);

            // Tiny accent bar at top
            using var accentBar = new LinearGradientBrush(
                new Point(0, 0), new Point(w, 0),
                C_ACCENT, Color.FromArgb(0, C_ACCENT));
            g.FillRectangle(accentBar, 0, 0, w, 2);

            // Logo icon
            DrawBolt(g, 16, 18, 36);

            // Title
            using var titleFont = new Font("Segoe UI", 15F, FontStyle.Bold);
            using var titleBrush = new SolidBrush(C_TEXT1);
            g.DrawString("Mega Download Enhancer", titleFont, titleBrush, 62, 10);

            // Subtitle
            using var subFont = new Font("Segoe UI", 8.5F);
            using var subBrush = new SolidBrush(C_TEXT2);
            g.DrawString("VPN Auto-Cycler  ·  MEGAsync Companion", subFont, subBrush, 64, 40);
        }

        private void DrawBolt(Graphics g, int x, int y, int sz)
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(255, 8, 12, 20));
            using var path = AppIcon.RoundedRect(new Rectangle(x, y, sz, sz), sz / 5);
            g.FillPath(bgBrush, path);

            using var borderPen = new Pen(Color.FromArgb(60, 0, 207, 168), 1f);
            g.DrawPath(borderPen, path);

            float s = sz;
            PointF[] bolt =
            {
                new(x + s * 0.58f, y + s * 0.07f),
                new(x + s * 0.27f, y + s * 0.51f),
                new(x + s * 0.50f, y + s * 0.51f),
                new(x + s * 0.42f, y + s * 0.93f),
                new(x + s * 0.73f, y + s * 0.49f),
                new(x + s * 0.50f, y + s * 0.49f),
            };
            using var boltBrush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + sz),
                C_ACCENT, Color.FromArgb(0, 148, 120));
            g.FillPolygon(boltBrush, bolt);
        }

        private void PaintVpnCard(object? sender, PaintEventArgs e)
        {
            var statusColor = _vpnConnected ? C_ACCENT : C_DANGER;
            PaintCard(e.Graphics, _vpnCard.ClientRectangle,
                "VPN",
                _vpnConnected ? "Connected" : "Offline",
                string.IsNullOrEmpty(_settings.VpnName) ? "Not configured" : _settings.VpnName,
                statusColor,
                _vpnConnected && _vpnService.IsRunning);
        }

        private void PaintMegaCard(object? sender, PaintEventArgs e)
        {
            var color = _megaSyncing ? C_BLUE : _megaRunning ? C_ACCENT : C_DANGER;
            PaintCard(e.Graphics, _megaCard.ClientRectangle,
                "MEGAsync",
                _megaSyncing ? "Syncing" : _megaRunning ? "Running" : "Offline",
                _settings.MonitoredProcessDisplayName,
                color,
                _megaRunning || _megaSyncing);
        }

        private void PaintCard(Graphics g, Rectangle b, string label,
            string status, string detail, Color accent, bool animated)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Card background
            using var bgBrush = new SolidBrush(C_SURFACE);
            using var cardPath = AppIcon.RoundedRect(b, 6);
            g.FillPath(bgBrush, cardPath);

            // Border
            using var borderPen = new Pen(C_BORDER, 1f);
            g.DrawPath(borderPen, cardPath);

            // Top accent strip
            using var stripBrush = new SolidBrush(accent);
            g.FillRectangle(stripBrush, b.X + 6, b.Y, b.Width - 12, 3);

            // Label
            using var lblFont = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            using var lblBrush = new SolidBrush(C_TEXT2);
            g.DrawString(label.ToUpper(), lblFont, lblBrush, b.X + 14, b.Y + 14);

            // Pulse dot
            bool dotVisible = !animated || _pulseOn;
            if (dotVisible)
            {
                int dotSz = 8, dotX = b.Right - 22, dotY = b.Y + 15;
                using var glowBrush = new SolidBrush(Color.FromArgb(35, accent));
                g.FillEllipse(glowBrush, dotX - 4, dotY - 4, dotSz + 8, dotSz + 8);
                using var dotBrush = new SolidBrush(accent);
                g.FillEllipse(dotBrush, dotX, dotY, dotSz, dotSz);
            }

            // Status value
            float fSize = status.Length > 9 ? 16F : 20F;
            using var valFont = new Font("Segoe UI", fSize, FontStyle.Bold);
            using var valBrush = new SolidBrush(accent);
            g.DrawString(status, valFont, valBrush, b.X + 14, b.Y + 36);

            // Detail
            using var detFont = new Font("Consolas", 7.5F);
            using var detBrush = new SolidBrush(C_TEXT2);
            var detStr = detail.Length > 20 ? detail[..20] + "…" : detail;
            g.DrawString(detStr, detFont, detBrush, b.X + 14, b.Y + 92);
        }

        private void PaintCycleCard(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var b = _cycleCard.ClientRectangle;
            bool running = _vpnService.IsRunning;
            Color accent = running ? C_ACCENT : C_DIM;

            using var bgBrush = new SolidBrush(C_SURFACE);
            using var cardPath = AppIcon.RoundedRect(b, 6);
            g.FillPath(bgBrush, cardPath);
            using var borderPen = new Pen(C_BORDER, 1f);
            g.DrawPath(borderPen, cardPath);

            // Top strip
            using var stripBrush = new SolidBrush(accent);
            g.FillRectangle(stripBrush, b.X + 6, b.Y, b.Width - 12, 3);

            // CYCLE label
            using var lblFont = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            using var lblBrush = new SolidBrush(C_TEXT2);
            g.DrawString("CYCLE", lblFont, lblBrush, b.X + 14, b.Y + 13);

            // State badge
            using var stFont = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            using var stBrush = new SolidBrush(running ? C_ACCENT : C_TEXT2);
            g.DrawString(running ? "● ACTIVE" : "○ IDLE", stFont, stBrush, b.X + 60, b.Y + 13);

            // Progress bar track
            int barX = b.X + 14, barY = b.Y + 34, barW = b.Width - 80, barH = 5;
            using var trackBrush = new SolidBrush(C_DIM);
            using var trackPath = AppIcon.RoundedRect(new Rectangle(barX, barY, barW, barH), 2);
            g.FillPath(trackBrush, trackPath);

            // Progress fill + countdown
            float progress = 0f;
            string timeLabel = "—";
            if (running && _cycleStartTime.HasValue)
            {
                double totalSec = _settings.CycleDurationMinutes * 60.0;
                double elapsed = (DateTime.Now - _cycleStartTime.Value).TotalSeconds % totalSec;
                progress = (float)(elapsed / totalSec);
                double remain = totalSec - elapsed;
                timeLabel = remain < 60 ? $"{(int)remain}s" : $"{(int)(remain / 60)}m {(int)(remain % 60):D2}s";
            }

            int fillW = Math.Max(0, (int)(barW * progress));
            if (fillW > 4)
            {
                using var fillBrush = new LinearGradientBrush(
                    new Point(barX, barY), new Point(barX + barW, barY),
                    C_ACCENT, Color.FromArgb(0, 160, 130));
                using var fillPath = AppIcon.RoundedRect(new Rectangle(barX, barY, fillW, barH), 2);
                g.FillPath(fillBrush, fillPath);
            }

            // Countdown label
            using var timeFont = new Font("Consolas", 8F);
            using var timeBrush = new SolidBrush(running ? C_ACCENT : C_TEXT2);
            g.DrawString(timeLabel, timeFont, timeBrush, barX + barW + 8, barY - 4);

            // Info line
            using var infoFont = new Font("Segoe UI", 8F);
            using var infoBrush = new SolidBrush(C_TEXT2);
            string infoText = running && _cycleStartTime.HasValue
                ? $"Active since {_cycleStartTime.Value:HH:mm:ss}  ·  Toggle every {_settings.CycleDurationMinutes} min"
                : "Click  ▶  to start cycling your IP for uninterrupted MEGA downloads";
            g.DrawString(infoText, infoFont, infoBrush, b.X + 14, b.Y + 52);
        }

        // ── Status Updates ───────────────────────────────────────

        private async Task UpdateVpnStatusAsync()
        {
            try
            {
                var status = await _vpnService.GetCurrentStatusAsync(_settings.VpnName);
                bool was = _vpnConnected;
                _vpnConnected = status.IsConnected;
                _lastVpnStatus = status;
                _vpnCard?.Invalidate();

                if (_vpnService.IsRunning && was != _vpnConnected)
                    _trayIcon?.ShowBalloonTip(2000, "Mega Download Enhancer",
                        _vpnConnected ? "VPN connected" : "VPN disconnected", ToolTipIcon.Info);

                SetStatus(_vpnConnected ? "VPN connected" : "VPN offline");
            }
            catch (Exception ex) { SetStatus($"Error: {ex.Message}"); }
        }

        private void UpdateMegaStatus()
        {
            try
            {
                var s = _megaService.GetCurrentStatus();
                _megaRunning = s.IsRunning;
                _megaSyncing = s.IsSyncing;
                _megaCard?.Invalidate();
            }
            catch { }
        }

        private void UpdateButtonStates()
        {
            bool running = _vpnService.IsRunning;
            _btnToggle.Text = running ? "■  Stop Cycle" : "▶  Start Cycle";
            _btnToggle.BackColor = running ? C_DANGER : C_ACCENT;
            _btnToggle.ForeColor = running ? C_TEXT1 : Color.FromArgb(7, 11, 18);
            _cycleCard?.Invalidate();
        }

        private void SetStatus(string msg)
        {
            if (_lblFooterLeft == null) return;
            if (_lblFooterLeft.InvokeRequired)
                _lblFooterLeft.BeginInvoke(() => _lblFooterLeft.Text = msg);
            else
                _lblFooterLeft.Text = msg;
        }

        // ── Button Handlers ──────────────────────────────────────

        private async void BtnToggle_Click(object? sender, EventArgs e)
        {
            if (_toggling) return;
            _toggling = true;
            try
            {
                if (_vpnService.IsRunning)
                {
                    SetStatus("Stopping cycle…");
                    await Task.Run(() => _vpnService.StopVpnCycle());
                    _cycleStartTime = null;
                    SetStatus("Cycle stopped");
                    UpdateButtonStates();
                    await UpdateVpnStatusAsync();
                    UpdateMegaStatus();
                }
                else
                {
                    var err = ValidateSettings();
                    if (err != null)
                    {
                        var r = MessageBox.Show($"{err}\n\nOpen Settings?",
                            "Configuration Required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (r == DialogResult.Yes) ShowSettings();
                        return;
                    }
                    SetStatus("Starting cycle…");
                    _vpnService.StartVpnCycle(_settings);
                    _cycleStartTime = DateTime.Now;
                    SetStatus("Cycle running");
                    UpdateButtonStates();
                    await UpdateVpnStatusAsync();
                    UpdateMegaStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error");
            }
            finally { _toggling = false; }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            BeginInvoke(new Action(async () =>
            {
                await UpdateVpnStatusAsync();
                UpdateMegaStatus();
                SetStatus("Refreshed");
            }));
        }

        private void ShowSettings()
        {
            var f = new SettingsForm(_settings);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                _settings.Save();
                _refreshTimer.Interval = Math.Max(1000, _settings.StatusRefreshIntervalSeconds * 1000);
                ApplyAutoStart();
                UpdateButtonStates();
                BeginInvoke(new Action(async () =>
                {
                    await UpdateVpnStatusAsync();
                    UpdateMegaStatus();
                }));
                SetStatus("Settings saved");
            }
        }

        // ── Tray ─────────────────────────────────────────────────

        private void ShowWindow()
        {
            _exiting = false;
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void ToggleAutoStart(object? sender, EventArgs e)
        {
            _settings.AutoStart = !_settings.AutoStart;
            _settings.Save();
            ApplyAutoStart();
            UpdateTrayAutoStartCheck();
            _trayIcon.ShowBalloonTip(2000, "Mega Download Enhancer",
                _settings.AutoStart ? "Will auto-start with Windows" : "Auto-start disabled", ToolTipIcon.Info);
        }

        private void UpdateTrayAutoStartCheck()
        {
            foreach (ToolStripItem item in _trayMenu.Items)
            {
                if (item.Text?.Contains("Auto-start") == true)
                {
                    item.Text = _settings.AutoStart
                        ? "✓  Auto-start with Windows"
                        : "    Auto-start with Windows";
                    break;
                }
            }
        }

        private void ApplyAutoStart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[AutoStart] Failed to open HKCU Run key — key is null");
                    return;
                }

                if (_settings.AutoStart)
                {
                    var path = $"\"{Application.ExecutablePath}\"";
                    key.SetValue("MegaDownloadEnhancer", path);
                    System.Diagnostics.Debug.WriteLine(
                        $"[AutoStart] Registered: {path}");
                }
                else
                {
                    var existing = key.GetValue("MegaDownloadEnhancer");
                    if (existing != null)
                    {
                        key.DeleteValue("MegaDownloadEnhancer", false);
                        System.Diagnostics.Debug.WriteLine(
                            "[AutoStart] Removed registry entry");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "[AutoStart] No registry entry to remove (already clean)");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AutoStart] Error: {ex.Message}");
            }
        }

        // ── Validation ───────────────────────────────────────────

        private string? ValidateSettings()
        {
            if (string.IsNullOrEmpty(_settings.VpnName))
                return "VPN not configured.\nSelect one in Settings.";
            if (!_vpnService.IsVpnAvailable(_settings.VpnName))
                return $"'{_settings.VpnName}' not available.\nFor WARP: ensure warp-cli.exe is in PATH.";
            return null;
        }

        private void ShowFirstTimeSetup()
        {
            var r = MessageBox.Show(
                "Welcome to Mega Download Enhancer!\n\n" +
                "Configure a VPN connection to begin.\n" +
                "Recommended: Cloudflare WARP (free).\n\n" +
                "Open Settings now?",
                "First-Time Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == DialogResult.Yes) ShowSettings();
        }

        // ── Shutdown ─────────────────────────────────────────────

        public void Shutdown()
        {
            _exiting = true;
            _refreshTimer.Stop(); _pulseTimer.Stop(); _clockTimer.Stop();
            _trayIcon.Visible = false;
            _vpnService.Dispose();
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Dispose();
                _pulseTimer?.Dispose();
                _clockTimer?.Dispose();
                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
                _vpnService?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ── Helpers ──────────────────────────────────────────────

        static Panel DB(Panel p)
        {
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(p, true);
            return p;
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
            b.FlatAppearance.MouseOverBackColor = Lighten(bg, 20);
            b.FlatAppearance.MouseDownBackColor = Darken(bg, 15);
            return b;
        }

        static Color Lighten(Color c, int amt) =>
            Color.FromArgb(c.A, Math.Min(255, c.R + amt), Math.Min(255, c.G + amt), Math.Min(255, c.B + amt));
        static Color Darken(Color c, int amt) =>
            Color.FromArgb(c.A, Math.Max(0, c.R - amt), Math.Max(0, c.G - amt), Math.Max(0, c.B - amt));

        static void Add(ContextMenuStrip m, string text, EventHandler h)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += h;
            m.Items.Add(item);
        }

        static void StyleContextMenu(ContextMenuStrip m)
        {
            m.BackColor = Color.FromArgb(13, 20, 32);
            m.ForeColor = Color.FromArgb(226, 235, 248);
            m.Font = new Font("Segoe UI", 9F);
            m.RenderMode = ToolStripRenderMode.System;
        }
    }
}
