﻿using CodeBox.Commands;
using CodeBox.Styling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBox.CallTips;
using System.Text;
using CodeBox.Core.ViewModel;
using System.IO;
using System.Drawing;
using CodeBox.Core.Output;

namespace CodeBox.ObjectModel
{
    public class DocumentBuffer : IMaterialBuffer
    {
        private int counter;
        private bool undoGroup;
        private EditorLock editorLock;
        private volatile int lockCount;

        private sealed class EditorLock : IEditorLock
        {
            private readonly DocumentBuffer man;

            internal EditorLock(DocumentBuffer man)
            {
                this.man = man;
            }

            public void Release()
            {
                man.lockCount--;
            }
        }

        public DocumentBuffer(Document doc, FileInfo file, Encoding encoding)
        {
            Document = doc;
            Selections = new SelectionList(doc);
            UndoStack = new LimitedStack<CommandInfo>();
            RedoStack = new LimitedStack<CommandInfo>();
            Tips = new List<CallTip>();
            Encoding = encoding;
            File = file;
            editorLock = new EditorLock(this);
        }

        public string GetText()
        {
            var @lock = ObtainLock();

            try
            {
                return string.Join(Eol.AsString(), Document.Lines.Select(ln => ln.Text));
            }
            finally
            {
                @lock.Release();
            }
        }

        public string GetContents() => GetText();

        public void Truncate(string text = "")
        {
            var @lock = ObtainLock();

            try
            {
                Document.Lines.Clear();
                Document.Lines.Add(Line.FromString(text));
                Selections.Set(new Pos(0, 0));
                Edits = 0;

                foreach (var v in Views)
                    v.AttachBuffer(this);
            }
            finally
            {
                @lock.Release();
            }
        }

        public IEditorLock ObtainLock()
        {
            lockCount++;
            return editorLock;
        }

        public bool BeginUndoAction()
        {
            if (!undoGroup)
            {
                counter++;
                undoGroup = true;
                return true;
            }

            return false;
        }

        public void EndUndoAction() => undoGroup = false;

        internal void AddCommand(EditorCommand cmd) =>
            UndoStack.Push(new CommandInfo { Id = counter, Command = cmd });

        public void Serialize(Stream stream)
        {
            throw new NotSupportedException();
        }

        public void ClearDirtyFlag()
        {
            Edits = 0;
            LastAtomicChange = false;
            RequestRedraw();
        }

        internal readonly List<Editor> Views = new List<Editor>();
        public void RequestRedraw()
        {
            foreach (var e in Views)
                e.Redraw();
        }

        internal void InvalidateLines()
        {
            foreach (var e in Views)
                e.Scroll.InvalidateLines();
        }

        internal void ScrollToCaret()
        {
            foreach (var e in Views)
                e.Scroll.UpdateVisibleRectangle();
        }

        public Point ScrollPosition { get; internal set; }

        internal bool LastAtomicChange { get; set; }

        public bool Locked => lockCount > 0;

        internal List<CallTip> Tips { get; }

        internal LimitedStack<CommandInfo> UndoStack { get; }

        internal LimitedStack<CommandInfo> RedoStack { get; }

        public Document Document { get; internal set; }

        public SelectionList Selections { get; }

        internal int Edits { get; set; }

        public bool Overtype { get; set; }

        private bool? _wordWrap;
        public bool? WordWrap
        {
            get { return _wordWrap; }
            set
            {
                _wordWrap = value;

                foreach (var v in Views)
                {
                    v.Scroll.ScrollPosition = new Point(0, v.Scroll.ScrollPosition.Y);
                    v.Scroll.InvalidateLines(InvalidateFlags.Force);
                    v.Redraw();
                }
            }
        }

        public int? WordWrapColumn { get; set; }

        public bool? UseTabs { get; set; }

        public int? IndentSize { get; set; }

        public bool? ShowEol { get; set; }

        public ShowWhitespace? ShowWhitespace { get; set; }

        public bool? ShowLineLength { get; set; }

        public bool? CurrentLineIndicator { get; set; }

        private bool _readOnly;
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (value != _readOnly)
                {
                    _readOnly = value;
                    RequestRedraw();
                }
            }
        }

        public bool IsDirty
        {
            get { return Edits > 0; }
        }

        private Eol _eol;
        public Eol Eol
        {
            get
            {
                return _eol == Eol.Auto ? Document.OriginalEol : _eol;
            }
            set { _eol = value; }
        }

        public string GrammarKey { get; set; }

        public FileInfo File { get; internal set; }

        public Encoding Encoding { get; set; }

        public DateTime LastAccess { get; set; }

        public string Mode
        {
            get { return GrammarKey; }
            set
            {
                GrammarKey = value;

                foreach (var ed in Views)
                    ed.Styles.RestyleDocument();
            }
        }
    }
}
