﻿using System;
using CodeBox.ObjectModel;
using System.Drawing;
using CodeBox.Folding;
using CodeBox.Commands;

namespace CodeBox
{
    [Flags]
    public enum InvalidateFlags
    {
        None = 0x00,
        Atomic = 0x01,
        Force = 0x02
    }
    public sealed class ScrollingManager
    {
        private readonly Editor editor;
        private Point pointer;

        internal ScrollingManager(Editor editor)
        {
            this.editor = editor;
        }

        internal bool UpdateVisibleRectangle()
        {
            var caret = editor.Buffer.Selections.Main.Caret;

            if (caret.Line >= editor.Document.Lines.Count)
                return false;

            var sv = IsLineStripeVisible(caret);
            var update = false;

            if (sv != 0)
            {
                var sign = Math.Sign(sv);

                if (Math.Abs(sv) > 1 && caret.Line + sign < editor.Lines.Count
                    && sign < 0 && IsLineStripeVisible(new Pos(caret.Line + sign, caret.Col)) == 0)
                    sv = sign;

                ScrollY(sv);
                update = true;
            }

            if (!editor.WordWrap)
            {
                sv = IsColumnVisible(caret);

                if (sv != 0)
                {
                    var sign = Math.Sign(sv);

                    if (Math.Abs(sv) > 10 && IsColumnVisible(new Pos(caret.Line, caret.Col + sign * 10)) == 0)
                        sv = sign * 10;

                    ScrollX(sv);
                    update = true;
                }
            }

            return update;
        }

        private int IsColumnVisible(Pos pos)
        {
            var tetras = editor.Lines[pos.Line].GetTetras(pos.Col, editor.IndentSize);
            var curpos = tetras * editor.Info.CharWidth + editor.Info.TextLeft + ScrollPosition.X;

            if (curpos > editor.Info.TextRight)
            {
                curpos += editor.Info.CharWidth;
                var diff = curpos - editor.Info.TextRight;
                return -(int)(Math.Ceiling((double)diff / editor.Info.CharWidth));
            }
            else if (curpos < editor.Info.TextLeft)
                return (int)Math.Ceiling((editor.Info.TextLeft - curpos) / (double)editor.Info.CharWidth);
            else
                return 0;
        }

        private int IsLineStripeVisible(Pos pos)
        {
            var ln = editor.Document.Lines[pos.Line];
            var stripe = ln.GetStripe(pos.Col);
            var cy = ln.Y + stripe * editor.Info.LineHeight + ScrollPosition.Y;

            if (cy < 0)
                return -(cy / editor.Info.LineHeight);
            else if (cy + editor.Info.LineHeight > editor.Info.TextBottom)
                return -((cy - editor.Info.TextTop) / editor.Info.LineHeight);
            else
                return 0;
        }

        public void ScrollY(int times) => SetScrollPositionY(ScrollPosition.Y + times * editor.Info.LineHeight);

        public void ScrollX(int times) => SetScrollPositionX(ScrollPosition.X + times * editor.Info.CharWidth);

        public void SetScrollPositionY(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -ScrollBounds.Height)
                value = -ScrollBounds.Height;

            //scroll by whole lines
            var lines = (int)Math.Round((double)value / editor.Info.LineHeight);
            value = lines * editor.Info.LineHeight;
            var change = ScrollPosition.Y - value;
            ScrollPosition = new Point(ScrollPosition.X, value);
            ResetFirstLast();
            OnScroll(0, change);
        }

        public void SetScrollPositionX(int value)
        {
            if (value > 0)
                value = 0;

            if (value < -ScrollBounds.Width)
                value = -ScrollBounds.Width;

            //scroll by whole chars
            var chars = (int)Math.Round((double)value / editor.Info.CharWidth);
            value = chars * editor.Info.CharWidth;
            var change = ScrollPosition.X - value;
            ScrollPosition = new Point(value, ScrollPosition.Y);
            OnScroll(change, 0);
        }

        private void OnScroll(int xChange, int yChange)
        {
            if (!SuppressOnScroll)
            {
                editor.Redraw();

                if (editor.Autocomplete.WindowShown)
                    editor.Autocomplete.ShiftLocation(xChange, yChange);
            }
        }

        private int CalculateFirstVisibleLine()
        {
            var stripes = Math.Abs(ScrollPosition.Y / editor.Info.LineHeight);

            if (stripes == 0)
                return -1;

            var len = editor.Document.Lines.Count;
            var cs = 0;

            for (var i = 0; i < len; i++)
            {
                var ln = editor.Lines[i];

                if (ln.Folding.Has(FoldingStates.Invisible))
                    continue;

                cs += ln.Stripes;

                if (cs == stripes)
                    return i;
                else if (cs > stripes)
                    return i - 1;
            }

            return len - 1;
        }

        private int? CalculateLastVisibleLine()
        {
            var len = editor.Document.Lines.Count;
            var lh = editor.Info.LineHeight;
            var maxh = editor.Info.TextBottom - editor.Info.TextTop - ScrollPosition.Y;

            for (var i = FirstVisibleLine; i < len; i++)
            {
                var ln = editor.Document.Lines[i];

                if (ln.Folding.Has(FoldingStates.Invisible))
                    continue;

                var lnEnd = ln.Y + ln.Stripes * lh;

                if (ln.Y >= maxh)
                    return i > FirstVisibleLine ? i - 1 : i;
            }

            return len > FirstVisibleLine ? (int?)len - 1 : null;
        }

        private void FastInvalidateLines()
        {
            var ln = editor.Lines[editor.Buffer.Selections.Main.Caret.Line];
            var w = ln.GetTetras(editor.IndentSize) * editor.Info.CharWidth
                - editor.Info.TextWidth + editor.Info.CharWidth * 5;

            if (w > ScrollBounds.Width)
                ScrollBounds = new Size(w, ScrollBounds.Height);
        }

        internal void InvalidateLines(InvalidateFlags flags = InvalidateFlags.None)
        {
            var dt = DateTime.Now;

            var startLine = 0;
            var endLine = editor.Lines.Count;
            var forced = (flags & InvalidateFlags.Force) == InvalidateFlags.Force;

            if (!editor.WordWrap)
            {
                if ((flags & InvalidateFlags.Atomic) == InvalidateFlags.Atomic)
                {
                    FastInvalidateLines();
                    return;
                }

                var maxHeight = 0;
                var maxWidth = 0;
                var y = 0;

                for (var i =  startLine; i < endLine; i++)
                {
                    var ln = editor.Lines[i];

                    if (ln.Folding.Has(FoldingStates.Invisible))
                        continue;

                    ln.Y = y;
                    var w = ln.GetTetras(editor.IndentSize) * editor.Info.CharWidth;
                    y += editor.Info.LineHeight;

                    if (w > maxWidth)
                        maxWidth = w;

                    maxHeight += editor.Info.LineHeight;
                }

                var xmax = maxWidth - editor.Info.TextWidth + editor.Info.CharWidth * 5;
                ScrollBounds = new Size(xmax < 0 ? 0 : xmax, maxHeight);
            }
            else
            {
                var maxHeight = 0;
                var twidth = editor.WordWrapColumn > 0 ? editor.WordWrapColumn * editor.Info.CharWidth
                    : editor.Info.TextWidth;

                for (var i = startLine; i < endLine; i++)
                {
                    var ln = editor.Lines[i];

                    if (ln.Folding.Has(FoldingStates.Invisible))
                        continue;

                    if (!ln.Invalidated || forced)
                        ln.RecalculateCuts(twidth, editor.Info.CharWidth, editor.IndentSize);
                    ln.Y = maxHeight;
                    maxHeight += ln.Stripes * editor.Info.LineHeight;
                }

                ScrollBounds = new Size(0, maxHeight);
                ResetFirstLast();
            }

            var ymax = ScrollBounds.Height - editor.Info.TextHeight + editor.Info.LineHeight * 5;
            ScrollBounds = new Size(ScrollBounds.Width, ymax < 0 ? 0 : ymax);
            _firstVisibleLine = null;
            _lastVisibleLine = null;
        }

        private void ResetFirstLast()
        {
            _firstVisibleLine = null;
            _lastVisibleLine = null;
        }

        internal void OnPointerDown(Point loc) => pointer = loc;

        internal void OnPointerUp(Point loc) => pointer = default(Point);

        internal void OnPointerUpdate(Point loc)
        {
            var diffY = Math.Abs(loc.Y - pointer.Y);
            var diffX = Math.Abs(loc.X - pointer.X);

            if (diffY < editor.Info.LineHeight && diffX < editor.Info.CharWidth)
                return;

            if (diffY > diffX)
                ScrollY((loc.Y - pointer.Y) / editor.Info.LineHeight);
            else
                ScrollX((loc.X - pointer.X) / editor.Info.CharWidth);

            pointer = loc;
        }

        private int? _firstVisibleLine;
        public int FirstVisibleLine
        {
            get
            {
                if (_firstVisibleLine == null)
                {
                    _firstVisibleLine = CalculateFirstVisibleLine() + 1;

                    if (_firstVisibleLine.Value >= editor.Lines.Count)
                        _firstVisibleLine = editor.Lines.Count - 1;
                }

                return _firstVisibleLine.Value;
            }
        }

        private int? _lastVisibleLine;
        public int LastVisibleLine
        {
            get
            {
                if (_lastVisibleLine == null)
                {
                    _lastVisibleLine = CalculateLastVisibleLine();

                    if (_lastVisibleLine.Value < _firstVisibleLine.Value)
                        _lastVisibleLine = _firstVisibleLine;
                }

                return _lastVisibleLine != null ? _lastVisibleLine.Value : 0;
            }
        }

        public Point ScrollPosition { get; private set; }

        public Size ScrollBounds { get; private set; }

        internal bool SuppressOnScroll { get; set; }
    }
}
