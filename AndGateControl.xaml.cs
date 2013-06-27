
namespace CanvasDiagramEditor
{
    #region References

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    #endregion

    #region AndGateControl

    public partial class AndGateControl : UserControl
    {
        #region Constructor

        public AndGateControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Pin Events

        private void Pin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.Print("Pin_PreviewMouseLeftButtonDown, sender: {0}, {1}", sender.GetType(), (sender as FrameworkElement).Name);
        }

        #endregion
    } 

    #endregion
}
