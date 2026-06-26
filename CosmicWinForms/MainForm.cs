using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CosmicWinForms
{
    public sealed class MainForm : Form
    {
        private readonly Color background = Color.FromArgb(10, 11, 10);
        private readonly Color surface = Color.FromArgb(18, 18, 17);
        private readonly Color surfaceLight = Color.FromArgb(31, 30, 26);
        private readonly Color border = Color.FromArgb(36, 36, 34);
        private readonly Color gold = Color.FromArgb(212, 170, 62);
        private readonly Color muted = Color.FromArgb(104, 103, 94);

        public MainForm()
        {
            Text = "Cosmic";
            MinimumSize = new Size(980, 560);
            Size = new Size(1470, 735);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = background;
            ForeColor = Color.Gainsboro;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;

            BuildLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void BuildLayout()
        {
            var titleBar = AddPanel(DockStyle.Top, 38, background);
            titleBar.MouseDown += DragWindow;
            titleBar.Controls.Add(CreateLabel("✦", gold, new Font("Segoe UI", 15F), new Point(12, 8), new Size(20, 22)));
            titleBar.Controls.Add(CreateLabel("Cosmic", Color.FromArgb(126, 122, 104), Bold(9F), new Point(36, 12), new Size(65, 18)));
            titleBar.Controls.Add(CreateLabel("●", Color.FromArgb(73, 70, 60), new Font("Segoe UI", 8F), new Point(87, 13), new Size(16, 16)));
            titleBar.Controls.Add(CreateLabel("Inject me first (-> <-)", Color.FromArgb(126, 122, 104), Bold(9F), new Point(104, 12), new Size(180, 18)));
            titleBar.Controls.Add(CreateTitleButton("×", Width - 31));
            titleBar.Controls.Add(CreateTitleButton("□", Width - 66));
            titleBar.Controls.Add(CreateTitleButton("−", Width - 101));
            titleBar.Resize += (s, e) => PositionTitleButtons(titleBar);

            var shell = AddPanel(DockStyle.Fill, 0, surface);
            var commandBar = AddPanel(shell, DockStyle.Bottom, 38, surface);
            var activity = AddPanel(shell, DockStyle.Left, 48, surface);
            var sidebar = AddPanel(shell, DockStyle.Left, 238, surface);
            var editorHost = AddPanel(shell, DockStyle.Fill, 0, background);

            BuildActivityBar(activity);
            BuildSidebar(sidebar);
            BuildEditor(editorHost);
            BuildCommandBar(commandBar);
        }

        private void BuildActivityBar(Panel activity)
        {
            activity.Controls.Add(CreateNavButton("⌂", 7, false));
            activity.Controls.Add(CreateNavButton("</>", 48, true));
            activity.Controls.Add(CreateNavButton("▤", 96, false));
            activity.Controls.Add(CreateLabel("◎", muted, new Font("Segoe UI Symbol", 15F), new Point(14, activity.Height - 36), new Size(24, 24), AnchorStyles.Left | AnchorStyles.Bottom));
            activity.Paint += (s, e) => DrawBottomDivider(e.Graphics, activity.ClientRectangle);
        }

        private void BuildSidebar(Panel sidebar)
        {
            sidebar.Padding = new Padding(10, 10, 10, 10);
            sidebar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(border), sidebar.Width - 1, 34, sidebar.Width - 1, sidebar.Height);

            sidebar.Controls.Add(CreateLabel("⌕", muted, new Font("Segoe UI Symbol", 13F), new Point(12, 12), new Size(24, 22)));
            sidebar.Controls.Add(CreateLabel("Filter scripts...", Color.FromArgb(70, 70, 66), new Font("Segoe UI", 8.5F), new Point(36, 14), new Size(150, 20)));
            sidebar.Controls.Add(CreateLabel("⌄  SCRIPTS (1)", Color.FromArgb(91, 89, 79), Bold(8F), new Point(12, 48), new Size(150, 20)));

            var file = new Panel { BackColor = surfaceLight, Location = new Point(0, 66), Size = new Size(sidebar.Width, 25), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };
            file.Controls.Add(CreateLabel("📄", gold, new Font("Segoe UI Emoji", 9F), new Point(18, 3), new Size(22, 20)));
            file.Controls.Add(CreateLabel("script_1.lua", Color.Gainsboro, Bold(8.5F), new Point(42, 5), new Size(150, 18)));
            sidebar.Controls.Add(file);

            var drop = new DashedPanel { BorderColor = Color.FromArgb(42, 42, 38), Location = new Point(10, sidebar.Height - 68), Size = new Size(sidebar.Width - 20, 58), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            drop.Controls.Add(CreateLabel("⇧", muted, new Font("Segoe UI Symbol", 17F), new Point((drop.Width / 2) - 12, 10), new Size(30, 24), AnchorStyles.Top));
            drop.Controls.Add(CreateLabel("Drop .lua or .txt files here", Color.FromArgb(72, 72, 65), new Font("Segoe UI", 8F), new Point(52, 35), new Size(150, 18)));
            sidebar.Controls.Add(drop);
        }

        private void BuildEditor(Panel host)
        {
            var tab = new RoundedPanel { BackColor = surface, Radius = 6, Location = new Point(6, 8), Size = new Size(120, 28) };
            tab.Controls.Add(CreateLabel("◰  script_1.lua  ×", Color.Gainsboro, Bold(8.5F), new Point(12, 6), new Size(105, 18)));
            host.Controls.Add(tab);

            var gutter = new Panel { BackColor = background, Location = new Point(0, 36), Size = new Size(58, host.Height - 74), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            gutter.Controls.Add(CreateLabel("1", Color.FromArgb(143, 124, 53), new Font("Consolas", 10F), new Point(30, 14), new Size(26, 18)));
            gutter.Controls.Add(CreateLabel("2", Color.FromArgb(143, 124, 53), new Font("Consolas", 10F), new Point(30, 35), new Size(26, 18)));
            gutter.Controls.Add(CreateLabel("3", gold, new Font("Consolas", 10F), new Point(30, 56), new Size(26, 18)));
            host.Controls.Add(gutter);

            var code = new CodePanel { BackColor = background, Location = new Point(58, 36), Size = new Size(host.Width - 100, host.Height - 74), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            code.Controls.Add(CreateLabel("--discord.gg/4r3XgYQdhn", Color.FromArgb(119, 111, 93), new Font("Consolas", 10F, FontStyle.Italic), new Point(6, 14), new Size(245, 18)));
            code.Controls.Add(CreateLabel("print", Color.FromArgb(72, 141, 255), new Font("Consolas", 10F), new Point(6, 35), new Size(40, 18)));
            code.Controls.Add(CreateLabel("(\"Cosmic on top!\")", Color.FromArgb(59, 236, 144), new Font("Consolas", 10F, FontStyle.Bold), new Point(46, 35), new Size(175, 18)));
            host.Controls.Add(code);

            var minimap = new Panel { BackColor = Color.FromArgb(13, 14, 13), Dock = DockStyle.Right, Width = 96 };
            minimap.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(56, 48, 24))) e.Graphics.DrawLine(pen, minimap.Width - 3, 0, minimap.Width - 3, minimap.Height);
                using (var pen = new Pen(gold)) e.Graphics.DrawLine(pen, minimap.Width - 4, 48, minimap.Width - 4, 94);
                e.Graphics.DrawString("==--", new Font("Consolas", 3.5F), new SolidBrush(Color.FromArgb(55, 100, 95)), 6, 1);
            };
            host.Controls.Add(minimap);
        }

        private void BuildCommandBar(Panel bar)
        {
            bar.Padding = new Padding(6, 4, 6, 4);
            bar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(border), 0, 0, bar.Width, 0);
            AddActionButton(bar, "▷ Execute", 6, true);
            AddActionButton(bar, "♙ Clear", 94, false);
            AddActionButton(bar, "◴ Save", 170, false);
            AddActionButton(bar, "▣ Open", 240, false);
            AddRightButton(bar, "🔗 Inject", 146);
            AddRightButton(bar, "🔔 Clients", 68, true);
            bar.Resize += (s, e) => { foreach (Control c in bar.Controls) if (c.Tag is int offset) c.Left = bar.Width - offset; };
        }

        private Panel AddPanel(DockStyle dock, int size, Color color) { var p = new Panel { Dock = dock, BackColor = color }; if (dock == DockStyle.Top || dock == DockStyle.Bottom) p.Height = size; else if (dock == DockStyle.Left || dock == DockStyle.Right) p.Width = size; Controls.Add(p); p.BringToFront(); return p; }
        private Panel AddPanel(Control parent, DockStyle dock, int size, Color color) { var p = new Panel { Dock = dock, BackColor = color }; if (dock == DockStyle.Top || dock == DockStyle.Bottom) p.Height = size; else if (dock == DockStyle.Left || dock == DockStyle.Right) p.Width = size; parent.Controls.Add(p); p.BringToFront(); return p; }
        private Font Bold(float size) { return new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Point); }
        private Label CreateLabel(string text, Color color, Font font, Point location, Size size, AnchorStyles anchor = AnchorStyles.Left | AnchorStyles.Top) { return new Label { Text = text, ForeColor = color, BackColor = Color.Transparent, Font = font, Location = location, Size = size, Anchor = anchor, AutoSize = false }; }
        private Button CreateTitleButton(string text, int left) { return new Button { Text = text, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(76, 74, 66), BackColor = background, Location = new Point(left, 2), Size = new Size(34, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right, TabStop = false }; }
        private void PositionTitleButtons(Control titleBar) { var x = titleBar.Width - 34; foreach (Control c in titleBar.Controls) if (c is Button) { c.Left = x; x -= 35; } }
        private Button CreateNavButton(string text, int top, bool active) { var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, ForeColor = active ? gold : muted, BackColor = active ? Color.FromArgb(37, 34, 23) : surface, Location = new Point(5, top), Size = new Size(36, 38), Font = new Font("Segoe UI Symbol", 10F, FontStyle.Bold), TabStop = false }; b.FlatAppearance.BorderSize = 0; return b; }
        private void AddActionButton(Control parent, string text, int left, bool primary) { var b = new Button { Text = text, Location = new Point(left, 6), Size = new Size(primary ? 82 : 66, 28), FlatStyle = FlatStyle.Flat, ForeColor = primary ? gold : muted, BackColor = surface, Font = Bold(8F), TabStop = false }; b.FlatAppearance.BorderColor = primary ? Color.FromArgb(92, 74, 25) : border; parent.Controls.Add(b); }
        private void AddRightButton(Control parent, string text, int rightOffset, bool primary = false) { var b = new Button { Text = text, Size = new Size(primary ? 70 : 70, 28), Location = new Point(parent.Width - rightOffset, 6), Anchor = AnchorStyles.Top | AnchorStyles.Right, Tag = rightOffset, FlatStyle = FlatStyle.Flat, ForeColor = primary ? gold : muted, BackColor = surface, Font = Bold(8F), TabStop = false }; b.FlatAppearance.BorderColor = primary ? Color.FromArgb(92, 74, 25) : border; parent.Controls.Add(b); }
        private static void DrawBottomDivider(Graphics g, Rectangle bounds) { using (var pen = new Pen(Color.FromArgb(36, 36, 34))) g.DrawLine(pen, 7, bounds.Height - 52, bounds.Width - 12, bounds.Height - 52); }
        private void DragWindow(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { NativeMethods.ReleaseCapture(); NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0); } }

        private sealed class RoundedPanel : Panel { public int Radius { get; set; } = 8; protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var path = new GraphicsPath()) { path.AddArc(0, 0, Radius, Radius, 180, 90); path.AddArc(Width - Radius - 1, 0, Radius, Radius, 270, 90); path.AddLine(Width - 1, Height - 1, 0, Height - 1); path.CloseFigure(); using (var pen = new Pen(Color.FromArgb(33, 33, 31))) e.Graphics.DrawPath(pen, path); } } }
        private sealed class DashedPanel : Panel { public Color BorderColor { get; set; } protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var pen = new Pen(BorderColor) { DashStyle = DashStyle.Dash }) e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1); } }
        private sealed class CodePanel : Panel { protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var pen = new Pen(Color.FromArgb(18, 18, 17))) e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1); } }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
