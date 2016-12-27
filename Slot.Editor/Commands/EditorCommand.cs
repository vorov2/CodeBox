﻿using System;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using Slot.Core;

namespace Slot.Editor.Commands
{
    public abstract class EditorCommand
    {
        internal abstract ActionResults Execute(Selection sel, params object[] args);

        public virtual ActionResults Undo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        public virtual ActionResults Redo(out Pos pos)
        {
            pos = Pos.Empty;
            return None;
        }

        protected T GetArg<T>(int num, object[] args, T def = default(T))
        {
            if (args == null || args.Length <= num)
                return def;

            var obj = args[num];
            object res;
            return Converter.Convert(obj, typeof(T), out res) ? (T)res : def;
        }

        internal EditorControl Ed { get; set; }

        internal bool GroupUndo { get; set; }

        internal virtual bool SingleRun => false;

        internal virtual bool ModifyContent => false;

        internal virtual bool SupportLimitedMode => false;

        internal virtual EditorCommand Clone() => this;

        protected DocumentBuffer Buffer => Ed.Buffer;

        protected Document Document => Ed.Buffer.Document;

        protected EditorSettings Settings => Ed.EditorSettings;
    }
}
