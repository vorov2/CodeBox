﻿using System;
using CodeBox.ObjectModel;
using CodeBox.Affinity;

namespace CodeBox.Commands
{
    public sealed class WordLeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = WordLeft(Context, sel);
            sel.SetToRestore(pos);
            return pos;
        }

        internal static Pos WordLeft(IEditorContext ctx, Selection sel)
        {
            var caret = sel.Caret;
            var line = ctx.Buffer.Document.Lines[caret.Line];

            if (caret.Col > 0)
            {
                var seps = ctx.AffinityManager.GetAffinity(caret).GetNonWordSymbols(ctx, caret);
                var c = line.CharAt(caret.Col - 1);
                var strat = SelectWordCommand.GetStrategy(seps, c);
                var pos = SelectWordCommand.FindBoundLeft(seps, line, caret.Col - 1, strat);

                if (Math.Abs(pos - caret.Col) > 1 && pos > 0)
                    pos++;

                return new Pos(caret.Line, pos);
            }
            else
                return LeftCommand.MoveLeft(ctx.Buffer.Document, sel);
        }
    }
}
