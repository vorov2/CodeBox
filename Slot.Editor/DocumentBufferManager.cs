﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.IO;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using Slot.Editor.ObjectModel;
using Slot.Core;
using Slot.Core.Output;

namespace Slot.Editor
{
    [Export(typeof(IBufferManager))]
    [ComponentData(Name)]
    public sealed class DocumentBufferManager : IBufferManager
    {
        public const string Name = "buffermanager.default";
        private const int MAX_RECENT = 100;

        private readonly List<IBuffer> buffers = new List<IBuffer>();
        private readonly List<Recent> recents = new List<Recent>();

        class Recent
        {
            public FileInfo File;
            public DateTime Date;
        }

        public IBuffer CreateBuffer()
        {
            var num = buffers.Count(b => !b.File.Exists);
            return InternalCreateBuffer(new FileInfo($"untitled-{num + 1}"), Encoding.UTF8);
        }

        public void CloseBuffer(IBuffer buffer)
        {
            var idx = buffers.IndexOf(buffer);

            if (idx != -1)
                buffers.RemoveAt(idx);
        }

        public void SaveBuffer(IBuffer buffer, FileInfo file, Encoding encoding)
        {
            var docb = buffer as DocumentBuffer;

            if (docb == null)
            {
                App.Ext.Log("Invalid document type.", EntryType.Error);
                return;
            }

            var txt = docb.GetText();

            if (!FileUtil.WriteFile(file, txt, encoding))
                return;

            docb.File = file;
            docb.Encoding = encoding;
            docb.ClearDirtyFlag();
        }

        public IBuffer CreateBuffer(FileInfo fileName, Encoding encoding)
        {
            fileName.Refresh();

            if (!fileName.Exists)
            {
                var buf = InternalCreateBuffer(fileName, encoding);
                buf.Edits++;
                return buf;
            }

            return InternalCreateBuffer(fileName, encoding);
        }

        private Document CreateDocument(FileInfo fileName, Encoding encoding)
        {
            fileName.Refresh();

            if (!fileName.Exists)
                return Document.FromString("");

            string txt = null;
            var res = FileUtil.ReadFile(fileName, encoding, out txt);
            return res ? Document.FromString(txt) : null;
        }

        private DocumentBuffer InternalCreateBuffer(FileInfo file, Encoding enc)
        {
            var buf = buffers.FirstOrDefault(b => 
                b.File.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase));

            if (buf == null)
            {
                var doc = CreateDocument(file, enc);

                if (doc == null)
                    return null;

                buf = new DocumentBuffer(doc, file, enc);
                buffers.Add(buf);
                var rd = recents.FirstOrDefault(r => r.File.FullName.Equals(file.FullName, StringComparison.OrdinalIgnoreCase));

                if (rd != null)
                    rd.Date = DateTime.Now;
                else
                    recents.Add(new Recent { File = file, Date = DateTime.Now });

                if (recents.Count > MAX_RECENT)
                    recents.RemoveAt(0);
            }

            return (DocumentBuffer)buf;
        }

        public IEnumerable<IBuffer> EnumerateBuffers() => buffers.OrderByDescending(b => b.LastAccess);

        public IEnumerable<FileInfo> EnumerateRecent() => recents.OrderByDescending(r => r.Date).Select(r => r.File);
    }
}
