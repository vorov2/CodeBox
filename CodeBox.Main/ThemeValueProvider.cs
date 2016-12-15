﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Themes;

namespace CodeBox.Main
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.themes")]
    public sealed class ThemeValueProvider : IArgumentValueProvider
    {
        public IEnumerable<ValueItem> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;
            var theme = App.Catalog<IThemeComponent>().Default();
            return theme.EnumerateThemes()
                .Where(t => str == null || t.Key.ToString().IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(t => new ValueItem(t.Key.ToString(), t.Name));
        }
    }
}
