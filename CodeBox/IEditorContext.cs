﻿using CodeBox.Autocomplete;
using CodeBox.Folding;
using CodeBox.Affinity;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using CodeBox.Indentation;

namespace CodeBox
{
    public interface IEditorContext
    {
        AffinityManager AffinityManager { get; }

        CallTipManager CallTips { get; }

        StyleManager Styles { get; }

        AutocompleteManager Autocomplete { get; }

        LocationManager Locations { get; }

        FoldingManager Folding { get; }

        IndentManager Indents { get; }

        ScrollingManager Scroll { get; }

        CommandManager Commands { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool WordWrap { get; }

        int WordWrapColumn { get; }

        bool Overtype { get; set; }
    }
}
