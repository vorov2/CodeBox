﻿using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox
{
    [Export(typeof(IComponent))]
    [ComponentData("test")]
    public sealed class TestCommandDispatcher : CommandDispatcher
    {
        [Command]
        public void OpenFile(string fileName, string encoding = "utf-8")
        {
            var enc = Encoding.GetEncodings()
                .FirstOrDefault(e => e.Name.Equals(encoding, StringComparison.OrdinalIgnoreCase))
                ?.GetEncoding()
                ?? Encoding.UTF8;

            try
            {
                var txt = File.ReadAllText(Uri.UnescapeDataString(fileName), enc);
                //((Editor)ctx).AttachBuffer(new DocumentBuffer(Document.FromString(txt), fileName, enc));
            }
            catch (Exception)
            {
                //logging
            }
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.encoding")]
    public sealed class EncodingValueProvider : IArgumentValueProvider
    {
        public IEnumerable<Value> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string;

            return Encoding.GetEncodings()
                .Where(e => str == null || e.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                .Select(e => new EncodingArgumentValue(e))
                .OrderBy(e => e.Data);
        }

        class EncodingArgumentValue : Value
        {
            private readonly EncodingInfo enc;
            internal EncodingArgumentValue(EncodingInfo enc)
            {
                this.enc = enc;
                Data = enc.Name.ToUpper();
            }

            public override string ToString()
            {
                var idx = enc.DisplayName.IndexOf('(');
                var nam = enc.DisplayName;

                if (idx > -1)
                    nam = nam.Substring(0, idx - 1).TrimEnd();

                return $"{Data} ({nam})";
            }
        }
    }

    [Export(typeof(IComponent))]
    [ComponentData("values.systempath")]
    public sealed class SystemPathValueProvider : IArgumentValueProvider
    {
        public IEnumerable<Value> EnumerateArgumentValues(object curvalue)
        {
            var str = curvalue as string ?? "";

            //if (!string.IsNullOrWhiteSpace(str))
                return GetPathElements(str) ?? Enumerable.Empty<Value>();

            //return Enumerable.Empty<ArgumentValue>();
        }

        private IEnumerable<Value> GetPathElements(string pat)
        {
            if (pat.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                return null;

            try
            {
                if (pat.EndsWith("\\") || pat.EndsWith("//"))
                    return Directory.EnumerateFileSystemEntries(pat)
                        .Where(v => v.StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new Value { Data = sy });
                else
                {
                    FileInfo fi;
                    var dir = string.IsNullOrWhiteSpace(pat) || (fi = new FileInfo(pat)).Directory == null
                        ? new DirectoryInfo(Environment.CurrentDirectory) : fi.Directory;

                    var loc = Environment.CurrentDirectory == dir.Name;
                    return dir.EnumerateFileSystemInfos()
                        .Where(v => (loc ? v.Name : v.FullName).StartsWith(pat, StringComparison.OrdinalIgnoreCase))
                        .Select(sy => new Value { Data = sy.FullName });
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
