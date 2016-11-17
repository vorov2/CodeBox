﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.deletewordback")]
    public sealed class DeleteWordBackCommand : DeleteBackCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            var ln = Document.Lines[sel.Caret.Line];

            if (sel.Caret.Col == 0)
                return base.Execute(sel);

            var seps = Context.Settings.NonWordSymbols;
            var st = SelectWordCommand.GetStrategy(seps, ln.CharAt(sel.Caret.Col - 1));
            var col = SelectWordCommand.FindBoundLeft(seps, ln, sel.Caret.Col - 1, st);
            sel.End = new Pos(sel.Caret.Line, col != 0 ? col + 1 : col);
            return base.Execute(sel);
        }

        public override IEditorCommand Clone()
        {
            return new DeleteWordBackCommand();
        }

        public override bool ModifyContent => true;
    }
}
