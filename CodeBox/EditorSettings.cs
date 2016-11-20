﻿using CodeBox.Drawing;
using CodeBox.Affinity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Indentation;
using System.Windows.Forms;

namespace CodeBox
{
    public sealed class EditorSettings : IDocumentAffinity
    {
        private const string SEPS = "`~!@#$%^&*()-=+[{]}\\|;:'\",.<>/?";
        private const string BRACKETS = "()[]{}";

        public EditorSettings()
        {
            //Defaults
            NonWordSymbols = SEPS;
            BracketSymbols = BRACKETS;
            Font = new Font(FontFamily.GenericMonospace, 11);
            LongLineIndicators = new List<int>();
        }

        #region IDocumentAffinity
        public string NonWordSymbols { get; set; }

        public string BracketSymbols { get; set; }

        public NumberLiteral NumberLiteral { get; set; }

        string IDocumentAffinity.CommentMask { get; }

        public string IndentComponentKey { get; set; }

        public string FoldingComponentKey { get; set; }

        public string AutocompleteSymbols { get; set; }
        #endregion

        public bool MatchBrackets { get; set; }

        public bool WordWrap { get; set; }

        public int WordWrapColumn { get; set; }

        public Eol Eol { get; set; }

        public bool UseTabs { get; set; }

        public int IndentSize { get; set; }

        public double LinePadding { get; set; }

        public bool ShowEol { get; set; }

        public bool ShowWhitespace { get; set; }

        public bool ShowLineLength { get; set; }

        public bool CurrentLineIndicator { get; set; }

        internal int CharWidth { get; private set; }
        internal int CharHeight { get; private set; }
        internal int SmallCharWidth { get; private set; }
        internal int SmallCharHeight { get; private set; }

        private Font _font = SystemFonts.DefaultFont;
        public Font Font
        {
            get { return _font; }
            set
            {
                if (value != _font)
                {
                    FontExtensions.Clean(_font);
                    _font = value;
                    SmallFont = new Font(value.Name, value.Size - 1, value.Style);

                    using (var ctl = new Control())
                    using (var g = ctl.CreateGraphics())
                    {
                        var size1 = g.MeasureString("<W>", value);
                        var size2 = g.MeasureString("<>", value);
                        CharWidth = (int)(size1.Width - size2.Width);
                        CharHeight = (int)value.GetHeight(g);
                    }
                }
            }
        }

        private Font _smallFont;
        internal Font SmallFont
        {
            get { return _smallFont; }
            private set
            {
                if (value != _smallFont)
                {
                    FontExtensions.Clean(_smallFont);
                    _smallFont = value;

                    using (var ctl = new Control())
                    using (var g = ctl.CreateGraphics())
                    {
                        var size1 = g.MeasureString("<F>", value);
                        var size2 = g.MeasureString("<>", value);
                        SmallCharWidth = (int)(size1.Width - size2.Width);
                        SmallCharHeight = (int)value.GetHeight(g);
                    }
                }
            }
        }
        
        public List<int> LongLineIndicators { get; }
    }

    public enum Eol
    {
        Auto = 0,

        Cr,

        Lf,

        CrLf
    }

    internal static class EolExtensions
    {
        public static string AsString(this Eol eol)
        {
            switch (eol)
            {
                case Eol.Cr: return "\r";
                case Eol.Lf: return "\n";
                case Eol.CrLf: return "\r\n";
                default: return Environment.NewLine;
            }
        }
    }
}
