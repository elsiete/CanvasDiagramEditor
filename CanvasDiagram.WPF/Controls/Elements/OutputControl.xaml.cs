// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Core;
using CanvasDiagram.Editor;
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

namespace CanvasDiagram.WPF.Elements
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

                    HistoryEditor.Add(canvas);

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
