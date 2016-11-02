﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(SingleRun)]
    public sealed class UndoCommand : Command
    {
        public override ActionResult Execute(CommandArgument arg, Selection sel)
        {
            Context.Commands.Undo();
            return ActionResult.Standard;
        }
    }
}
