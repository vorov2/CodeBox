﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.extendpagedown")]
    public sealed class ExtendPageDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => PageDownCommand.PageDown(View);
    }
}
