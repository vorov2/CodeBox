﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Folding;
using CodeBox.Drawing;
using CodeBox.Core.Themes;

namespace CodeBox.Margins
{
    public sealed class LineNumberMargin : GutterMargin
    {
        public LineNumberMargin(Editor editor) : base(editor)
        {

        }

        protected override bool OnDraw(Graphics g, Rectangle bounds)
        {
            OnSizeChanged();
            var sc = Editor.Scroll.ScrollPosition;
            var lns = Editor.Theme.GetStyle(StandardStyle.LineNumbers);
            var alns = Editor.Theme.GetStyle(StandardStyle.CurrentLineNumber);

            var lines = Editor.Document.Lines;
            var info = Editor.Info;
            var len = lines.Count.ToString().Length;
            var caret = Editor.Buffer.Selections.Main.Caret;
            var backBrush = lns.BackColor.Brush();
            var font = Editor.Settings.Font.Get(lns.FontStyle);
            g.FillRectangle(backBrush, bounds);

            for (var i = Editor.Scroll.FirstVisibleLine; i < Editor.Scroll.LastVisibleLine + 1; i++)
            {
                var line = lines[i];
                var x = bounds.X + info.CharWidth;
                var y = line.Y + sc.Y + info.TextTop;

                if (line.Folding.Has(FoldingStates.Invisible))
                    continue;

                if (line.Y >= sc.Y && y >= bounds.Y)
                {
                    var str = (i + 1).ToString().PadLeft(len);
                    var col = lns.ForeColor.Brush();

                    if (i == caret.Line && MarkCurrentLine)
                    {
                        var selBrush = alns.BackColor.Brush();

                        if (selBrush != backBrush)
                            g.FillRectangle(selBrush, new Rectangle(bounds.X, y, bounds.Width, info.LineHeight));

                        col = alns.ForeColor.Brush();
                    }

                    g.DrawString(str, font, col, x, y, TextFormats.Compact);
                }
            }

            return true;
        }

        public override int CalculateSize() =>
            (Editor.Document.Lines.Count.ToString().Length + 2) * Editor.Info.CharWidth;

        public bool MarkCurrentLine { get; set; }
    }
}
