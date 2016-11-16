﻿using System;
using CodeBox.ObjectModel;

namespace CodeBox.Commands
{
    public class HomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel)
        {
            var pos = MoveHome(Document, sel.Caret);
            sel.SetToRestore(pos);
            return pos;
        }

        internal static Pos MoveHome(Document doc, Pos pos)
        {
            var ln = doc.Lines[pos.Line];
            var ch = '\0';

            if (pos.Col > 0 && ((ch = ln.CharAt(pos.Col - 1)) == ' ' || ch == '\t'))
                return new Pos(pos.Line, 0);

            for (var i = 0; i < ln.Length; i++)
            {
                var c = ln.CharAt(i);

                if (c != ' ' && c != '\t')
                    return new Pos(pos.Line, i);
            }

            return new Pos(pos.Line, 0);
        }
    }
}
