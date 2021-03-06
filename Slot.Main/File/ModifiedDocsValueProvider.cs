﻿using Slot.Core.CommandModel;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Slot.Main.File
{
    [Export(typeof(IArgumentValueProvider))]
    [ComponentData("values.modifieddocs")]
    public sealed class ModifiedDocsValueProvider : IArgumentValueProvider
    {
        [Import]
        private IBufferManager bufferManager = null;

        public IEnumerable<ValueItem> EnumerateArgumentValues()
        {
            return bufferManager.EnumerateBuffers()
                .Where(b => b.IsDirty)
                .Select(b => new ValueItem(b.File.Name, b.File.DirectoryName));
        }
    }
}
