using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace VPNManager.Forms
{
    public static class AppIcon
    {
        static readonly Color BG    = Color.FromArgb(8, 12, 20);
        static readonly Color Teal  = Color.FromArgb(0, 207, 168);
        static readonly Color Teal2 = Color.FromArgb(0, 148, 120);

        public static Icon Create(int size = 32)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Color.Transparent);

            int r = Math.Max(4, size / 5);
            using var bgBrush = new SolidBrush(BG);
            using var bgPath = RoundedRect(new Rectangle(0, 0, size - 1, size - 1), r);
            g.FillPath(bgBrush, bgPath);

            using var glowPen = new Pen(Color.FromArgb(50, 0, 207, 168), 1.2f);
            g.DrawPath(glowPen, bgPath);

            float s = size;
            PointF[] bolt =
            {
                new(s * 0.58f, s * 0.07f),
                new(s * 0.27f, s * 0.51f),
                new(s * 0.50f, s * 0.51f),
                new(s * 0.42f, s * 0.93f),
                new(s * 0.73f, s * 0.49f),
                new(s * 0.50f, s * 0.49f),
            };

            using var boltBrush = new LinearGradientBrush(
                new PointF(0, 0), new PointF(0, s), Teal, Teal2);
            g.FillPolygon(boltBrush, bolt);

            var hIcon = bmp.GetHicon();
            var icon = Icon.FromHandle(hIcon);
            bmp.Dispose();
            return icon;
        }

        internal static GraphicsPath RoundedRect(Rectangle b, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(b.X, b.Y, d, d, 180, 90);
            p.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            p.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
