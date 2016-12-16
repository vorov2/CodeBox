﻿using System;
using Slot.Editor.ObjectModel;
using Slot.ComponentModel;
using System.ComponentModel.Composition;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.undo")]
    public sealed class UndoCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            return Pure | NeedUndo | KeepRedo;
        }

        internal override bool SingleRun => true;
    }
}
