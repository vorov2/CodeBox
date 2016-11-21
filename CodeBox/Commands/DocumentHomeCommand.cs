﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(ICommand))]
    [CommandData("editor.documenthome", "ecdh")]
    public sealed class DocumentHomeCommand : CaretCommand
    {
        protected override Pos GetPosition(Selection sel) => default(Pos);

        internal override bool SupportLimitedMode => true;
    }
}
