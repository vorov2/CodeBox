﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.Margins;
using CodeBox.ObjectModel;
using CodeBox.Styling;
using CodeBox.Commands;
using CodeBox.Folding;
using CodeBox.Lexing;
using CodeBox.Indentation;
using CodeBox.Affinity;
using CodeBox.Core.Keyboard;
using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.CommandBar;
using CodeBox.Core.ComponentModel;
using CodeBox.ComponentModel;
using CodeBox.StatusBar;

namespace CodeBox.Test
{
    public partial class MainForm : Form
    {
        private Editor ed;
        private Editor output;
        private CommandBarControl commandBar;

        public MainForm()
        {
            InitializeComponent();
            Initialize();
            InitializeOutput();
        }

        private void Initialize()
        {
            ed = new Editor { Dock = DockStyle.Fill };

            commandBar = new CommandBarControl(ed) { Dock = DockStyle.Top };


            SettingsReader.Read(File.ReadAllText("samples\\settings.json"), ed.Settings);
            ed.LeftMargins.Add(new LineNumberMargin(ed) { MarkCurrentLine = true });
            ed.LeftMargins.Add(new FoldingMargin(ed));
            ed.RightMargins.Add(new VerticalScrollBarMargin(ed));
            ed.BottomMargins.Add(new ScrollBarMargin(ed, Orientation.Horizontal));
            ed.TopMargins.Add(new TopMargin(ed));

            var statusBar = new StatusBarControl(ed) { Dock = DockStyle.Bottom };
            statusBar.Tiles.Add(new HelpTile(ed));
            statusBar.Tiles.Add(new ModeTile(ed));
            statusBar.Tiles.Add(new EolTile(ed));
            statusBar.Tiles.Add(new EncodingTile(ed));

            statusBar.Tiles.Add(new OutputToggleTile(this));
            statusBar.Tiles.Add(new PosTile(ed));
            //statusBar.Tiles.Add(new ErrorsTile(ed));
            statusBar.Tiles.Add(new OvrTile(ed));
            statusBar.Tiles.Add(new WrapTile(ed));


            splitContainer.Panel1.Controls.Add(ed);
            splitContainer.Panel1.Controls.Add(commandBar);
            splitContainer.Panel1.Controls.Add(statusBar);
            //statusBar.BringToFront();
            ed.Paint += (o, e) =>
            {
                statusBar.Invalidate();
                commandBar.Invalidate();
            };
            //var coll = StylesReader.Read(File.ReadAllText("samples\\theme2.json"));
            //ed.Styles.Theme = coll;
        }

        private void InitializeOutput()
        {
            output = new Editor { Dock = DockStyle.Fill };
            output.ReadOnly = true;
            output.LeftMargins.Add(new GutterMargin(output));
            output.RightMargins.Add(new ScrollBarMargin(output, Orientation.Vertical));
            output.BottomMargins.Add(new ScrollBarMargin(output, Orientation.Horizontal));
            output.ThinCaret = true;
            splitContainer.Panel2.Controls.Add(output);
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}


        public SplitContainer SplitContainer => splitContainer;

        private string LocalFile(string fileName)
        {
            return Path.Combine(new FileInfo(typeof(MainForm).Assembly.Location).DirectoryName, fileName);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }


        private void Form1_Activated(object sender, EventArgs e)
        {
            Activations++;
            //ed.Focus();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //Focus();
        }

        public Editor Editor => ed;

        public int Activations { get; private set; }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (App.Terminating || Application.OpenForms.Count > 1 || !(e.Cancel = !App.Close()))
                ed.DetachBuffer();
        }
    }
}
