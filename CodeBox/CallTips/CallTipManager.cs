﻿using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CodeBox.CallTips
{
    public sealed class CallTipManager
    {
        private readonly Editor editor;
        private Pos shownTip = Pos.Empty;
        private CallTip lastTip;
        private static readonly StringFormat format = new StringFormat(StringFormat.GenericTypographic)
        {
            LineAlignment = StringAlignment.Near,
            Alignment = StringAlignment.Near,
            Trimming = StringTrimming.None
        };

        class TipInfo
        {
            public Size Size;
            public List<List<Char>> Lines;
        }

        struct Char
        {
            public Char(char val, FontStyle style, Color color)
            {
                Val = val;
                Style = style;
                Color = color;
            }

            public char Val;
            public FontStyle Style;
            public Color Color;
        }

        internal CallTipManager(Editor editor)
        {
            this.editor = editor;
        }

        public void ShowCallTip(Pos pos, string text)
        {
            using (var g = editor.CreateGraphics())
            {
                var info = ProcessHtmlString(text);
                ShowCallTip(g, pos, info.Size, (gr,pt) => DrawTip(info, gr, pt));
            }
        }

        private void DrawTip(TipInfo info, Graphics g, Point pt)
        {
            var x = pt.X + editor.Info.SmallCharWidth;
            var y = pt.Y + editor.Info.SmallCharHeight / 2;
            var w = editor.Info.SmallCharWidth;

            foreach (var ln in info.Lines)
            {
                foreach (var c in ln)
                {
                    var font = editor.CachedSmallFont.Create(c.Style);
                    var brush = editor.CachedBrush.Create(c.Color);
                    g.DrawString(c.Val.ToString(), font, brush, x, y, format);
                    x += editor.Info.SmallCharWidth;
                }

                y += editor.Info.SmallCharHeight;
                x = pt.X + editor.Info.SmallCharWidth;
            }
        }

        private TipInfo ProcessHtmlString(string text)
        {
            var font = editor.Settings.SmallFont;
            var cw = editor.Info.SmallCharWidth;
            var max = editor.Info.TextWidth / 2;
            var width = 0;
            var maxwidth = 0;
            var height = 0;
            var chars = new List<Char>();
            var lines = new List<List<Char>>();
            var style = default(FontStyle);
            var color = default(Color);

            var doc = new XmlDocument();
            doc.LoadXml("<tip>" + text.Replace("<br>", "<br/>") + "</tip>");
            var node = doc.FirstChild;

            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                var cn = node.ChildNodes[i];
                var str = "";
                var endline = i == node.ChildNodes.Count - 1
                    || node.ChildNodes[i + 1].Name.Equals("br", StringComparison.OrdinalIgnoreCase);

                if (cn.NodeType == XmlNodeType.Text)
                {
                    str = cn.Value;
                    style = FontStyle.Regular;
                    color = editor.Settings.PopupForeColor;
                }
                else if (cn.NodeType == XmlNodeType.Element)
                {
                    str = cn.InnerText;

                    if (cn.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                    {
                        style = FontStyle.Bold;
                        color = editor.Settings.PopupForeColor;
                    }
                    else if (cn.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        style = FontStyle.Italic;
                        color = editor.Settings.PopupForeColor;
                    }
                    else if (!cn.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
                    {
                        var ss = StandardStyle.Default;

                        if (Enum.TryParse(cn.Name, true, out ss))
                        {
                            var so = editor.Styles.GetStyle((int)ss);
                            style = so.FontStyle;
                            color = so.ForeColor;
                        }
                    }
                    else if (endline)
                    {
                        width = 0;
                        height += editor.Info.SmallCharHeight;
                        lines.Add(chars);
                        chars = new List<Char>();
                    }
                }

                for (var j = 0; j < str.Length; j++)
                {
                    var c = str[j];
                    if (c == '\r'
                        || c == '\n'
                        || c == '\t'
                        || (c == ' ' && width == 0 && str.Length > 0))
                        continue;

                    width += cw;
                    chars.Add(new Char(c, style, color));

                    if (width >= max || j == str.Length - 1 && endline)
                    {
                        if (width > maxwidth)
                            maxwidth = width;

                        width = 0;
                        height += editor.Info.SmallCharHeight;
                        lines.Add(chars);
                        chars = new List<Char>();
                    }
                }
            }

            return new TipInfo
            {
                Size = new Size(maxwidth + cw * 2, height + editor.Info.SmallCharHeight),
                Lines = lines
            };
        }

        private TipInfo MeasureString(string text)
        {
            var font = editor.Settings.SmallFont;
            var cw = editor.Info.SmallCharWidth;
            var max = editor.Info.TextWidth / 2;
            var width = 0;
            var maxwidth = 0;
            var height = 0;
            var chars = new List<Char>();
            var lines = new List<List<Char>>();
            var style = FontStyle.Regular;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var ctx = i == 0
                    || text[i - 1] == ' '
                    || text[i - 1] == '\n'
                    || text[i - 1] == '\t';
                var nctx = i == text.Length - 1
                    || text[i + 1] == ' '
                    || text[i + 1] == '\n'
                    || text[i + 1] == '\t';
                var skip = false;

                if (ctx && c == '*')
                {
                    style = FontStyle.Bold;
                    skip = true;
                }
                else if (ctx && c == '_')
                {
                    style = FontStyle.Italic;
                    skip = true;
                }
                else if (nctx && c == '*' && style == FontStyle.Bold)
                {
                    style = FontStyle.Regular;
                    skip = true;
                }
                else if (nctx && c == '_' && style == FontStyle.Italic)
                {
                    style = FontStyle.Regular;
                    skip = true;
                }

                if (c == '\r'
                    || (c == ' ' && width == 0)
                    || (c == '\t' && width == 0)
                    || skip)
                    continue;

                if (c != '\n')
                {
                    width += cw;
                    chars.Add(new Char(c == '\t' ? ' ' : c, style, editor.Settings.PopupForeColor));
                }

                if (width >= max || c == '\n' || i == text.Length - 1)
                {
                    if (width > maxwidth)
                        maxwidth = width;

                    width = 0;
                    height += editor.Info.SmallCharHeight;
                    lines.Add(chars);
                    chars = new List<Char>();
                }
            }

            return new TipInfo
            {
                Size = new Size(maxwidth + cw * 2, height + editor.Info.SmallCharHeight),
                Lines = lines
            };
        }

        public void ShowCallTip(Pos pos, Size size, Action<Graphics,Point> draw)
        {
            using (var g = editor.CreateGraphics())
                ShowCallTip(g, pos, size, draw);
        }

        public void ShowCallTip(Graphics g, Pos pos, Size size, Action<Graphics,Point> draw)
        {
            if (shownTip != Pos.Empty && shownTip != pos)
                HideCallTip();

            if (pos != shownTip)
            {
                var ln = editor.Lines[pos.Line];
                var x = 0;
                var p = 0;

                foreach (var c in ln)
                {
                    x += c.Char == '\t' ? editor.TabSize * editor.Info.CharWidth : editor.Info.CharWidth;

                    if (p++ == pos.Col)
                        break;
                }

                x = x + editor.Info.TextLeft + editor.Scroll.X;
                var y = ln.Y + editor.Info.TextTop + editor.Scroll.Y + editor.Info.LineHeight;

                if (y + size.Height > editor.Info.TextHeight)
                    y -= editor.Info.LineHeight + size.Height;

                if (x + size.Width > editor.Info.TextWidth)
                    x -= size.Width;

                var pt = new Point(x, y);
                var rect = new Rectangle(pt, size);
                g.FillRectangle(editor.CachedBrush.Create(editor.Settings.PopupBackColor), rect);
                g.DrawRectangle(editor.CachedPen.Create(editor.Settings.PopupBorderColor), rect);
                draw(g, pt);
                shownTip = pos;
            }
        }

        public void HideCallTip()
        {
            if (shownTip != Pos.Empty)
            {
                editor.Refresh();
                shownTip = Pos.Empty;
            }
        }

        public void ClearCallTips() => editor.Buffer.Tips.Clear();

        public void BindCallTip(string data, Pos start, Pos end) =>
            editor.Buffer.Tips.Add(new CallTip(data, start, end));

        public CallTip FindCallTip(Pos pos)
        {
            foreach (var c in editor.Buffer.Tips)
                if (pos >= c.Start && pos <= c.End)
                    return c;

            return CallTip.Empty;
        }

        internal void MouseDwell(Pos p)
        {
            var ln = editor.Lines[p.Line];
            var tip = false;
            var ct = FindCallTip(p);

            if (ct != CallTip.Empty)
            {
                if (ct.Start != lastTip.Start || ct.End != lastTip.End)
                {
                    ShowCallTip(p, ct.Data);
                    lastTip = ct;
                }

                tip = true;
            }

            if (!tip)
            {
                HideCallTip();
                lastTip = CallTip.Empty;
            }
        }
    }
}
