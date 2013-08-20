// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#endregion

namespace CanvasDiagramEditor.Elements
{
    #region OutputControl

    public partial class OutputControl : UserControl
    {
        #region Constructor

        public OutputControl()
        {
            InitializeComponent();
        } 

        #endregion

        #region Drag & Drop

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Tag"))
            {
                var tag = e.Data.GetData("Tag") as Tag;
                if (tag != null)
                {
                    var thumb = this.TemplatedParent as ElementThumb;
                    var canvas = thumb.GetParent() as ICanvas;

                    History.Add(canvas);

                    thumb.SetData(tag);
                    e.Handled = true;
                }
            }
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Tag") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        #endregion
    } 

    #endregion
}
