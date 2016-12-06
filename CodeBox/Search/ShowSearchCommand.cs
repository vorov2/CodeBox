﻿using CodeBox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Core.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Search
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.showsearch")]
    public sealed class ShowSearchCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            if (!View.Search.IsFocused)
                View.Search.ShowSearch();
            else
                View.Focus();

            return ActionResults.Clean;
        }

        internal override bool SingleRun => true;
    }
}