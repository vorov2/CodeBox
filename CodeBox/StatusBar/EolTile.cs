﻿using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using System;

namespace CodeBox.StatusBar
{
    public sealed class EolTile : StatusBarTile
    {
        private readonly Editor editor;

        public EolTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get
            {
                return editor.Buffer.Eol.ToString().ToUpper();
            }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.Run(editor, Cmd.SetBufferEol);
            base.PerformClick();
        }
    }

}
