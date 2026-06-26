using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        private readonly List<ScriptDocument> scripts = new List<ScriptDocument>();
        private readonly Timer uptimeTimer = new Timer();

        private Panel editorHost;
        private Panel homeView;
        private Panel scriptListPanel;
        private Label scriptsHeader;
        private Label selectedTabLabel;
        private Label statusLabel;
        private Label uptimeLabel;
        private Label scriptCountLabel;
        private Label injectedStateLabel;
        private RichTextBox editor;
        private Button homeButton;
        private Button scriptsButton;
        private Button injectButton;
        private DateTime injectedAt;
        private int activeScriptIndex;
        private bool isInjected;

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

            scripts.Add(new ScriptDocument("script_1.lua", "--discord.gg/4r3XgYQdhn" + Environment.NewLine + "print(\"Cosmic on top!\")"));
            uptimeTimer.Interval = 1000;
            uptimeTimer.Tick += (s, e) => UpdateHomeStats();

            BuildLayout();
            SelectScript(0);
            ShowScriptsView();
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
            statusLabel = CreateLabel("Ready", Color.FromArgb(126, 122, 104), Bold(9F), new Point(104, 12), new Size(420, 18));
            titleBar.Controls.Add(statusLabel);
            titleBar.Controls.Add(CreateTitleButton("×", Width - 31));
            titleBar.Controls.Add(CreateTitleButton("□", Width - 66));
            titleBar.Controls.Add(CreateTitleButton("−", Width - 101));
            titleBar.Resize += (s, e) => PositionTitleButtons(titleBar);

            var shell = AddPanel(DockStyle.Fill, 0, surface);
            var commandBar = AddPanel(shell, DockStyle.Bottom, 38, surface);
            var activity = AddPanel(shell, DockStyle.Left, 48, surface);
            var sidebar = AddPanel(shell, DockStyle.Left, 238, surface);
            editorHost = AddPanel(shell, DockStyle.Fill, 0, background);

            BuildActivityBar(activity);
            BuildSidebar(sidebar);
            BuildEditor(editorHost);
            BuildHomeView(editorHost);
            BuildCommandBar(commandBar);
        }

        private void BuildActivityBar(Panel activity)
        {
            homeButton = CreateNavButton("⌂", 7, false);
            scriptsButton = CreateNavButton("</>", 48, true);
            homeButton.Click += (s, e) => ShowHomeView();
            scriptsButton.Click += (s, e) => ShowScriptsView();
            activity.Controls.Add(homeButton);
            activity.Controls.Add(scriptsButton);
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
            var addButton = CreateSmallButton("+", new Point(sidebar.Width - 35, 10), new Size(24, 22));
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.Click += (s, e) => AddNewScript();
            sidebar.Controls.Add(addButton);

            scriptsHeader = CreateLabel("⌄  SCRIPTS (1)", Color.FromArgb(91, 89, 79), Bold(8F), new Point(12, 48), new Size(150, 20));
            sidebar.Controls.Add(scriptsHeader);

            scriptListPanel = new Panel { BackColor = surface, Location = new Point(0, 66), Size = new Size(sidebar.Width, sidebar.Height - 144), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom, AutoScroll = true };
            sidebar.Controls.Add(scriptListPanel);

            var drop = new DashedPanel { BorderColor = Color.FromArgb(42, 42, 38), Location = new Point(10, sidebar.Height - 68), Size = new Size(sidebar.Width - 20, 58), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            drop.AllowDrop = true;
            drop.DragEnter += Drop_DragEnter;
            drop.DragDrop += Drop_DragDrop;
            drop.Controls.Add(CreateLabel("⇧", muted, new Font("Segoe UI Symbol", 17F), new Point((drop.Width / 2) - 12, 10), new Size(30, 24), AnchorStyles.Top));
            drop.Controls.Add(CreateLabel("Drop .lua or .txt files here", Color.FromArgb(72, 72, 65), new Font("Segoe UI", 8F), new Point(52, 35), new Size(150, 18)));
            sidebar.Controls.Add(drop);

            RefreshScriptList();
        }

        private void BuildEditor(Panel host)
        {
            var tab = new RoundedPanel { BackColor = surface, Radius = 6, Location = new Point(6, 8), Size = new Size(150, 28) };
            selectedTabLabel = CreateLabel("◰  script_1.lua  ×", Color.Gainsboro, Bold(8.5F), new Point(12, 6), new Size(130, 18));
            tab.Controls.Add(selectedTabLabel);
            host.Controls.Add(tab);

            var gutter = new LineNumberPanel { BackColor = background, ForeColor = gold, Location = new Point(0, 36), Size = new Size(58, host.Height - 74), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            host.Controls.Add(gutter);

            editor = new RichTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = background,
                ForeColor = Color.FromArgb(59, 236, 144),
                Font = new Font("Consolas", 10F),
                Location = new Point(58, 36),
                Size = new Size(host.Width - 154, host.Height - 74),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AcceptsTab = true,
                WordWrap = false,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both
            };
            editor.TextChanged += (s, e) =>
            {
                if (activeScriptIndex >= 0 && activeScriptIndex < scripts.Count)
                {
                    scripts[activeScriptIndex].Content = editor.Text;
                }
                gutter.LineCount = Math.Max(1, editor.Lines.Length);
                gutter.Invalidate();
            };
            host.Controls.Add(editor);

            var minimap = new Panel { BackColor = Color.FromArgb(13, 14, 13), Dock = DockStyle.Right, Width = 96 };
            minimap.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(56, 48, 24))) e.Graphics.DrawLine(pen, minimap.Width - 3, 0, minimap.Width - 3, minimap.Height);
                using (var pen = new Pen(gold)) e.Graphics.DrawLine(pen, minimap.Width - 4, 48, minimap.Width - 4, 94);
                using (var brush = new SolidBrush(Color.FromArgb(55, 100, 95))) e.Graphics.DrawString("==--", new Font("Consolas", 3.5F), brush, 6, 1);
            };
            host.Controls.Add(minimap);
        }

        private void BuildHomeView(Panel host)
        {
            homeView = new Panel { BackColor = background, Location = new Point(0, 36), Size = new Size(host.Width - 96, host.Height - 74), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Visible = false };
            homeView.Controls.Add(CreateLabel("Cosmic status", gold, new Font("Segoe UI", 18F, FontStyle.Bold), new Point(34, 30), new Size(240, 34)));
            injectedStateLabel = CreateLabel("Not injected", Color.Gainsboro, new Font("Segoe UI", 12F, FontStyle.Bold), new Point(38, 86), new Size(340, 26));
            uptimeLabel = CreateLabel("Open time: 00:00:00", muted, new Font("Segoe UI", 10F), new Point(40, 122), new Size(360, 24));
            scriptCountLabel = CreateLabel("Scripts open: 1", muted, new Font("Segoe UI", 10F), new Point(40, 152), new Size(360, 24));
            homeView.Controls.Add(injectedStateLabel);
            homeView.Controls.Add(uptimeLabel);
            homeView.Controls.Add(scriptCountLabel);
            homeView.Controls.Add(CreateLabel("This build provides UI state only. It does not inject DLLs into Roblox or any other process.", Color.FromArgb(126, 122, 104), new Font("Segoe UI", 9F), new Point(40, 194), new Size(620, 48)));
            host.Controls.Add(homeView);
        }

        private void BuildCommandBar(Panel bar)
        {
            bar.Padding = new Padding(6, 4, 6, 4);
            bar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(border), 0, 0, bar.Width, 0);
            AddActionButton(bar, "▷ Execute", 6, true, (s, e) => SetStatus("Executed current script text locally in the editor mock."));
            AddActionButton(bar, "♙ Clear", 94, false, (s, e) => editor.Clear());
            AddActionButton(bar, "◴ Save", 170, false, (s, e) => SaveCurrentScript());
            AddActionButton(bar, "▣ Open", 240, false, (s, e) => OpenScriptFromDisk());
            injectButton = AddRightButton(bar, "🔗 Inject", 146, false);
            injectButton.Click += (s, e) => SimulateInject();
            AddRightButton(bar, "🔔 Clients", 68, true);
            bar.Resize += (s, e) => { foreach (Control c in bar.Controls) if (c.Tag is int) c.Left = bar.Width - (int)c.Tag; };
        }

        private void AddNewScript()
        {
            scripts.Add(new ScriptDocument("script_" + (scripts.Count + 1) + ".lua", "print(\"New Cosmic script\")"));
            RefreshScriptList();
            SelectScript(scripts.Count - 1);
            ShowScriptsView();
            SetStatus("Added " + scripts[activeScriptIndex].Name);
        }

        private void RefreshScriptList()
        {
            if (scriptListPanel == null) return;
            scriptListPanel.Controls.Clear();
            scriptsHeader.Text = "⌄  SCRIPTS (" + scripts.Count + ")";
            for (var i = 0; i < scripts.Count; i++)
            {
                var index = i;
                var file = new Panel { BackColor = index == activeScriptIndex ? surfaceLight : surface, Location = new Point(0, i * 27), Size = new Size(scriptListPanel.Width - 1, 25), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand };
                file.Click += (s, e) => SelectScript(index);
                file.Controls.Add(CreateLabel("📄", gold, new Font("Segoe UI Emoji", 9F), new Point(18, 3), new Size(22, 20)));
                var nameLabel = CreateLabel(scripts[i].Name, Color.Gainsboro, Bold(8.5F), new Point(42, 5), new Size(150, 18));
                nameLabel.Click += (s, e) => SelectScript(index);
                file.Controls.Add(nameLabel);
                scriptListPanel.Controls.Add(file);
            }
            UpdateHomeStats();
        }

        private void SelectScript(int index)
        {
            if (index < 0 || index >= scripts.Count) return;
            activeScriptIndex = index;
            editor.Text = scripts[index].Content;
            selectedTabLabel.Text = "◰  " + scripts[index].Name + "  ×";
            RefreshScriptList();
        }

        private void ShowHomeView()
        {
            homeView.Visible = true;
            editor.Visible = false;
            homeButton.BackColor = Color.FromArgb(37, 34, 23);
            homeButton.ForeColor = gold;
            scriptsButton.BackColor = surface;
            scriptsButton.ForeColor = muted;
            UpdateHomeStats();
        }

        private void ShowScriptsView()
        {
            homeView.Visible = false;
            editor.Visible = true;
            scriptsButton.BackColor = Color.FromArgb(37, 34, 23);
            scriptsButton.ForeColor = gold;
            homeButton.BackColor = surface;
            homeButton.ForeColor = muted;
        }

        private void SimulateInject()
        {
            if (isInjected) return;
            injectButton.Text = "⏳ Injecting";
            SetStatus("Injecting UI state...");
            var delay = new Timer { Interval = 900 };
            delay.Tick += (s, e) =>
            {
                delay.Stop();
                delay.Dispose();
                isInjected = true;
                injectedAt = DateTime.Now;
                injectButton.Text = "✓ Injected";
                SetStatus("Injected UI state is on. DLL/process injection is intentionally not implemented.");
                uptimeTimer.Start();
                ShowHomeView();
            };
            delay.Start();
        }

        private void UpdateHomeStats()
        {
            if (scriptCountLabel != null) scriptCountLabel.Text = "Scripts open: " + scripts.Count;
            if (injectedStateLabel != null) injectedStateLabel.Text = isInjected ? "Injected UI state: On" : "Not injected";
            if (uptimeLabel != null)
            {
                var elapsed = isInjected ? DateTime.Now - injectedAt : TimeSpan.Zero;
                uptimeLabel.Text = "Open time: " + elapsed.ToString(@"hh\:mm\:ss");
            }
        }

        private void SaveCurrentScript()
        {
            if (activeScriptIndex < 0 || activeScriptIndex >= scripts.Count) return;
            using (var dialog = new SaveFileDialog { Filter = "Lua files (*.lua)|*.lua|Text files (*.txt)|*.txt|All files (*.*)|*.*", FileName = scripts[activeScriptIndex].Name })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, editor.Text);
                    scripts[activeScriptIndex].Name = Path.GetFileName(dialog.FileName);
                    RefreshScriptList();
                    SelectScript(activeScriptIndex);
                    SetStatus("Saved " + scripts[activeScriptIndex].Name);
                }
            }
        }

        private void OpenScriptFromDisk()
        {
            using (var dialog = new OpenFileDialog { Filter = "Lua/text files (*.lua;*.txt)|*.lua;*.txt|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    scripts.Add(new ScriptDocument(Path.GetFileName(dialog.FileName), File.ReadAllText(dialog.FileName)));
                    RefreshScriptList();
                    SelectScript(scripts.Count - 1);
                    ShowScriptsView();
                    SetStatus("Opened " + scripts[activeScriptIndex].Name);
                }
            }
        }

        private void Drop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Drop_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension == ".lua" || extension == ".txt") scripts.Add(new ScriptDocument(Path.GetFileName(file), File.ReadAllText(file)));
            }
            RefreshScriptList();
            SelectScript(scripts.Count - 1);
            ShowScriptsView();
        }

        private void SetStatus(string message)
        {
            statusLabel.Text = message;
        }

        private Panel AddPanel(DockStyle dock, int size, Color color) { var p = new Panel { Dock = dock, BackColor = color }; if (dock == DockStyle.Top || dock == DockStyle.Bottom) p.Height = size; else if (dock == DockStyle.Left || dock == DockStyle.Right) p.Width = size; Controls.Add(p); p.BringToFront(); return p; }
        private Panel AddPanel(Control parent, DockStyle dock, int size, Color color) { var p = new Panel { Dock = dock, BackColor = color }; if (dock == DockStyle.Top || dock == DockStyle.Bottom) p.Height = size; else if (dock == DockStyle.Left || dock == DockStyle.Right) p.Width = size; parent.Controls.Add(p); p.BringToFront(); return p; }
        private Font Bold(float size) { return new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Point); }
        private Label CreateLabel(string text, Color color, Font font, Point location, Size size, AnchorStyles anchor = AnchorStyles.Left | AnchorStyles.Top) { return new Label { Text = text, ForeColor = color, BackColor = Color.Transparent, Font = font, Location = location, Size = size, Anchor = anchor, AutoSize = false }; }
        private Button CreateTitleButton(string text, int left) { var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(76, 74, 66), BackColor = background, Location = new Point(left, 2), Size = new Size(34, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right, TabStop = false }; b.FlatAppearance.BorderSize = 0; return b; }
        private void PositionTitleButtons(Control titleBar) { var x = titleBar.Width - 34; foreach (Control c in titleBar.Controls) if (c is Button) { c.Left = x; x -= 35; } }
        private Button CreateNavButton(string text, int top, bool active) { var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, ForeColor = active ? gold : muted, BackColor = active ? Color.FromArgb(37, 34, 23) : surface, Location = new Point(5, top), Size = new Size(36, 38), Font = new Font("Segoe UI Symbol", 10F, FontStyle.Bold), TabStop = false }; b.FlatAppearance.BorderSize = 0; return b; }
        private Button CreateSmallButton(string text, Point location, Size size) { var b = new Button { Text = text, FlatStyle = FlatStyle.Flat, ForeColor = muted, BackColor = surface, Location = location, Size = size, Font = Bold(10F), TabStop = false }; b.FlatAppearance.BorderSize = 0; return b; }
        private void AddActionButton(Control parent, string text, int left, bool primary, EventHandler click) { var b = new Button { Text = text, Location = new Point(left, 6), Size = new Size(primary ? 82 : 66, 28), FlatStyle = FlatStyle.Flat, ForeColor = primary ? gold : muted, BackColor = surface, Font = Bold(8F), TabStop = false }; b.FlatAppearance.BorderColor = primary ? Color.FromArgb(92, 74, 25) : border; b.Click += click; parent.Controls.Add(b); }
        private Button AddRightButton(Control parent, string text, int rightOffset, bool primary = false) { var b = new Button { Text = text, Size = new Size(primary ? 70 : 80, 28), Location = new Point(parent.Width - rightOffset, 6), Anchor = AnchorStyles.Top | AnchorStyles.Right, Tag = rightOffset, FlatStyle = FlatStyle.Flat, ForeColor = primary ? gold : muted, BackColor = surface, Font = Bold(8F), TabStop = false }; b.FlatAppearance.BorderColor = primary ? Color.FromArgb(92, 74, 25) : border; parent.Controls.Add(b); return b; }
        private static void DrawBottomDivider(Graphics g, Rectangle bounds) { using (var pen = new Pen(Color.FromArgb(36, 36, 34))) g.DrawLine(pen, 7, bounds.Height - 52, bounds.Width - 12, bounds.Height - 52); }
        private void DragWindow(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { NativeMethods.ReleaseCapture(); NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0); } }

        private sealed class ScriptDocument
        {
            public ScriptDocument(string name, string content) { Name = name; Content = content; }
            public string Name { get; set; }
            public string Content { get; set; }
        }

        private sealed class RoundedPanel : Panel { public int Radius { get; set; } = 8; protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var path = new GraphicsPath()) { path.AddArc(0, 0, Radius, Radius, 180, 90); path.AddArc(Width - Radius - 1, 0, Radius, Radius, 270, 90); path.AddLine(Width - 1, Height - 1, 0, Height - 1); path.CloseFigure(); using (var pen = new Pen(Color.FromArgb(33, 33, 31))) e.Graphics.DrawPath(pen, path); } } }
        private sealed class DashedPanel : Panel { public Color BorderColor { get; set; } protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using (var pen = new Pen(BorderColor) { DashStyle = DashStyle.Dash }) e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1); } }
        private sealed class LineNumberPanel : Panel
        {
            public int LineCount { get; set; } = 1;
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var font = new Font("Consolas", 10F))
                using (var brush = new SolidBrush(ForeColor))
                {
                    for (var i = 1; i <= Math.Min(LineCount + 1, 200); i++) e.Graphics.DrawString(i.ToString(), font, brush, 30, 14 + ((i - 1) * 21));
                }
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
