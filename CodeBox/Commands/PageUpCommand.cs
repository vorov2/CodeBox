﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll | ClearSelections)]
    internal sealed class PageUpCommand : CaretCommand
    {
        protected override Pos GetPosition(EditorContext context, Pos caret)
        {
            return PageUp(context);
        }

        internal static Pos PageUp(EditorContext context)
        {
            var lines = context.Document.Lines;
            var caret = context.Document.Selections.Main.Caret;
            var line = lines[caret.Line];
            var stripes = 0;
            var lastLine = default(Line);
            var lastLineIndex = 0;

            for (var i = caret.Line; i > -1; i--)
            {
                lastLine = lines[i];
                lastLineIndex = i;
                stripes += lastLine.Stripes;

                if (stripes >= context.StripesPerScreen)
                    break;
            }

            return new Pos(lastLineIndex, caret.Col > lastLine.Length ? lastLine.Length : caret.Col);
        }
    }
}
