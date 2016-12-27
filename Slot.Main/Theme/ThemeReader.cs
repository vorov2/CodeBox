﻿using System;
using System.Collections.Generic;
using System.Linq;
using Json;
using Slot.Drawing;
using Slot.Core.Themes;
using Slot.Editor;

namespace Slot.Main.Theme
{
    using MAP = Dictionary<string, object>;

    public static class ThemeReader
    {
        public static IEnumerable<StyleInfo> Read(string source)
        {
            ColorExtensions.Clean();
            var json = new JsonParser(source);
            var list = json.Parse() as List<object>;
            return list != null ? ReadStyles(list) : Enumerable.Empty<StyleInfo>();
        }

        private static IEnumerable<StyleInfo> ReadStyles(List<object> styles)
        {
            foreach (var o in styles)
            {
                var dict = o as MAP;

                if (dict != null)
                {
                    var styleKey = dict.String("key");

                    if (styleKey != null)
                        yield return new StyleInfo(
                            StandardStyleConverter.FromString(styleKey), ReadStyle(dict));
                }
            }
        }

        private static Style ReadStyle(MAP dict) =>
            new Style(
                dict.Color("color"),
                dict.Color("backColor"),
                dict.Color("adornmentColor"),
                dict.Adornments(),
                dict.FontStyles());
    }
}
