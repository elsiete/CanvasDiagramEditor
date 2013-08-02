// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
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

namespace CanvasDiagramEditor
{
    #region TableEditorControl

    public partial class TableEditorControl : UserControl
    {
        #region Properties

        public List<object> Tables { get; set; }
        public List<ICanvas> Selected { get; set; }

        #endregion

        #region Constructor

        public TableEditorControl()
        {
            InitializeComponent();
        }

        #endregion

    } 

    #endregion
}
