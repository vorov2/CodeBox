﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using CodeBox.Drawing;
using CodeBox.Styling;
using CodeBox.Folding;
using CodeBox.Autocomplete;
using CodeBox.Affinity;
using CodeBox.CallTips;
using CodeBox.Core;
using CodeBox.Core.Keyboard;
using System.Text;
using System.IO;
using CodeBox.Core.ViewModel;
using CodeBox.Search;
using CodeBox.Core.Settings;
using CodeBox.Core.Themes;

namespace CodeBox
{
    public class Editor : Control, IView
    {
        public readonly static bool Mono = Type.GetType("Mono.Runtime") != null;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERUPDATE = 0x0245;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_CHAR = 0x102;

        private readonly Timer timer = new Timer { Interval = 500 };
        private Pos movePosition;
        private Point mousePosition;

        public Editor() : this(App.Catalog<ISettingsProvider>().Default().Get<EditorSettings>())
        {

        }

        public Editor(EditorSettings settings)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            Cursor = Cursors.IBeam;
            TabStop = true;
            AllowDrop = true;

            TopMargins = new MarginList(this);
            LeftMargins = new MarginList(this);
            RightMargins = new MarginList(this);
            BottomMargins = new MarginList(this);
            Info = new EditorInfo(this);
            CaretRenderer = new CaretRenderer(this);
            Renderer = new Renderer(this);
            Scroll = new ScrollingManager(this);
            Locations = new LocationManager(this);//+
            Folding = new FoldingManager(this);
            CallTips = new CallTipManager(this);
            MatchBrackets = new MatchBracketManager(this);
            MatchWords = new MatchWordManager(this);
            Autocomplete = new AutocompleteManager(this);
            AffinityManager = new AffinityManager(this);
            Search = new SearchManager(this);
            Settings = settings;
            Styles = new StyleManager(this);
            Theme = App.Catalog<IThemeComponent>().Default();
            Text = "";

            timer.Tick += Tick;
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                drgevent.Effect = DragDropEffects.Copy;

            base.OnDragEnter(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            var data = drgevent.Data.GetData(DataFormats.FileDrop) as string[];

            if (data != null)
            {
                foreach (var f in data)
                    RunCommand((Identifier)"file.openfile", f);
            }

            base.OnDragDrop(drgevent);
        }

        private volatile bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                CaretRenderer.Dispose();

            disposed = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POINTERUPDATE || m.Msg == WM_POINTERUP || m.Msg == WM_POINTERDOWN)
            {
                var x = m.LParam.ToInt32() & 0x0000FFFF;
                var y = (int)((m.LParam.ToInt32() & 0xFFFF0000) >> 16);
                var pos = PointToClient(new Point(x, y));

                if (m.Msg == WM_POINTERUPDATE)
                {
                    Scroll.OnPointerUpdate(pos);
                    return;
                }
                else if (m.Msg == WM_POINTERDOWN)
                {
                    Scroll.OnPointerDown(pos);
                    return;
                }
                else if (m.Msg == WM_POINTERUP)
                {
                    Scroll.OnPointerUp(pos);
                    return;
                }
            }
            else if (m.Msg == WM_MOUSEHWHEEL)
            {
                var delta = (short)((m.WParam.ToInt32() >> 16) & 0xFFFF);
                Scroll.ScrollX((delta / 120) * 2);
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CaretRenderer.Suspend();
            var dt = DateTime.Now;

            e.Graphics.FillRectangle(Theme.GetStyle(StandardStyle.Default).BackColor.Brush(),
                new Rectangle(Info.TextLeft, Info.TextTop, Info.TextWidth, Info.TextHeight));

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Renderer.DrawMargins(0, e.Graphics, TopMargins);
            Renderer.DrawMargins(0, e.Graphics, LeftMargins);
            e.Graphics.TranslateTransform(Scroll.ScrollPosition.X, Scroll.ScrollPosition.Y);

            var carets = new List<CaretData>();
            Renderer.DrawLines(e.Graphics, carets);

            e.Graphics.ResetTransform();
            Renderer.DrawMargins(Info.TextBottom, e.Graphics, BottomMargins);
            if (RightMargins.Count > 0)
                Renderer.DrawMargins(ClientSize.Width - RightMargins.First().CalculateSize(), e.Graphics, RightMargins);

            Renderer.DrawLongLineIndicators(e.Graphics);
            Renderer.DrawWordWrapColumn(e.Graphics);

            e.Graphics.TranslateTransform(Scroll.ScrollPosition.X, Scroll.ScrollPosition.Y);

            if (!LimitedMode || Focused)
                foreach (var c in carets)
                    CaretRenderer.DrawCaret(e.Graphics, c.X, c.Y, c.Blink);

            if (Scroll.ScrollPosition.X != 0)
            {
                var cs = Theme.GetStyle(StandardStyle.Default);
                e.Graphics.FillRectangle(ControlPaint.Dark(cs.BackColor, .05f).Brush(),
                    new Rectangle(Info.TextLeft - Scroll.ScrollPosition.X,
                        Info.TextTop - Scroll.ScrollPosition.Y, Dpi.GetWidth(2),
                        Info.TextHeight + BottomMargins.TotalWidth));
            }

            Console.WriteLine("OnPaint time: " + (DateTime.Now - dt).TotalMilliseconds);
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            Scroll.InvalidateLines(InvalidateFlags.Force);
            //Styles.Restyle();
            Invalidate();
            Autocomplete.HideAutocomplete();
            Search.UpdateSearchPanel();
            base.OnResize(eventargs);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Autocomplete.WindowShown && Autocomplete.InWindowLocation(
                Mono ? PointToClient(e.Location) : e.Location))
                return;

            if (LockMouseScrolling || Lines.Count < 2)
                return;

            base.OnMouseWheel(e);
            var scrollBy = (e.Delta / 120) * 2;

            if ((scrollBy > 0 && Scroll.ScrollPosition.Y < 0)
                || (scrollBy < 0 && -Scroll.ScrollPosition.Y < Scroll.ScrollBounds.Height))
                Scroll.ScrollY(scrollBy);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseThief != null)
            {
                mouseThief.MouseMove(e.Location);
                return;
            }

            var mlist = Locations.FindMargin(e.Location);

            if (mlist != null)
            {
                mlist.CallMarginMethod(MarginMethod.MouseMove, e.Location);
                Cursor = Cursors.Arrow;
            }

            var leftMouseDown = e.Button == MouseButtons.Left;
            var p = Locations.LocationToPosition(e.Location);

            if (leftMouseDown && p.IsEmpty)
            {
                if (e.Y - Scroll.ScrollPosition.Y > Info.TextIntegralHeight && Scroll.LastVisibleLine < Lines.Count - 1)
                    p = new Pos(Scroll.LastVisibleLine + 1, Lines[Scroll.LastVisibleLine + 1].Length);
                else if (e.Y < Info.TextTop && Scroll.FirstVisibleLine > 0)
                    p = new Pos(Scroll.FirstVisibleLine - 1, 0);
            }

            if (!p.IsEmpty && Lines[p.Line].Length == p.Col && Folding.IsCollapsedHeader(p.Line))
                Cursor = Cursors.Arrow;
            else if (mlist == null)
                Cursor = Cursors.IBeam;

            mousePosition = e.Location;
            movePosition = p;
            Caret = p;

            if (!p.IsEmpty)
            {
                var keys = ModifierKeys.ToModifiers();
                var mouse = e.GetSpecialKey();

                if (mouse != SpecialKey.None || keys != Modifiers.None)
                    RunCommand(new KeyInput(keys | Modifiers.Move, mouse));

                timer.Start();
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (PointToClient(Cursor.Position) == mousePosition
                && !movePosition.IsEmpty && movePosition.Line < Lines.Count)
            {
                CallTips.MouseDwell(movePosition);
            }
            else
                CallTips.HideCallTip();

            timer.Stop();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mouseThief != null)
            {
                mouseThief.MouseUp(e.Location);
                mouseThief = null;
                Redraw();
            }

            var mlist = Locations.FindMargin(e.Location);

            if (mlist != null)
                mlist.CallMarginMethod(MarginMethod.MouseUp, e.Location);
        }

        internal Margin mouseThief;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var mouse = e.GetSpecialKey();

            if (e.Button == MouseButtons.Left)
            {
                var mlist = Locations.FindMargin(e.Location);

                if (mlist != null)
                    mlist.CallMarginMethod(MarginMethod.MouseDown, e.Location);
                else
                {
                    var pos = Locations.LocationToPosition(e.Location);
                    Caret = pos.IsEmpty ? default(Pos) : pos;
                    RunCommand(new KeyInput(ModifierKeys.ToModifiers(), mouse));
                    var idx = Buffer.Selections.IndexOfCaret(pos);

                    if (idx != -1)
                    {
                        var sca = Buffer.Selections[idx].Caret;
                        Buffer.Selections[idx].SetToRestore(
                            Document.Lines[sca.Line].GetStripeCol(sca.Col));
                    }

                    if (Lines[pos.Line].Length == pos.Col && Folding.IsCollapsedHeader(pos.Line))
                    {
                        Folding.ToggleExpand(pos.Line);
                        Scroll.InvalidateLines();
                        Redraw();
                    }

                    if (!Focused)
                        Focus();
                }
            }
        }

        protected override bool ProcessMnemonic(char charCode) =>
            ProcessKey(charCode, ModifierKeys.ToModifiers()) || base.ProcessMnemonic(charCode);

        protected override bool ProcessKeyMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR)
                ProcessMnemonic(Convert.ToChar(m.WParam.ToInt32()));

            return base.ProcessKeyMessage(ref m);
        }

        private bool ProcessKey(char ch, Modifiers keys)
        {
            if (Focused && (keys == Modifiers.None || keys == Modifiers.Shift))
            {
                switch (ch)
                {
                    case '\b':
                    case '\r':
                    case '\t':
                    case '\u001b': //Esc
                        return true;
                    default:
                        InputChar = ch;
                        RunCommand((Identifier)"editor.insertchar");
                        break;
                }

                return true;
            }

            return false;
        }

        internal void Redraw() => Invalidate();

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                case Keys.Tab:
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.Insert:
                case Keys.Control | Keys.Right:
                case Keys.Control | Keys.Left:
                case Keys.Control | Keys.Up:
                case Keys.Control | Keys.Down:
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                case Keys.Shift | Keys.Tab:
                case Keys.Control | Keys.PageDown:
                case Keys.Control | Keys.PageUp:
                case Keys.Control | Keys.Home:
                case Keys.Control | Keys.End:
                case Keys.Control | Keys.Shift | Keys.End:
                case Keys.Control | Keys.Shift | Keys.Home:
                    return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Autocomplete.WindowShown && Autocomplete.ListenKeys(e.KeyData))
                return;

            var inp = e.GetKeyInput();

            if (!inp.IsEmpty())
                RunCommand(inp);

            base.OnKeyDown(e);
        }

        public void RunCommand(KeyInput input)
        {
            if (KeyboardAdapter.Instance.ProcessInput(input) == InputState.Complete)
            {
                if (!RunCommand(KeyboardAdapter.Instance.LastKey))
                    OnCommandRejected();
            }
        }

        public event EventHandler CommandRejected;
        private void OnCommandRejected() => CommandRejected?.Invoke(this, EventArgs.Empty);

        internal bool RunCommand(Identifier commandKey, params object[] args)
        {
            return App.Ext.Run(this, commandKey, args);
        }

        public override Color BackColor => Theme.GetStyle(StandardStyle.Default).BackColor;

        public override Color ForeColor => Theme.GetStyle(StandardStyle.Default).ForeColor;

        public override Font Font => Settings.Font;

        public void DetachBuffer()
        {
            var buf = Buffer;

            if (buf != null)
            {
                buf.ScrollPosition = Scroll.ScrollPosition;
                buf.Views.Remove(this);
                MatchWords.ClearMatches();
                MatchBrackets.ClearMatches();
                Search.ClearMatches();
            }
        }

        public void AttachBuffer(IBuffer buf)
        {
            var buffer = buf as DocumentBuffer;

            if (buffer == null)
                throw new NotSupportedException();

            if (Buffer != null && buffer != Buffer)
                DetachBuffer();
            else if (Buffer == buffer)
                return;

            var @lock = Buffer?.ObtainLock();

            try
            {
                buffer.LastAccess = DateTime.Now;

                if (buffer != Buffer)
                {
                    buffer.Views.Add(this);
                    Buffer = buffer;
                }

                Buffer.GrammarKey = App.Ext.Grammars()
                    .GetGrammarByFile(buffer.File)?.Key;
                Scroll.InvalidateLines(InvalidateFlags.Force);
                Scroll.ScrollPosition = buffer.ScrollPosition;
                Styles.RestyleDocument();
                Folding.RebuildFolding(full: true);
                Redraw();
                MatchBrackets.Match();
                Search.HideSearch();
                TopMargins.ResetMargins();
                BottomMargins.ResetMargins();
                LeftMargins.ResetMargins();
                RightMargins.ResetMargins();
            }
            finally
            {
                if (@lock != null)
                    @lock.Release();
            }
        }

        void IView.Close()
        {
            FindForm().Close();
        }

        private DirectoryInfo _workspace;
        DirectoryInfo IView.Workspace
        {
            get { return _workspace; }
            set { _workspace = value; }
        }

        string IView.Mode
        {
            get { return Buffer.Mode; }
            set { Buffer.Mode = value; }
        }

        [Browsable(false)]
        public DocumentBuffer Buffer { get; private set; }

        [Browsable(false)]
        public Document Document => Buffer.Document;

        internal List<Line> Lines => Document.Lines;

        [Browsable(false)]
        public override string Text
        {
            get { return Buffer?.GetText(); }
            set
            {
                var doc = Document.FromString(value);
                var buffer = new DocumentBuffer(doc, new FileInfo("untitled"), Encoding.UTF8);
                AttachBuffer(buffer);
            }
        }

        [Browsable(false)]
        public bool WordWrap => Buffer.WordWrap ?? Settings.WordWrap;

        [Browsable(false)]
        public int WordWrapColumn => Buffer.WordWrapColumn ?? Settings.WordWrapColumn;

        [Browsable(false)]
        public bool UseTabs => Buffer.UseTabs ?? Settings.UseTabs;

        [Browsable(false)]
        public int IndentSize => Buffer.IndentSize ?? Settings.IndentSize;

        [Browsable(false)]
        public bool ShowEol => Buffer.ShowEol ?? Settings.ShowEol;

        [Browsable(false)]
        public ShowWhitespace ShowWhitespace => LimitedMode ? ShowWhitespace.None
            : (Buffer.ShowWhitespace ?? Settings.ShowWhitespace);

        [Browsable(false)]
        public bool ShowLineLength => !LimitedMode && (Buffer.ShowLineLength ?? Settings.ShowLineLength);

        [Browsable(false)]
        public bool ShowLineNumbers => !LimitedMode && Settings.ShowLineNumbers;

        [Browsable(false)]
        public bool CurrentLineIndicator => !LimitedMode && (Buffer.CurrentLineIndicator ?? Settings.CurrentLineIndicator);

        [Browsable(false)]
        public bool ReadOnly
        {
            get { return Buffer.ReadOnly; }
            set { Buffer.ReadOnly = value; }
        }

        [Browsable(false)]
        public EditorInfo Info { get; }

        [Browsable(false)]
        public EditorSettings Settings { get; }

        [Browsable(false)]
        public MarginList TopMargins { get; }

        [Browsable(false)]
        public MarginList LeftMargins { get; }

        [Browsable(false)]
        public MarginList RightMargins { get; }

        [Browsable(false)]
        public MarginList BottomMargins { get; }

        internal CaretRenderer CaretRenderer { get; }

        internal Renderer Renderer { get; }

        public bool LockMouseScrolling { get; set; }

        public Pos Caret { get; private set; }

        internal char InputChar { get; private set; }

        [Browsable(false)]
        public bool ThinCaret
        {
            get { return CaretRenderer.ThinCaret; }
            set { CaretRenderer.ThinCaret = value; }
        }

        [Browsable(false)]
        public StyleManager Styles { get; }

        [Browsable(false)]
        public ScrollingManager Scroll { get; }

        [Browsable(false)]
        public LocationManager Locations { get; }

        [Browsable(false)]
        public FoldingManager Folding { get; }

        [Browsable(false)]
        public CallTipManager CallTips { get; }

        [Browsable(false)]
        public MatchBracketManager MatchBrackets { get; }

        [Browsable(false)]
        public MatchWordManager MatchWords { get; }

        [Browsable(false)]
        public AutocompleteManager Autocomplete { get; }

        [Browsable(false)]
        public AffinityManager AffinityManager { get; }

        [Browsable(false)]
        public int FirstEditLine { get; set; }

        [Browsable(false)]
        public int LastEditLine { get; set; }

        [Browsable(false)]
        public bool LimitedMode { get; set; }

        [Browsable(false)]
        public SearchManager Search { get; }

        [Browsable(false)]
        public IThemeComponent Theme { get; }

        IBuffer IView.Buffer => Buffer;

        public event EventHandler<TextEventArgs> BeforePaste;
        internal bool HasBeforePaste => BeforePaste != null;
        internal void OnBeforePaste(TextEventArgs e) => BeforePaste?.Invoke(this, e);

        public event EventHandler ContentModified;
        internal bool HasContentModified => ContentModified != null;
        internal void OnContentModified() => ContentModified?.Invoke(this, EventArgs.Empty); 
    }
}
