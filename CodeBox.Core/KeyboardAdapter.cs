﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Core
{
    using MAP = Dictionary<KeyInput, object>;

    public enum InputState
    {
        Unrecognized,
        Chord,
        Complete
    }

    public static class KeymapReader
    {
        public static void Read(string source, KeyboardAdapter adapter)
        {
            var dict = new Json.JsonParser(source).Parse() as Dictionary<string, object>;

            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    var lst = kv.Value as List<object>;

                    if (lst != null)
                    {
                        foreach (var o in lst)
                        {
                            var str = o as string;

                            if (str != null)
                                adapter.RegisterInput(kv.Key, str);
                        }
                    }
                    else
                    {
                        var str = kv.Value as string;
                        if (str != null)
                            adapter.RegisterInput(kv.Key, str);
                    }
                }
            }
        }
    }

    public sealed class KeyboardAdapter
    {
        private readonly MAP inputs = new MAP();
        private MAP lastDict;

        public void RegisterInput(string key, string shortcut)
        {
            Parse(key, shortcut);
        }

        public InputState ProcessInput(KeyInput input)
        {
            var map = lastDict ?? inputs;
            object obj;

            if (!map.TryGetValue(input, out obj))
            {
                lastDict = null;
                return InputState.Unrecognized;
            }
            else if (obj is MAP)
            {
                lastDict = obj as MAP;
                return InputState.Chord;
            }
            else
            {
                lastDict = null;
                LastKey = (string)obj;
                return InputState.Complete;
            }
        }

        private void Parse(string key, string shortcut)
        {
            var arr = shortcut.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var dict = inputs;

            for (var i = 0; i < arr.Length; i++)
            {
                var mod = Modifiers.None;
                var a = arr[i];
                var last = i == arr.Length - 1;
                var arr2 = a.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

                for (var j = 0; j < arr2.Length; j++)
                {
                    var ch = arr2[j];

                    if (j != arr2.Length - 1)
                    {
                        mod |=
                              Eq(ch, "Ctrl") ? Modifiers.Ctrl
                            : Eq(ch, "Alt") ? Modifiers.Alt
                            : Eq(ch, "Shift") ? Modifiers.Shift
                            : Eq(ch, "Win") ? Modifiers.Cmd
                            : Eq(ch, "Cmd") ? Modifiers.Cmd
                            : Eq(ch, "Move") ? Modifiers.Move
                            : Modifiers.None;
                    }
                    else
                    {
                        var input = ch.Length == 1 ? new KeyInput(mod, ch[0])
                            : new KeyInput(mod, ParseSpecialKey(ch));

                        object obj;
                        var found = dict.TryGetValue(input, out obj);

                        if (found && last)
                            dict[input] = key;
                        else if (found && !last)
                        {
                            var dict2 = obj as MAP;

                            if (dict2 == null)
                                dict2 = new MAP();

                            dict = dict2;
                        }
                        else if (!found && last)
                            dict.Add(input, key);
                        else if (!found && !last)
                        {
                            var dict2 = new MAP();
                            dict.Add(input, dict2);
                            dict = dict2;
                        }
                    }
                }
            }
        }

        private static Dictionary<string, SpecialKey> specialKeys;

        private static bool Eq(string fst, string snd)
        {
            return string.Equals(fst, snd, StringComparison.OrdinalIgnoreCase);
        }

        private static SpecialKey ParseSpecialKey(string value)
        {
            if (specialKeys == null)
            {
                specialKeys = new Dictionary<string, SpecialKey>(StringComparer.OrdinalIgnoreCase);
                var names = typeof(SpecialKey).GetEnumNames();
                var values = typeof(SpecialKey).GetEnumValues().Cast<SpecialKey>().ToList();

                for (var i = 0; i < names.Length; i++)
                    specialKeys.Add(names[i], values[i]);
            }

            SpecialKey ret;

            if (specialKeys.TryGetValue(value, out ret))
                return ret;

            return SpecialKey.None;
        }

        public string LastKey { get; private set; }
    }

    public struct KeyInput : IEquatable<KeyInput>
    {
        public static readonly KeyInput Empty = new KeyInput(Modifiers.None, '\0');

        public KeyInput(Modifiers mod, char key)
        {
            Modifier = mod;
            Key = (int)(char.ToUpper(key));
        }

        public KeyInput(Modifiers mod, SpecialKey key)
        {
            Modifier = mod;
            Key = (int)key;
        }

        public bool IsEmpty()
        {
            return Modifier == Modifiers.None && Key == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Modifier.GetHashCode();
                hash = hash * 23 + Key.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return Modifier == Modifiers.None ? KeyToString()
                : $"{Modifier.ToString().Replace(", ", "+")}+{KeyToString()}";
        }

        private string KeyToString()
        {
            return Key >= (int)SpecialKey.Del
                ? ((SpecialKey)Key).ToString() : ((char)Key).ToString();
        }

        public bool Equals(KeyInput other)
        {
            return Modifier == other.Modifier && Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyInput && Equals((KeyInput)obj);
        }

        public Modifiers Modifier { get; }

        public int Key { get; }
    }

    public static class KeysExtensions
    {
        public static Modifiers KeysToModifier(this Keys keys)
        {
            switch (keys)
            {
                case Keys.ControlKey:
                case Keys.Control:
                    return Modifiers.Ctrl;
                case Keys.Shift:
                    return Modifiers.Shift;
                case Keys.Alt:
                    return Modifiers.Alt;
                case Keys.LWin:
                case Keys.RWin:
                    return Modifiers.Cmd;
                default:
                    return Modifiers.None;
            }
        }

        public static SpecialKey KeysToSpecialKey(this Keys keys)
        {
            switch (keys)
            {
                case Keys.Delete: return SpecialKey.Del;
                case Keys.Tab: return SpecialKey.Tab;
                case Keys.Home: return SpecialKey.Home;
                case Keys.End: return SpecialKey.End;
                case Keys.Insert: return SpecialKey.Ins;
                case Keys.Back: return SpecialKey.Back;
                case Keys.PageUp: return SpecialKey.PageUp;
                case Keys.PageDown: return SpecialKey.PageDown;
                case Keys.Return: return SpecialKey.Enter;
                case Keys.Escape: return SpecialKey.Esc;
                case Keys.F1: return SpecialKey.F1;
                case Keys.F2: return SpecialKey.F2;
                case Keys.F3: return SpecialKey.F3;
                case Keys.F4: return SpecialKey.F4;
                case Keys.F5: return SpecialKey.F5;
                case Keys.F6: return SpecialKey.F6;
                case Keys.F7: return SpecialKey.F7;
                case Keys.F8: return SpecialKey.F8;
                case Keys.F9: return SpecialKey.F9;
                case Keys.F10: return SpecialKey.F10;
                case Keys.F11: return SpecialKey.F11;
                case Keys.F12: return SpecialKey.F12;
                case Keys.Up: return SpecialKey.Up;
                case Keys.Down: return SpecialKey.Down;
                case Keys.Left: return SpecialKey.Left;
                case Keys.Right: return SpecialKey.Right;
                default: return SpecialKey.None;
            }
        }
    }

    [Flags]
    public enum Modifiers
    {
        None = 0,
        Ctrl = 0x01,
        Shift = 0x02,
        Alt = 0x04,
        Cmd = 0x08,
        Move = 0x10
    }

    public enum SpecialKey
    {
        None = 0,
        Del = 0x1001,
        Tab = 0x1002,
        Home = 0x1003,
        End = 0x1004,
        Ins = 0x1005,
        Back = 0x1006,
        PageUp = 0x1007,
        PageDown = 0x1008,
        Enter = 0x1009,
        Esc = 0x1010,
        F1 = 0x1011,
        F2 = 0x1012,
        F3 = 0x1013,
        F4 = 0x1014,
        F5 = 0x1015,
        F6 = 0x1016,
        F7 = 0x1017,
        F8 = 0x1018,
        F9 = 0x1019,
        F10 = 0x1020,
        F11 = 0x1021,
        F12 = 0x1022,
        Up = 0x1023,
        Down = 0x1024,
        Left = 0x1025,
        Right = 0x1026,

        Click = 0x1027,
        RightClick = 0x1028,
        DoubleClick = 0x1029,
        Move = 0x1030
    }
}
