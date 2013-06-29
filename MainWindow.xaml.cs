#region References

using CanvasDiagramEditor.Controls;
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

namespace CanvasDiagramEditor
{
    #region Tuple Aliases

    using TagMap = Tuple<Line, FrameworkElement, FrameworkElement>;
    using PinMap = Tuple<string, string>;
    using WireMap = Tuple<FrameworkElement, List<Tuple<string, string>>>;

    #endregion

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private Line _line = null;
        private FrameworkElement _root = null;

        private int pinCounter = 0;
        private int wireCounter = 0;
        private int inputCounter = 0;
        private int outputCounter = 0;
        private int andGateCounter = 0;
        private int orGateCounter = 0;

        private Point rightClick;

        private string lastInsert = "Input";

        private bool enableSnap = true;
        private double snap = 15;
        private double snapOffsetX = 0;
        private double snapOffsetY = 0;

        private bool skipContextMenu = false;
        private bool skipLeftClick = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            EnableSnap.IsChecked = this.enableSnap;
        }

        #endregion

        #region Snap

        public double Snap(double original, double snap, double offset)
        {
            return Snap(original - offset, snap) + offset;
        }

        public double Snap(double original, double snap)
        {
            return original + ((Math.Round(original / snap) - original / snap) * snap);
        }

        private double SnapX(double original)
        {
            return this.enableSnap == true ? Snap(original, this.snap, this.snapOffsetX) : original;
        }

        private double SnapY(double original)
        {
            return this.enableSnap == true ? Snap(original, this.snap, this.snapOffsetY) : original;
        }

        #endregion

        #region Move

        private void SetCanvasPosition(FrameworkElement element, double left, double top)
        {
            Canvas.SetLeft(element, SnapX(left));
            Canvas.SetTop(element, SnapY(top));
        }

        private void MoveRoot(FrameworkElement element, double dX, double dY)
        {
            double left = Canvas.GetLeft(element) + dX;
            double top = Canvas.GetTop(element) + dY;

            SetCanvasPosition(element, left, top);

            MoveLines(element, dX, dY);
        }
        
        private void MoveLines(FrameworkElement element, double dX, double dY)
        {
            if (element != null && element.Tag != null)
            {
                var tuples = element.Tag as List<TagMap>;

                foreach (var tuple in tuples)
                {
                    var line = tuple.Item1;
                    var start = tuple.Item2;
                    var end = tuple.Item3;

                    if (start != null)
                    {
                        line.X1 = SnapX(line.X1 + dX);
                        line.Y1 = SnapY(line.Y1 + dY);
                    }
                    else if (end != null)
                    {
                        line.X2 = SnapX(line.X2 + dX);
                        line.Y2 = SnapY(line.Y2 + dY);
                    }
                }
            }
        }

        #endregion

        #region Create

        private Thumb CreatePin(double x, double y, int id)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["PinControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Pin|" + id.ToString()
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            SetCanvasPosition(thumb, x, y);

            return thumb;
        }

        private Line CreateWire(double x1, double y1, double x2, double y2, int id)
        {
            var line = new Line()
            {
                Style = Application.Current.Resources["LineStyleKey"] as Style,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Uid = "Wire|" + id.ToString()
            };

            return line;
        }

        private Thumb CreateInput(double x, double y, int id)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["InputControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Input|" + id.ToString()
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            SetCanvasPosition(thumb, x, y);

            return thumb;
        }

        private Thumb CreateOutput(double x, double y, int id)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["OutputControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Output|" + id.ToString()
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            SetCanvasPosition(thumb, x, y);

            return thumb;
        }

        private Thumb CreateAndGate(double x, double y, int id)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["AndGateControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "AndGate|" + id.ToString()
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            SetCanvasPosition(thumb, x, y);

            return thumb;
        }

        private Thumb CreateOrGate(double x, double y, int id)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["OrGateControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "OrGate|" + id.ToString()
            };

            thumb.DragDelta += this.RootElement_DragDelta;

            SetCanvasPosition(thumb, x, y);

            return thumb;
        }

        private void ConnectPins(Canvas canvas, FrameworkElement pin)
        {
            var root = ((pin.Parent as FrameworkElement).Parent as FrameworkElement).TemplatedParent as FrameworkElement;

            this._root = root;

            System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(this._root);
            double ry = Canvas.GetTop(this._root);
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);
            double x = rx + px;
            double y = ry + py;

            System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            ConnectPins(canvas, x, y);
        }

        private void ConnectPins(Canvas canvas, double x, double y)
        {
            if (this._root.Tag == null)
            {
                this._root.Tag = new List<TagMap>();
            }

            var tuples = this._root.Tag as List<TagMap>;

            if (this._line == null)
            {
                var line = CreateWire(x, y, x, y, this.wireCounter);
                this.wireCounter += 1;

                this._line = line;

                var tuple = new TagMap(this._line, this._root, null);
                tuples.Add(tuple);

                canvas.Children.Add(this._line);
            }
            else
            {
                this._line.X2 = x;
                this._line.Y2 = y;

                var tuple = new TagMap(this._line, null, this._root);
                tuples.Add(tuple);

                this._line = null;
                this._root = null;
            }
        }

        #endregion

        #region Insert

        private FrameworkElement InsertPin(Canvas canvas, Point point)
        {
            var thumb = CreatePin(point.X, point.Y, pinCounter);
            this.pinCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, this.inputCounter);
            this.inputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, this.outputCounter);
            this.outputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, this.andGateCounter);
            this.andGateCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, this.orGateCounter);
            this.orGateCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertLast(Canvas canvas, string type, Point point)
        {
            switch (type)
            {
                case "Input":
                    return InsertInput(canvas, point);
                case "Output":
                    return InsertOutput(canvas, point);
                case "AndGate":
                    return InsertAndGate(canvas, point);
                case "OrGate":
                    return InsertOrGate(canvas, point);
                case "Pin":
                    return InsertPin(canvas, point);
                default:
                    return null;
            }
        }

        #endregion

        #region Delete

        private void DeleteElement(Canvas canvas, Point point)
        {
            var res = VisualTreeHelper.HitTest(canvas, point);
            var element = res.VisualHit as FrameworkElement;

            FrameworkElement parent = element.Parent as FrameworkElement;

            if (parent != null)
            {
                while (!(parent.TemplatedParent is Thumb))
                {
                    parent = parent.Parent as FrameworkElement;

                    if (parent == null)
                        break;
                }

                if (parent != null)
                {
                    FrameworkElement root = parent.TemplatedParent as FrameworkElement;

                    System.Diagnostics.Debug.Print("DeleteElement, root: {0}, uid: {1}", root.GetType(), root.Uid);

                    if (root != null && root.Parent == canvas)
                    {
                        canvas.Children.Remove(root);
                    }
                }
            }
        }

        #endregion

        #region Diagram Model

        private bool CompareStrings(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private void ClearDiagramModel(Canvas canvas)
        {
            canvas.Children.Clear();

            this.pinCounter = 0;
            this.wireCounter = 0;
            this.inputCounter = 0;
            this.outputCounter = 0;
            this.andGateCounter = 0;
            this.orGateCounter = 0;
        }

        private string GenerateDiagramModel(Canvas canvas)
        {
            var sb = new StringBuilder();

            string header = "[Diagram]";

            System.Diagnostics.Debug.Print(header);
            sb.AppendLine(header);

            foreach (var child in canvas.Children)
            {
                var element = child as FrameworkElement;

                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);

                if (element.Uid.StartsWith("Wire"))
                {
                    var line = element as Line;

                    string str = string.Format("+;{0};{1};{2};{3};{4}", element.Uid, line.X1, line.Y1, line.X2, line.Y2);
                    sb.AppendLine(str);

                    System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("+;{0};{1};{2}", element.Uid, x, y);
                    sb.AppendLine(str);

                    System.Diagnostics.Debug.Print(str);
                }

                if (element.Tag != null)
                {
                    var tuples = element.Tag as List<TagMap>;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        if (start != null)
                        {
                            // Start
                            string str = string.Format("-;{0};Start", line.Uid);
                            sb.AppendLine(str);

                            System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("-;{0};End", line.Uid);
                            sb.AppendLine(str);

                            System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private void ParseDiagramModel(string diagram, Canvas canvas)
        {
            var lines = diagram.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var dict = new Dictionary<string, WireMap>();
            Tuple<FrameworkElement, List<Tuple<string,string>>> tuple = null;

            string name = null;

            ClearDiagramModel(canvas);

            // create roor elements
            foreach (var line in lines)
            {
                var args = line.Split(';');
                int length = args.Length;

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareStrings(args[0], "+"))
                    {
                        if (name.StartsWith("Pin", StringComparison.InvariantCultureIgnoreCase) && length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.pinCounter = Math.Max(this.pinCounter, id + 1);

                            var element = CreatePin(x, y, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Input", StringComparison.InvariantCultureIgnoreCase) && length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.inputCounter = Math.Max(this.inputCounter, id + 1);

                            var element = CreateInput(x, y, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Output", StringComparison.InvariantCultureIgnoreCase) && length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.outputCounter = Math.Max(this.outputCounter, id + 1);

                            var element = CreateOutput(x, y, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("AndGate", StringComparison.InvariantCultureIgnoreCase) && length == 4)
                        {
                            double x = double.Parse(args[2]);  
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.andGateCounter = Math.Max(this.andGateCounter, id + 1);

                            var element = CreateAndGate(x, y, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("OrGate", StringComparison.InvariantCultureIgnoreCase) && length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.orGateCounter = Math.Max(this.orGateCounter, id + 1);

                            var element = CreateOrGate(x, y, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Wire", StringComparison.InvariantCultureIgnoreCase) && length == 6)
                        {
                            double x1 = double.Parse(args[2]);  
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            int id = int.Parse(name.Split('|')[1]);

                            this.wireCounter = Math.Max(this.wireCounter, id + 1);

                            var element = CreateWire(x1, y1, x2, y2, id);
                            canvas.Children.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                    }
                    else if (CompareStrings(args[0], "-"))
                    {
                        if (tuple != null)
                        {
                            var wires = tuple.Item2;

                            wires.Add(new PinMap(name, args[2]));
                        }
                    }
                }
            }

            // update wire connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1;
                var wires = item.Value.Item2;

                if (element.Tag == null)
                {
                    element.Tag = new List<TagMap>();
                }

                if (wires.Count > 0)
                {
                    var tuples = element.Tag as List<TagMap>;

                    foreach (var wire in wires)
                    {
                        string _name = wire.Item1;
                        string _type = wire.Item2;

                        if (CompareStrings(_type, "Start"))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, element, null);
                            tuples.Add(_tuple);
                        }
                        else if (CompareStrings(_type, "End"))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, null, element);
                            tuples.Add(_tuple);
                        }
                    }
                }
            }
        }

        #endregion

        #region Open/Save

        private void Save(string fileName, Canvas canvas)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                string diagram = GenerateDiagramModel(canvas);

                writer.Write(diagram);
            }
        }

        private void Open(string fileName, Canvas canvas)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                ParseDiagramModel(diagram, canvas);
            }
        }

        #endregion

        #region Handlers

        private void HandleLeftDown(Canvas canvas, Point point)
        {
            if (this._root != null && this._line != null)
            {
                var root = InsertPin(canvas, point);

                this._root = root;

                System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(this._root);
                double ry = Canvas.GetTop(this._root);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                ConnectPins(canvas, x, y);

                this._root = root;

                ConnectPins(canvas, x, y);
            }
            else
            {
                InsertLast(canvas, this.lastInsert, point);
            }
        }

        private bool HandlePreviewLeftDown(Canvas canvas, FrameworkElement pin)
        {
            if (pin != null &&
                !CompareStrings(pin.Name, "MiddlePin") || Keyboard.Modifiers == ModifierKeys.Control)
            {
                ConnectPins(canvas, pin);

                return true;
            }

            return false;
        }

        private void HandleMove(Canvas canvas, Point point)
        {
            if (this._root != null && this._line != null)
            {
                double x = point.X;
                double y = point.Y;

                if (this._line.X2 != x)
                {
                    //this._line.X2 = SnapX(x);
                    this._line.X2 = x;
                }

                if (this._line.Y2 != y)
                {
                    //this._line.Y2 = SnapY(y);
                    this._line.Y2 = y;
                }
            }
        }

        private bool HandleRightDown(Canvas canvas)
        {
            if (this._root != null && this._line != null)
            {
                var tuples = this._root.Tag as List<TagMap>;

                var last = tuples.LastOrDefault();
                tuples.Remove(last);

                canvas.Children.Remove(this._line);

                this._line = null;
                this._root = null;

                return true;
            }

            return false;
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

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            var point = e.GetPosition(canvas);

            HandleLeftDown(canvas, point);
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.skipLeftClick == true)
            {
                this.skipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = this.DiagramCanvas;
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = HandlePreviewLeftDown(canvas, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            var point = e.GetPosition(canvas);

            HandleMove(canvas, point);
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            this.rightClick = e.GetPosition(canvas);

            var result = HandleRightDown(canvas);
            if (result == true)
            {
                this.skipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region Button Events

        private void OpenModel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = this.DiagramCanvas;

                this.Open(dlg.FileName, canvas);
            }
        }

        private void SaveModel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Diagram",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = this.DiagramCanvas;

                this.Save(dlg.FileName, canvas);
            }
        }

        private void ClearModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            ClearDiagramModel(canvas);
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            var text = GenerateDiagramModel(canvas);

            model.Text = text;
        }

        private void ParseModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            var text = model.Text;

            ParseDiagramModel(text, canvas);
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.skipContextMenu == true)
            {
                this.skipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                this.skipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            InsertPin(canvas, this.rightClick);

            this.lastInsert = "Pin";
            this.skipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            InsertInput(canvas, this.rightClick);

            this.lastInsert = "Input";
            this.skipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            InsertOutput(canvas, this.rightClick);

            this.lastInsert = "Output";
            this.skipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            InsertAndGate(canvas, this.rightClick);

            this.lastInsert = "AndGate";
            this.skipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            InsertOrGate(canvas, this.rightClick);

            this.lastInsert = "OrGate";
            this.skipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = DiagramCanvas;
            var menu = sender as MenuItem;
            var point = new Point(this.rightClick.X, this.rightClick.Y);

            DeleteElement(canvas, point);
            this.skipLeftClick = false;
        }

        #endregion

        #region CheckBox Events

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            this.enableSnap = EnableSnap.IsChecked == true ? true : false;
        } 

        #endregion
    }

    #endregion
}
