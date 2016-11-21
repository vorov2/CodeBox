﻿using CodeBox.Autocomplete;
using CodeBox.Folding;
using CodeBox.Affinity;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using System;
using CodeBox.Indentation;
using CodeBox.CallTips;
using CodeBox.Core.ComponentModel;
using CodeBox.Lexing;

namespace CodeBox
{
    public interface IEditorView : IExecutionContext
    {
        AffinityManager AffinityManager { get; }

        CallTipManager CallTips { get; }//?

        StyleManager Styles { get; }//?

        AutocompleteManager Autocomplete { get; } //-

        FoldingManager Folding { get; }

        ScrollingManager Scroll { get; } //?

        GrammarManager GrammarManager { get; }

        DocumentBuffer Buffer { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        bool WordWrap { get; }

        int WordWrapColumn { get; }

        bool UseTabs { get; }

        int IndentSize { get; }

        bool Overtype { get; set; }

        bool ShowEol { get; }

        bool ShowWhitespace { get; }

        bool CurrentLineIndicator { get; }

        bool ShowLineLength { get; }

        Pos Caret { get; }

        MatchBracketManager MatchBrackets { get; } //-

        int FirstEditLine { get; set; }

        int LastEditLine { get; set; }

        bool LimitedMode { get; }
    }
}
