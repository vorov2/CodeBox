﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Commands;

namespace CodeBox.Folding
{
    public sealed class IndentFoldingProvider : IFoldingProvider
    {
        public void Fold(IEditorContext ctx, Range range)
        {
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

            for (var i = li; i < ctx.Buffer.Document.Lines.Count; i++)
            {
                var line = ctx.Buffer.Document.Lines[i];

                if (line.IsEmpty())
                {
                    line.Folding &= ~FoldingStates.Header;
                    continue;
                }

                var txt = line.Text;
                var indent = 0;

                foreach (var c in txt)
                {
                    if (c == ' ')
                        indent++;
                    else if (c == '\t')
                        indent += ctx.Settings.TabSize;
                    else
                        break;
                }

                indent /= ctx.Settings.TabSize;

                if (indent > prevIndent && i > 0)
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
