﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;

namespace Slot.Editor.BufferCommands
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.eol")]
    public sealed class EolValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            return Enums.GetDisplayNames<Eol>()
                .Where(e => str == null || e.Contains(str))
                .Select(e => new ValueItem(e));
        }
    }
}
