﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.deleteword", "eedw")]
    public sealed class DeleteWordCommand : DeleteCommand
    {
        private Selection redoSel;

        internal override ActionResults Execute(Selection sel, object arg = null)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == ln.Length)
                return base.Execute(sel);

            var aff = View.AffinityManager.GetAffinity(sel.Caret);
            var seps = aff.NonWordSymbols ?? View.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col));
            var col = SelectWordCommand.FindBoundRight(seps, ln, sel.Caret.Col, st);
            var newSel = new Selection(sel.Caret, new Pos(sel.Caret.Line, col));
            redoSel = newSel;
            return base.Execute(newSel);
        }

        internal override EditorCommand Clone()
        {
            return new DeleteWordCommand();
        }

        public override ActionResults Redo(out Pos pos)
        {
            Execute(redoSel);
            pos = undoPos;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            var ret = base.Undo(out pos);
            pos = base.undoPos;
            return ret;
        }

        internal override bool ModifyContent => true;

        internal override bool SupportLimitedMode => true;
    }
}
