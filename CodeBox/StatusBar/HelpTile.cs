﻿using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.StatusBar
{
    public sealed class HelpTile : StatusBarTile
    {
        private readonly Editor editor;

        public HelpTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get { return "?"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            App.Ext.RunCommand(editor, (Identifier)"test.commandpalette");
            base.PerformClick();
        }
    }
}
