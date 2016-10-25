﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    public sealed class ExtendDocumentHomeCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return default(Pos);
        }
    }
}
