﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using CodeBox.Affinity;
using System.ComponentModel.Composition;
using static CodeBox.Commands.ActionResults;
using CodeBox.Core.ComponentModel;
using CodeBox.Core;

namespace CodeBox.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.newline")]
    public class InsertNewLineCommand : EditorCommand
    {
        private IEnumerable<Character> @string;
        internal Pos undoPos;
        private Selection redoSel;
        private int indent;
        private IEnumerable<Character> unindent;

        internal override ActionResults Execute(Selection selection, params object[] args)
        {
            undoPos = selection.Start;
            redoSel = selection.Clone();

            if (!selection.IsEmpty)
                @string = DeleteRangeCommand.DeleteRange(View, selection);

            var indentKey = View.AffinityManager.GetAffinity(new Pos(undoPos.Line, 0)).GetIndentComponentKey(View);
            var pos = InsertNewLine(Document, undoPos);
            selection.Clear(pos);

            var comp = indentKey != null ? App.Catalog<IDentComponent>().GetComponent(indentKey) : null;
            indent = comp != null ? comp.CalculateIndentation(View, pos.Line) : 0;

            if (indent > 0)
            {
                if (pos.Line > 0)
                {
                    var ln = Document.Lines[pos.Line - 1];

                    if (ln.IsEmpty() && ln.Length > 0)
                    {
                        unindent = ln.ToList();
                        ln.RemoveRange(0, ln.Length);
                    }
                }

                var str = View.UseTabs ? new string('\t', indent / View.IndentSize)
                    : new string(' ', indent);
                Document.Lines[pos.Line].Insert(0, str.MakeCharacters());
                selection.Clear(new Pos(pos.Line, pos.Col + str.Length));
            }

            return Change;
        }

        public override ActionResults Redo(out Pos pos)
        {
            @string = null;
            var sel = redoSel;
            Execute(sel);
            pos = sel.Caret;
            return Change;
        }

        public override ActionResults Undo(out Pos pos)
        {
            pos = undoPos;

            if (@string != null)
                pos = InsertRangeCommand.InsertRange(Document, undoPos, @string);

            var line = Document.Lines[pos.Line];
            var nextLine = Document.Lines[pos.Line + 1];
            Document.Lines.Remove(nextLine);

            if (indent > 0)
            {
                var real = View.UseTabs ? indent / View.IndentSize : indent;
                nextLine.RemoveRange(0, real);
            }

            if (unindent != null)
                line.Insert(0, unindent);

            line.Append(nextLine);
            return Change;
        }

        internal static Pos InsertNewLine(Document doc, Pos pos)
        {
            var ln = pos.Line >= 0 ? doc.Lines[pos.Line] : null;
            var str = default(IEnumerable<Character>);

            if (ln != null && pos.Col != ln.Length && pos.Col < ln.Length)
            {
                str = ln.GetRange(pos.Col, ln.Length - pos.Col);
                ln.RemoveRange(pos.Col, ln.Length - pos.Col);
            }

            var newLn = new Line(str);
            doc.Lines.Insert(pos.Line + 1, newLn);
            pos = new Pos(pos.Line + 1, 0);
            return pos;
        }

        internal override EditorCommand Clone()
        {
            return new InsertNewLineCommand();
        }

        internal override bool ModifyContent => true;
    }
}
