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

        private bool enableInsertLast = true;
        private string lastInsert = "Input";

        private bool enableSnap = true;
        private bool snapOnRelease = false;
        private double snapX = 15;
        private double snapY = 15;
        private double snapOffsetX = 0;
        private double snapOffsetY = 0;

        private bool skipContextMenu = false;
        private bool skipLeftClick = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            EnableInsertLast.IsChecked = this.enableInsertLast;

            EnableSnap.IsChecked = this.enableSnap;
            SnapOnRelease.IsChecked = this.snapOnRelease;
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

        private double SnapX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, this.snapX, this.snapOffsetX) : original;
        }

        private double SnapY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, this.snapY, this.snapOffsetY) : original;
        }

        #endregion

        #region Move

        private void SetCanvasPosition(FrameworkElement element, double left, double top, bool snap)
        {
            Canvas.SetLeft(element, SnapX(left, snap));
            Canvas.SetTop(element, SnapY(top, snap));
        }

        private void MoveRoot(FrameworkElement element, double dX, double dY, bool snap)
        {
            double left = Canvas.GetLeft(element) + dX;
            double top = Canvas.GetTop(element) + dY;

            SetCanvasPosition(element, left, top, snap);

            MoveLines(element, dX, dY, snap);
        }
        
        private void MoveLines(FrameworkElement element, double dX, double dY, bool snap)
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
                        line.X1 = SnapX(line.X1 + dX, snap);
                        line.Y1 = SnapY(line.Y1 + dY, snap);
                    }
                    else if (end != null)
                    {
                        line.X2 = SnapX(line.X2 + dX, snap);
                        line.Y2 = SnapY(line.Y2 + dY, snap);
                    }
                }
            }
        }

        #endregion

        #region Create

        private void SetThumbEvents(Thumb thumb)
        {
            thumb.DragDelta += this.RootElement_DragDelta;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

        private Thumb CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["PinControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Pin|" + id.ToString()
            };

            SetThumbEvents(thumb);
            SetCanvasPosition(thumb, x, y, snap);

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

        private Thumb CreateInput(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["InputControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Input|" + id.ToString()
            };

            SetThumbEvents(thumb);
            SetCanvasPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateOutput(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["OutputControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "Output|" + id.ToString()
            };

            SetThumbEvents(thumb);
            SetCanvasPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["AndGateControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "AndGate|" + id.ToString()
            };

            SetThumbEvents(thumb);
            SetCanvasPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources["OrGateControlTemplateKey"] as ControlTemplate,
                Style = Application.Current.Resources["RootThumbStyleKey"] as Style,
                Uid = "OrGate|" + id.ToString()
            };

            SetThumbEvents(thumb);
            SetCanvasPosition(thumb, x, y, snap);

            return thumb;
        }

        private void ConnectPins(Canvas canvas, FrameworkElement pin)
        {
            var root = 
                (
                    (pin.Parent as FrameworkElement)
                    .Parent as FrameworkElement
                ).TemplatedParent as FrameworkElement;

            this._root = root;

            System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", 
                pin.GetType(), pin.Name);

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
            var thumb = CreatePin(point.X, point.Y, this.pinCounter, this.enableSnap);
            this.pinCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, this.inputCounter, this.enableSnap);
            this.inputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, this.outputCounter, this.enableSnap);
            this.outputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, this.andGateCounter, this.enableSnap);
            this.andGateCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        private FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, this.orGateCounter, this.enableSnap);
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

                    System.Diagnostics.Debug.Print("DeleteElement, root: {0}, uid: {1}", 
                        root.GetType(), root.Uid);

                    if (root != null && root.Parent == canvas)
                    {
                        canvas.Children.Remove(root);
                    }
                }
            }
        }

        #endregion

        #region Diagram Model

        private static bool CompareString(string strA, string strB)
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

                    string str = string.Format("+;{0};{1};{2};{3};{4}", 
                        element.Uid, 
                        line.X1, line.Y1, 
                        line.X2, line.Y2);

                    sb.AppendLine(str);

                    System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("+;{0};{1};{2}", 
                        element.Uid, 
                        x, y);

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

        private void ParseDiagramModel(string diagram, 
            Canvas canvas, 
            double offsetX, 
            double offsetY, 
            bool appendIds,
            bool updateIds)
        {
            int _pinCounter = 0;
            int _wireCounter = 0;
            int _inputCounter = 0;
            int _outputCounter = 0;
            int _andGateCounter = 0;
            int _orGateCounter = 0;

            WireMap tuple = null;
            string name = null;

            var dict = new Dictionary<string, WireMap>();
            var elements = new List<FrameworkElement>();

            var lines = diagram.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // create root elements
            foreach (var line in lines)
            {
                var args = line.Split(';');
                int length = args.Length;

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareString(args[0], "+"))
                    {
                        if (name.StartsWith("Pin", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            _pinCounter = Math.Max(_pinCounter, id + 1);

                            var element = CreatePin(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Input", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            _inputCounter = Math.Max(_inputCounter, id + 1);

                            var element = CreateInput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Output", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            _outputCounter = Math.Max(_outputCounter, id + 1);

                            var element = CreateOutput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("AndGate", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);  
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            _andGateCounter = Math.Max(_andGateCounter, id + 1);

                            var element = CreateAndGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("OrGate", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split('|')[1]);

                            _orGateCounter = Math.Max(_orGateCounter, id + 1);

                            var element = CreateOrGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith("Wire", StringComparison.InvariantCultureIgnoreCase) && 
                            length == 6)
                        {
                            double x1 = double.Parse(args[2]);  
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            int id = int.Parse(name.Split('|')[1]);

                            _wireCounter = Math.Max(_wireCounter, id + 1);

                            var element = CreateWire(x1 + offsetX, y1 + offsetY, 
                                x2 + offsetX, y2 + offsetY, 
                                id);

                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                    }
                    else if (CompareString(args[0], "-"))
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

                        if (CompareString(_type, "Start"))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, element, null);
                            tuples.Add(_tuple);
                        }
                        else if (CompareString(_type, "End"))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, null, element);
                            tuples.Add(_tuple);
                        }
                    }
                }
            }

            if (appendIds == true)
            {
                // append ids to the existing elements in canvas
                System.Diagnostics.Debug.Print("Appending Ids:");

                foreach (var element in elements)
                {
                    string[] uid = element.Uid.Split('|');

                    string type = uid[0];
                    int id = int.Parse(uid[1]);
                    int appendedId = -1;

                    switch (type)
                    {
                        case "Wire":
                            appendedId = this.wireCounter;
                            this.wireCounter += 1;
                            break;
                        case "Input":
                            appendedId = this.inputCounter;
                            this.inputCounter += 1;
                            break;
                        case "Output":
                            appendedId = this.outputCounter;
                            this.outputCounter += 1;
                            break;
                        case "AndGate":
                            appendedId = this.andGateCounter;
                            this.andGateCounter += 1;
                            break;
                        case "OrGate":
                            appendedId = this.orGateCounter;
                            this.orGateCounter += 1;
                            break;
                        case "Pin":
                            appendedId = this.pinCounter;
                            this.pinCounter += 1;
                            break;
                        default:
                            throw new Exception("Unknown type.");
                    }

                    System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                    string appendedUid = string.Concat(type, "|", appendedId.ToString());
                    element.Uid = appendedUid;

                    //if (element.Tag != null)
                    //{
                    //    var _tuples = element.Tag as List<TagMap>;
                    //    foreach (var _tuple in _tuples)
                    //    {
                    //        System.Diagnostics.Debug.Print("  -{0}", _tuple.Item1.Uid);
                    //    }
                    //}
                }
            }
            else
            {
                if (updateIds == true)
                {
                    // reset existing counters
                    this.pinCounter = Math.Max(this.pinCounter, _pinCounter);
                    this.wireCounter = Math.Max(this.wireCounter, _wireCounter);
                    this.inputCounter = Math.Max(this.inputCounter, _inputCounter);
                    this.outputCounter = Math.Max(this.outputCounter, _outputCounter);
                    this.andGateCounter = Math.Max(this.andGateCounter, _andGateCounter);
                    this.orGateCounter = Math.Max(this.orGateCounter, _orGateCounter);
                }
            }

            // add elements to canvas
            foreach (var element in elements)
            {
                canvas.Children.Add(element);
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

                //this.TextModel.Text = diagram;
            }
        }

        private void Open(string fileName, Canvas canvas)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                ClearDiagramModel(canvas);
                ParseDiagramModel(diagram, canvas, 0, 0, false, true);

                //this.TextModel.Text = diagram;
            }
        }

        private void Import(string fileName)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();
                this.TextModel.Text = diagram;
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
            else if (this.enableInsertLast == true)
            {
                InsertLast(canvas, this.lastInsert, point);
            }
        }

        private bool HandlePreviewLeftDown(Canvas canvas, FrameworkElement pin)
        {
            if (pin != null &&
                !CompareString(pin.Name, "MiddlePin") || Keyboard.Modifiers == ModifierKeys.Control)
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

            if (this.snapOnRelease == true && this.enableSnap == true)
            {
                MoveRoot(thumb, dX, dY, false);
            }
            else
            {
                MoveRoot(thumb, dX, dY, this.enableSnap);
            }
        }

        void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.snapOnRelease == true && this.enableSnap == true)
            {
                var thumb = sender as Thumb;

                MoveRoot(thumb, 0, 0, this.enableSnap);
            }
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

        private void PrintModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = GenerateDiagramModel(this.DiagramCanvas);

            var canvas = new Canvas()
            {
                Background = Brushes.Black,
                Width = 780,
                Height = 660
            };

            ParseDiagramModel(diagram, canvas, 0, 0, false, false);

            Visual visual = canvas; // this.DiagramCanvas;

            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(visual, "diagram");
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

            this.TextModel.Text = text;
        }

        private void ImportModel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                this.Import(dlg.FileName);
            }
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            var diagram = this.TextModel.Text;

            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            //ClearDiagramModel(canvas);
            ParseDiagramModel(diagram, canvas, offsetX, offsetY, true, true);
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

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            this.snapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            this.enableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        } 
    
        #endregion

        #region Zoom

        private void Zoom(double zoom)
        {
            double defaultThickness = 1.0;

            var st = (RootGrid.LayoutTransform as TransformGroup).Children.First(t => t is ScaleTransform) as ScaleTransform;

            st.ScaleX = zoom;
            st.ScaleY = zoom;

            Application.Current.Resources["LogicStrokeThicknessKey"] = defaultThickness / zoom;
        }

        private void ZoomIn()
        {
            double zoom = ZoomSlider.Value;
            zoom += 0.1;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;
            zoom -= 0.1;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoom = ZoomSlider.Value;

            Zoom(zoom);
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                return;

            if (e.Delta > 0)
            {
                ZoomIn();

                e.Handled = true;
            }
            else if (e.Delta < 0)
            {
                ZoomOut();

                e.Handled = true;
            }
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }

        #endregion
    }

    #endregion
}
