#region References

using CanvasDiagramEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    // TemplatedParent.Tag: Item1: IsSelected, Item2: TagMap
    using Selection = Tuple<bool, List<Tuple<Line, FrameworkElement, FrameworkElement>>>;

    // Canvas.Tag => Item1: undoHistory, Item2: redoHistory
    using History = Tuple<Stack<string>, Stack<string>>;

    #endregion

    #region  Tuple .NET 3.5

    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        internal Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }
        public T3 Item3 { get; private set; }
        internal Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }

    #endregion

    #region SelectionThumb

    public class SelectionThumb : Thumb
    {
        #region IsSelected Attached Property

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(SelectionThumb),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)); 

        #endregion
    }

    #endregion

    #region IDiagramExport

    public interface IDiagramExport
    {
        void CreateDocument(string fileName, IEnumerable<string> diagrams);
    }

    #endregion

    #region Constants

    public static class ModelConstants
    {
        #region Model String Constants

        public const char ArgumentSeparator = ';';
        public const string PrefixRootElement = "+";
        public const string PrefixChildElement = "-";

        public const char TagNameSeparator = '|';

        public const string TagDiagramHeader = "Diagram";

        public const string TagElementPin = "Pin";
        public const string TagElementWire = "Wire";
        public const string TagElementInput = "Input";
        public const string TagElementOutput = "Output";
        public const string TagElementAndGate = "AndGate";
        public const string TagElementOrGate = "OrGate";

        public const string WireStartType = "Start";
        public const string WireEndType = "End";

        #endregion
    }

    public static class ResourceConstants
    {
        #region Resource String Constants

        public const string KeyStrokeThickness = "LogicStrokeThicknessKey";

        public const string KeyTemplatePin = "PinControlTemplateKey";
        public const string KeyTemplateInput = "InputControlTemplateKey";
        public const string KeyTemplateOutput = "OutputControlTemplateKey";
        public const string KeyTemplateAndGate = "AndGateControlTemplateKey";
        public const string KeyTemplateOrGate = "OrGateControlTemplateKey";

        public const string KeySyleRootThumb = "RootThumbStyleKey";
        public const string KeyStyleWireLine = "LineStyleKey";

        public const string StandalonePinName = "MiddlePin";

        #endregion
    }

    #endregion

    #region Parser

    public class IdCounter
    {
        public IdCounter()
        {
            PinCount = 0;
            WireCount = 0;
            InputCount = 0;
            OutputCount = 0;
            AndGateCount = 0;
            OrGateCount = 0;
        }

        public int PinCount { get; set; }
        public int WireCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public int AndGateCount { get; set; }
        public int OrGateCount { get; set; }
    }

    public interface IDiagramCreator
    {
        object CreatePin(int id, double x, double y);
        object CreateWire(int id, double x1, double y1, double x2, double y2);
        object CreateInput(int id, double x, double y, string text);
        object CreateOutput(int id, double x, double y, string text);
        object CreateAndGate(int id, double x, double y);
        object CreateOrGate(int id, double x, double y);
    }

    public interface IDiagramParser
    {
        IEnumerable<object> Parse(string diagram, IDiagramCreator creator);
    }

    public class DiagramParser : IDiagramParser
    {
        public IEnumerable<object> Parse(string diagram, IDiagramCreator creator)
        {
            var elements = new List<object>();

            // ...

            return elements;
        }
    }

    #endregion

    #region DiagramEditorOptions

    public class DiagramEditorOptions
    {
        #region Fields

        public Canvas currentCanvas = null;
        public Path currentPathGrid = null;

        public bool enableHistory = true;

        public Line currentLine = null;
        public FrameworkElement currentRoot = null;

        public int pinCounter = 0;
        public int wireCounter = 0;
        public int inputCounter = 0;
        public int outputCounter = 0;
        public int andGateCounter = 0;
        public int orGateCounter = 0;

        public Point rightClick;

        public bool enableInsertLast = false;
        public string lastInsert = ModelConstants.TagElementInput;

        public double defaultGridSize = 30;

        public bool enableSnap = true;
        public bool snapOnRelease = false;
        public double snapX = 15;
        public double snapY = 15;
        public double snapOffsetX = 0;
        public double snapOffsetY = 0;

        public bool moveWithShift = false;

        public double hitTestRadiusX = 4.0;
        public double hitTestRadiusY = 4.0;

        public bool skipContextMenu = false;
        public bool skipLeftClick = false;

        public Point panStart;
        public double previousScrollOffsetX = -1.0;
        public double previousScrollOffsetY = -1.0;

        public double defaultStrokeThickness = 1.0;

        public double zoomInFactor = 0.1;
        public double zoomOutFactor = 0.1;

        public Point zoomPoint;

        public double reversePanDirection = -1.0; // reverse: 1.0, normal: -1.0
        public double panSpeedFactor = 3.0; // pan speed factor, depends on current zoom
        public MouseButton panButton = MouseButton.Middle;

        #endregion
    }

    #endregion

    #region DiagramEditor

    public class DiagramEditor
    {
        #region Fields

        public DiagramEditorOptions options = null;

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
                Snap(original, options.snapX, options.snapOffsetX) : original;
        }

        private double SnapY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.snapY, options.snapOffsetY) : original;
        }

        #endregion

        #region Grid

        public void GenerateGrid(Path path, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();

            double originX = 0;
            double originY = 0;

            double startX = size;
            double startY = size;

            // horizontal lines
            for (double y = startY + originY /* originY + size */; y < height + originY; y += size)
            {
                sb.AppendFormat("M{0},{1}", originX, y);
                sb.AppendFormat("L{0},{1}", width + originX, y);
            }

            // vertical lines
            for (double x = startX + originX /* originX + size */; x < width + originX; x += size)
            {
                sb.AppendFormat("M{0},{1}", x, originY);
                sb.AppendFormat("L{0},{1}", x, height + originY);
            }

            string s = sb.ToString();

            path.Data = Geometry.Parse(s);

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateGrid() in {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public void SetDiagramSize(Canvas canvas, double width, double height)
        {
            canvas.Width = width;
            canvas.Height = height;
        }

        #endregion

        #region History

        private History GetHistory(Canvas canvas)
        {
            if (canvas.Tag == null)
            {
                canvas.Tag = new History(new Stack<string>(), new Stack<string>());
            }

            var tuple = canvas.Tag as History;

            return tuple;
        }

        public void AddToHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            var model = GenerateDiagramModel(canvas);

            undoHistory.Push(model);

            redoHistory.Clear();
        }

        private void RollbackUndoHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // remove unused history
            undoHistory.Pop();
        }

        private void RollbackRedoHistory(Canvas canvas)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // remove unused history
            redoHistory.Pop();
        }

        public void ClearHistory(Canvas canvas)
        {
            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            undoHistory.Clear();
            redoHistory.Clear();
        }

        private void Undo(Canvas canvas, Path path, bool pushRedo)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

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
            ParseDiagramModel(model, canvas, path, 0, 0, false, true);
        }

        private void Redo(Canvas canvas, Path path, bool pushUndo)
        {
            if (options.enableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

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
            ParseDiagramModel(model, canvas, path, 0, 0, false, true);
        }

        public void Undo()
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            this.Undo(canvas, path, true);
        }

        public void Redo()
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            this.Redo(canvas, path, true);
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
                var selection = element.Tag as Selection;
                var tuples = selection.Item2;

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

        #region Drag

        public void Drag(Canvas canvas, SelectionThumb element, double dX, double dY)
        {
            bool snap = (options.snapOnRelease == true && options.enableSnap == true) ? false : options.enableSnap;

            if (Keyboard.Modifiers == ModifierKeys.Shift || options.moveWithShift == true)
            {
                // move all elements when Shift key is pressed
                var thumbs = canvas.Children.OfType<SelectionThumb>();

                foreach (var thumb in thumbs)
                {
                    MoveRoot(thumb, dX, dY, snap);
                }
            }
            else
            {
                // move only selected element

                MoveRoot(element, dX, dY, snap);
            }
        }

        public void DragStart(Canvas canvas, SelectionThumb element)
        {
            AddToHistory(canvas);

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                options.moveWithShift = true;

                SetSelectionAll(canvas, true);

                SelectionThumb.SetIsSelected(canvas, true);
            }
            else
            {
                options.moveWithShift = false;

                // select
                SelectionThumb.SetIsSelected(element, true);
            }
        }

        public void DragEnd(Canvas canvas, SelectionThumb element)
        {
            if (options.snapOnRelease == true && options.enableSnap == true)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift || options.moveWithShift == true)
                {
                    // move all elements when Shift key is pressed

                    var thumbs = canvas.Children.OfType<SelectionThumb>();

                    SelectionThumb.SetIsSelected(canvas, false);

                    foreach (var thumb in thumbs)
                    {
                        // deselect
                        SelectionThumb.SetIsSelected(thumb, false);

                        MoveRoot(thumb, 0.0, 0.0, options.enableSnap);
                    }
                }
                else
                {
                    // move only selected element

                    // deselect
                    SelectionThumb.SetIsSelected(element, false);

                    MoveRoot(element, 0.0, 0.0, options.enableSnap);
                }
            }
            else
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift || options.moveWithShift == true)
                {
                    SelectionThumb.SetIsSelected(canvas, false);

                    SetSelectionAll(canvas, false);
                }
                else
                {
                    // de-select
                    SelectionThumb.SetIsSelected(element, false);
                }
            }

            options.moveWithShift = false;
        }

        #endregion

        #region Thumb Events

        private void RootElement_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            Drag(canvas, element, dX, dY);
        }

        private void RootElement_DragStarted(object sender, DragStartedEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            DragStart(canvas, element);
        }

        private void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var canvas = options.currentCanvas;
            var element = sender as SelectionThumb;

            DragEnd(canvas, element);
        }

        #endregion

        #region Create

        private void SetThumbEvents(SelectionThumb thumb)
        {
            thumb.DragDelta += this.RootElement_DragDelta;
            thumb.DragStarted += this.RootElement_DragStarted;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

        private SelectionThumb CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementPin + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private Line CreateWire(double x1, double y1, double x2, double y2, int id)
        {
            var line = new Line()
            {
                Style = Application.Current.Resources[ResourceConstants.KeyStyleWireLine] as Style,
                X1 = 0, //X1 = x1,
                Y1 = 0, //Y1 = y1,
                Margin = new Thickness(x1, y1, 0, 0),
                X2 = x2 - x1, // X2 = x2,
                Y2 = y2 - y1, // Y2 = y2,
                Uid = ModelConstants.TagElementWire + ModelConstants.TagNameSeparator + id.ToString()
            };

            return line;
        }

        private SelectionThumb CreateInput(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementInput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private SelectionThumb CreateOutput(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOutput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private SelectionThumb CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementAndGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        private SelectionThumb CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new SelectionThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOrGate + ModelConstants.TagNameSeparator + id.ToString()
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

            options.currentRoot = root;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(options.currentRoot);
            double ry = Canvas.GetTop(options.currentRoot);
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

            if (options.currentRoot.Tag == null)
            {
                options.currentRoot.Tag = new Selection(false, new List<TagMap>());
            }

            var selection = options.currentRoot.Tag as Selection;
            var tuples = selection.Item2;

            if (options.currentLine == null)
            {
                var line = CreateWire(x, y, x, y, options.wireCounter);
                options.wireCounter += 1;

                options.currentLine = line;

                // update connections
                var tuple = new TagMap(options.currentLine, options.currentRoot, null);
                tuples.Add(tuple);

                canvas.Children.Add(options.currentLine);

                result = line;
            }
            else
            {
                var margin = options.currentLine.Margin;

                options.currentLine.X2 = x - margin.Left;
                options.currentLine.Y2 = y - margin.Top;

                // update connections
                var tuple = new TagMap(options.currentLine, null, options.currentRoot);
                tuples.Add(tuple);

                result = options.currentLine;

                options.currentLine = null;
                options.currentRoot = null;
            }

            return result;
        }

        #endregion

        #region Insert

        public FrameworkElement InsertPin(Canvas canvas, Point point)
        {
            var thumb = CreatePin(point.X, point.Y, options.pinCounter, options.enableSnap);
            options.pinCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, options.inputCounter, options.enableSnap);
            options.inputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, options.outputCounter, options.enableSnap);
            options.outputCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, options.andGateCounter, options.enableSnap);
            options.andGateCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, options.orGateCounter, options.enableSnap);
            options.orGateCounter += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertLast(Canvas canvas, string type, Point point)
        {
            switch (type)
            {
                case ModelConstants.TagElementInput:
                    return InsertInput(canvas, point);
                case ModelConstants.TagElementOutput:
                    return InsertOutput(canvas, point);
                case ModelConstants.TagElementAndGate:
                    return InsertAndGate(canvas, point);
                case ModelConstants.TagElementOrGate:
                    return InsertOrGate(canvas, point);
                case ModelConstants.TagElementPin:
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
                uid.StartsWith(ModelConstants.TagElementWire, StringComparison.InvariantCultureIgnoreCase))
            {
                var line = element as Line;

                DeleteWire(canvas, line);

            }
            else
            {
                canvas.Children.Remove(element);
            }
        }

        private FrameworkElement HitTest(Canvas canvas, ref Point point)
        {
            var selectedElements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
            {
                RadiusX = options.hitTestRadiusX,
                RadiusY = options.hitTestRadiusY,
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
                    uid.StartsWith(ModelConstants.TagElementPin, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_element.Tag != null)
                    {
                        var selection = _element.Tag as Selection;
                        var tuples = selection.Item2;

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
                    var selection = _element.Tag as Selection;
                    var tuples = selection.Item2;

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

        public void Delete(Canvas canvas, Point point)
        {
            AddToHistory(canvas);

            DeleteElement(canvas, point);

            options.skipLeftClick = false;
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

            options.pinCounter = 0;
            options.wireCounter = 0;
            options.inputCounter = 0;
            options.outputCounter = 0;
            options.andGateCounter = 0;
            options.orGateCounter = 0;
        }

        private string GenerateDiagramModel(Canvas diagram)
        {
            var diagrams = new List<Canvas>();

            diagrams.Add(diagram);

            return GenerateDiagramModel(diagrams);
        }

        private string GenerateDiagramModel(IEnumerable<Canvas> diagrams)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            foreach (var canvas in diagrams)
            {
                var children = canvas.Children;
                double width = canvas.Width;
                double height = canvas.Height;

                string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}",
                    ModelConstants.PrefixRootElement,
                    ModelConstants.ArgumentSeparator,
                    ModelConstants.TagDiagramHeader,
                    width, height, options.defaultGridSize);

                diagram.AppendLine(header);
                //System.Diagnostics.Debug.Print(header);

                foreach (var child in children)
                {
                    var element = child as FrameworkElement;

                    double x = Canvas.GetLeft(element);
                    double y = Canvas.GetTop(element);

                    if (element.Uid.StartsWith(ModelConstants.TagElementWire))
                    {
                        var line = element as Line;
                        var margin = line.Margin;

                        string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}",
                            element.Uid,
                            margin.Left, margin.Top, //line.X1, line.Y1,
                            line.X2 + margin.Left, line.Y2 + margin.Top,
                            ModelConstants.ArgumentSeparator,
                            ModelConstants.PrefixRootElement);

                        diagram.AppendLine(str);

                        //System.Diagnostics.Debug.Print(str);
                    }
                    else
                    {
                        string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                            element.Uid,
                            x, y,
                            ModelConstants.ArgumentSeparator,
                            ModelConstants.PrefixRootElement);

                        diagram.AppendLine(str);

                        //System.Diagnostics.Debug.Print(str);
                    }

                    if (element.Tag != null)
                    {
                        var selection = element.Tag as Selection;
                        var tuples = selection.Item2;

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
                                    ModelConstants.WireStartType,
                                    ModelConstants.ArgumentSeparator,
                                    ModelConstants.PrefixChildElement);

                                diagram.AppendLine(str);

                                //System.Diagnostics.Debug.Print(str);
                            }
                            else if (end != null)
                            {
                                // End
                                string str = string.Format("{3}{2}{0}{2}{1}",
                                    line.Uid,
                                    ModelConstants.WireEndType,
                                    ModelConstants.ArgumentSeparator,
                                    ModelConstants.PrefixChildElement);

                                diagram.AppendLine(str);

                                //System.Diagnostics.Debug.Print(str);
                            }
                        }
                    }
                }
            }

            var result = diagram.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        private void ParseDiagramModel(string diagram,
            Canvas canvas, Path path,
            double offsetX, double offsetY,
            bool appendIds, bool updateIds)
        {
            int _pinCounter = 0;
            int _wireCounter = 0;
            int _inputCounter = 0;
            int _outputCounter = 0;
            int _andGateCounter = 0;
            int _orGateCounter = 0;

            WireMap tuple = null;
            string name = null;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var dict = new Dictionary<string, WireMap>();
            var elements = new List<FrameworkElement>();

            var lines = diagram.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            //System.Diagnostics.Debug.Print("Parsing diagram model:");

            // create root elements
            foreach (var line in lines)
            {
                var args = line.Split(ModelConstants.ArgumentSeparator);
                int length = args.Length;

                //System.Diagnostics.Debug.Print(line);

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareString(args[0], ModelConstants.PrefixRootElement))
                    {
                        if (name.StartsWith(ModelConstants.TagDiagramHeader, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 5)
                        {
                            double width = double.Parse(args[2]);
                            double height = double.Parse(args[3]);
                            double size = double.Parse(args[4]);

                            GenerateGrid(path, width, height, size);
                            SetDiagramSize(canvas, width, height);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementPin, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _pinCounter = Math.Max(_pinCounter, id + 1);

                            var element = CreatePin(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementInput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _inputCounter = Math.Max(_inputCounter, id + 1);

                            var element = CreateInput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOutput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _outputCounter = Math.Max(_outputCounter, id + 1);

                            var element = CreateOutput(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementAndGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _andGateCounter = Math.Max(_andGateCounter, id + 1);

                            var element = CreateAndGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOrGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            double x = double.Parse(args[2]);
                            double y = double.Parse(args[3]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _orGateCounter = Math.Max(_orGateCounter, id + 1);

                            var element = CreateOrGate(x + offsetX, y + offsetY, id, false);
                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                        else if (name.StartsWith(ModelConstants.TagElementWire, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 6)
                        {
                            double x1 = double.Parse(args[2]);
                            double y1 = double.Parse(args[3]);
                            double x2 = double.Parse(args[4]);
                            double y2 = double.Parse(args[5]);

                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            _wireCounter = Math.Max(_wireCounter, id + 1);

                            var element = CreateWire(x1 + offsetX, y1 + offsetY,
                                x2 + offsetX, y2 + offsetY,
                                id);

                            elements.Add(element);

                            tuple = new WireMap(element, new List<PinMap>());

                            dict.Add(args[1], tuple);
                        }
                    }
                    else if (CompareString(args[0], ModelConstants.PrefixChildElement))
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
                    options.pinCounter = Math.Max(options.pinCounter, _pinCounter);
                    options.wireCounter = Math.Max(options.wireCounter, _wireCounter);
                    options.inputCounter = Math.Max(options.inputCounter, _inputCounter);
                    options.outputCounter = Math.Max(options.outputCounter, _outputCounter);
                    options.andGateCounter = Math.Max(options.andGateCounter, _andGateCounter);
                    options.orGateCounter = Math.Max(options.orGateCounter, _orGateCounter);
                }
            }

            // add elements to canvas
            foreach (var element in elements)
            {
                canvas.Children.Add(element);
            }

            sw.Stop();
            System.Diagnostics.Debug.Print("ParseDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);
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
                    element.Tag = new Selection(false, new List<TagMap>());
                }

                if (wires.Count > 0)
                {
                    var selection = element.Tag as Selection;
                    var tuples = selection.Item2;

                    foreach (var wire in wires)
                    {
                        string _name = wire.Item1;
                        string _type = wire.Item2;

                        if (CompareString(_type, ModelConstants.WireStartType))
                        {
                            var line = dict[_name].Item1 as Line;

                            var _tuple = new TagMap(line, element, null);
                            tuples.Add(_tuple);
                        }
                        else if (CompareString(_type, ModelConstants.WireEndType))
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
                string[] uid = element.Uid.Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);
                int appendedId = -1;

                switch (type)
                {
                    case ModelConstants.TagElementWire:
                        appendedId = options.wireCounter;
                        options.wireCounter += 1;
                        break;
                    case ModelConstants.TagElementInput:
                        appendedId = options.inputCounter;
                        options.inputCounter += 1;
                        break;
                    case ModelConstants.TagElementOutput:
                        appendedId = options.outputCounter;
                        options.outputCounter += 1;
                        break;
                    case ModelConstants.TagElementAndGate:
                        appendedId = options.andGateCounter;
                        options.andGateCounter += 1;
                        break;
                    case ModelConstants.TagElementOrGate:
                        appendedId = options.orGateCounter;
                        options.orGateCounter += 1;
                        break;
                    case ModelConstants.TagElementPin:
                        appendedId = options.pinCounter;
                        options.pinCounter += 1;
                        break;
                    default:
                        throw new Exception("Unknown element type.");
                }

                //System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                string appendedUid = string.Concat(type, ModelConstants.TagNameSeparator, appendedId.ToString());
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

        private void Open(string fileName, Canvas canvas, Path path)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                AddToHistory(canvas);

                ClearDiagramModel(canvas);
                ParseDiagramModel(diagram, canvas, path, 0, 0, false, true);
            }
        }

        public string Import(string fileName)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                return diagram;
            }
        }

        public void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;
                var path = options.currentPathGrid;

                this.Open(dlg.FileName, canvas, path);
            }
        }

        public void Save()
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
                var canvas = options.currentCanvas;

                this.Save(dlg.FileName, canvas);
            }
        }

        public void Export()
        {
            //Export(new MsoWord.MsoWordExport(), false);
            Export(new OpenXml.OpenXmlExport(), false);
        }

        public void ExportHistory()
        {
            //Export(new MsoWord.MsoWordExport(), true);
            Export(new OpenXml.OpenXmlExport(), true);
        }

        private void Export(IDiagramExport export, bool exportHistory)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Word Document (*.docx)|*.docx|All Files (*.*)|*.*",
                Title = "Export to Word Document",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = options.currentCanvas;
                var fileName = dlg.FileName;

                Export(export, exportHistory, canvas, fileName);

                MessageBox.Show("Exported document: " +
                    System.IO.Path.GetFileName(dlg.FileName),
                    "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Export(IDiagramExport export, bool exportHistory, Canvas canvas, string fileName)
        {
            List<string> diagrams = null;

            var currentDiagram = GenerateDiagramModel(canvas);

            if (exportHistory == false)
            {
                diagrams = new List<string>();
            }
            else
            {
                var history = GetHistory(canvas);
                var undoHistory = history.Item1;
                var redoHistory = history.Item2;

                diagrams = new List<string>(undoHistory.Reverse());
            }

            diagrams.Add(currentDiagram);

            if (diagrams == null)
                throw new NullReferenceException();

            export.CreateDocument(fileName, diagrams);
        }

        public void Print()
        {
            var model = GenerateDiagramModel(options.currentCanvas);

            var canvas = new Canvas()
            {
                Background = Brushes.Black,
                Width = options.currentCanvas.Width,
                Height = options.currentCanvas.Height
            };

            Path path = new Path();

            ParseDiagramModel(model, canvas, path, 0, 0, false, false);

            Visual visual = canvas;

            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(visual, "diagram");
        }

        public string Import()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var diagram = this.Import(dlg.FileName);

                return diagram;
            }

            return null;
        }

        public void Insert(string diagram, double offsetX, double offsetY)
        {
            var canvas = options.currentCanvas;
            var path = options.currentPathGrid;

            AddToHistory(canvas);

            ParseDiagramModel(diagram, canvas, path, offsetX, offsetY, true, true);
        }

        public void Clear()
        {
            var canvas = options.currentCanvas;

            AddToHistory(canvas);

            ClearDiagramModel(canvas);
        }

        public string Generate()
        {
            var canvas = options.currentCanvas;

            var diagram = GenerateDiagramModel(canvas);

            return diagram;
        }

        #endregion

        #region Edit

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Copy()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void SetSelectionAll(Canvas canvas, bool selected)
        {
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                SelectionThumb.SetIsSelected(thumb, selected);
            }
        }

        public void SelectAll()
        {
            var canvas = options.currentCanvas;

            SetSelectionAll(canvas, true);

            SelectionThumb.SetIsSelected(canvas, true);
        }

        public void DeselectAll()
        {
            var canvas = options.currentCanvas;

            SetSelectionAll(canvas, false);

            SelectionThumb.SetIsSelected(canvas, false);
        }

        public void Preferences()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Handlers

        public void HandleLeftDown(Canvas canvas, Point point)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                var root = InsertPin(canvas, point);

                options.currentRoot = root;

                //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(options.currentRoot);
                double ry = Canvas.GetTop(options.currentRoot);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                CreatePinConnection(canvas, x, y);

                options.currentRoot = root;

                CreatePinConnection(canvas, x, y);
            }
            else if (options.enableInsertLast == true)
            {
                AddToHistory(canvas);

                InsertLast(canvas, options.lastInsert, point);
            }
        }

        public bool HandlePreviewLeftDown(Canvas canvas, FrameworkElement pin)
        {
            if (pin != null &&
                (!CompareString(pin.Name, ResourceConstants.StandalonePinName) || Keyboard.Modifiers == ModifierKeys.Control))
            {
                if (options.currentLine == null)
                    AddToHistory(canvas);

                CreatePinConnection(canvas, pin);

                return true;
            }

            return false;
        }

        public void HandleMove(Canvas canvas, Point point)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                var margin = options.currentLine.Margin;

                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (options.currentLine.X2 != x)
                {
                    //this._line.X2 = SnapX(x);
                    options.currentLine.X2 = x;
                }

                if (options.currentLine.Y2 != y)
                {
                    //this._line.Y2 = SnapY(y);
                    options.currentLine.Y2 = y;
                }
            }
        }

        public bool HandleRightDown(Canvas canvas, Path path)
        {
            if (options.currentRoot != null && options.currentLine != null)
            {
                if (options.enableHistory == true)
                {
                    Undo(canvas, path, false);
                }
                else
                {
                    var selection = options.currentRoot.Tag as Selection;
                    var tuples = selection.Item2;

                    var last = tuples.LastOrDefault();
                    tuples.Remove(last);

                    canvas.Children.Remove(options.currentLine);
                }

                options.currentLine = null;
                options.currentRoot = null;

                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private DiagramEditor editor = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeOptions();
        }

        private void InitializeOptions()
        {
            editor = new DiagramEditor();
            editor.options = new DiagramEditorOptions();

            editor.options.currentCanvas = this.DiagramCanvas;
            editor.options.currentPathGrid = this.PathGrid;

            EnableHistory.IsChecked = editor.options.enableHistory;
            EnableInsertLast.IsChecked = editor.options.enableInsertLast;
            EnableSnap.IsChecked = editor.options.enableSnap;
            SnapOnRelease.IsChecked = editor.options.snapOnRelease;
        }

        #endregion

        #region Grid

        private void GenerateGrid()
        {
            var canvas = editor.options.currentCanvas;
            var path = this.PathGrid;

            int width = int.Parse(TextGridWidth.Text);
            int height = int.Parse(TextGridHeight.Text);
            int size = int.Parse(TextGridSize.Text);

            editor.AddToHistory(canvas);

            editor.GenerateGrid(path, width, height, size);

            editor.SetDiagramSize(canvas, width, height);
        }

        #endregion

        #region Pan

        private void BeginPan(Point point)
        {
            editor.options.panStart = point;

            editor.options.previousScrollOffsetX = -1.0;
            editor.options.previousScrollOffsetY = -1.0;

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
            double scrollOffsetX = point.X - editor.options.panStart.X;
            double scrollOffsetY = point.Y - editor.options.panStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;
            double panSpeed = zoom / editor.options.panSpeedFactor;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * panSpeed) * editor.options.reversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * panSpeed) * editor.options.reversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != editor.options.previousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                editor.options.previousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != editor.options.previousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                editor.options.previousScrollOffsetY = scrollOffsetY;
            }

            editor.options.panStart = point;
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

        #region Zoom

        private double zoomLogBase = 1.8;
        private double zoomExpFactor = 1.3;

        public double CalculateZoom(double x)
        {
            double l = Math.Log(x, zoomLogBase);
            double e = Math.Exp(l / zoomExpFactor);
            double y = x + x * l * e;
            return y;
        }

        private void Zoom(double zoom)
        {
            if (editor == null || editor.options == null)
                return;

            double zoom_fx = CalculateZoom(zoom);

            System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            //var tg = RootGrid.RenderTransform as TransformGroup;
            var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is ScaleTransform) as ScaleTransform;

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            Application.Current.Resources[ResourceConstants.KeyStrokeThickness] = editor.options.defaultStrokeThickness / zoom_fx;

            // zoom to point
            ZoomToPoint(zoom_fx, oldZoom);
        }

        private void ZoomToPoint(double zoom, double oldZoom)
        {
            double offsetX = 0;
            double offsetY = 0;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = this.PanScrollViewer.HorizontalOffset;
            double scrollOffsetY = this.PanScrollViewer.VerticalOffset;

            double oldX = editor.options.zoomPoint.X * oldZoom;
            double oldY = editor.options.zoomPoint.Y * oldZoom;

            double newX = editor.options.zoomPoint.X * zoom;
            double newY = editor.options.zoomPoint.Y * zoom;

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

            zoom += editor.options.zoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= editor.options.zoomOutFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        #endregion

        #region Zoom Events

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoom = ZoomSlider.Value;

            zoom = Math.Round(zoom, 1);

            if (e.OldValue != e.NewValue)
            {
                Zoom(zoom);
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                return;

            var canvas = editor.options.currentCanvas;

            editor.options.zoomPoint = e.GetPosition(canvas);

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

        #endregion

        #region PanScrollViewer Events

        private void PanScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.panButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.panButton)
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

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = e.GetPosition(canvas);

            editor.HandleLeftDown(canvas, point);
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editor.options.skipLeftClick == true)
            {
                editor.options.skipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = editor.options.currentCanvas;
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = editor.HandlePreviewLeftDown(canvas, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            var point = e.GetPosition(canvas);

            editor.HandleMove(canvas, point);
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var path = editor.options.currentPathGrid;

            editor.options.rightClick = e.GetPosition(canvas);

            var result = editor.HandleRightDown(canvas, path);
            if (result == true)
            {
                editor.options.skipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (editor.options.skipContextMenu == true)
            {
                editor.options.skipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                editor.options.skipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertPin(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementPin;
            editor.options.skipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertInput(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementInput;
            editor.options.skipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOutput(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementOutput;
            editor.options.skipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertAndGate(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementAndGate;
            editor.options.skipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOrGate(canvas, editor.options.rightClick);

            editor.options.lastInsert = ModelConstants.TagElementOrGate;
            editor.options.skipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.currentCanvas;
            var point = new Point(editor.options.rightClick.X, editor.options.rightClick.Y);

            editor.Delete(canvas, point);
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableHistory = EnableHistory.IsChecked == true ? true : false;

            if (editor.options.enableHistory == false)
            {
                var canvas = editor.options.currentCanvas;

                editor.ClearHistory(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            editor.options.snapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            editor.options.enableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        } 
    
        #endregion

        #region Button Events

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.Generate();

            this.TextModel.Text = diagram;
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            editor.Insert(diagram, offsetX, offsetY);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            GenerateGrid();
        }

        #endregion

        #region Main Menu Events

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            editor.Open();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            editor.Save();
        }

        private void FileImport_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.Import();

            if (diagram != null)
            {
                this.TextModel.Text = diagram;
            }
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            editor.Export();
        }

        private void FileExportHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportHistory();
        }

        private void FilePrint_Click(object sender, RoutedEventArgs e)
        {
            editor.Print();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            editor.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            editor.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            editor.Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            editor.Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            editor.Paste();
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            editor.Delete();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.SelectAll();
        }

        private void EditDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.DeselectAll();
        }

        private void EditClear_Click(object sender, RoutedEventArgs e)
        {
            editor.Clear();
        }

        private void EditPreferences_Click(object sender, RoutedEventArgs e)
        {
            editor.Preferences();
        }

        #endregion
    }

    #endregion
}

namespace MsoWord
{
    #region References

    using CanvasDiagramEditor;
    using System;
    using Word = Microsoft.Office.Interop.Word;
    using Office = Microsoft.Office.Core;

    #endregion

    #region MsoWordExport

    public class MsoWordExport : IDiagramExport
    {
        #region Microsoft Word 2013 Export

        private Office.MsoShapeStyleIndex defaultShapeStyle = Office.MsoShapeStyleIndex.msoShapeStylePreset25;
        private Office.MsoShapeStyleIndex defaultLineStyle = Office.MsoShapeStyleIndex.msoLineStylePreset20;

        public void CreateDocument(string fileName, IEnumerable<string> diagrams)
        {
            System.Diagnostics.Debug.Print("Creating document: {0}", fileName);

            var word = new Word.Application();

            var doc = CreateDocument(word, diagrams);

            // save and close document
            doc.SaveAs2(fileName);

            (doc as Word._Document).Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            (word as Word._Application).Quit(Word.WdSaveOptions.wdDoNotSaveChanges);

            System.Diagnostics.Debug.Print("Done.");
        }

        private Word.Document CreateDocument(Word.Application word, IEnumerable<string> diagrams)
        {
            // create new document
            var doc = word.Documents.Add();

            doc.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
            doc.PageSetup.Orientation = Word.WdOrientation.wdOrientLandscape;

            // margin = 20.0f;
            // 801.95 + 40, 555.35 + 40
            // 841.95, 595.35
            // 780, 540
            // left,right: 30.975f top,bottom: 27.675f

            doc.PageSetup.LeftMargin = 30.975f;
            doc.PageSetup.RightMargin = 30.975f;
            doc.PageSetup.TopMargin = 27.675f;
            doc.PageSetup.BottomMargin = 27.675f;

            foreach (var diagram in diagrams)
            {
                // create diagram canvas
                var canvas = CreateCanvas(doc);
                var items = canvas.CanvasItems;

                CreateElements(items, diagram);
            }

            return doc;
        }

        private static Word.Shape CreateCanvas(Word.Document doc)
        {
            float left = doc.PageSetup.LeftMargin;
            float top = doc.PageSetup.TopMargin;
            float width = doc.PageSetup.PageWidth - doc.PageSetup.LeftMargin - doc.PageSetup.RightMargin;
            float height = doc.PageSetup.PageHeight - doc.PageSetup.TopMargin - doc.PageSetup.BottomMargin;

            System.Diagnostics.Debug.Print("document width, height: {0},{1}", width, height);

            var canvas = doc.Shapes.AddCanvas(left, top, width, height);

            canvas.WrapFormat.AllowOverlap = (int)Office.MsoTriState.msoFalse;
            canvas.WrapFormat.Type = Word.WdWrapType.wdWrapInline;

            return canvas;
        }

        private void CreateElements(Word.CanvasShapes items, string diagram)
        {
            string name = null;
            var lines = diagram.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            var elements = new List<Action>();
            var wires = new List<Action>();

            foreach (var line in lines)
            {
                var args = line.Split(ModelConstants.ArgumentSeparator);
                int length = args.Length;

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareString(args[0], ModelConstants.PrefixRootElement))
                    {
                        if (name.StartsWith(ModelConstants.TagElementPin, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreatePin(items, x, y);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementInput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreateInput(items, x, y, "Input");
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOutput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreateOutput(items, x, y, "Output");
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementAndGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreateAndGate(items, x, y);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOrGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                CreateOrGate(items, x, y);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementWire, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 6)
                        {
                            float x1 = float.Parse(args[2]);
                            float y1 = float.Parse(args[3]);
                            float x2 = float.Parse(args[4]);
                            float y2 = float.Parse(args[5]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            wires.Add(() =>
                            {
                                CreateWire(items, x1, y1, x2, y2);
                            });
                        }
                    }
                }
            }

            // create wires, bottom ZOrder
            foreach (var action in wires)
            {
                action();
            }

            // create elements, top ZOrder
            foreach (var action in elements)
            {
                action();
            }
        }

        private void CreatePin(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeOval,
                x - 4.0f, y - 4.0f, 8.0f, 8.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateWire(Word.CanvasShapes items, float x1, float y1, float x2, float y2)
        {
            var line = items.AddLine(x1, y1, x2, y2);

            line.Line.ForeColor.RGB = unchecked((int)0x00000000);
            line.Line.Weight = 1.0f;

            line.ShapeStyle = defaultLineStyle;
        }

        private void CreateInput(Word.CanvasShapes items, float x, float y, string text)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 180.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateOutput(Word.CanvasShapes items, float x, float y, string text)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 180.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateAndGate(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 30.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = "&";

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateOrGate(Word.CanvasShapes items, float x, float y)
        {
            var rect = items.AddShape((int)Office.MsoAutoShapeType.msoShapeRectangle,
                x, y, 30.0f, 30.0f);

            rect.Fill.ForeColor.RGB = unchecked((int)0x00FFFFFF);
            rect.Line.ForeColor.RGB = unchecked((int)0x00000000);
            rect.Line.Weight = 1.0f;

            var textFrame = rect.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = "≥1";

            rect.ShapeStyle = defaultShapeStyle;
        }

        private void CreateText(Word.CanvasShapes items, float x, float y, float width, float height, string text)
        {
            var textBox = items.AddTextbox(Office.MsoTextOrientation.msoTextOrientationHorizontal,
                x, y, width, height);

            textBox.Line.Visible = Office.MsoTriState.msoFalse;
            textBox.Fill.Visible = Office.MsoTriState.msoFalse;

            var textFrame = textBox.TextFrame;

            SetTextFrameFormat(textFrame);

            textFrame.TextRange.Text = text;
        }

        private void SetTextFrameFormat(Word.TextFrame textFrame)
        {
            textFrame.AutoSize = (int)Office.MsoAutoSize.msoAutoSizeNone;

            textFrame.VerticalAnchor = Office.MsoVerticalAnchor.msoAnchorMiddle;

            textFrame.MarginLeft = 0.0f;
            textFrame.MarginTop = 0.0f;
            textFrame.MarginRight = 0.0f;
            textFrame.MarginBottom = 0.0f;

            textFrame.TextRange.Font.Name = "Arial";
            textFrame.TextRange.Font.Size = 12.0f;
            textFrame.TextRange.Font.TextColor.RGB = unchecked((int)0x00000000);

            textFrame.TextRange.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            textFrame.TextRange.Paragraphs.SpaceAfter = 0.0f;
            textFrame.TextRange.Paragraphs.SpaceBefore = 0.0f;
            textFrame.TextRange.Paragraphs.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
        }

        private static bool CompareString(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        #endregion
    }

    #endregion
}

namespace OpenXml
{
    #region References

    using CanvasDiagramEditor;

    using DocumentFormat.OpenXml.Packaging;
    using Ap = DocumentFormat.OpenXml.ExtendedProperties;
    using Vt = DocumentFormat.OpenXml.VariantTypes;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Wp = DocumentFormat.OpenXml.Drawing.Wordprocessing;
    using A = DocumentFormat.OpenXml.Drawing;
    using Wpc = DocumentFormat.OpenXml.Office2010.Word.DrawingCanvas;
    using Wps = DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
    using V = DocumentFormat.OpenXml.Vml;
    using Ovml = DocumentFormat.OpenXml.Vml.Office;
    using Wvml = DocumentFormat.OpenXml.Vml.Wordprocessing;
    using M = DocumentFormat.OpenXml.Math;
    using Ds = DocumentFormat.OpenXml.CustomXmlDataProperties;

    #endregion

    #region OpenXmlExport

    public class OpenXmlExport : IDiagramExport
    {
        #region Open XmlOpen XML SDK 2.5 for Microsoft Office

        // Conversion factor from XAML units to Centimenters, 30 XAML units = 1.0cm
        private static double xamlToCm = 30;

        // 1 Centimeter = 360000 EMUs
        private static double emu_1cm = 360000L;

        public void CreateDocument(string filePath, IEnumerable<string> diagrams)
        {
            using (WordprocessingDocument document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();

                Document document1 = CreateDocument();

                Body body1 = new Body();

                foreach (var diagram in diagrams)
                {
                    Paragraph paragraph1 = CreateParagraph(diagram);
                    SectionProperties sectionProperties1 = CreateSectionProperties();

                    body1.Append(paragraph1);
                    body1.Append(sectionProperties1);
                }

                document1.Append(body1);

                mainPart.Document = document1;
            }
        }

        private static Document CreateDocument()
        {
            Document document1 = new Document() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "w14 w15 wp14" } };

            document1.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
            document1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            document1.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
            document1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            document1.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
            document1.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
            document1.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
            document1.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            document1.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
            document1.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            document1.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            document1.AddNamespaceDeclaration("w15", "http://schemas.microsoft.com/office/word/2012/wordml");
            document1.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
            document1.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
            document1.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
            document1.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");

            return document1;
        }

        private static Paragraph CreateParagraph(string diagram)
        {
            Paragraph paragraph1 = new Paragraph() { RsidParagraphAddition = "00DD51F1", RsidParagraphProperties = "00170F2C", RsidRunAdditionDefault = "00566780" };

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };

            paragraphProperties1.Append(spacingBetweenLines1);

            BookmarkStart bookmarkStart1 = new BookmarkStart() { Name = "_GoBack", Id = "0" };

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            NoProof noProof1 = new NoProof();
            Languages languages1 = new Languages() { EastAsia = "pl-PL" };

            runProperties1.Append(noProof1);
            runProperties1.Append(languages1);

            AlternateContent alternateContent1 = new AlternateContent();

            AlternateContentChoice alternateContentChoice1 = new AlternateContentChoice() { Requires = "wpc" };

            Drawing drawing1 = CreateDrawing(diagram);

            alternateContentChoice1.Append(drawing1);

            //AlternateContentFallback alternateContentFallback1 = new AlternateContentFallback();

            //Picture picture1 = new Picture();

            //V.Group group1 = CreateGroup();
            //picture1.Append(group1);

            //alternateContentFallback1.Append(picture1);

            alternateContent1.Append(alternateContentChoice1);
            //alternateContent1.Append(alternateContentFallback1);

            run1.Append(runProperties1);
            run1.Append(alternateContent1);

            BookmarkEnd bookmarkEnd1 = new BookmarkEnd() { Id = "0" };

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(bookmarkStart1);
            paragraph1.Append(run1);
            paragraph1.Append(bookmarkEnd1);

            return paragraph1;
        }

        private static SectionProperties CreateSectionProperties()
        {
            SectionProperties sectionProperties1 = new SectionProperties() { RsidR = "00DD51F1", RsidSect = "00170F2C" };

            PageSize pageSize1 = new PageSize() { Width = (UInt32Value)16838U, Height = (UInt32Value)11906U, Orient = PageOrientationValues.Landscape };
            PageMargin pageMargin1 = new PageMargin() { Top = 1418, Right = (UInt32Value)1332U, Bottom = 1418, Left = (UInt32Value)1332U, Header = (UInt32Value)709U, Footer = (UInt32Value)709U, Gutter = (UInt32Value)0U };
            Columns columns1 = new Columns() { Space = "708" };
            DocGrid docGrid1 = new DocGrid() { LinePitch = 360 };

            sectionProperties1.Append(pageSize1);
            sectionProperties1.Append(pageMargin1);
            sectionProperties1.Append(columns1);
            sectionProperties1.Append(docGrid1);

            return sectionProperties1;
        }

        private static Drawing CreateDrawing(string diagram)
        {
            Drawing drawing1 = new Drawing();

            Wp.Inline inline1 = new Wp.Inline() { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U };
            Wp.Extent extent1 = new Wp.Extent() { Cx = 9000000L, Cy = 5760000L };
            Wp.EffectExtent effectExtent1 = new Wp.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L };
            Wp.DocProperties docProperties1 = new Wp.DocProperties() { Id = (UInt32Value)2U, /* Name = "Kanwa 2" */ Name = Guid.NewGuid().ToString() };

            Wp.NonVisualGraphicFrameDrawingProperties nonVisualGraphicFrameDrawingProperties1 = new Wp.NonVisualGraphicFrameDrawingProperties();

            A.GraphicFrameLocks graphicFrameLocks1 = new A.GraphicFrameLocks();
            graphicFrameLocks1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            nonVisualGraphicFrameDrawingProperties1.Append(graphicFrameLocks1);

            A.Graphic graphic1 = new A.Graphic();

            graphic1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            A.GraphicData graphicData1 = new A.GraphicData() { Uri = "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas" };

            Wpc.WordprocessingCanvas wordprocessingCanvas1 = CreateCanvas(diagram);

            graphicData1.Append(wordprocessingCanvas1);

            graphic1.Append(graphicData1);

            inline1.Append(extent1);
            inline1.Append(effectExtent1);
            inline1.Append(docProperties1);
            inline1.Append(nonVisualGraphicFrameDrawingProperties1);
            inline1.Append(graphic1);

            drawing1.Append(inline1);

            return drawing1;
        }

        private static Wpc.WordprocessingCanvas CreateCanvas(string diagram)
        {
            Wpc.WordprocessingCanvas wordprocessingCanvas1 = new Wpc.WordprocessingCanvas();

            Wpc.BackgroundFormatting backgroundFormatting1 = new Wpc.BackgroundFormatting();
            Wpc.WholeFormatting wholeFormatting1 = new Wpc.WholeFormatting();

            //Wps.WordprocessingShape wordprocessingShape1 = CreateWpShape1_AndGate();
            //Wps.WordprocessingShape wordprocessingShape2 = CreateWpShape2_Wire();

            wordprocessingCanvas1.Append(backgroundFormatting1);
            wordprocessingCanvas1.Append(wholeFormatting1);

            //wordprocessingCanvas1.Append(wordprocessingShape1);
            //wordprocessingCanvas1.Append(wordprocessingShape2);

            CreateElements(wordprocessingCanvas1, diagram);

            return wordprocessingCanvas1;
        }

        private static void CreateElements(Wpc.WordprocessingCanvas wordprocessingCanvas, string diagram)
        {
            string name = null;
            var lines = diagram.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            var elements = new List<Action>();
            var wires = new List<Action>();

            foreach (var line in lines)
            {
                var args = line.Split(ModelConstants.ArgumentSeparator);
                int length = args.Length;

                if (length >= 2)
                {
                    name = args[1];

                    if (CompareString(args[0], ModelConstants.PrefixRootElement))
                    {
                        if (name.StartsWith(ModelConstants.TagElementPin, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                //Wps.WordprocessingShape wordprocessingShape = CreateWpShapePin(x / xamlToCm, y / xamlToCm);
                                //wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementInput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeInput(x / xamlToCm, y / xamlToCm, "Input");
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOutput, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeOutput(x / xamlToCm, y / xamlToCm, "Output");
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementAndGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeAndGate(x / xamlToCm, y / xamlToCm);
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementOrGate, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 4)
                        {
                            float x = float.Parse(args[2]);
                            float y = float.Parse(args[3]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            elements.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeOrGate(x / xamlToCm, y / xamlToCm);
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                        else if (name.StartsWith(ModelConstants.TagElementWire, StringComparison.InvariantCultureIgnoreCase) &&
                            length == 6)
                        {
                            float x1 = float.Parse(args[2]);
                            float y1 = float.Parse(args[3]);
                            float x2 = float.Parse(args[4]);
                            float y2 = float.Parse(args[5]);
                            int id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);

                            wires.Add(() =>
                            {
                                Wps.WordprocessingShape wordprocessingShape = CreateWpShapeWire(x1 / xamlToCm, y1 / xamlToCm, x2 / xamlToCm, y2 / xamlToCm);
                                wordprocessingCanvas.Append(wordprocessingShape);
                            });
                        }
                    }
                }
            }

            // create wires, bottom ZOrder
            foreach (var action in wires)
            {
                action();
            }

            // create elements, top ZOrder
            foreach (var action in elements)
            {
                action();
            }
        }

        private static Wps.WordprocessingShape CreateWpShapePin(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm) - 30000L;
            Int64 Y = (Int64)(y * emu_1cm) - 30000L;

            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = X, Y = Y };
            A.Extents extents2 = new A.Extents() { Cx = 60000L, Cy = 60000L }; // Width,Height = 60000L, Margin Left/Top = -30000L;

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Ellipse };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStylePin();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        private static Wps.WordprocessingShape CreateWpShapeWire(double x1, double y1, double x2, double y2)
        {
            // x1, y1, x2, y2 are in Centimeters

            Int64 X = 0L;
            Int64 Y = 0L;
            Int64 Cx = 0L;
            Int64 Cy = 0L;

            if (x2 >= x1)
            {
                X = (Int64)(x1 * emu_1cm);
                Cx = (Int64)((x2 - x1) * emu_1cm);
            }
            else
            {
                X = (Int64)(x2 * emu_1cm);
                Cx = (Int64)((x1 - x2) * emu_1cm);
            }

            if (y2 >= y1)
            {
                Y = (Int64)(y1 * emu_1cm);
                Cy = (Int64)((y2 - y1) * emu_1cm);
            }
            else
            {
                Y = (Int64)(y2 * emu_1cm);
                Cy = (Int64)((y1 - y2) * emu_1cm);
            }

            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = X, Y = Y };
            A.Extents extents2 = new A.Extents() { Cx = Cx, Cy = Cy };

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Line };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStyle2();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        private static Wps.WordprocessingShape CreateWpShapeAndGate(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L }; // 1cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "&";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeOrGate(double x, double y)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L }; // 1cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "≥1";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeInput(double x, double y, string text)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 6L * 360000L, Cy = 360000L }; // 6cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = text;

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        private static Wps.WordprocessingShape CreateWpShapeOutput(double x, double y, string text)
        {
            // x, y are in Centimeters

            Int64 X = (Int64)(x * emu_1cm);
            Int64 Y = (Int64)(y * emu_1cm);

            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = Guid.NewGuid().ToString() };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = X, Y = Y };
            A.Extents extents1 = new A.Extents() { Cx = 6L * 360000L, Cy = 360000L }; // 6cm x 1cm

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = text;

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        // NOT USED
        private static Wps.WordprocessingShape CreateWpShape1_AndGate()
        {
            Wps.WordprocessingShape wordprocessingShape1 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties1 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)1U, Name = "Prostokąt 1" };
            Wps.NonVisualDrawingShapeProperties nonVisualDrawingShapeProperties1 = new Wps.NonVisualDrawingShapeProperties();

            Wps.ShapeProperties shapeProperties1 = new Wps.ShapeProperties();

            A.Transform2D transform2D1 = new A.Transform2D();
            A.Offset offset1 = new A.Offset() { X = 0L, Y = 720090L };
            A.Extents extents1 = new A.Extents() { Cx = 360000L, Cy = 360000L };

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            A.PresetGeometry presetGeometry1 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
            A.AdjustValueList adjustValueList1 = new A.AdjustValueList();

            presetGeometry1.Append(adjustValueList1);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);

            Wps.ShapeStyle shapeStyle1 = CreateShapeStyle1();

            Wps.TextBoxInfo2 textBoxInfo21 = new Wps.TextBoxInfo2();

            TextBoxContent textBoxContent1 = new TextBoxContent();

            Paragraph paragraph2 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines2 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties2.Append(spacingBetweenLines2);
            paragraphProperties2.Append(justification1);
            paragraphProperties2.Append(paragraphMarkRunProperties1);

            Run run2 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties2.Append(runFonts2);
            Text text1 = new Text();
            text1.Text = "&";

            run2.Append(runProperties2);
            run2.Append(text1);

            paragraph2.Append(paragraphProperties2);
            paragraph2.Append(run2);

            textBoxContent1.Append(paragraph2);

            textBoxInfo21.Append(textBoxContent1);

            Wps.TextBodyProperties textBodyProperties1 = new Wps.TextBodyProperties() { Rotation = 0, UseParagraphSpacing = false, VerticalOverflow = A.TextVerticalOverflowValues.Overflow, HorizontalOverflow = A.TextHorizontalOverflowValues.Overflow, Vertical = A.TextVerticalValues.Horizontal, Wrap = A.TextWrappingValues.Square, LeftInset = 0, TopInset = 0, RightInset = 0, BottomInset = 0, ColumnCount = 1, ColumnSpacing = 0, RightToLeftColumns = false, FromWordArt = false, Anchor = A.TextAnchoringTypeValues.Center, AnchorCenter = false, ForceAntiAlias = false, CompatibleLineSpacing = true };

            A.PresetTextWrap presetTextWrap1 = new A.PresetTextWrap() { Preset = A.TextShapeValues.TextNoShape };
            A.AdjustValueList adjustValueList2 = new A.AdjustValueList();

            presetTextWrap1.Append(adjustValueList2);
            A.NoAutoFit noAutoFit1 = new A.NoAutoFit();

            textBodyProperties1.Append(presetTextWrap1);
            textBodyProperties1.Append(noAutoFit1);

            wordprocessingShape1.Append(nonVisualDrawingProperties1);
            wordprocessingShape1.Append(nonVisualDrawingShapeProperties1);
            wordprocessingShape1.Append(shapeProperties1);
            wordprocessingShape1.Append(shapeStyle1);
            wordprocessingShape1.Append(textBoxInfo21);
            wordprocessingShape1.Append(textBodyProperties1);

            return wordprocessingShape1;
        }

        // NOT USED
        private static Wps.WordprocessingShape CreateWpShape2_Wire()
        {
            Wps.WordprocessingShape wordprocessingShape2 = new Wps.WordprocessingShape();

            Wps.NonVisualDrawingProperties nonVisualDrawingProperties2 = new Wps.NonVisualDrawingProperties() { Id = (UInt32Value)3U, Name = "Łącznik prosty 3" };
            Wps.NonVisualConnectorProperties nonVisualConnectorProperties1 = new Wps.NonVisualConnectorProperties();

            Wps.ShapeProperties shapeProperties2 = new Wps.ShapeProperties();

            A.Transform2D transform2D2 = new A.Transform2D();
            A.Offset offset2 = new A.Offset() { X = 542778L, Y = 901798L };
            A.Extents extents2 = new A.Extents() { Cx = 1440000L, Cy = 0L };

            transform2D2.Append(offset2);
            transform2D2.Append(extents2);

            A.PresetGeometry presetGeometry2 = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Line };
            A.AdjustValueList adjustValueList3 = new A.AdjustValueList();

            presetGeometry2.Append(adjustValueList3);
            A.Outline outline1 = new A.Outline() { Width = 12700 };

            shapeProperties2.Append(transform2D2);
            shapeProperties2.Append(presetGeometry2);
            shapeProperties2.Append(outline1);

            Wps.ShapeStyle shapeStyle2 = CreateShapeStyle2();

            Wps.TextBodyProperties textBodyProperties2 = new Wps.TextBodyProperties();

            wordprocessingShape2.Append(nonVisualDrawingProperties2);
            wordprocessingShape2.Append(nonVisualConnectorProperties1);
            wordprocessingShape2.Append(shapeProperties2);
            wordprocessingShape2.Append(shapeStyle2);
            wordprocessingShape2.Append(textBodyProperties2);

            return wordprocessingShape2;
        }

        // AndGate/OrGate/Input/Output Style
        private static Wps.ShapeStyle CreateShapeStyle1()
        {
            Wps.ShapeStyle shapeStyle1 = new Wps.ShapeStyle();

            A.LineReference lineReference1 = new A.LineReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor1 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference1.Append(schemeColor1);

            A.FillReference fillReference1 = new A.FillReference() { Index = (UInt32Value)1U };
            A.SchemeColor schemeColor2 = new A.SchemeColor() { Val = A.SchemeColorValues.Light1 };

            fillReference1.Append(schemeColor2);

            A.EffectReference effectReference1 = new A.EffectReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor3 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference1.Append(schemeColor3);

            A.FontReference fontReference1 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor4 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fontReference1.Append(schemeColor4);

            shapeStyle1.Append(lineReference1);
            shapeStyle1.Append(fillReference1);
            shapeStyle1.Append(effectReference1);
            shapeStyle1.Append(fontReference1);

            return shapeStyle1;
        }

        // Wire Style
        private static Wps.ShapeStyle CreateShapeStyle2()
        {
            Wps.ShapeStyle shapeStyle2 = new Wps.ShapeStyle();

            A.LineReference lineReference2 = new A.LineReference() { Index = (UInt32Value)3U };
            A.SchemeColor schemeColor5 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference2.Append(schemeColor5);

            A.FillReference fillReference2 = new A.FillReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor6 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fillReference2.Append(schemeColor6);

            A.EffectReference effectReference2 = new A.EffectReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor7 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference2.Append(schemeColor7);

            A.FontReference fontReference2 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor8 = new A.SchemeColor() { Val = A.SchemeColorValues.Text1 };

            fontReference2.Append(schemeColor8);

            shapeStyle2.Append(lineReference2);
            shapeStyle2.Append(fillReference2);
            shapeStyle2.Append(effectReference2);
            shapeStyle2.Append(fontReference2);

            return shapeStyle2;
        }

        // Pin Style
        private static Wps.ShapeStyle CreateShapeStylePin()
        {
            Wps.ShapeStyle shapeStyle1 = new Wps.ShapeStyle();

            A.LineReference lineReference1 = new A.LineReference() { Index = (UInt32Value)2U };
            A.SchemeColor schemeColor1 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            lineReference1.Append(schemeColor1);

            A.FillReference fillReference1 = new A.FillReference() { Index = (UInt32Value)1U };
            A.SchemeColor schemeColor2 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fillReference1.Append(schemeColor2);

            A.EffectReference effectReference1 = new A.EffectReference() { Index = (UInt32Value)0U };
            A.SchemeColor schemeColor3 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            effectReference1.Append(schemeColor3);

            A.FontReference fontReference1 = new A.FontReference() { Index = A.FontCollectionIndexValues.Minor };
            A.SchemeColor schemeColor4 = new A.SchemeColor() { Val = A.SchemeColorValues.Dark1 };

            fontReference1.Append(schemeColor4);

            shapeStyle1.Append(lineReference1);
            shapeStyle1.Append(fillReference1);
            shapeStyle1.Append(effectReference1);
            shapeStyle1.Append(fontReference1);

            return shapeStyle1;
        }

        // NOT USED
        private static V.Group CreateGroup()
        {
            V.Group group1 = new V.Group() { Id = "Kanwa 2", Style = "width:708.65pt;height:453.55pt;mso-position-horizontal-relative:char;mso-position-vertical-relative:line", CoordinateSize = "89998,57594", OptionalString = "_x0000_s1026", EditAs = V.EditAsValues.Canvas };

            group1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQC2gziS/gAAAOEBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRQU7DMBBF\n90jcwfIWJU67QAgl6YK0S0CoHGBkTxKLZGx5TGhvj5O2G0SRWNoz/78nu9wcxkFMGNg6quQqL6RA\n0s5Y6ir5vt9lD1JwBDIwOMJKHpHlpr69KfdHjyxSmriSfYz+USnWPY7AufNIadK6MEJMx9ApD/oD\nOlTrorhX2lFEilmcO2RdNtjC5xDF9pCuTyYBB5bi6bQ4syoJ3g9WQ0ymaiLzg5KdCXlKLjvcW893\nSUOqXwnz5DrgnHtJTxOsQfEKIT7DmDSUCaxw7Rqn8787ZsmRM9e2VmPeBN4uqYvTtW7jvijg9N/y\nJsXecLq0q+WD6m8AAAD//wMAUEsDBBQABgAIAAAAIQA4/SH/1gAAAJQBAAALAAAAX3JlbHMvLnJl\nbHOkkMFqwzAMhu+DvYPRfXGawxijTi+j0GvpHsDYimMaW0Yy2fr2M4PBMnrbUb/Q94l/f/hMi1qR\nJVI2sOt6UJgd+ZiDgffL8ekFlFSbvV0oo4EbChzGx4f9GRdb25HMsYhqlCwG5lrLq9biZkxWOiqY\n22YiTra2kYMu1l1tQD30/bPm3wwYN0x18gb45AdQl1tp5j/sFB2T0FQ7R0nTNEV3j6o9feQzro1i\nOWA14Fm+Q8a1a8+Bvu/d/dMb2JY5uiPbhG/ktn4cqGU/er3pcvwCAAD//wMAUEsDBBQABgAIAAAA\nIQB6MeNb8AIAAMIHAAAOAAAAZHJzL2Uyb0RvYy54bWy0VV1P2zAUfZ+0/2D5faQfjEJFiqoipkkI\nqsHEs+s4bYRjZ7ZpUt72wD9j/2vHTtIyKFRiWx/cm/h+33Nujk+qXJKlMDbTKqbdvQ4lQnGdZGoe\n0+/XZ58OKbGOqYRJrURMV8LSk9HHD8dlMRQ9vdAyEYbAibLDsojpwrliGEWWL0TO7J4uhMJlqk3O\nHB7NPEoMK+E9l1Gv0zmISm2SwmgurMXb0/qSjoL/NBXcXaapFY7ImCI3F04Tzpk/o9ExG84NKxYZ\nb9Jg78giZ5lC0LWrU+YYuTPZC1d5xo22OnV7XOeRTtOMi1ADqul2nlUzYWrJbCiGozttgpD+od/Z\nHD2Ay2GJYYggYxS2WA/F/l2wqwUrRKjBDvnFcmpIlgAplCiWAxBTtMPp28cHR7p+GmUR1K6KqWme\nLETf2io1uf9H00gVJriK6QAgOGrGKCpHOG76Bx38KOG4b2S4ijYeCmPdF6Fz4oWYGqAkDI8tz62r\nVVsV2PmM6hyC5FZS+DSk+iZS1IKAvWAdMCsm0pAlA9qS21APwgZNb5JmUq6NutuMpGuNGl1vJgKO\n14adbYabaGvtEFErtzbMM6XN28Zprd9WXdfqy3bVrGrmMdPJClM0uiaTLfhZhj6eM+umzIA9aD02\ngrvEkUpdxlQ3EiULbe63vff6gBluKSnBxpjaH3fMCErkVwUAeuq2gmmFWSuou3yi0XKACtkEEQbG\nyVZMjc5vsCjGPgqumOKIFVPuTPswcfVWwKrhYjwOaqBowdy5uvKEq+flcXFd3TBTNOBxQN2FbkHO\nhs8wVOv6USg9vnM6zQLAfEvrPjatBuFq7P935vVb5v36+fjA71V2i0rAwRXpP+HfRO3i3+f93mCA\n7Q6SHXW6g6PDepe2JOzu729YGAj6OgFlpvyGeNE8z1H/WioCFHV7A7Da03M3I/tvw3w7I3cQ63VG\n7uD/OxjpqvUeeI2RNXzadgT8oDHhO4EW/fElevoc2rf59I5+AwAA//8DAFBLAwQUAAYACAAAACEA\n4PXjX9oAAAAGAQAADwAAAGRycy9kb3ducmV2LnhtbEyPwU7DMBBE70j9B2uRuFE7UNES4lQVAgRH\nUuDsxkscYa+D7Tbh73G50MtKoxnNvK3Wk7PsgCH2niQUcwEMqfW6p07C2/bxcgUsJkVaWU8o4Qcj\nrOvZWaVK7Ud6xUOTOpZLKJZKgklpKDmPrUGn4twPSNn79MGplGXouA5qzOXO8ishbrhTPeUFowa8\nN9h+NXsngVA8NDbw59S+fwzme9U9vSxGKS/Op80dsIRT+g/DET+jQ52Zdn5POjIrIT+S/u7RWxTL\na2A7CbdiWQCvK36KX/8CAAD//wMAUEsBAi0AFAAGAAgAAAAhALaDOJL+AAAA4QEAABMAAAAAAAAA\nAAAAAAAAAAAAAFtDb250ZW50X1R5cGVzXS54bWxQSwECLQAUAAYACAAAACEAOP0h/9YAAACUAQAA\nCwAAAAAAAAAAAAAAAAAvAQAAX3JlbHMvLnJlbHNQSwECLQAUAAYACAAAACEAejHjW/ACAADCBwAA\nDgAAAAAAAAAAAAAAAAAuAgAAZHJzL2Uyb0RvYy54bWxQSwECLQAUAAYACAAAACEA4PXjX9oAAAAG\nAQAADwAAAAAAAAAAAAAAAABKBQAAZHJzL2Rvd25yZXYueG1sUEsFBgAAAAAEAAQA8wAAAFEGAAAA\nAA==\n"));

            V.Shapetype shapetype1 = new V.Shapetype() { Id = "_x0000_t75", CoordinateSize = "21600,21600", Filled = false, Stroked = false, OptionalNumber = 75, PreferRelative = true, EdgePath = "m@4@5l@4@11@9@11@9@5xe" };
            V.Stroke stroke1 = new V.Stroke() { JoinStyle = V.StrokeJoinStyleValues.Miter };

            V.Formulas formulas1 = CreateFormulas();

            V.Path path1 = new V.Path() { AllowGradientShape = true, ConnectionPointType = Ovml.ConnectValues.Rectangle, AllowExtrusion = false };
            Ovml.Lock lock1 = new Ovml.Lock() { Extension = V.ExtensionHandlingBehaviorValues.Edit, AspectRatio = true };

            shapetype1.Append(stroke1);
            shapetype1.Append(formulas1);
            shapetype1.Append(path1);
            shapetype1.Append(lock1);

            V.Shape shape1 = new V.Shape() { Id = "_x0000_s1027", Style = "position:absolute;width:89998;height:57594;visibility:visible;mso-wrap-style:square", Type = "#_x0000_t75" };
            V.Fill fill1 = new V.Fill() { DetectMouseClick = true };
            V.Path path2 = new V.Path() { ConnectionPointType = Ovml.ConnectValues.None };

            shape1.Append(fill1);
            shape1.Append(path2);

            V.Rectangle rectangle1 = new V.Rectangle() { Id = "Prostokąt 1", Style = "position:absolute;top:7200;width:3600;height:3600;visibility:visible;mso-wrap-style:square;v-text-anchor:middle", OptionalString = "_x0000_s1028", FillColor = "white [3201]", StrokeColor = "black [3200]", StrokeWeight = "1pt" };
            rectangle1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQDw94q7/QAAAOIBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRzUrEMBDH\n74LvEOYqbaoHEWm6B6tHFV0fYEimbdg2CZlYd9/edD8u4goeZ+b/8SOpV9tpFDNFtt4puC4rEOS0\nN9b1Cj7WT8UdCE7oDI7ekYIdMayay4t6vQvEIrsdKxhSCvdSsh5oQi59IJcvnY8TpjzGXgbUG+xJ\n3lTVrdTeJXKpSEsGNHVLHX6OSTxu8/pAEmlkEA8H4dKlAEMYrcaUSeXszI+W4thQZudew4MNfJUx\nQP7asFzOFxx9L/lpojUkXjGmZ5wyhjSRJQ8YKGvKv1MWzIkL33VWU9lGfl98J6hz4cZ/uUjzf7Pb\nbHuj+ZQu9z/UfAMAAP//AwBQSwMEFAAGAAgAAAAhADHdX2HSAAAAjwEAAAsAAABfcmVscy8ucmVs\nc6SQwWrDMAyG74O9g9G9cdpDGaNOb4VeSwe7CltJTGPLWCZt376mMFhGbzvqF/o+8e/2tzCpmbJ4\njgbWTQuKomXn42Dg63xYfYCSgtHhxJEM3Elg372/7U40YalHMvokqlKiGBhLSZ9aix0poDScKNZN\nzzlgqWMedEJ7wYH0pm23Ov9mQLdgqqMzkI9uA+p8T9X8hx28zSzcl8Zy0Nz33r6iasfXeKK5UjAP\nVAy4LM8w09zU50C/9q7/6ZURE31X/kL8TKv1x6wXNXYPAAAA//8DAFBLAwQUAAYACAAAACEAMy8F\nnkEAAAA5AAAAEAAAAGRycy9zaGFwZXhtbC54bWyysa/IzVEoSy0qzszPs1Uy1DNQUkjNS85PycxL\nt1UKDXHTtVBSKC5JzEtJzMnPS7VVqkwtVrK34+UCAAAA//8DAFBLAwQUAAYACAAAACEAYzhX6cEA\nAADaAAAADwAAAGRycy9kb3ducmV2LnhtbERPS2sCMRC+F/ofwhR6q1mFFlmNIoqoZYv4OHgcNuNm\ncTNZkqjrv2+EQk/Dx/ec8bSzjbiRD7VjBf1eBoK4dLrmSsHxsPwYgggRWWPjmBQ8KMB08voyxly7\nO+/oto+VSCEcclRgYmxzKUNpyGLouZY4cWfnLcYEfSW1x3sKt40cZNmXtFhzajDY0txQedlfrYK5\nKzark78sFsXpczssfmbme10p9f7WzUYgInXxX/znXus0H56vPK+c/AIAAP//AwBQSwECLQAUAAYA\nCAAAACEA8PeKu/0AAADiAQAAEwAAAAAAAAAAAAAAAAAAAAAAW0NvbnRlbnRfVHlwZXNdLnhtbFBL\nAQItABQABgAIAAAAIQAx3V9h0gAAAI8BAAALAAAAAAAAAAAAAAAAAC4BAABfcmVscy8ucmVsc1BL\nAQItABQABgAIAAAAIQAzLwWeQQAAADkAAAAQAAAAAAAAAAAAAAAAACkCAABkcnMvc2hhcGV4bWwu\neG1sUEsBAi0AFAAGAAgAAAAhAGM4V+nBAAAA2gAAAA8AAAAAAAAAAAAAAAAAmAIAAGRycy9kb3du\ncmV2LnhtbFBLBQYAAAAABAAEAPUAAACGAwAAAAA=\n"));

            V.TextBox textBox1 = new V.TextBox() { Inset = "0,0,0,0" };

            TextBoxContent textBoxContent2 = new TextBoxContent();

            Paragraph paragraph3 = new Paragraph() { RsidParagraphMarkRevision = "001769AD", RsidParagraphAddition = "001769AD", RsidParagraphProperties = "001769AD", RsidRunAdditionDefault = "001769AD" };

            ParagraphProperties paragraphProperties3 = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines3 = new SpacingBetweenLines() { After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto };
            Justification justification2 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties2 = new ParagraphMarkRunProperties();
            RunFonts runFonts3 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            paragraphMarkRunProperties2.Append(runFonts3);

            paragraphProperties3.Append(spacingBetweenLines3);
            paragraphProperties3.Append(justification2);
            paragraphProperties3.Append(paragraphMarkRunProperties2);

            Run run3 = new Run() { RsidRunProperties = "001769AD" };

            RunProperties runProperties3 = new RunProperties();
            RunFonts runFonts4 = new RunFonts() { Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial" };

            runProperties3.Append(runFonts4);
            Text text2 = new Text();
            text2.Text = "&";

            run3.Append(runProperties3);
            run3.Append(text2);

            paragraph3.Append(paragraphProperties3);
            paragraph3.Append(run3);

            textBoxContent2.Append(paragraph3);

            textBox1.Append(textBoxContent2);

            rectangle1.Append(textBox1);

            V.Line line1 = new V.Line() { Id = "Łącznik prosty 3", Style = "position:absolute;visibility:visible;mso-wrap-style:square", OptionalString = "_x0000_s1029", StrokeColor = "black [3200]", StrokeWeight = "1pt", ConnectorType = Ovml.ConnectorValues.Straight, From = "5427,9017", To = "19827,9017" };
            line1.SetAttribute(new OpenXmlAttribute("o", "gfxdata", "urn:schemas-microsoft-com:office:office", "UEsDBBQABgAIAAAAIQD+JeulAAEAAOoBAAATAAAAW0NvbnRlbnRfVHlwZXNdLnhtbJSRzU7EIBDH\n7ya+A+FqWqoHY0zpHqwe1Zj1AQhMW2I7EAbr7ts73e5ejGviEeb/8RuoN7tpFDMk8gG1vC4rKQBt\ncB57Ld+3T8WdFJQNOjMGBC33QHLTXF7U230EEuxG0nLIOd4rRXaAyVAZIiBPupAmk/mYehWN/TA9\nqJuqulU2YAbMRV4yZFO30JnPMYvHHV+vJAlGkuJhFS5dWpoYR29NZlI1o/vRUhwbSnYeNDT4SFeM\nIdWvDcvkfMHR98JPk7wD8WpSfjYTYyiXaNkAweaQWFf+nbSgTlSErvMWyjYRL7V6T3DnSlz4wgTz\nf/Nbtr3BfEpXh59qvgEAAP//AwBQSwMEFAAGAAgAAAAhAJYFM1jUAAAAlwEAAAsAAABfcmVscy8u\ncmVsc6SQPWsDMQyG90L/g9He8yVDKSW+bIWsIYWuxtZ9kLNkJHNN/n1MoaVXsnWUXvQ8L9rtL2k2\nC4pOTA42TQsGKXCcaHDwfnp7egGjxVP0MxM6uKLCvnt82B1x9qUe6ThlNZVC6mAsJb9aq2HE5LXh\njFSTniX5UkcZbPbh7Ae027Z9tvKbAd2KaQ7RgRziFszpmqv5DztNQVi5L03gZLnvp3CPaiN/0hGX\nSvEyYHEQRb+WgktTy4G979380xuYCENh+aiOlfwnqfbvBnb1zu4GAAD//wMAUEsDBBQABgAIAAAA\nIQAzLwWeQQAAADkAAAAUAAAAZHJzL2Nvbm5lY3RvcnhtbC54bWyysa/IzVEoSy0qzszPs1Uy1DNQ\nUkjNS85PycxLt1UKDXHTtVBSKC5JzEtJzMnPS7VVqkwtVrK34+UCAAAA//8DAFBLAwQUAAYACAAA\nACEAl7YFf8EAAADaAAAADwAAAGRycy9kb3ducmV2LnhtbESPQYvCMBSE7wv+h/AEb2vqCmVbjSKC\nsBdBXXfx+GyebbF5KUnU+u+NIHgcZuYbZjrvTCOu5HxtWcFomIAgLqyuuVSw/119foPwAVljY5kU\n3MnDfNb7mGKu7Y23dN2FUkQI+xwVVCG0uZS+qMigH9qWOHon6wyGKF0ptcNbhJtGfiVJKg3WHBcq\nbGlZUXHeXYyCP/o/uzTL5Op4uGxOZp+lWq6VGvS7xQREoC68w6/2j1YwhueVeAPk7AEAAP//AwBQ\nSwECLQAUAAYACAAAACEA/iXrpQABAADqAQAAEwAAAAAAAAAAAAAAAAAAAAAAW0NvbnRlbnRfVHlw\nZXNdLnhtbFBLAQItABQABgAIAAAAIQCWBTNY1AAAAJcBAAALAAAAAAAAAAAAAAAAADEBAABfcmVs\ncy8ucmVsc1BLAQItABQABgAIAAAAIQAzLwWeQQAAADkAAAAUAAAAAAAAAAAAAAAAAC4CAABkcnMv\nY29ubmVjdG9yeG1sLnhtbFBLAQItABQABgAIAAAAIQCXtgV/wQAAANoAAAAPAAAAAAAAAAAAAAAA\nAKECAABkcnMvZG93bnJldi54bWxQSwUGAAAAAAQABAD5AAAAjwMAAAAA\n"));
            V.Stroke stroke2 = new V.Stroke() { JoinStyle = V.StrokeJoinStyleValues.Miter };

            line1.Append(stroke2);
            Wvml.AnchorLock anchorLock1 = new Wvml.AnchorLock();

            group1.Append(shapetype1);
            group1.Append(shape1);
            group1.Append(rectangle1);
            group1.Append(line1);
            group1.Append(anchorLock1);

            return group1;
        }

        // NOT USED
        private static V.Formulas CreateFormulas()
        {
            V.Formulas formulas1 = new V.Formulas();

            V.Formula formula1 = new V.Formula() { Equation = "if lineDrawn pixelLineWidth 0" };
            V.Formula formula2 = new V.Formula() { Equation = "sum @0 1 0" };
            V.Formula formula3 = new V.Formula() { Equation = "sum 0 0 @1" };
            V.Formula formula4 = new V.Formula() { Equation = "prod @2 1 2" };
            V.Formula formula5 = new V.Formula() { Equation = "prod @3 21600 pixelWidth" };
            V.Formula formula6 = new V.Formula() { Equation = "prod @3 21600 pixelHeight" };
            V.Formula formula7 = new V.Formula() { Equation = "sum @0 0 1" };
            V.Formula formula8 = new V.Formula() { Equation = "prod @6 1 2" };
            V.Formula formula9 = new V.Formula() { Equation = "prod @7 21600 pixelWidth" };
            V.Formula formula10 = new V.Formula() { Equation = "sum @8 21600 0" };
            V.Formula formula11 = new V.Formula() { Equation = "prod @7 21600 pixelHeight" };
            V.Formula formula12 = new V.Formula() { Equation = "sum @10 21600 0" };

            formulas1.Append(formula1);
            formulas1.Append(formula2);
            formulas1.Append(formula3);
            formulas1.Append(formula4);
            formulas1.Append(formula5);
            formulas1.Append(formula6);
            formulas1.Append(formula7);
            formulas1.Append(formula8);
            formulas1.Append(formula9);
            formulas1.Append(formula10);
            formulas1.Append(formula11);
            formulas1.Append(formula12);

            return formulas1;
        }

        private static bool CompareString(string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase) == 0;
        } 

        #endregion
    } 
    
    #endregion
}
