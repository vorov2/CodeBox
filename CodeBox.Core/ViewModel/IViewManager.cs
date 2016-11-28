﻿using CodeBox.Core.ComponentModel;
using System;

namespace CodeBox.Core.ViewModel
{
    public interface IViewManager : IComponent
    {
        IView CreateView();

        IView GetActiveView();
    }
}
