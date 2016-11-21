﻿using System;
using System.ComponentModel.Composition;

namespace CodeBox.Core.ComponentModel
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandDataAttribute : Attribute, ICommandMetadata
    {
        public CommandDataAttribute(string key, string alias)
        {
            Key = key;
            Alias = alias;
        }

        public string Key { get; }

        public string Alias { get; }

        public ArgumentType ArgumentType { get; set; }

        public string ArgumentName { get; set; }
    }
}
