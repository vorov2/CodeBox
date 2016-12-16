﻿using Slot.Editor.ComponentModel;
using System;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Indentation
{
    [Export(typeof(IDentComponent))]
    [ComponentData(Name)]
    public sealed class CurlyDentProvider : IDentComponent
    {
        public const string Name = "indent.curly";

        public int CalculateIndentation(IExecutionContext context, int lineIndex)
        {
            var ctx = (EditorControl)context;

            if (lineIndex > 0)
            {
                var ln = ctx.Buffer.Document.Lines[lineIndex - 1];
                var idx = ln.Length - 1;
                var indent = 0;
                var curly = false;

                while (idx > -1)
                {
                    var ch = ln[idx--];

                    if (ch.Char == '{')
                    {
                        curly = true;
                        break;
                    }
                    else if (ch.Char != ' ')
                        break;
                }

                foreach (var c in ln)
                    if (c.Char == ' ')
                        indent++;
                    else if (c.Char == '\t')
                        indent += ctx.IndentSize;
                    else
                        break;
                
                if (curly)
                    indent += ctx.IndentSize;

                return indent;
            }
            else
                return 0;
        }
    }
}
