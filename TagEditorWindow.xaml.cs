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
    #region TagEditorWindow

    public partial class TagEditorWindow : Window
    {
        #region Constructor

        public TagEditorWindow()
        {
            InitializeComponent();
        } 

        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }

    #endregion
}
