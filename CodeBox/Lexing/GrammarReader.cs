﻿using CodeBox.Affinity;
using CodeBox.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public static class GrammarReader
    {
        public static Grammar Read(string source)
        {
            var json = new Json.JsonParser(source) { SkipNulls = true };
            var dict = json.Parse() as Dictionary<string, object>;

            if (dict != null)
            {
                var sectionMap = new Dictionary<string, Tuple<string, string, GrammarSection>>();
                var grammarKey = dict.String("key");
                var grammar = ProcessSection<Grammar>(grammarKey, dict).Item3;
                grammar.BracketSymbols = dict.String("brackets");
                grammar.NonWordSymbols = dict.String("delimeters");
                grammar.CommentMask = dict.String("commentMask");
                grammar.NumberLiteral = new NumberLiteral(dict.String("numbers"));
                grammar.IndentProviderKey = dict.String("indentProvider");
                sectionMap.Add("root", Tuple.Create<string, string, GrammarSection>("root", null, grammar));

                var sections = dict.Object("sections") as List<object>;
                byte id = 0;

                foreach (var o in sections)
                {
                    var nd = o as Dictionary<string, object>;

                    var tup = ProcessSection<GrammarSection>(grammar.GrammarKey, nd);
                    tup.Item3.Id = id++;
                    var key = id == 1 ? "root" : tup.Item1; 

                    if (key != null)
                    {
                        if (sectionMap.ContainsKey(tup.Item1))
                            throw new CodeBoxException($"Duplicate section key {tup.Item1} in grammar {grammar.GrammarKey}.");

                        sectionMap.Add(tup.Item1, tup);
                    }
                }

                foreach (var tup in sectionMap.Values)
                {
                    var parentKey = tup.Item2 ?? "root";
                    Tuple<string, string, GrammarSection> parent;

                    if (!sectionMap.TryGetValue(parentKey, out parent))
                        throw new CodeBoxException($"A parent section with key {parentKey} not found in grammar {grammar.GrammarKey}.");

                    parent.Item3.Sections.Add(tup.Item3);
                    grammar.Sections.Add(tup.Item3);
                }

                return grammar;
            }

            return null;
        }

        private static Tuple<string, string, T> ProcessSection<T>(string grammar, Dictionary<string, object> dict)
            where T : GrammarSection, new()
        {
            if (dict == null)
                return null;

            string tmp;
            var ignoreCase = dict.Bool("ignoreCase");

            var sect = new T
            {
                GrammarKey = grammar,
                ExternalGrammarKey = dict.String("grammar"),
                IgnoreCase = ignoreCase,
                ContextChars = dict.String($"context"),
                ContinuationChar = dict.Char("contination"),
                EscapeChar = dict.Char("escape"),
                Multiline = dict.Bool("multiline"),
                DontStyleCompletely = dict.Bool("dontStyleCompletely"),
                StyleBrackets = dict.Bool("styleBrackets"),
                StyleNumbers = dict.Bool("styleNumbers"),
                Style = dict.Style("style"),
                IdentifierStyle = dict.Style("identifierStyle"),
                ContextIdentifierStyle = dict.Style("contextIdentifierStyle"),
                Start = (tmp = dict.String("start")) != null ? new SectionSequence(tmp, !ignoreCase) : null,
                End = (tmp = dict.String("end")) != null ? new SectionSequence(tmp, !ignoreCase) : null,
                Keywords = ProcessKeywords(ignoreCase, dict.Object("keywords") as List<object>)
            };

            return Tuple.Create(dict.String("key"), dict.String("parent"), sect);
        }

        private static StringTable ProcessKeywords(bool ignoreCase, List<object> keywords)
        {
            var keys = new StringTable(ignoreCase);

            if (keywords != null)
            {
                foreach (var o in keywords)
                {
                    var dict = o as Dictionary<string, object>;

                    if (dict == null)
                        continue;

                    var style = dict.Style("style");
                    var words = dict.String("words");

                    if (words != null)
                        keys.AddRange(words, (int)style);
                }
            }

            return keys;
        }
    }

    internal static class DictionaryExtensions
    {
        public static object Object(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res;
        }

        public static string String(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null ? res.ToString() : null;
        }

        public static bool Bool(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null && res is bool ? (bool)res
                : res != null ? res.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                : false;
        }

        public static char Char(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null ? res.ToString()[0] : '\0';
        }

        private static Dictionary<string, StandardStyle> styles;
        public static StandardStyle Style(this Dictionary<string, object> dict, string key)
        {
            if (styles == null)
            {
                styles = new Dictionary<string, StandardStyle>(Json.DictionaryComparer.Instance);

                foreach (var fi in typeof(StandardStyle).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = Attribute.GetCustomAttribute(fi, typeof(FieldNameAttribute));
                    var val = (StandardStyle)fi.GetValue(null);
                    var ekey = attr != null ? attr.ToString() : fi.Name;
                    styles.Add(ekey, val);
                }
            }

            var str = String(dict, key);
            var ret = StandardStyle.Default;

            if (str != null)
                styles.TryGetValue(str, out ret);

            return ret;
        }
    }
}
