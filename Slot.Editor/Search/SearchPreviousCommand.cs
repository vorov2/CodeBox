﻿using Slot.Editor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace Slot.Editor.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.searchprevious")]
    public sealed class SearchPreviousCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!Ed.Search.IsSearchVisible)
                Ed.Search.ShowSearch();

            var caret = sel.Caret;
            var found = false;

            foreach (var sr in Ed.Search.EnumerateSearchResults().OrderByDescending(s => new Pos(s.Line, s.StartCol)))
                if (sr.Line < caret.Line || (sr.Line == caret.Line && sr.StartCol < caret.Col && sr.EndCol + 1 < caret.Col))
                {
                    Ed.Buffer.Selections.Set(new Selection(
                        new Pos(sr.Line, sr.StartCol),
                        new Pos(sr.Line, sr.EndCol + 1)
                        ));
                    found = true;
                    break;
                }

            var pos = new Pos(Ed.Lines.Count, Ed.Lines[Ed.Lines.Count - 1].Length);
            if (!found && caret != pos)
                return Execute(new Selection(pos));

            return ActionResults.Clean | ActionResults.Scroll;
        }

        internal override bool SingleRun => true;
    }
}
