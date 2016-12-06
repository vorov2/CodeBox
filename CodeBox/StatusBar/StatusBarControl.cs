﻿using CodeBox.Drawing;
using CodeBox.Margins;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.StatusBar
{
    public sealed class StatusBarControl : Control
    {
        public StatusBarControl(Editor editor)
        {
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.FixedHeight, true);
            Cursor = Cursors.Default;
            Editor = editor;
            Height = /*SysFont.Font.Height*/editor.Info.LineHeight + Dpi.GetHeight(4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = e.ClipRectangle;
            var style = (MarginStyle)Editor.Theme.GetStyle(StandardStyle.StatusBar);
            g.FillRectangle(style.BackColor.Brush(), bounds);

            var ys = Dpi.GetHeight(2);
            g.FillRectangle(style.BackColor.Brush(),
                new Rectangle(bounds.X, bounds.Y, bounds.Width, ys));
            var pad = Dpi.GetWidth(6);
            var space = Editor.Info.SmallCharWidth;

            var lefts = Tiles.Where(t => t.Alignment == TileAlignment.Left);
            var x = bounds.X + pad;

            foreach (var tile in lefts)
            {
                var foreColor = style.ForeColor;
                tile.Font = Editor.Settings.SmallFont;//SysFont.Font;//
                var width = tile.MeasureWidth(g);

                if (x + width > bounds.Width)
                    break;

                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys * 2);

                if (tile.Hover)
                {
                    g.FillRectangle(style.ActiveBackColor.Brush(), rect);
                    foreColor = style.ActiveForeColor;
                }

                tile.Draw(g, foreColor, rect);
                tile.Left = x;
                tile.Right = x + width;
                x += width + space;
            }

            var maxx = x;
            var rights = Tiles.Where(t => t.Alignment == TileAlignment.Right);
            x = bounds.X + bounds.Width - pad;

            foreach (var tile in rights)
            {
                var foreColor = style.ForeColor;
                tile.Font = Editor.Settings.SmallFont;//SysFont.Font; //
                var width = tile.MeasureWidth(g);
                x -= width;

                if (x < maxx)
                    break;

                var rect = new Rectangle(x, bounds.Y + ys, width, bounds.Height - ys*2);

                if (tile.Hover)
                {
                    g.FillRectangle(style.ActiveBackColor.Brush(), rect);
                    foreColor = style.ActiveForeColor;
                }

                tile.Left = x;
                tile.Right = x + width;
                tile.Draw(g, foreColor, rect);
                x -= space;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (tile == hover)
                return;

            if (hover != null)
                hover.Hover = false;

            if (tile != null)
                tile.Hover = true;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            var hover = Tiles.FirstOrDefault(t => t.Hover);

            if (hover != null)
            {
                hover.Hover = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var tile = Tiles.FirstOrDefault(t => e.X >= t.Left && e.X <= t.Right);

            if (tile != null)
                tile.PerformClick();
        }

        public Editor Editor { get; }

        public List<StatusBarTile> Tiles { get; } = new List<StatusBarTile>();
    }
}