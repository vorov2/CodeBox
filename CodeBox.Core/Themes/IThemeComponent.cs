﻿using System;
using System.Collections.Generic;
using Slot.Core.ComponentModel;

namespace Slot.Core.Themes
{
    public interface IThemeComponent : IComponent
    {
        IEnumerable<ThemeInfo> EnumerateThemes();

        bool ChangeTheme(Identifier themeKey);

        Style GetStyle(StandardStyle styleId);

        ThemeInfo Theme { get; }

        event EventHandler ThemeChanged;
    }
}
