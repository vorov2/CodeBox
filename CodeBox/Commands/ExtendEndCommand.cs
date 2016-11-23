﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendend")]
    public sealed class ExtendEndCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => EndCommand.MoveEnd(Document, sel.Caret);

        internal override bool SupportLimitedMode => true;
    }
}
