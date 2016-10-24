﻿using System;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Scroll)]
    internal sealed class ExtendDownCommand : SelectionCommand
    {
        protected override Pos Select(Pos pos)
        {
            return DownCommand.MoveDown(Context, pos);
        }
    }
}
