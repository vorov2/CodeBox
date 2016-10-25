﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Modify | RestoreCaret | Undoable | Scroll)]
    public sealed class PasteCommand : InsertRangeCommand
    {
        public override void Execute(CommandArgument arg, Selection sel)
        {
            var str = Clipboard.GetText();
            arg = new CommandArgument(str);
            base.Execute(arg, sel);
        }
    }
}
