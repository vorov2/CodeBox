﻿using Slot.Editor.Commands;
using Slot.Core;
using Slot.Core.ComponentModel;
using Slot.Core.Themes;
using Slot.Editor.ObjectModel;
using Slot.Editor.Search;
using Slot.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Slot.Editor
{
    public sealed class MatchWordManager
    {
        private readonly EditorControl editor;
        private readonly List<SearchResult> finds = new List<SearchResult>();
        private string lastWord;
        private Pos requestCaret = Pos.Empty;
        private DateTime requestTime;
        private readonly Timer timer = new Timer();

        public MatchWordManager(EditorControl editor)
        {
            this.editor = editor;
            timer.Interval = 500;
            timer.Tick += (o, e) => Match();
            timer.Start();
        }

        public void RequestMatch()
        {
            requestCaret = editor.Buffer.Selections.Main.Caret;
            requestTime = DateTime.Now;
        }

        public void ClearMatches() => InternalClearMatches();

        private bool InternalClearMatches()
        {
            var clear = false;

            foreach (var f in finds)
            {
                if (editor.Lines.Count > f.Line)
                {
                    editor.Lines[f.Line].AppliedStyles.Remove(f.Style);
                    clear = true;
                }
            }

            finds.Clear();
            return clear;
        }

        private void Match()
        {
            if (!editor.EditorSettings.MatchWords)
                return;

            var caret = editor.Buffer.Selections.Main.Caret;
            var range = SelectWordCommand.SelectWord(editor, caret, SelectWordCommand.Strategy.Word);
            var txt = range != null ? CopyCommand.GetTextRange(editor, range) : null;

            if (txt == lastWord && finds.Count > 0)
                return;

            var needRedraw = InternalClearMatches();
            
            if (caret != requestCaret || (DateTime.Now - requestTime).TotalMilliseconds < 500)
            {
                if (needRedraw)
                    editor.Buffer.RequestRedraw();
                return;
            }

            lastWord = txt;

            if (range == null)
            {
                if (needRedraw)
                    editor.Buffer.RequestRedraw();
                return;
            }

            var grmId = editor.AffinityManager.GetAffinityId(caret);
            var grm = grmId != 0 ? App.Ext.Grammars().GetGrammar(grmId) : null;
            var seps = (" \t" + (grm?.NonWordSymbols ?? editor.EditorSettings.NonWordSymbols)).ToCharArray();
            var regex = new Regex("\\b" + Regex.Escape(txt) + "\\b");

            for (var i = 0; i < editor.Lines.Count; i++)
            {
                var line = editor.Lines[i];
                var ln = line.Text;

                foreach (Match m in regex.Matches(ln))
                    if (m.Success && editor.AffinityManager.GetAffinityId(i, m.Index) == grmId)
                    {
                        var aps = new AppliedStyle(StandardStyle.MatchedWord, m.Index, m.Index + m.Length - 1);
                        line.AppliedStyles.Add(aps);
                        finds.Add(new SearchResult(i, aps));
                    }
            }

            if (finds.Count == 1)
            {
                editor.Lines[finds[0].Line].AppliedStyles.Remove(finds[0].Style);
                finds.Clear();
            }

            if (finds.Count > 0 || needRedraw)
                editor.Buffer.RequestRedraw();

            requestCaret = Pos.Empty;
        }

        internal bool HasSearchResults => finds.Count > 0;

        internal IEnumerable<SearchResult> EnumerateSearchResults() => finds;
    }
}
