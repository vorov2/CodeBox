﻿using System;
using Slot.Editor.ObjectModel;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.left")]
    public class LeftCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => MoveLeft(Ed, sel);

        internal static Pos MoveLeft(EditorControl ctx, Selection sel)
        {
            var pos = new Pos(sel.Caret.Line, sel.Caret.Col - 1);

            for (;;)
            {
                pos = InternalMoveLeft(ctx.Document, sel, pos);

                if (!ctx.Folding.IsLineVisible(pos.Line) && pos.Line > 0)
                    pos = new Pos(pos.Line - 1, ctx.Document.Lines[pos.Line - 1].Length);
                else
                    break;
            }

            return pos;
        }

        private static Pos InternalMoveLeft(Document doc, Selection sel, Pos pos)
        {
            if (pos.Col < 0 && pos.Line > 0)
            {
                var line = doc.Lines[pos.Line - 1];
                pos = new Pos(pos.Line - 1, line.Length);
            }
            else if (pos.Col < 0)
                pos = new Pos(pos.Line, 0);

            sel.SetToRestore(doc.Lines[pos.Line].GetStripeCol(pos.Col));
            return pos;
        }

        internal override bool SupportLimitedMode => true;
    }
}
