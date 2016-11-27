﻿using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeBox.Core.ViewModel
{
    public interface IBufferManager : IComponent
    {
        IBuffer CreateBuffer();

        IBuffer CreateBuffer(FileInfo fileName, Encoding encoding);

        IEnumerable<IBuffer> EnumerateBuffers();
    }
}
