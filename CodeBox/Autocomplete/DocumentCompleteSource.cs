﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Editor.Lexing;
using Slot.Editor.Affinity;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Autocomplete
{
    public sealed class DocumentCompleteSource : ICompleteSource
    {
        private Dictionary<int, Dictionary<string, object>> completes = new Dictionary<int, Dictionary<string, object>>();
        private volatile bool busy;
        private DateTime lastUpdate;
        private int lastEdits = -1;
        private EditorControl editor;
        private const string SEPS = " \r\n\t`~!@#$%^&*()=+[{]}\\|;:'\",.<>/?";

        public void Initialize(IExecutionContext context)
        {
            this.editor = (EditorControl)context;
        }

        public IEnumerable<string> GetItems()
        {
            var t = default(Task);

            if (!busy &&
                (lastUpdate == DateTime.MinValue || DateTime.Now - lastUpdate > TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("Generate CompleteSource");
                t = Task.Run(() => WalkDocument(editor));
            }

            if (completes.Count == 0 && t != null)
                t.Wait();

            var id = editor.AffinityManager.GetAffinityId(editor.Buffer.Selections.Main.Caret);

            if (id != 0)
            {
                Dictionary<string, object> dict;

                if (completes.TryGetValue(id, out dict))
                {
                    var items = dict.Keys.ToList();
                    items.Sort();
                    return items;
                }
            }

            return Enumerable.Empty<string>();
        }

        private void WalkDocument(EditorControl ctx)
        {
            if (ctx.Buffer.Edits == lastEdits)
                return;

            busy = true;
            completes.Clear();
            var arr = ctx.Buffer.Document.Lines.ToList();

            for (var i = 0; i < arr.Count; i++)
            {
                var line = arr[i];

                if (line.Length < 2)
                    continue;

                var grm = ctx.AffinityManager.GetAffinityId(new Pos(i, 0));
                var seps = ctx.AffinityManager.GetAffinity(new Pos(i, 0)).GetNonWordSymbols(ctx);
                var dict = default(Dictionary<string, object>);

                if (!completes.TryGetValue(grm, out dict))
                    completes.Add(grm, dict = new Dictionary<string, object>());

                foreach (var str in line.Text.Split((" \t" + seps).ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str.Length > 1 && !dict.ContainsKey(str) && !char.IsDigit(str[0]))
                        dict.Add(str, null);
                }
            }

            lastUpdate = DateTime.Now;
            busy = false;
        }
    }
}
