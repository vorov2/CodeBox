﻿using System;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.paste")]
    public sealed class PasteCommand : InsertRangeCommand
    {
        protected override ActionResults Execute(Selection sel)
        {
            var str = Clipboard.GetText();
            base.insertString = str.MakeCharacters();
            return base.Execute(sel);
        }

        internal override EditorCommand Clone()
        {
            return new PasteCommand();
        }
    }
}
