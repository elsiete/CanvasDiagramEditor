
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

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private Line _line = null;
        private FrameworkElement _root = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Thumb Events

        private void RootElement_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;

            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            MoveRoot(thumb, dX, dY);
        }

        #endregion

        #region Move

        private void MoveRoot(FrameworkElement element, double dX, double dY)
        {
            double left = Canvas.GetLeft(element) + dX;
            double top = Canvas.GetTop(element) + dY;

            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);

            MovePins(element, dX, dY);
        }

        private void MovePins(FrameworkElement element, double dX, double dY)
        {
            if (element.Tag != null)
            {
                var tuples = element.Tag as List<Tuple<Line, FrameworkElement, FrameworkElement>>;

                foreach (var tuple in tuples)
                {
                    var line = tuple.Item1;
                    var start = tuple.Item2;
                    var end = tuple.Item3;

                    if (start != null)
                    {
                        line.X1 = line.X1 + dX;
                        line.Y1 = line.Y1 + dY;
                    }
                    else if (end != null)
                    {
                        line.X2 = line.X2 + dX;
                        line.Y2 = line.Y2 + dY;
                    }
                }
            }
        }

        #endregion

        #region Create

        private Line CreateWire(double x, double y)
        {
            var line = new Line()
            {
                Style = this.Resources["LineStyleKey"] as Style,
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y,
                Uid = "Wire"
            };

            return line;
        }

        private Thumb CreateAndGate(double x, double y)
        {
            var thumb = new Thumb()
            {
                Template = this.Resources["AndGateControlTemplateKey"] as ControlTemplate,
                Style = this.Resources["RootThumbStyleKey"] as Style,
                Uid = "AndGate"
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            Canvas.SetLeft(thumb, x);
            Canvas.SetTop(thumb, y);

            return thumb;
        }

        private void ConnectPins(Canvas canvas, FrameworkElement pin)
        {
            var root = ((pin.Parent as FrameworkElement).Parent as FrameworkElement).TemplatedParent as FrameworkElement;

            _root = root;

            System.Diagnostics.Debug.Print("Canvas_PreviewMouseLeftButtonDown, source: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(_root);
            double ry = Canvas.GetTop(_root);
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);
            double x = rx + px;
            double y = ry + py;

            System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            if (_root.Tag == null)
            {
                _root.Tag = new List<Tuple<Line, FrameworkElement, FrameworkElement>>();
            }

            var tuples = _root.Tag as List<Tuple<Line, FrameworkElement, FrameworkElement>>;

            if (_line == null)
            {
                var line = CreateWire(x, y);

                _line = line;

                var tuple = new Tuple<Line, FrameworkElement, FrameworkElement>(_line, _root, null);
                tuples.Add(tuple);

                canvas.Children.Add(_line);
            }
            else
            {
                _line.X2 = x;
                _line.Y2 = y;

                var tuple = new Tuple<Line, FrameworkElement, FrameworkElement>(_line, null, _root);
                tuples.Add(tuple);

                _line = null;
                _root = null;
            }
        }

        #endregion

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            var point = e.GetPosition(canvas);

            var thumb = CreateAndGate(point.X, point.Y);
            canvas.Children.Add(thumb);
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            if (pin != null)
            {
                ConnectPins(canvas, pin);

                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = sender as Canvas;

            if (_root != null && _line != null)
            {
                var point = e.GetPosition(canvas);

                double x = point.X;
                double y = point.Y;

                _line.X2 = x;
                _line.Y2 = y;
            }
        }

        #endregion

        #region Button Events

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            foreach (var child in canvas.Children)
            {
                var element = child as FrameworkElement;

                System.Diagnostics.Debug.Print("-{0}",element.Uid);

                if (element.Tag != null)
                {
                    var tuples = element.Tag as List<Tuple<Line, FrameworkElement, FrameworkElement>>;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        System.Diagnostics.Debug.Print("  +{0},{1},{2}", 
                            line.Uid, 
                            start != null ? start.Uid : "<null>",
                            end != null ? end.Uid : "<null>");
                    }
                }
            }
        } 

        #endregion
    }

    #endregion
}
