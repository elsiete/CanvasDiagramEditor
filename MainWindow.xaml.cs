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

    using PinMap = Tuple<string, string>;
    using TagMap = Tuple<Line, FrameworkElement, FrameworkElement>;
    using WireMap = Tuple<FrameworkElement, List<Tuple<string, string>>>;

    #endregion

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Model String Constants

        private const char argumentSeparator = ';';
        private const string prefixRootElement = "+";
        private const string prefixChildElement = "-";

        private const char tagNameSeparator = '|';

        private const string tagDiagramHeader = "[Diagram]";

        private const string tagElementPin = "Pin";
        private const string tagElementWire = "Wire";
        private const string tagElementInput = "Input";
        private const string tagElementOutput = "Output";
        private const string tagElementAndGate = "AndGate";
        private const string tagElementOrGate = "OrGate";

        private const string wireStartType = "Start";
        private const string wireEndType = "End";

        #endregion

        #region Resource String Constants

        private const string keyStrokeThickness = "LogicStrokeThicknessKey";

        private const string keyTemplatePin = "PinControlTemplateKey";
        private const string keyTemplateInput = "InputControlTemplateKey";
        private const string keyTemplateOutput = "OutputControlTemplateKey";
        private const string keyTemplateAndGate = "AndGateControlTemplateKey";
        private const string keyTemplateOrGate = "OrGateControlTemplateKey";

        private const string keySyleRootThumb = "RootThumbStyleKey";
        private const string keyStyleWireLine = "LineStyleKey";

        private const string standalonePinName = "MiddlePin";

        #endregion

        #region Fields

        private bool enableHistory = true;
        private Stack<string> undoHistory = new Stack<string>();
        private Stack<string> redoHistory = new Stack<string>();

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
        private string lastInsert = tagElementInput;

        private double diagramWidth = 780;
        private double diagramHeight = 660;

        private bool enableSnap = true;
        private bool snapOnRelease = false;
        private double snapX = 15;
        private double snapY = 15;
        private double snapOffsetX = 0;
        private double snapOffsetY = 0;

        private double hitTestRadiusX = 4.0;
        private double hitTestRadiusY = 4.0;

        private bool skipContextMenu = false;
        private bool skipLeftClick = false;

        private Point panStart;
        private double previousScrollOffsetX = -1.0;
        private double previousScrollOffsetY = -1.0;

        private double defaultStrokeThickness = 1.0;

        private double zoomInFactor = 0.1;
        private double zoomOutFactor = 0.1;

        private double reversePanDirection = -1.0; // reverse: 1.0, normal: -1.0
        private double panSpeedFactor = 3.0; // pan speed factor, depends on current zoom
        private MouseButton panButton = MouseButton.Middle;

        #endregion

        #region History (Undo/Redo)

        private void AddToHistory(Canvas canvas)
        {
            if (this.enableHistory != true)
                return;

            var model = GenerateDiagramModel(canvas);

            undoHistory.Push(model);

            redoHistory.Clear();
        }

        private void RollbackUndoHistory(Canvas canvas)
        {
            if (this.enableHistory != true)
                return;

            if (undoHistory.Count <= 0)
                return;

            // remove unused history
            undoHistory.Pop();
        }

        private void RollbackRedoHistory(Canvas canvas)
        {
            if (this.enableHistory != true)
                return;

            if (redoHistory.Count <= 0)
                return;

            // remove unused history
            redoHistory.Pop();
        }

        private void ClearHistory(Canvas canvas)
        {
            undoHistory.Clear();
            redoHistory.Clear();
        }

        private void Undo(Canvas canvas, bool pushRedo)
        {
            if (this.enableHistory != true)
                return;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = GenerateDiagramModel(canvas);
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, 0, 0, false, true);
        }

        private void Redo(Canvas canvas, bool pushUndo)
        {
            if (this.enableHistory != true)
                return;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = GenerateDiagramModel(canvas);
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();
        
            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, 0, 0, false, true);
        }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            EnableHistory.IsChecked = this.enableHistory;
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

        private void SetElementPosition(FrameworkElement element, double left, double top, bool snap)
        {
            Canvas.SetLeft(element, SnapX(left, snap));
            Canvas.SetTop(element, SnapY(top, snap));
        }

        private void MoveRoot(FrameworkElement element, double dX, double dY, bool snap)
        {
            double left = Canvas.GetLeft(element) + dX;
            double top = Canvas.GetTop(element) + dY;

            SetElementPosition(element, left, top, snap);

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

                    var margin = line.Margin;
                    double left = margin.Left;
                    double top = margin.Top;
                    double x = 0;
                    double y = 0;

                    if (start != null)
                    {
                        //line.X1 = SnapX(line.X1 + dX, snap);
                        //line.Y1 = SnapY(line.Y1 + dY, snap);
                        
                        x = SnapX(left + dX, snap);
                        y = SnapX(top + dY, snap);

                        if (left != x || top != y)
                        {
                            line.X2 += left - x;
                            line.Y2 += top - y;
                            line.Margin = new Thickness(x, y, 0, 0);
                        }
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
            thumb.DragStarted += this.RootElement_DragStarted;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

        private Thumb CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources[keyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[keySyleRootThumb] as Style,
                Uid = tagElementPin + tagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private Line CreateWire(double x1, double y1, double x2, double y2, int id)
        {
            var line = new Line()
            {
                Style = Application.Current.Resources[keyStyleWireLine] as Style,
                X1 = 0, //X1 = x1,
                Y1 = 0, //Y1 = y1,
                Margin = new Thickness(x1, y1, 0, 0),
                X2 = x2 - x1, // X2 = x2,
                Y2 = y2 - y1, // Y2 = y2,
                Uid = tagElementWire + tagNameSeparator + id.ToString()
            };

            return line;
        }

        private Thumb CreateInput(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources[keyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[keySyleRootThumb] as Style,
                Uid = tagElementInput + tagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateOutput(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources[keyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[keySyleRootThumb] as Style,
                Uid = tagElementOutput + tagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources[keyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[keySyleRootThumb] as Style,
                Uid = tagElementAndGate + tagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private Thumb CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new Thumb()
            {
                Template = Application.Current.Resources[keyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[keySyleRootThumb] as Style,
                Uid = tagElementOrGate + tagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private void CreatePinConnection(Canvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            var root = 
                (
                    (pin.Parent as FrameworkElement).Parent as FrameworkElement
                ).TemplatedParent as FrameworkElement;

            this._root = root;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(this._root);
            double ry = Canvas.GetTop(this._root);
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);
            double x = rx + px;
            double y = ry + py;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreatePinConnection(canvas, x, y);
        }

        private Line CreatePinConnection(Canvas canvas, double x, double y)
        {
            Line result = null;

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

                // update connections
                var tuple = new TagMap(this._line, this._root, null);
                tuples.Add(tuple);

                canvas.Children.Add(this._line);

                result = line;
            }
            else
            {
                var margin = this._line.Margin;

                this._line.X2 = x - margin.Left;
                this._line.Y2 = y - margin.Top;

                // update connections
                var tuple = new TagMap(this._line, null, this._root);
                tuples.Add(tuple);

                result = this._line;

                this._line = null;
                this._root = null;
            }

            return result;
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
                case tagElementInput:
                    return InsertInput(canvas, point);
                case tagElementOutput:
                    return InsertOutput(canvas, point);
                case tagElementAndGate:
                    return InsertAndGate(canvas, point);
                case tagElementOrGate:
                    return InsertOrGate(canvas, point);
                case tagElementPin:
                    return InsertPin(canvas, point);
                default:
                    return null;
            }
        }

        #endregion

        #region Delete

        private void DeleteElement(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point);
            if (element == null)
                return;

            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is Line && uid != null && 
                uid.StartsWith(tagElementWire, StringComparison.InvariantCultureIgnoreCase))
            {
                var line = element as Line;

                DeleteWire(canvas, line);

            }
            else
            {
                canvas.Children.Remove(element);

                //FrameworkElement parent = element.Parent as FrameworkElement;
                //if (parent != null)
                //{
                //    while (!(parent.TemplatedParent is Thumb))
                //    {
                //        parent = parent.Parent as FrameworkElement;
                //        if (parent == null)
                //            break;
                //    }
                //    if (parent != null)
                //    {
                //        FrameworkElement root = parent.TemplatedParent as FrameworkElement;
                //        System.Diagnostics.Debug.Print("DeleteElement, root: {0}, uid: {1}",
                //            root.GetType(), root.Uid);
                //        if (root != null && root.Parent == canvas)
                //        {
                //            canvas.Children.Remove(root);
                //        }
                //    }
                //}
            }
        }

        private FrameworkElement HitTest(Canvas canvas, ref Point point)
        {
            //var res = VisualTreeHelper.HitTest(canvas, point);
            //var element = res.VisualHit as FrameworkElement;
            //return element;

            var selectedElements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
                {
                    RadiusX = hitTestRadiusX,
                    RadiusY = hitTestRadiusY,
                    Center = new Point(point.X, point.Y),
                };

            var hitTestParams = new GeometryHitTestParameters(elippse);

            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    if (VisualTreeHelper.GetParent(element) == canvas)
                    {
                        selectedElements.Add(element);
                    }
                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(canvas, filterCallback, resultCallback, hitTestParams);

            return selectedElements.FirstOrDefault() as FrameworkElement;
        }

        private static void DeleteWire(Canvas canvas, Line line)
        {
            canvas.Children.Remove(line);

            // remove wire connections
            RemoveWireConnections(canvas, line);

            // find empty pins
            var pins = FindEmptyPins(canvas);

            // remove empty pins
            foreach (var pin in pins)
            {
                canvas.Children.Remove(pin);
            }
        }

        private static List<FrameworkElement> FindEmptyPins(Canvas canvas)
        {
            var pins = new List<FrameworkElement>();

            foreach (var child in canvas.Children)
            {
                var _element = child as FrameworkElement;

                string uid = _element.Uid;

                if (uid != null &&
                    uid.StartsWith(tagElementPin, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_element.Tag != null)
                    {
                        var tuples = _element.Tag as List<TagMap>;
                        if (tuples.Count <= 0)
                        {
                            pins.Add(_element);
                        }
                    }
                    else
                    {
                        pins.Add(_element);
                    }
                }
            }

            return pins;
        }

        private static void RemoveWireConnections(Canvas canvas, Line line)
        {
            foreach (var child in canvas.Children)
            {
                var _element = child as FrameworkElement;

                if (_element.Tag != null)
                {
                    var tuples = _element.Tag as List<TagMap>;
                    var remove = new List<TagMap>();

                    foreach (var tuple in tuples)
                    {
                        var _line = tuple.Item1;

                        if (CompareString(_line.Uid, line.Uid))
                        {
                            remove.Add(tuple);
                        }
                    }

                    foreach (var tuple in remove)
                    {
                        tuples.Remove(tuple);
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
            var model = new StringBuilder();

            string header = tagDiagramHeader;

            //System.Diagnostics.Debug.Print("Generating diagram model:");

            //System.Diagnostics.Debug.Print(header);
            model.AppendLine(header);

            foreach (var child in canvas.Children)
            {
                var element = child as FrameworkElement;

                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);

                if (element.Uid.StartsWith(tagElementWire))
                {
                    var line = element as Line;
                    var margin = line.Margin;

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}", 
                        element.Uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.X2 + margin.Left, line.Y2 + margin.Top,
                        argumentSeparator,
                        prefixRootElement);

                    model.AppendLine(str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}", 
                        element.Uid, 
                        x,  y,
                        argumentSeparator,
                        prefixRootElement);

                    model.AppendLine(str);

                    //System.Diagnostics.Debug.Print(str);
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
                            string str = string.Format("{3}{2}{0}{2}{1}", 
                                line.Uid, 
                                wireStartType, 
                                argumentSeparator, 
                                prefixChildElement);

                            model.AppendLine(str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("{3}{2}{0}{2}{1}", 
                                line.Uid, 
                                wireEndType,
                                argumentSeparator,
                                prefixChildElement);

                            model.AppendLine(str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }

            return model.ToString();
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

            var lines = diagram.Split(Environment.NewLine.ToCharArray(), 
                StringSplitOptions.RemoveEmptyEntries);

            //System.Diagnostics.Debug.Print("Parsing diagram model:");

            // create root elements
            foreach (var line in lines)
            {
                var args = line.Split(argumentSeparator);
                int length = args.Length;

                //System.Diagnostics.Debug.Print(line);

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareString(args[0], prefixRootElement))
                    {
                        if (name.StartsWith(tagElementPin, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _pinCounter = Math.Max(_pinCounter, id + 1);

                            var element = CreatePin(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(tagElementInput, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _inputCounter = Math.Max(_inputCounter, id + 1);

                            var element = CreateInput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(tagElementOutput, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _outputCounter = Math.Max(_outputCounter, id + 1);

                            var element = CreateOutput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(tagElementAndGate, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);  
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _andGateCounter = Math.Max(_andGateCounter, id + 1);

                            var element = CreateAndGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(tagElementOrGate, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _orGateCounter = Math.Max(_orGateCounter, id + 1);

                            var element = CreateOrGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(tagElementWire, StringComparison.InvariantCultureIgnoreCase) && 
                            length == 6)
                        {
                            double x1 = double.Parse(args[2]);  
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            int id = int.Parse(name.Split(tagNameSeparator)[1]);

                            _wireCounter = Math.Max(_wireCounter, id + 1);

                            var element = CreateWire(x1 + offsetX, y1 + offsetY, 
                                x2 + offsetX, y2 + offsetY, 
                                id);

                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                    }
                    else if (CompareString(args[0], prefixChildElement))
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
            UpdateWireConnections(dict);

            if (appendIds == true)
            {
                AppendElementIds(elements);
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

        private void UpdateWireConnections(Dictionary<string, WireMap> dict)
        {
            // update wire to element connections
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

                        if (CompareString(_type, wireStartType))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, element, null);
                            tuples.Add(_tuple);
                        }
                        else if (CompareString(_type, wireEndType))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, null, element);
                            tuples.Add(_tuple);
                        }
                    }
                }
            }
        }

        private void AppendElementIds(List<FrameworkElement> elements)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements)
            {
                string[] uid = element.Uid.Split(tagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);
                int appendedId = -1;

                switch (type)
                {
                    case tagElementWire:
                        appendedId = this.wireCounter;
                        this.wireCounter += 1;
                        break;
                    case tagElementInput:
                        appendedId = this.inputCounter;
                        this.inputCounter += 1;
                        break;
                    case tagElementOutput:
                        appendedId = this.outputCounter;
                        this.outputCounter += 1;
                        break;
                    case tagElementAndGate:
                        appendedId = this.andGateCounter;
                        this.andGateCounter += 1;
                        break;
                    case tagElementOrGate:
                        appendedId = this.orGateCounter;
                        this.orGateCounter += 1;
                        break;
                    case tagElementPin:
                        appendedId = this.pinCounter;
                        this.pinCounter += 1;
                        break;
                    default:
                        throw new Exception("Unknown element type.");
                }

                //System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                string appendedUid = string.Concat(type, tagNameSeparator, appendedId.ToString());
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

        #endregion

        #region Open/Save

        private void Save(string fileName, Canvas canvas)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                string model = GenerateDiagramModel(canvas);

                writer.Write(model);
            }
        }

        private void Open(string fileName, Canvas canvas)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                AddToHistory(canvas);

                ClearDiagramModel(canvas);
                ParseDiagramModel(diagram, canvas, 0, 0, false, true);
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

                //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(this._root);
                double ry = Canvas.GetTop(this._root);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                CreatePinConnection(canvas, x, y);

                this._root = root;

                CreatePinConnection(canvas, x, y);
            }
            else if (this.enableInsertLast == true)
            {
                AddToHistory(canvas);

                InsertLast(canvas, this.lastInsert, point);
            }
        }

        private bool HandlePreviewLeftDown(Canvas canvas, FrameworkElement pin)
        {
            if (pin != null &&
                (!CompareString(pin.Name, standalonePinName) || Keyboard.Modifiers == ModifierKeys.Control))
            {
                if (this._line == null)
                    AddToHistory(canvas);

                CreatePinConnection(canvas, pin);

                return true;
            }

            return false;
        }

        private void HandleMove(Canvas canvas, Point point)
        {
            if (this._root != null && this._line != null)
            {
                var margin = this._line.Margin;

                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

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
                if (this.enableHistory == true)
                {
                    Undo(canvas, false);
                }
                else
                {
                    var tuples = this._root.Tag as List<TagMap>;

                    var last = tuples.LastOrDefault();
                    tuples.Remove(last);

                    canvas.Children.Remove(this._line);

                    //RollbackUndoHistory(canvas);
                }
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
            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            bool snap = (this.snapOnRelease == true && this.enableSnap == true) ? false : this.enableSnap;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // move all elements when Control key is pressed
                var canvas = this.DiagramCanvas;
                var thumbs = canvas.Children.OfType<Thumb>();

                foreach(var thumb in thumbs)
                {
                    MoveRoot(thumb, dX, dY, snap);
                }
            }
            else
            {
                // move only selected element
                var thumb = sender as Thumb;

                MoveRoot(thumb, dX, dY, snap);
            }
        }

        private void RootElement_DragStarted(object sender, DragStartedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);
        }

        private void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.snapOnRelease == true && this.enableSnap == true)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    // move all elements when Control key is pressed
                    var canvas = this.DiagramCanvas;
                    var thumbs = canvas.Children.OfType<Thumb>();

                    foreach (var thumb in thumbs)
                    {
                        MoveRoot(thumb, 0.0, 0.0, this.enableSnap);
                    }
                }
                else
                {
                    // move only selected element
                    var thumb = sender as Thumb;

                    MoveRoot(thumb, 0.0, 0.0, this.enableSnap);
                }
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

        private void Open()
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

        private void Save()
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

        private void Print()
        {
            var model = GenerateDiagramModel(this.DiagramCanvas);

            var canvas = new Canvas()
            {
                Background = Brushes.Black,
                Width = this.diagramWidth,
                Height = this.diagramHeight
            };

            ParseDiagramModel(model, canvas, 0, 0, false, false);

            Visual visual = canvas; // this.DiagramCanvas;

            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(visual, "diagram");
        }

        private void Import()
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

        private void Insert()
        {
            var canvas = this.DiagramCanvas;
            var diagram = this.TextModel.Text;

            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            AddToHistory(canvas);

            //ClearDiagramModel(canvas);
            ParseDiagramModel(diagram, canvas, offsetX, offsetY, true, true);
        }

        private void OpenModel_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void SaveModel_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void PrintModel_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void UndoHistory_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            this.Undo(canvas, true);
        }

        private void RedoHistory_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;
            this.Redo(canvas, true);
        }

        private void ClearModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);

            ClearDiagramModel(canvas);
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            var model = GenerateDiagramModel(canvas);

            this.TextModel.Text = model;
        }

        private void ImportModel_Click(object sender, RoutedEventArgs e)
        {
            Import();
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            Insert();
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

            AddToHistory(canvas);

            InsertPin(canvas, this.rightClick);

            this.lastInsert = tagElementPin;
            this.skipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);

            InsertInput(canvas, this.rightClick);

            this.lastInsert = tagElementInput;
            this.skipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);

            InsertOutput(canvas, this.rightClick);

            this.lastInsert = tagElementOutput;
            this.skipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);

            InsertAndGate(canvas, this.rightClick);

            this.lastInsert = tagElementAndGate;
            this.skipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = this.DiagramCanvas;

            AddToHistory(canvas);

            InsertOrGate(canvas, this.rightClick);

            this.lastInsert = tagElementOrGate;
            this.skipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = DiagramCanvas;
            var menu = sender as MenuItem;
            var point = new Point(this.rightClick.X, this.rightClick.Y);

            AddToHistory(canvas);

            DeleteElement(canvas, point);
            this.skipLeftClick = false;
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            this.enableHistory = EnableHistory.IsChecked == true ? true : false;

            if (this.enableHistory == false)
            {
                var canvas = this.DiagramCanvas;

                ClearHistory(canvas);
            }
        }

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

        private Point zoomPoint;

        private void Zoom(double zoom)
        {
            //var tg = RootGrid.RenderTransform as TransformGroup;
            var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is ScaleTransform) as ScaleTransform;

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom;
            st.ScaleY = zoom;

            Application.Current.Resources[keyStrokeThickness] = defaultStrokeThickness / zoom;

            // zoom to point
            ZoomToPoint(zoom, oldZoom);
        }

        private void ZoomToPoint(double zoom, double oldZoom)
        {
            double offsetX = 0;
            double offsetY = 0;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = this.PanScrollViewer.HorizontalOffset;
            double scrollOffsetY = this.PanScrollViewer.VerticalOffset;

            double oldX = zoomPoint.X * oldZoom;
            double oldY = zoomPoint.Y * oldZoom;

            double newX = zoomPoint.X * zoom;
            double newY = zoomPoint.Y * zoom;

            offsetX = newX - oldX;
            offsetY = newY - oldY;

            //System.Diagnostics.Debug.Print("");
            //System.Diagnostics.Debug.Print("zoomPoint: {0},{1}", Math.Round(zoomPoint.X, 0), Math.Round(zoomPoint.Y, 0));
            //System.Diagnostics.Debug.Print("scrollableWidth/Height: {0},{1}", scrollableWidth, scrollableHeight);
            //System.Diagnostics.Debug.Print("scrollOffsetX/Y: {0},{1}", scrollOffsetX, scrollOffsetY);
            //System.Diagnostics.Debug.Print("oldX/Y: {0},{1}", oldX, oldY);
            //System.Diagnostics.Debug.Print("newX/Y: {0},{1}", newX, newY);
            //System.Diagnostics.Debug.Print("offsetX/Y: {0},{1}", offsetX, offsetY);

            if (scrollableWidth <= 0)
                offsetX = 0.0;

            if (scrollableHeight <= 0)
                offsetY = 0.0;

            PanToOffset(offsetX, offsetY);
        }

        private void ZoomIn()
        {
            double zoom = ZoomSlider.Value;
            zoom += zoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;
            zoom -= zoomOutFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoom = ZoomSlider.Value;

            if (e.OldValue != e.NewValue)
            {
                Zoom(zoom);
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                return;

            var canvas = this.DiagramCanvas;
            this.zoomPoint = e.GetPosition(canvas);

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

        #region Pan

        private void BeginPan(Point point)
        {
            this.panStart = point;

            this.previousScrollOffsetX = -1.0;
            this.previousScrollOffsetY = -1.0;

            this.Cursor = Cursors.ScrollAll;
            this.PanScrollViewer.CaptureMouse();
        }

        private void EndPan()
        {
            if (PanScrollViewer.IsMouseCaptured == true)
            {
                this.Cursor = Cursors.Arrow;
                this.PanScrollViewer.ReleaseMouseCapture();
            }
        } 

        private void PanToPoint(Point point)
        {
            double scrollOffsetX = point.X - panStart.X;
            double scrollOffsetY = point.Y - panStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;
            double panSpeed = zoom / panSpeedFactor;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * panSpeed) * reversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * panSpeed) * reversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != this.previousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                this.previousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != this.previousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                this.previousScrollOffsetY = scrollOffsetY;
            }

            this.panStart = point;
        }

        private void PanToOffset(double offsetX, double offsetY)
        {
            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = Math.Round(horizontalOffset + offsetX, 0);
            double scrollOffsetY = Math.Round(verticalOffset + offsetY, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
            this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
        }

        #endregion

        #region PanScrollViewer Events

        private void PanScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == panButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == panButton)
            {
                EndPan();
            }
        }

        private void PanScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.PanScrollViewer.IsMouseCaptured == true)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                PanToPoint(point);
            }
        }

        #endregion
    }

    #endregion
}
