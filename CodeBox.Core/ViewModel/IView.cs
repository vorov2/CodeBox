﻿using System;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Core.ViewModel
{
    public interface IView : IExecutionContext
    {
        void AttachBuffer(IBuffer buffer);

        void DetachBuffer();

        IBuffer Buffer { get; }
    }
}
