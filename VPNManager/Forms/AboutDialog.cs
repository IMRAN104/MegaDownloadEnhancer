using System;
using System.Drawing;
using System.Windows.Forms;

namespace VPNManager.Forms
{
    public class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About Mega Download Enhancer";
            Size = new Size(400, 310);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(248, 250, 252);

            var title = new Label
            {
                Text = "Mega Download Enhancer",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            var version = new Label
            {
                Text = $"Version {Application.ProductVersion}",
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(20, 52),
                ForeColor = Color.FromArgb(113, 113, 122)
            };

            var desc = new Label
            {
                Text = "VPN Auto-Cycler for seamless MEGA.nz downloads.\nAutomatically rotates Cloudflare WARP to bypass\nfree-tier transfer limits.",
                AutoSize = true,
                Location = new Point(20, 82),
                ForeColor = Color.FromArgb(63, 63, 70),
                MaximumSize = new Size(350, 0)
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
                Text = "github.com/IMRAN104/MegaDownloadEnhancer",
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

            var btnOk = new Button
            {
                Text = "OK",
                Size = new Size(80, 32),
                Location = new Point(300, 225),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            Controls.Add(title);
            Controls.Add(version);
            Controls.Add(desc);
            Controls.Add(copyright);
            Controls.Add(repo);
            Controls.Add(btnOk);
        }
    }
}
