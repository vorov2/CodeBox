﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommandComponent))]
    [CommandComponentData("editor.extenddown", "esd")]
    public sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => DownCommand.MoveDown(View, sel);
    }
}
