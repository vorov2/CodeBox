﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal sealed class PageDownCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return PageDown(context);
        }

        internal static Pos PageDown(EditorContext ctx)
        {
            var lines = ctx.Document.Lines;
            var caret = ctx.Document.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i < lines.Count; i++)
            {
                lastLine = lines[i];
                lastLineIndex = i;
                stripes += lastLine.Stripes;

                if (stripes >= ctx.Info.StripesPerScreen)
                    break;
            }

            return new Pos(lastLineIndex, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
