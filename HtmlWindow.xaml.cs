// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

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
using System.Windows.Shapes;

#endregion

namespace CanvasDiagramEditor
{
    #region HtmlWindow

    public partial class HtmlWindow : Window
    {
        #region Constructor

        public HtmlWindow()
        {
            InitializeComponent();

            this.Loaded += HtmlWindow_Loaded;
        }

        void HtmlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Html.Focus();
        } 

        #endregion
    }

    #endregion

}
