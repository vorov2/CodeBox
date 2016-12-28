﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slot.Core.CommandModel
{
    public abstract class EnumValueProvider<T> : IArgumentValueProvider
        where T : struct
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return Enums.GetDisplayNames<T>().Select(s => new ValueItem(s));
        }
    }
}
