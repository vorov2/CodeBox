﻿using System;
using CodeBox.Core;
using CodeBox.Commands;

namespace CodeBox
{
    public static class Cmd
    {
        public static readonly Identifier SetBufferEol = new Identifier("buffer.setBufferEol");
        public static readonly Identifier ToggleWordWrap = new Identifier("buffer.toggleWordWrap");
        public static readonly Identifier GotoLine = new Identifier(GotoLineCommand.Name);
    }
}
