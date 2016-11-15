﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;

namespace CodeBox.Commands
{
    public abstract class CaretCommand : Command
    {
        public override ActionResults Execute(Selection sel)
        {
            var pos = GetPosition(sel);
            sel.Clear(pos);
            Buffer.Selections.ValidateCaret(sel, Document);
            return Clean | Scroll;
        }

        protected abstract Pos GetPosition(Selection sel);
    }
}
