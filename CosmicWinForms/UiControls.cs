using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CosmicWinForms
{
    internal sealed class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 8;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, Radius, Radius, 180, 90);
                path.AddArc(Width - Radius - 1, 0, Radius, Radius, 270, 90);
                path.AddLine(Width - 1, Height - 1, 0, Height - 1);
                path.CloseFigure();
                using (var pen = new Pen(Color.FromArgb(33, 33, 31)))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }

    internal sealed class DashedPanel : Panel
    {
        public Color BorderColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(BorderColor) { DashStyle = DashStyle.Dash })
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    internal sealed class LineNumberPanel : Panel
    {
        public int LineCount { get; set; } = 1;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var font = new Font("Consolas", 10F))
            using (var brush = new SolidBrush(ForeColor))
            {
                for (var i = 1; i <= Math.Min(LineCount + 1, 200); i++)
                {
                    e.Graphics.DrawString(i.ToString(), font, brush, 30, 14 + ((i - 1) * 21));
                }
            }
        }
    }
}
