﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Slot.Core;
using Slot.Core.Themes;
using Slot.Drawing;
using Slot.Editor;

namespace Slot.Main.CommandBar
{
    public sealed class MessageOverlay : Overlay
    {
        private ITheme theme;

        public MessageOverlay(EditorControl editor)
        {
            theme = editor.Theme;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var st = theme.GetStyle(StandardStyle.Popup);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(Text, Font, st.ForeColor.Brush(),
                new Rectangle(
                    Padding.Left + BorderWidth,
                    Padding.Top + BorderWidth,
                    Width - Padding.Right - Padding.Left - BorderWidth * 2,
                    Height - Padding.Top - Padding.Bottom - BorderWidth * 2),
                TextFormats.Wrap);
            e.Graphics.FillRectangle(theme.GetStyle(StandardStyle.Error).ForeColor.Brush(),
                new Rectangle(
                    Padding.Left + BorderWidth,
                    Padding.Top + BorderWidth
                        + (int)Math.Round((Font.Height() - Font.Width()) / 2d, MidpointRounding.AwayFromZero),
                    Font.Width(),
                    Font.Width()));
        }

        public int BorderWidth => Dpi.GetWidth(1);

        public override Color BackgroundColor => theme.GetStyle(StandardStyle.Popup).BackColor;

        public override Color BorderColor => theme.GetStyle(StandardStyle.PopupBorder).ForeColor;
    }
}
