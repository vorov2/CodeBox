﻿using CodeBox.Core.ComponentModel;
using CodeBox.Core.ViewModel;
using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    [Export(Name, typeof(IComponent))]
    [ComponentData(Name)]
    public sealed class ViewManager : IViewManager
    {
        public const string Name = "viewmanager.default";

        public IView CreateView()
        {
            var frm = new MainForm();
            frm.Show();
            return frm.Editor;
        }

        public IView GetActiveView()
        {
            MainForm frm = Form.ActiveForm as MainForm;

            if (frm == null)
                frm = Application.OpenForms.OfType<MainForm>()
                    .OrderByDescending(f => f.Activations)
                    .FirstOrDefault();

            return frm?.Editor;
        }
    }
}
