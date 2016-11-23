﻿using System;
using System.Collections.Generic;

namespace CodeBox.CommandLine
{
    public sealed class Statement : CommandLineItem
    {
        internal Statement()
        {

        }

        public string Command { get; internal set; }

        public bool HasArguments => _arguments != null && _arguments.Count > 0;

        private List<StatementArgument> _arguments;
        public List<StatementArgument> Arguments
        {
            get
            {
                if (_arguments == null)
                    _arguments = new List<StatementArgument>();

                return _arguments;
            }
        }
    }
}
