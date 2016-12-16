﻿using System;
using Slot.Editor.ObjectModel;
using Slot.Editor.Commands;
using Slot.Core.ComponentModel;
using System.ComponentModel.Composition;
using Slot.Editor.ComponentModel;

namespace Slot.Editor.Folding
{
    [Export(typeof(IFoldingComponent))]
    [ComponentData(Name)]
    public sealed class IndentFoldingComponent : IFoldingComponent
    {
        public const string Name = "folding.indent";

        public void Fold(IExecutionContext context, Range range)
        {
            var ctx = (EditorControl)context;
            var prevIndent = 0;
            var li = range.Start.Line;

            if (li > 0)
            {
                var ln = ctx.Buffer.Document.Lines[li - 1];

                for (var i = 0; ; i++)
                {
                    var c = ln.CharAt(i);

                    if (c == ' ' || c == '\t')
                        prevIndent++;
                    else
                        break;
                }
            }

            var initIndent = prevIndent;

            for (var i = li; i < range.End.Line + 1; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];
                
                if (line.IsEmpty())
                {
                    line.Folding &= ~FoldingStates.Header;
                    ctx.Folding.SetFoldingLevel(i, prevIndent);
                    continue;
                }

                var txt = line.Text;
                var indent = 0;

                foreach (var c in txt)
                {
                    if (c == ' ')
                        indent++;
                    else if (c == '\t')
                        indent += ctx.IndentSize;
                    else
                        break;
                }

                indent /= ctx.IndentSize;

                if (indent > prevIndent && i > 0 && !ctx.Buffer.Document.Lines[i - 1].IsEmpty())
                {
                    ctx.Folding.SetFoldingHeader(i - 1);
                    ctx.Folding.SetFoldingLevel(i, indent);
                }
                else
                    ctx.Folding.SetFoldingLevel(i, indent);

                if (i >= range.End.Line && indent == initIndent)
                    break;

                prevIndent = indent;
            }
        }
    }
}
