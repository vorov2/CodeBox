﻿using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.wordleft")]
    public sealed class WordLeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = WordLeft(View, sel);
            sel.SetToRestore(pos);
            return pos;
        }

        internal static Pos WordLeft(Editor ctx, Selection sel)
        {
            var caret = sel.Caret;
            var line = ctx.Buffer.Document.Lines[caret.Line];

            if (caret.Col > 0)
            {
                var seps = ctx.AffinityManager.GetAffinity(caret).GetNonWordSymbols(ctx);
                var c = line.CharAt(caret.Col - 1);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundLeft(seps, line, caret.Col - 1, strat);

                if (pos < 0)
                    return LeftCommand.MoveLeft(ctx, sel);

                if (Math.Abs(pos - caret.Col) > 1 && pos > 0)
                    pos++;

                return new Pos(caret.Line, pos);
            }
            else
                return LeftCommand.MoveLeft(ctx, sel);
        }

        internal override bool SupportLimitedMode => true;
    }
}
