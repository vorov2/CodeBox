﻿using System;

namespace CodeBox.Lexing
{
    public sealed class GrammarSectionSettings
    {
        public bool Multiline { get; set; }

        public bool CaseSensitive { get; set; }

        public char Escape { get; set; }

        public char Continuation { get; set; }
    }
}
