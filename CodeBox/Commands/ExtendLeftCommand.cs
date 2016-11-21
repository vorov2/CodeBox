﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.extendleft", "esl")]
    public sealed class ExtendLeftCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => LeftCommand.MoveLeft(Document, sel);

        internal override bool SupportLimitedMode => true;
    }
}
