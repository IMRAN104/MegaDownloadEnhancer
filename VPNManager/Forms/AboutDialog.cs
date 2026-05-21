using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace VPNManager.Forms
{
    public class AboutDialog : Form
    {
        static readonly Color C_BG      = Color.FromArgb(7,   11,  18);
        static readonly Color C_SURFACE = Color.FromArgb(13,  20,  32);
        static readonly Color C_BORDER  = Color.FromArgb(28,  40,  68);
        static readonly Color C_ACCENT  = Color.FromArgb(0,  207, 168);
        static readonly Color C_TEXT1   = Color.FromArgb(226, 235, 248);
        static readonly Color C_TEXT2   = Color.FromArgb(80,  105, 145);

        public AboutDialog()
        {
            Icon = AppIcon.Create(16);
            Text = "About — Mega Download Enhancer";
            ClientSize = new Size(420, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = C_BG;
            ForeColor = C_TEXT1;
            Font = new Font("Segoe UI", 9F);
            DoubleBuffered = true;

            // ── Canvas ──────────────────────────────────────────
            var canvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            canvas.Paint += PaintAbout;

            // ── GitHub link ─────────────────────────────────────
            var link = new LinkLabel
            {
                Text = "github.com/IMRAN104/MegaDownloadEnhancer",
                Location = new Point(100, 196),
                AutoSize = true,
                Font = new Font("Consolas", 8F),
                LinkColor = C_ACCENT,
                BackColor = Color.Transparent
            };
            link.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/IMRAN104/MegaDownloadEnhancer",
                UseShellExecute = true
            });

            // ── OK button ───────────────────────────────────────
            var btnOk = new Button
            {
                Text = "Close",
                Size = new Size(90, 34),
                Location = new Point(315, 288),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(18, 27, 44),
                ForeColor = C_TEXT1,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(26, 40, 64);

            canvas.Controls.Add(link);
            canvas.Controls.Add(btnOk);
            Controls.Add(canvas);
        }

        private void PaintAbout(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = ClientSize.Width, h = ClientSize.Height;

            // Background gradient
            using var bgBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(0, h),
                C_BG, Color.FromArgb(10, 16, 28));
            g.FillRectangle(bgBrush, 0, 0, w, h);

            // Top accent
            using var accentBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(w, 0),
                C_ACCENT, Color.FromArgb(0, C_ACCENT));
            g.FillRectangle(accentBrush, 0, 0, w, 2);

            // Large bolt logo
            AppIcon_Draw(g, w / 2 - 28, 20, 56);

            // App name
            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var titleBrush = new SolidBrush(C_TEXT1);
            var title = "Mega Download Enhancer";
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, titleBrush, (w - titleSize.Width) / 2, 86);

            // Version
            var ver = Application.ProductVersion;
            var cleanVer = ver.Contains('+') ? ver[..ver.IndexOf('+')] : ver;
            using var verFont = new Font("Consolas", 8.5F);
            using var verBrush = new SolidBrush(C_TEXT2);
            var vStr = $"v{cleanVer}";
            var vSize = g.MeasureString(vStr, verFont);
            g.DrawString(vStr, verFont, verBrush, (w - vSize.Width) / 2, 116);

            // Divider
            using var divPen = new Pen(C_BORDER, 1f);
            g.DrawLine(divPen, 60, 140, w - 60, 140);

            // Description
            using var descFont = new Font("Segoe UI", 9F);
            using var descBrush = new SolidBrush(Color.FromArgb(170, 190, 220));
            string desc = "Automatically cycles your IP via Cloudflare WARP\nto bypass MEGA's free-tier bandwidth limits.";
            var descSize = g.MeasureString(desc, descFont, w - 120);
            g.DrawString(desc, descFont, descBrush,
                new RectangleF((w - 280) / 2f, 152, 280, 60),
                new StringFormat { Alignment = StringAlignment.Center });

            // Copyright
            using var cpFont = new Font("Segoe UI", 8F);
            using var cpBrush = new SolidBrush(C_TEXT2);
            g.DrawString("© 2026 Imran Rahman", cpFont, cpBrush, 100, 218);

            // Bottom divider
            g.DrawLine(divPen, 60, 272, w - 60, 272);
        }

        private static void AppIcon_Draw(Graphics g, int x, int y, int sz)
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(8, 12, 20));
            using var path = AppIcon.RoundedRect(new Rectangle(x, y, sz, sz), sz / 5);
            g.FillPath(bgBrush, path);

            using var glowPen = new Pen(Color.FromArgb(80, 0, 207, 168), 1.5f);
            g.DrawPath(glowPen, path);

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
                Color.FromArgb(0, 207, 168), Color.FromArgb(0, 148, 120));
            g.FillPolygon(boltBrush, bolt);
        }
    }
}
