﻿using System;
using CodeBox.ObjectModel;
using CodeBox.Folding;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.foldingtoggle")]
    public sealed class ToggleFoldingCommand : EditorCommand
    {
        private int undoLine;
        private Pos undoCaret;

        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            undoCaret = sel.Caret;
            var pos = args == null || args.Length == 0 || !(args[0] is int) ? View.Caret : new Pos((int)args[0], 0);
            var ln = pos.IsEmpty ? sel.Caret.Line : pos.Line;
            var level = -1;

            while (ln > -1)
            {
                var line = Document.Lines[ln];

                if (line.Folding.Has(FoldingStates.Header) && (level == -1 || line.FoldingLevel < level))
                {
                    undoLine = ln;
                    View.Folding.ToggleExpand(undoLine);
                    break;
                }

                if (level == -1)
                    level = line.FoldingLevel;

                ln--;
            }

            return Modify | ShallowChange;
        }

        public override ActionResults Undo(out Pos pos)
        {
            View.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        public override ActionResults Redo(out Pos pos)
        {
            View.Folding.ToggleExpand(undoLine);
            pos = undoCaret;
            return Modify | ShallowChange;
        }

        internal override EditorCommand Clone()
        {
            return new ToggleFoldingCommand();
        }

        internal override bool SingleRun => true;
    }
}
