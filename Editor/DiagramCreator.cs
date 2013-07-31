// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes; 
using System.Windows.Media;
using CanvasDiagramEditor.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

#endregion

namespace CanvasDiagramEditor.Editor
{
    #region Aliases

    using MapPin = Tuple<string, string>;
    using MapWire = Tuple<object, object, object>;
    using MapWires = Tuple<object, List<Tuple<string, string>>>;
    using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    using History = Tuple<Stack<string>, Stack<string>>;
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>;
    using TreeProject = Tuple<string, Stack<Stack<string>>>;
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    using TreeSolution = Tuple<string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;

    using Connection = Tuple<FrameworkElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<FrameworkElement, List<Tuple<object, object, object>>>>;
    
    #endregion

    #region DiagramCreator

    public class DiagramCreator : IDiagramCreator
    {
        #region Fields

        public DiagramCreatorOptions CurrentOptions = null;
        public Action UpdateDiagramProperties { get; set; }

        private Canvas ParserCanvas = null;
        private Path ParserPath = null;

        public LinkedList<FrameworkElement> SelectedThumbList = null;
        public LinkedListNode<FrameworkElement> CurrentThumbNode = null;

        #endregion

        #region Model

        public TreeSolution ParseDiagramModel(string model,
            Canvas canvas, Path path,
            double offsetX, double offsetY,
            bool appendIds, bool updateIds,
            bool select,
            bool createElements)
        {
            var parser = new DiagramParser();

            var parseOptions = new ParseOptions()
            {
                OffsetX = offsetX,
                OffsetY = offsetY,
                AppendIds = appendIds,
                UpdateIds = updateIds,
                Select = select,
                CreateElements = createElements,
                Counter = CurrentOptions.Counter,
                Properties = CurrentOptions.CurrentProperties
            };

            ParserCanvas = canvas;
            ParserPath = path;

            var result = parser.Parse(model, this, parseOptions);

            CurrentOptions.Counter = parseOptions.Counter;
            CurrentOptions.CurrentProperties = parseOptions.Properties;

            ParserCanvas = null;
            ParserPath = null;

            return result;
        }

        public void ClearModel(Canvas canvas)
        {
            canvas.Children.Clear();

            CurrentOptions.Counter.ResetDiagram();
        }

        public string GenerateModelFromSelected(Canvas canvas)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var elements = Editor.GetSelectedElements(canvas);

            string model = Editor.GenerateModel(elements);

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModelFromSelected() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return model;
        }

        #endregion

        #region IDiagramCreator

        public object CreatePin(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplatePin] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementPin + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateWire(double x1, double y1, double x2, double y2,
            bool startVisible, bool endVisible,
            bool startIsIO, bool endIsIO,
            int id)
        {
            var line = new LineEx()
            {
                Style = Application.Current.Resources[ResourceConstants.KeyStyleWireLine] as Style,
                X1 = 0, //X1 = x1,
                Y1 = 0, //Y1 = y1,
                Margin = new Thickness(x1, y1, 0, 0),
                X2 = x2 - x1, // X2 = x2,
                Y2 = y2 - y1, // Y2 = y2,
                IsStartVisible = startVisible,
                IsEndVisible = endVisible,
                IsStartIO = startIsIO,
                IsEndIO = endIsIO,
                Uid = ModelConstants.TagElementWire + ModelConstants.TagNameSeparator + id.ToString()
            };

            return line;
        }

        public object CreateInput(double x, double y, int id, int tagId, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateInput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementInput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            // set element Tag
            var tags = CurrentOptions.Tags;
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();

                if (tag != null)
                {
                    ElementThumb.SetData(thumb, tag);
                }
            }

            return thumb;
        }

        public object CreateOutput(double x, double y, int id, int tagId, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOutput] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOutput + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            // set element Tag
            var tags = CurrentOptions.Tags;
            if (tags != null)
            {
                var tag = tags.Cast<Tag>().Where(t => t.Id == tagId).FirstOrDefault();

                if (tag != null)
                {
                    ElementThumb.SetData(thumb, tag);
                }
            }

            return thumb;
        }

        public object CreateAndGate(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateAndGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementAndGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateOrGate(double x, double y, int id, bool snap)
        {
            var thumb = new ElementThumb()
            {
                Template = Application.Current.Resources[ResourceConstants.KeyTemplateOrGate] as ControlTemplate,
                Style = Application.Current.Resources[ResourceConstants.KeySyleRootThumb] as Style,
                Uid = ModelConstants.TagElementOrGate + ModelConstants.TagNameSeparator + id.ToString()
            };

            SetThumbEvents(thumb);
            SetElementPosition(thumb, x, y, snap);

            return thumb;
        }

        public object CreateDiagram(DiagramProperties properties)
        {
            var canvas = ParserCanvas;
            var path = ParserPath;

            if (path != null)
            {
                GenerateGrid(path,
                    properties.GridOriginX,
                    properties.GridOriginY,
                    properties.GridWidth,
                    properties.GridHeight,
                    properties.GridSize);
            }

            SetDiagramSize(canvas, properties.PageWidth, properties.PageHeight);

            return null;
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {
            var canvas = ParserCanvas;

            Editor.InsertElements(canvas, elements.Cast<FrameworkElement>(), select);
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            Editor.UpdateIdCounter(original, counter);
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            Editor.UpdateElementConnections(dict);
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            Editor.AppendElementIds(elements, CurrentOptions.Counter);
        }

        #endregion

        #region Wire Connections

        private static Tuple<double, double> GetPinPosition(FrameworkElement root, FrameworkElement pin)
        {
            // get root position in canvas
            double rx = Canvas.GetLeft(root);
            double ry = Canvas.GetTop(root);

            // get pin position in canvas (relative to root)
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);

            // calculate real pin position
            double x = rx + px;
            double y = ry + py;

            return new Tuple<double, double>(x, y);
        }

        private static FrameworkElement GetPinParent(FrameworkElement pin)
        {
            return (pin.Parent as FrameworkElement).Parent as FrameworkElement;
        }

        private static FrameworkElement GetPinTemplatedParent(FrameworkElement pin)
        {
            return GetPinParent(pin).TemplatedParent as FrameworkElement;
        }

        private void CreateConnection(Canvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            CurrentOptions.CurrentRoot = GetPinTemplatedParent(pin);

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            var position = GetPinPosition(CurrentOptions.CurrentRoot, pin);
            double x = position.Item1;
            double y = position.Item2;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreateConnection(canvas, x, y);
        }

        private LineEx CreateConnection(Canvas canvas, double x, double y)
        {
            LineEx result = null;

            if (CurrentOptions.CurrentRoot.Tag == null)
            {
                CurrentOptions.CurrentRoot.Tag = new Selection(false, new List<MapWire>());
            }

            var selection = CurrentOptions.CurrentRoot.Tag as Selection;
            var tuples = selection.Item2;

            if (CurrentOptions.CurrentLine == null)
            {
                result = CreateFirstConnection(canvas, x, y, tuples);
            }
            else
            {
                result = CreateSecondConnection(x, y, tuples);
            }

            return result;
        }

        private LineEx CreateFirstConnection(Canvas canvas, double x, double y, List<MapWire> tuples)
        {
            // update IsStartIO
            string rootUid = CurrentOptions.CurrentRoot.Uid;

            bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) 
                || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            var line = CreateWire(x, y, x, y,
                false, false,
                startIsIO, false,
                CurrentOptions.Counter.WireCount) as LineEx;

            CurrentOptions.Counter.WireCount += 1;
            CurrentOptions.CurrentLine = line;

            // update connections
            var tuple = new MapWire(CurrentOptions.CurrentLine, CurrentOptions.CurrentRoot, null);
            tuples.Add(tuple);

            canvas.Children.Add(CurrentOptions.CurrentLine);

            // line Tag is start root element
            if (CurrentOptions.CurrentLine != null || 
                !(CurrentOptions.CurrentLine is LineEx))
            {
                CurrentOptions.CurrentLine.Tag = CurrentOptions.CurrentRoot;
            }

            return line;
        }

        private LineEx CreateSecondConnection(double x, double y, List<MapWire> tuples)
        {
            var margin = CurrentOptions.CurrentLine.Margin;

            CurrentOptions.CurrentLine.X2 = x - margin.Left;
            CurrentOptions.CurrentLine.Y2 = y - margin.Top;

            // update IsEndIO flag
            string rootUid = CurrentOptions.CurrentRoot.Uid;

            bool endIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) ||
                StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            CurrentOptions.CurrentLine.IsEndIO = endIsIO;

            // update connections
            var tuple = new MapWire(CurrentOptions.CurrentLine, null, CurrentOptions.CurrentRoot);
            tuples.Add(tuple);

            // line Tag is start root element
            var line = CurrentOptions.CurrentLine;
            if (line != null && line.Tag != null)
            {
                // line Tag is start root element
                var start = line.Tag as FrameworkElement;
                if (start != null)
                {
                    // line Tag is Tuple of start & end root element
                    // this Tag is used to find all connected elements
                    line.Tag = new Tuple<object, object>(start, CurrentOptions.CurrentRoot);
                }
            }

            var result = CurrentOptions.CurrentLine;

            // reset current line and root
            CurrentOptions.CurrentLine = null;
            CurrentOptions.CurrentRoot = null;

            return result;
        }

        #endregion

        #region Wire Split

        private bool CreateWireSplit(Canvas canvas, LineEx line, ref Point point)
        {
            if (CurrentOptions.CurrentLine == null)
            {
                AddToHistory(canvas, true);
            }

            // create split pin
            var splitPin = InsertPin(canvas, point);
            CurrentOptions.CurrentRoot = splitPin;

            // connect current line to split pin
            double x = Canvas.GetLeft(CurrentOptions.CurrentRoot);
            double y = Canvas.GetTop(CurrentOptions.CurrentRoot);

            CreateConnection(canvas, x, y);

            // remove original hit tested line
            canvas.Children.Remove(line);

            // remove wire connections
            var connections = RemoveWireConnections(canvas, line);

            // connected original root element to split pin
            if (connections != null && connections.Count == 2)
            {
                CreateSplitConnections(canvas, line, splitPin, x, y, connections);
            }
            else
            {
                throw new InvalidOperationException("LineEx should have only two connections: Start and End.");
            }

            return true;
        }

        private void CreateSplitConnections(Canvas canvas, 
            LineEx line, FrameworkElement splitPin, 
            double x, double y, 
            Connections connections)
        {
            var c1 = connections[0];
            var c2 = connections[1];
            var map1 = c1.Item2.FirstOrDefault();
            var map2 = c2.Item2.FirstOrDefault();
            var startRoot = (map1.Item2 != null ? map1.Item2 : map2.Item2) as FrameworkElement;
            var endRoot = (map1.Item3 != null ? map1.Item3 : map2.Item3) as FrameworkElement;
            var location = GetLineExStartAndEnd(map1, map2);

            //System.Diagnostics.Debug.Print("c1: {0}", c1.Item1.Uid);
            //System.Diagnostics.Debug.Print("c2: {0}", c2.Item1.Uid);
            //System.Diagnostics.Debug.Print("startRoot: {0}", startRoot.Uid);
            //System.Diagnostics.Debug.Print("endRoot: {0}", endRoot.Uid);

            if (location.Item1.HasValue && location.Item2.HasValue)
            {
                Point start = location.Item1.Value;
                Point end = location.Item2.Value;
                double x1 = start.X;
                double y1 = start.Y;
                double x2 = x1 + end.X;
                double y2 = y1 + end.Y;
                bool isStartVisible = line.IsStartVisible;
                bool isEndVisible = line.IsEndVisible;
                bool isStartIO = line.IsStartIO;
                bool isEndIO = line.IsEndIO;

                //System.Diagnostics.Debug.Print("start: {0}", start);
                //System.Diagnostics.Debug.Print("end: {0}", end);
                //System.Diagnostics.Debug.Print("x1,y1: {0},{1}", x1, y1);
                //System.Diagnostics.Debug.Print("x2,y2: {0},{1}", x2, y2);

                CurrentOptions.CurrentRoot = startRoot;
                var startLine = CreateConnection(canvas, x1, y1);

                CurrentOptions.CurrentRoot = splitPin;
                CreateConnection(canvas, x, y);

                CurrentOptions.CurrentRoot = splitPin;
                var endLine = CreateConnection(canvas, x, y);

                CurrentOptions.CurrentRoot = endRoot;
                CreateConnection(canvas, x2, y2);

                // restore orignal line flags
                //System.Diagnostics.Debug.Print("startLine: {0}", startLine.Uid);
                //System.Diagnostics.Debug.Print("endLine: {0}", endLine.Uid);

                startLine.IsStartVisible = isStartVisible;
                startLine.IsStartIO = isStartIO;
                endLine.IsEndVisible = isEndVisible;
                endLine.IsEndIO = isEndIO;
            }
            else
            {
                throw new InvalidOperationException("LineEx should have corrent location info for Start and End.");
            }
        }

        #endregion

        #region Insert

        public FrameworkElement InsertPin(Canvas canvas, Point point)
        {
            var thumb = CreatePin(point.X, point.Y, CurrentOptions.Counter.PinCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.PinCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, CurrentOptions.Counter.InputCount, -1, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.InputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, CurrentOptions.Counter.OutputCount, -1, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.OutputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, CurrentOptions.Counter.AndGateCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.AndGateCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, CurrentOptions.Counter.OrGateCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.OrGateCount += 1;

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

        #region Snap

        public double Snap(double original, double snap, double offset)
        {
            return Snap(original - offset, snap) + offset;
        }

        public double Snap(double original, double snap)
        {
            return original + ((Math.Round(original / snap) - original / snap) * snap);
        }

        private double SnapOffsetX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, CurrentOptions.CurrentProperties.SnapX, CurrentOptions.CurrentProperties.SnapOffsetX) : original;
        }

        private double SnapOffsetY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, CurrentOptions.CurrentProperties.SnapY, CurrentOptions.CurrentProperties.SnapOffsetY) : original;
        }

        private double SnapX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, CurrentOptions.CurrentProperties.SnapX) : original;
        }

        private double SnapY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, CurrentOptions.CurrentProperties.SnapY) : original;
        }

        #endregion

        #region Grid

        public string GenerateGrid(double originX, double originY, double width, double height, double size)
        {
            var sb = new StringBuilder();

            double sizeX = size;
            double sizeY = size;

            // horizontal lines
            for (double y = sizeY + originY /* originY + size */; y < height + originY; y += size)
            {
                sb.AppendFormat("M{0},{1}", originX, y);
                sb.AppendFormat("L{0},{1}", width + originX, y);
            }

            // vertical lines
            for (double x = sizeX + originX /* originX + size */; x < width + originX; x += size)
            {
                sb.AppendFormat("M{0},{1}", x, originY);
                sb.AppendFormat("L{0},{1}", x, height + originY);
            }

            return sb.ToString();
        }

        public void GenerateGrid(Path path, double originX, double originY, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            string grid = GenerateGrid(originX, originY, width, height, size);

            path.Data = Geometry.Parse(grid);

            sw.Stop();
            //System.Diagnostics.Debug.Print("GenerateGrid() in {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public void SetDiagramSize(Canvas canvas, double width, double height)
        {
            canvas.Width = width;
            canvas.Height = height;
        }

        public void GenerateGrid(bool undo)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            if (UpdateDiagramProperties != null)
            {
                UpdateDiagramProperties();
            }

            if (undo == true)
            {
                AddToHistory(canvas, false);
            }

            var prop = CurrentOptions.CurrentProperties;

            if (path != null)
            {
                GenerateGrid(path,
                    prop.GridOriginX, prop.GridOriginY,
                    prop.GridWidth, prop.GridHeight,
                    prop.GridSize);
            }

            SetDiagramSize(canvas, prop.PageWidth, prop.PageHeight);
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

        public string AddToHistory(Canvas canvas, bool resetSelectedList)
        {
            if (CurrentOptions.EnableHistory != true)
                return null;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            var model = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

            undoHistory.Push(model);

            redoHistory.Clear();

            if (resetSelectedList == true)
            {
                ResetSelectedList();
            }

            return model;
        }

        private void RollbackUndoHistory(Canvas canvas)
        {
            if (CurrentOptions.EnableHistory != true)
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
            if (CurrentOptions.EnableHistory != true)
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
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ClearModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        private void Redo(Canvas canvas, Path path, bool pushUndo)
        {
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();

            ClearModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        public void Undo()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            this.Undo(canvas, path, true);
        }

        public void Redo()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            this.Redo(canvas, path, true);
        }

        public List<string> GetDiagramModelHistory(Canvas canvas)
        {
            List<string> diagrams = null;

            var currentDiagram = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

            var history = GetHistory(canvas);
            var undoHistory = history.Item1;
            var redoHistory = history.Item2;

            diagrams = new List<string>(undoHistory.Reverse());

            diagrams.Add(currentDiagram);

            return diagrams;
        }

        #endregion

        #region Move

        private void SetElementPosition(FrameworkElement element, double left, double top, bool snap)
        {
            Canvas.SetLeft(element, SnapOffsetX(left, snap));
            Canvas.SetTop(element, SnapOffsetY(top, snap));
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
                    MoveLine(dX, dY, snap, tuple);
                }
            }
        }

        private void MoveLine(double dX, double dY, bool snap, MapWire tuple)
        {
            var line = tuple.Item1 as LineEx;
            var start = tuple.Item2;
            var end = tuple.Item3;

            if (start != null)
            {
                var margin = line.Margin;
                double left = margin.Left;
                double top = margin.Top;
                double x = 0.0;
                double y = 0.0;

                //line.X1 = SnapOffsetX(line.X1 + dX, snap);
                //line.Y1 = SnapOffsetY(line.Y1 + dY, snap);

                x = SnapOffsetX(left + dX, snap);
                y = SnapOffsetY(top + dY, snap);

                if (left != x || top != y)
                {
                    line.X2 += left - x;
                    line.Y2 += top - y;
                    line.Margin = new Thickness(x, y, 0, 0);
                }
            }

            if (end != null)
            {
                double left = line.X2;
                double top = line.Y2;
                double x = 0.0;
                double y = 0.0;

                x = SnapX(left + dX, snap);
                y = SnapY(top + dY, snap);

                line.X2 = x;
                line.Y2 = y;
            }
        }

        private Tuple<Point?, Point?> GetLineExStartAndEnd(MapWire map1, MapWire map2)
        {
            var line1 = map1.Item1 as LineEx;
            var start1 = map1.Item2;
            var end1 = map1.Item3;

            var line2 = map2.Item1 as LineEx;
            var start2 = map2.Item2;
            var end2 = map2.Item3;

            Point? startPoint = null;
            Point? endPoint = null;

            if (start1 != null)
            {
                var margin = line1.Margin;
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new Point(left, top);
            }

            if (end1 != null)
            {
                double left = line1.X2;
                double top = line1.Y2;

                endPoint = new Point(left, top);
            }

            if (start2 != null)
            {
                var margin = line2.Margin;
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new Point(left, top);
            }

            if (end2 != null)
            {
                double left = line2.X2;
                double top = line2.Y2;

                endPoint = new Point(left, top);
            }

            return new Tuple<Point?, Point?>(startPoint, endPoint);
        }

        public void MoveSelectedElements(Canvas canvas, double dX, double dY, bool snap)
        {
            // move all selected elements
            var thumbs = canvas.Children.OfType<ElementThumb>().Where(x => ElementThumb.GetIsSelected(x));

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        public void MoveLeft(Canvas canvas)
        {
            AddToHistory(canvas, false);

            if (CurrentOptions.EnableSnap == true)
            {
                double delta = CurrentOptions.CurrentProperties.GridSize;
                MoveSelectedElements(canvas, -delta, 0.0, false);
            }
            else
            {
                MoveSelectedElements(canvas, -1.0, 0.0, false);
            }
        }

        public void MoveRight(Canvas canvas)
        {
            AddToHistory(canvas, false);

            if (CurrentOptions.EnableSnap == true)
            {
                double delta = CurrentOptions.CurrentProperties.GridSize;
                MoveSelectedElements(canvas, delta, 0.0, false);
            }
            else
            {
                MoveSelectedElements(canvas, 1.0, 0.0, false);
            }
        }

        public void MoveUp(Canvas canvas)
        {
            AddToHistory(canvas, false);

            if (CurrentOptions.EnableSnap == true)
            {
                double delta = CurrentOptions.CurrentProperties.GridSize;
                MoveSelectedElements(canvas, 0.0, -delta, false);
            }
            else
            {
                MoveSelectedElements(canvas, 0.0, -1.0, false);
            }
        }

        public void MoveDown(Canvas canvas)
        {
            AddToHistory(canvas, false);

            if (CurrentOptions.EnableSnap == true)
            {
                double delta = CurrentOptions.CurrentProperties.GridSize;
                MoveSelectedElements(canvas, 0.0, delta, false);
            }
            else
            {
                MoveSelectedElements(canvas, 0.0, 1.0, false);
            }
        }

        #endregion

        #region Drag

        public void Drag(Canvas canvas, ElementThumb element, double dX, double dY)
        {
            bool snap = (CurrentOptions.SnapOnRelease == true && CurrentOptions.EnableSnap == true) ? false : CurrentOptions.EnableSnap;

            if (CurrentOptions.MoveAllSelected == true)
            {
                MoveSelectedElements(canvas, dX, dY, snap);
            }
            else
            {
                // move only selected element
                MoveRoot(element, dX, dY, snap);
            }
        }

        public void DragStart(Canvas canvas, ElementThumb element)
        {
            AddToHistory(canvas, false);

            if (ElementThumb.GetIsSelected(element) == true)
            {
                CurrentOptions.MoveAllSelected = true;
            }
            else
            {
                CurrentOptions.MoveAllSelected = false;

                // select
                ElementThumb.SetIsSelected(element, true);
            }
        }

        public void DragEnd(Canvas canvas, ElementThumb element)
        {
            if (CurrentOptions.SnapOnRelease == true && CurrentOptions.EnableSnap == true)
            {
                if (CurrentOptions.MoveAllSelected == true)
                {
                    MoveSelectedElements(canvas, 0.0, 0.0, CurrentOptions.EnableSnap);
                }
                else
                {
                    // move only selected element

                    // deselect
                    ElementThumb.SetIsSelected(element, false);

                    MoveRoot(element, 0.0, 0.0, CurrentOptions.EnableSnap);
                }
            }
            else
            {
                if (CurrentOptions.MoveAllSelected != true)
                {
                    // de-select
                    ElementThumb.SetIsSelected(element, false);
                }
            }

            CurrentOptions.MoveAllSelected = false;
        }

        #endregion

        #region Thumb Events

        private void RootElement_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var element = sender as ElementThumb;

            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            Drag(canvas, element, dX, dY);
        }

        private void RootElement_DragStarted(object sender, DragStartedEventArgs e)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var element = sender as ElementThumb;

            DragStart(canvas, element);
        }

        private void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var element = sender as ElementThumb;

            DragEnd(canvas, element);
        }

        private void SetThumbEvents(ElementThumb thumb)
        {
            thumb.DragDelta += this.RootElement_DragDelta;
            thumb.DragStarted += this.RootElement_DragStarted;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

        #endregion

        #region HitTest

        public IEnumerable<FrameworkElement> HitTest(Canvas canvas, ref Point point)
        {
            var selectedElements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
            {
                RadiusX = CurrentOptions.HitTestRadiusX,
                RadiusY = CurrentOptions.HitTestRadiusY,
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

            return selectedElements.Cast<FrameworkElement>();
        }

        public IEnumerable<FrameworkElement> HitTest(Canvas canvas, ref Rect rect)
        {
            var selectedElements = new List<DependencyObject>();
            var rectangle = new RectangleGeometry(rect, 0.0, 0.0);
            var hitTestParams = new GeometryHitTestParameters(rectangle);
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

            return selectedElements.Cast<FrameworkElement>();
        }

        #endregion

        #region Delete

        private void DeleteElement(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point).FirstOrDefault() as FrameworkElement;
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        private void DeleteElement(Canvas canvas, FrameworkElement element)
        {
            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                DeleteWire(canvas, line);
            }
            else
            {
                canvas.Children.Remove(element);
            }
        }

        private static void DeleteWire(Canvas canvas, LineEx line)
        {
            canvas.Children.Remove(line);

            RemoveWireConnections(canvas, line);

            DeleteEmptyPins(canvas);
        }

        private static void DeleteEmptyPins(Canvas canvas)
        {
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

            foreach (var element in canvas.Children.Cast<FrameworkElement>())
            {
                string uid = element.Uid;

                if (IsElementPin(uid))
                {
                    if (element.Tag != null)
                    {
                        var selection = element.Tag as Selection;
                        var tuples = selection.Item2;

                        if (tuples.Count <= 0)
                        {
                            // empty pin
                            pins.Add(element);
                        }
                    }
                    else
                    {
                        // empty pin
                        pins.Add(element);
                    }
                }
            }

            return pins;
        }

        private static bool IsElementPin(string uid)
        {
            return uid != null &&
                   StringUtil.StartsWith(uid, ModelConstants.TagElementPin);
        }

        private static Connections RemoveWireConnections(Canvas canvas, LineEx line)
        {
            var connections = new Connections();

            foreach (var child in canvas.Children)
            {
                var element = child as FrameworkElement;

                if (element.Tag != null && !(element is LineEx))
                {
                    RemoveWireConnections(line, connections, element);
                }
            }

            return connections;
        }

        private static void RemoveWireConnections(LineEx line, Connections connections, FrameworkElement element)
        {
            var selection = element.Tag as Selection;
            var tuples = selection.Item2;
            var map = new List<MapWire>();

            CreateMapWire(line, tuples, map);

            if (map.Count > 0)
            {
                connections.Add(new Connection(element, map));
            }

            foreach (var tuple in map)
            {
                tuples.Remove(tuple);
            }
        }

        private static void CreateMapWire(LineEx line, List<MapWire> tuples, List<MapWire> map)
        {
            foreach (var tuple in tuples)
            {
                var _line = tuple.Item1 as LineEx;

                if (StringUtil.Compare(_line.Uid, line.Uid))
                {
                    map.Add(tuple);
                }
            }
        }

        public void Delete(Canvas canvas, Point point)
        {
            AddToHistory(canvas, true);

            DeleteElement(canvas, point);

            CurrentOptions.SkipLeftClick = false;
        }

        #endregion

        #region Invert Wire Start or End

        public LineEx FindLineEx(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point).FirstOrDefault() as FrameworkElement;
            if (element == null)
                return null;

            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("FindLineEx, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                return line;
            }

            return null;
        }

        public void ToggleWireStart(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas, false);

                line.IsStartVisible = line.IsStartVisible == true ? false : true;

                CurrentOptions.SkipLeftClick = false;
            }
        }

        public void ToggleWireEnd(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas, false);

                line.IsEndVisible = line.IsEndVisible == true ? false : true;

                CurrentOptions.SkipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save

        private void OpenDiagram(string fileName, Canvas canvas, Path path)
        {
            string diagram = Editor.OpenModel(fileName);

            AddToHistory(canvas, true);

            ClearModel(canvas);
            ParseDiagramModel(diagram, canvas, path, 0, 0, false, true, false, true);
        }

        private void SaveDiagram(string fileName, Canvas canvas)
        {
            string model = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

            Editor.SaveModel(fileName, model);
        }

        public void OpenDiagram()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = CurrentOptions.CurrentCanvas;
                var path = CurrentOptions.CurrentPathGrid;

                this.OpenDiagram(dlg.FileName, canvas, path);
            }
        }

        public void SaveDiagram()
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
                var canvas = CurrentOptions.CurrentCanvas;

                this.SaveDiagram(dlg.FileName, canvas);
            }
        }

        private TreeSolution OpenSolutionModel(string fileName)
        {
            TreeSolution solution = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                solution = ParseDiagramModel(diagram, null, null, 0, 0, false, false, false, false);
            }

            return solution;
        }

        private void SaveSolution(string fileName)
        {
            UpdateSelectedDiagramModel();

            var model = GenerateSolutionModel(fileName).Item1;
            Editor.SaveModel(fileName, model);
        }

        public TreeSolution OpenSolutionModel()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Solution"
            };

            TreeSolution solution = null;

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = CurrentOptions.CurrentCanvas;

                ClearModel(canvas);

                solution = OpenSolutionModel(dlg.FileName);
            }

            return solution;
        }

        public void OpenSolution()
        {
            var tree = CurrentOptions.CurrentTree;
            var solution = OpenSolutionModel();

            if (solution != null)
            {
                OpenSolution(tree, solution);
            }
        }

        public void SaveSolution()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Solution",
                FileName = "solution"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var fileName = dlg.FileName;

                var tree = CurrentOptions.CurrentTree;

                UpdateTags();

                SaveSolution(fileName);
            }
        }

        #endregion

        #region Model

        public string Import()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var diagram = Editor.OpenModel(dlg.FileName);

                return diagram;
            }

            return null;
        }

        public void Insert(string diagram, double offsetX, double offsetY)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            AddToHistory(canvas, true);

            DeselectAll();
            ParseDiagramModel(diagram, canvas, path, offsetX, offsetY, true, true, true, true);
        }

        public void Clear()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            AddToHistory(canvas, true);

            ClearModel(canvas);
        }

        public string GenerateModel()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

            return diagram;
        }

        public string GenerateModelFromSelected()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = GenerateModelFromSelected(canvas);

            return diagram;
        }

        #endregion

        #region Export To Dxf

        public string GenerateDxf(string model, bool shortenStart, bool shortenEnd)
        {
            var dxf = new DxfDiagramCreator()
            {
                ShortenStart = shortenStart,
                ShortenEnd = shortenEnd,
                DiagramProperties  = CurrentOptions.CurrentProperties,
                Tags = CurrentOptions.Tags
            };

            return dxf.GenerateDxfFromModel(model);
        }

        private void SaveDxf(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        private void ExportDiagramToDxf(string fileName, Canvas canvas, bool shortenStart, bool shortenEnd)
        {
            string model = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

            string dxf = GenerateDxf(model, shortenStart, shortenEnd);

            SaveDxf(fileName, dxf);
        }

        public void ExportToDxf(bool shortenStart, bool shortenEnd)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Dxf (*.dxf)|*.dxf|All Files (*.*)|*.*",
                Title = "Export Diagram to Dxf",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = CurrentOptions.CurrentCanvas;

                this.ExportDiagramToDxf(dlg.FileName, canvas, shortenStart, shortenEnd);
            }
        }

        #endregion

        #region Clipboard

        private static string GetClipboardText()
        {
            var model = Clipboard.GetText();
            return model;
        }

        private static void SetClipboardText(string model)
        {
            Clipboard.SetText(model);
        }

        #endregion

        #region Edit

        public void Cut()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            string model = GenerateModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);

                var elements = Editor.GetAllElements(canvas);

                Delete(canvas, elements);
            }
            else
            {
                Delete();
            }

            SetClipboardText(model);
        }

        public void Copy()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            string model = GenerateModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Editor.GenerateModel(canvas, null, CurrentOptions.CurrentProperties);
            }

            SetClipboardText(model);
        }

        public void Paste(Point point)
        {
            var model = GetClipboardText();

            if (model != null || model.Length > 0)
            {
                Insert(model, point.X, point.Y);
            }
        }

        public void Delete()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var elements = Editor.GetSelectedElements(canvas);

            Delete(canvas, elements);
        }

        public void Delete(Canvas canvas, IEnumerable<FrameworkElement> elements)
        {
            AddToHistory(canvas, true);

            DeleteThumbsAnndLines(canvas, elements);
        }

        private void DeleteThumbsAnndLines(Canvas canvas, IEnumerable<FrameworkElement> elements)
        {
            // delete thumbs & lines
            foreach (var element in elements)
            {
                DeleteElement(canvas, element);
            }
        }

        #endregion

        #region Selection

        public IEnumerable<FrameworkElement> GetSelectedElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetSelectedElements(canvas);
        }

        public IEnumerable<FrameworkElement> GetSelectedThumbElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetSelectedThumbElements(canvas);
        }

        public IEnumerable<FrameworkElement> GetThumbElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetThumbElements(canvas);
        }

        public IEnumerable<FrameworkElement> GetAllElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetAllElements(canvas);
        }

        public void SelectAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            Editor.SelectAll(canvas);
        }

        public void DeselectAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            Editor.DeselectAll(canvas);
        }

        public void SelectPrevious(bool deselect)
        {
            if (SelectedThumbList == null)
            {
                SelectPreviousInitialize(deselect);
            }
            else
            {
                SelectPreviousElement(deselect);
            }
        }

        private void SelectPreviousInitialize(bool deselect)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var elements = Editor.GetThumbElements(canvas);

            if (elements != null)
            {
                SelectedThumbList = new LinkedList<FrameworkElement>(elements);

                CurrentThumbNode = SelectedThumbList.Last;
                if (CurrentThumbNode != null)
                {
                    SelectOneElement(CurrentThumbNode.Value, deselect);
                }
            }
        }

        private void SelectPreviousElement(bool deselect)
        {
            // bool isCurrentSelected = ElementThumb.GetIsSelected(CurrentThumbNode.Value);
            // if (deselect == false && isCurrentSelected)
            // {
            //     ElementThumb.SetIsSelected(CurrentThumbNode.Value, false);
            // }

            if (CurrentThumbNode != null)
            {
                CurrentThumbNode = CurrentThumbNode.Previous;
                if (CurrentThumbNode == null)
                {
                    CurrentThumbNode = SelectedThumbList.Last;
                }

                SelectOneElement(CurrentThumbNode.Value, deselect);
            }
        }

        public void SelectNext(bool deselect)
        {
            if (SelectedThumbList == null)
            {
                SelectNextInitialize(deselect);
            }
            else
            {
                SelectNextElement(deselect);
            }
        }

        private void SelectNextInitialize(bool deselect)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var elements = Editor.GetThumbElements(canvas);

            if (elements != null)
            {
                SelectedThumbList = new LinkedList<FrameworkElement>(elements);

                CurrentThumbNode = SelectedThumbList.First;
                if (CurrentThumbNode != null)
                {
                    SelectOneElement(CurrentThumbNode.Value, deselect);
                }
            }
        }

        private void SelectNextElement(bool deselect)
        {
            if (CurrentThumbNode != null)
            {
                // bool isCurrentSelected = ElementThumb.GetIsSelected(CurrentThumbNode.Value);
                // if (deselect == false && isCurrentSelected)
                // {
                //     ElementThumb.SetIsSelected(CurrentThumbNode.Value, false);
                // }

                CurrentThumbNode = CurrentThumbNode.Next;
                if (CurrentThumbNode == null)
                {
                    CurrentThumbNode = SelectedThumbList.First;
                }

                SelectOneElement(CurrentThumbNode.Value, deselect);
            }
        }

        public void ResetSelectedList()
        {
            if (SelectedThumbList != null)
            {
                SelectedThumbList.Clear();
                SelectedThumbList = null;
                CurrentThumbNode = null;
            }
        }

        public void SelectOneElement(FrameworkElement element, bool deselect)
        {
            if (element != null)
            {
                if (deselect == true)
                {
                    DeselectAll();
                    ElementThumb.SetIsSelected(element, true);
                }
                else
                {
                    bool isSelected = ElementThumb.GetIsSelected(element);
                    ElementThumb.SetIsSelected(element, !isSelected);
                }
            }
        }

        public void SelectConnected()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            Editor.SelectConnected(canvas);
        }

        #endregion

        #region Mouse Handlers

        public void HandleLeftDown(Canvas canvas, Point point)
        {
            if (CanConnect())
            {
                 CreateCanvasPinConnection(canvas, point);
            }
            else if (CurrentOptions.EnableInsertLast == true)
            {
                AddToHistory(canvas, true);

                InsertLast(canvas, CurrentOptions.LastInsert, point);
            }
        }

        private void CreateCanvasPinConnection(Canvas canvas, Point point)
        {
            var root = InsertPin(canvas, point);

            CurrentOptions.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

            double x = Canvas.GetLeft(CurrentOptions.CurrentRoot);
            double y = Canvas.GetTop(CurrentOptions.CurrentRoot);

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreateConnection(canvas, x, y);

            CurrentOptions.CurrentRoot = root;
            CreateConnection(canvas, x, y);
        }

        private bool CanConnect()
        {
            return CurrentOptions.CurrentRoot != null && 
                   CurrentOptions.CurrentLine != null;
        }

        public bool HandlePreviewLeftDown(Canvas canvas, Point point, FrameworkElement pin)
        {
            if (IsPinConnectable(pin))
            {
                if (CurrentOptions.CurrentLine == null)
                {
                    AddToHistory(canvas, true);
                }

                CreateConnection(canvas, pin);

                return true;
            }
            else if (CurrentOptions.CurrentLine != null)
            {
                var element = GetElementAtPoint(canvas, ref point);

                System.Diagnostics.Debug.Print("Split wire: {0}", element == null ? "<null>" : element.Uid);

                if (CanSplitWire(element))
                {
                    return CreateWireSplit(canvas, element as LineEx,  ref point);
                }
            }

            if (CanToggleLine())
            {
                point = ToggleLineSelection(canvas, point);
            }

            return false;
        }

        private bool CanSplitWire(FrameworkElement element)
        {
            if (element == null)
            {
                return false;
            }

            var elementUid = element.Uid;
            var lineUid = CurrentOptions.CurrentLine.Uid;

            return element != null &&
                CanConnect() &&
                NotSameElement(elementUid, lineUid) &&
                ElementIsWire(elementUid);
        }

        private static bool NotSameElement(string uid1, string uid2)
        {
            return StringUtil.Compare(uid2, uid1) == false;
        }

        private static bool ElementIsWire(string elementUid)
        {
            return StringUtil.StartsWith(elementUid, ModelConstants.TagElementWire) == true;
        }

        private Point ToggleLineSelection(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point).FirstOrDefault() as FrameworkElement;

            if (element != null)
            {
                Editor.ToggleLineSelection(element);
            }
            else
            {
                Editor.SetLinesSelection(canvas, false);
            }

            return point;
        }

        private bool CanToggleLine()
        {
            return CurrentOptions.CurrentRoot == null &&
                CurrentOptions.CurrentLine == null &&
                Keyboard.Modifiers != ModifierKeys.Control;
        }

        private FrameworkElement GetElementAtPoint(Canvas canvas, ref Point point)
        {
            var element = HitTest(canvas, ref point)
                .Where(x => StringUtil.Compare(CurrentOptions.CurrentLine.Uid, x.Uid) == false)
                .FirstOrDefault() as FrameworkElement;

            return element;
        }

        private static bool IsPinConnectable(FrameworkElement pin)
        {
            return pin != null &&
            (
                !StringUtil.Compare(pin.Name, ResourceConstants.StandalonePinName)
                || Keyboard.Modifiers == ModifierKeys.Control
            );
        }

        public void HandleMove(Canvas canvas, Point point)
        {
            if (CanMoveCurrentLine())
            {
                var margin = CurrentOptions.CurrentLine.Margin;
                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (CurrentOptions.CurrentLine.X2 != x)
                {
                    //this._line.X2 = SnapX(x);
                    CurrentOptions.CurrentLine.X2 = x;
                }

                if (CurrentOptions.CurrentLine.Y2 != y)
                {
                    //this._line.Y2 = SnapY(y);
                    CurrentOptions.CurrentLine.Y2 = y;
                }
            }
        }

        private bool CanMoveCurrentLine()
        {
            return CurrentOptions.CurrentRoot != null &&
                CurrentOptions.CurrentLine != null;
        }

        public bool HandleRightDown(Canvas canvas, Path path)
        {
            if (CurrentOptions.CurrentRoot != null && 
                CurrentOptions.CurrentLine != null)
            {
                if (CurrentOptions.EnableHistory == true)
                {
                    Undo(canvas, path, false);
                }
                else
                {
                    RemoveCurrentLine(canvas);
                }

                CurrentOptions.CurrentLine = null;
                CurrentOptions.CurrentRoot = null;

                return true;
            }

            return false;
        }

        private void RemoveCurrentLine(Canvas canvas)
        {
            var selection = CurrentOptions.CurrentRoot.Tag as Selection;
            var tuples = selection.Item2;

            var last = tuples.LastOrDefault();
            tuples.Remove(last);

            canvas.Children.Remove(CurrentOptions.CurrentLine);
        }

        #endregion

        #region TreeView Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                item.BringIntoView();

                e.Handled = true;
            }
        }

        #endregion

        #region Tree View

        public void SelectPreviousTreeItem(bool selectParent)
        {
            var tree = CurrentOptions.CurrentTree;

            // get current diagram
            var selected = tree.SelectedItem as TreeViewItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.Uid, ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.Parent as TreeViewItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    var items = parent.Items;
                    int index = items.IndexOf(selected);
                    int count = items.Count;

                    // use '<' key for navigation in tree (project scope)
                    if (count > 0 && index > 0)
                    {
                        // select previous diagram
                        index = index - 1;

                        var item = items[index] as TreeViewItem;
                        item.IsSelected = true;
                        item.BringIntoView();
                    }

                    // use 'Ctrl + <' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        SelectPreviousParentTreeItem(parent);
                    }
                }
            }
        }

        private static void SelectPreviousParentTreeItem(TreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as TreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex > 0)
            {
                SelectLastItemInPreviousProject(parentParent, parentIndex);
            }
        }

        private static void SelectLastItemInPreviousProject(TreeViewItem parentParent, int parentIndex)
        {
            // get previous project
            int index = parentIndex - 1;
            var parentProject = (parentParent.Items[index] as TreeViewItem);

            // select last item in previous project
            if (parentProject.Items.Count > 0)
            {
                var item = (parentProject.Items[parentProject.Items.Count - 1] as TreeViewItem);
                item.IsSelected = true;
                item.BringIntoView();
            }
        }

        public void SelectNextTreeItem(bool selectParent)
        {
            var tree = CurrentOptions.CurrentTree;

            // get current diagram
            var selected = tree.SelectedItem as TreeViewItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.Uid, ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.Parent as TreeViewItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    var items = parent.Items;
                    int index = items.IndexOf(selected);
                    int count = items.Count;

                    // use '>' key for navigation in tree (project scope)
                    if (count > 0 && index < count - 1)
                    {
                        // select next diagram
                        index = index + 1;

                        var item = items[index] as TreeViewItem;
                        item.IsSelected = true;
                        item.BringIntoView();
                    }
             
                    // use 'Ctrl + >' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        SelectNextParentTreeItem(parent);
                    }
                }
            }
        }

        private static void SelectNextParentTreeItem(TreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as TreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex < parentCount - 1)
            {
                SelectFirstItemInNextProject(parentParent, parentIndex);
            }
        }

        private static void SelectFirstItemInNextProject(TreeViewItem parentParent, int parentIndex)
        {
            // get next project
            int index = parentIndex + 1;
            var parentProject = (parentParent.Items[index] as TreeViewItem);

            // select first item in next project
            if (parentProject.Items.Count > 0)
            {
                var item = (parentProject.Items[0] as TreeViewItem);
                item.IsSelected = true;
                item.BringIntoView();
            }
        }

        public bool SwitchItems(Canvas canvas, TreeViewItem oldItem, TreeViewItem newItem)
        {
            if (newItem == null)
                return false;

            string oldUid = oldItem == null ? null : oldItem.Uid;
            string newUid = newItem == null ? null : newItem.Uid;

            bool isOldItemDiagram = oldUid == null ? false : StringUtil.StartsWith(oldUid, ModelConstants.TagHeaderDiagram);
            bool isNewItemDiagram = newUid == null ? false : StringUtil.StartsWith(newUid, ModelConstants.TagHeaderDiagram);

            if (isOldItemDiagram == true)
            {
                // save current model
                StoreModel(canvas, oldItem);
            }

            if (isNewItemDiagram == true)
            {
                // load new model
                LoadModel(canvas, newItem);
            }

            System.Diagnostics.Debug.Print("Old Uid: {0}, new Uid: {1}", oldUid, newUid);

            return isNewItemDiagram;
        }

        private void LoadModel(Canvas canvas, TreeViewItem item)
        {
            var tag = item.Tag;

            ClearModel(canvas);

            if (tag != null)
            {
                LoadModelFromTag(canvas, tag);
            }
            else
            {
                canvas.Tag = new History(new Stack<string>(), new Stack<string>());

                GenerateGrid(false);
            }
        }

        private void LoadModelFromTag(Canvas canvas, object tag)
        {
            var diagram = tag as Diagram;

            var model = diagram.Item1;
            var history = diagram.Item2;

            canvas.Tag = history;

            ParseDiagramModel(model, canvas, CurrentOptions.CurrentPathGrid, 0, 0, false, true, false, true);
        }

        private void StoreModel(Canvas canvas, TreeViewItem item)
        {
            var uid = item.Uid;
            var model = Editor.GenerateModel(canvas, uid, CurrentOptions.CurrentProperties);

            if (item != null)
            {
                item.Tag = new Diagram(model, canvas != null ? canvas.Tag as History : null);
            }
        }
        
        private TreeViewItem CreateSolutionItem(string uid)
        {
            var solution = new TreeViewItem();

            solution.Header = ModelConstants.TagHeaderSolution;
            solution.ContextMenu = CurrentOptions.CurrentResources["SolutionContextMenuKey"] as ContextMenu;
            solution.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = 0; // there is only one solution allowed

                solution.Uid = ModelConstants.TagHeaderSolution + ModelConstants.TagNameSeparator + id.ToString();
                counter.SolutionCount = id++;
            }
            else
            {
                solution.Uid = uid;
            }

            solution.IsExpanded = true;

            return solution;
        }

        private TreeViewItem CreateProjectItem(string uid)
        {
            var project = new TreeViewItem();

            project.Header = ModelConstants.TagHeaderProject;
            project.ContextMenu = CurrentOptions.CurrentResources["ProjectContextMenuKey"] as ContextMenu;
            project.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = counter.ProjectCount;

                project.Uid = ModelConstants.TagHeaderProject + ModelConstants.TagNameSeparator + id.ToString();
                counter.ProjectCount++;
            }
            else
            {
                project.Uid = uid;
            }

            project.IsExpanded = true;

            return project;
        }

        private TreeViewItem CreateDiagramItem(string uid)
        {
            var diagram = new TreeViewItem();

            diagram.Header = ModelConstants.TagHeaderDiagram;
            diagram.ContextMenu = CurrentOptions.CurrentResources["DiagramContextMenuKey"] as ContextMenu;
            diagram.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = counter.DiagramCount;

                diagram.Uid = ModelConstants.TagHeaderDiagram + ModelConstants.TagNameSeparator + id.ToString();
                counter.DiagramCount++;
            }
            else
            {
                diagram.Uid = uid;
            }

            return diagram;
        }

        public void AddProject(TreeViewItem solution)
        {
            var project = CreateProjectItem(null);

            solution.Items.Add(project);

            System.Diagnostics.Debug.Print("Added project: {0} to solution: {1}", project.Uid, solution.Uid);
        }

        public void AddDiagram(TreeViewItem project, bool select)
        {
            var diagram = CreateDiagramItem(null);

            project.Items.Add(diagram);

            StoreModel(null, diagram);

            if (select == true)
            {
                diagram.IsSelected = true;
            }

            System.Diagnostics.Debug.Print("Added diagram: {0} to project: {1}", diagram.Uid, project.Uid);
        }

        private void DeleteSolution(TreeViewItem solution)
        {
            var tree = solution.Parent as TreeView;
            var projects = solution.Items.Cast<TreeViewItem>().ToList();

            foreach (var project in projects)
            {
                var diagrams = project.Items.Cast<TreeViewItem>().ToList();

                foreach (var diagram in diagrams)
                {
                    project.Items.Remove(diagram);
                }

                solution.Items.Remove(project);
            }

            tree.Items.Remove(solution);
        }

        public void DeleteProject(TreeViewItem project)
        {
            var solution = project.Parent as TreeViewItem;
            var diagrams = project.Items.Cast<TreeViewItem>().ToList();

            foreach (var diagram in diagrams)
            {
                project.Items.Remove(diagram);
            }

            solution.Items.Remove(project);
        }

        public void DeleteDiagram(TreeViewItem diagram)
        {
            var project = diagram.Parent as TreeViewItem;

            project.Items.Remove(diagram);
        }

        public void UpdateSelectedDiagramModel()
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;
            var item = tree.SelectedItem as TreeViewItem;

            if (item != null)
            {
                string uid = item.Uid;
                bool isDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var model = Editor.GenerateModel(canvas, uid, CurrentOptions.CurrentProperties);

                    item.Tag = new Diagram(model, canvas.Tag as History);
                }
            }
        }

        public Tuple<string, IEnumerable<string>> GenerateSolutionModel(string fileName)
        {
            var tree = CurrentOptions.CurrentTree;
            var tagFileName = CurrentOptions.TagFileName;

            return Editor.GenerateSolutionModel(tree, fileName, tagFileName);
        }

        private void OpenSolution(TreeView tree, TreeSolution solution)
        {
            ClearSolution();

            ParseSolution(tree, solution);
        }

        private void ParseSolution(TreeView tree, TreeSolution solution)
        {
            var counter = CurrentOptions.Counter;

            // create solution
            string tagFileName = null;

            string solutionName = solution.Item1;
            tagFileName = solution.Item2;
            var projects = solution.Item3.Reverse();

            LoadTags(tagFileName);

            //System.Diagnostics.Debug.Print("Solution: {0}", name);

            var solutionItem = CreateSolutionItem(solutionName);
            tree.Items.Add(solutionItem);

            ParseProjects(projects, counter, solutionItem);
        }

        private IEnumerable<TreeViewItem> ParseProjects(IEnumerable<TreeProject> projects, IdCounter counter, TreeViewItem solutionItem)
        {
            var diagramList = new List<TreeViewItem>();

            // create projects
            foreach (var project in projects)
            {
                string projectName = project.Item1;
                var diagrams = project.Item2.Reverse();

                //System.Diagnostics.Debug.Print("Project: {0}", name);

                // create project
                var projectItem = CreateProjectItem(projectName);
                solutionItem.Items.Add(projectItem);

                // update project count
                int projectId = int.Parse(projectName.Split(ModelConstants.TagNameSeparator)[1]);
                counter.ProjectCount = Math.Max(counter.ProjectCount, projectId + 1);

                ParseDiagrams(counter, diagrams, projectItem, diagramList);
            }

            var firstDiagram = diagramList.FirstOrDefault();
            if (firstDiagram != null)
            {
                firstDiagram.IsSelected = true;
            }

            return diagramList;
        }

        private void ParseDiagrams(IdCounter counter, IEnumerable<TreeDiagram> diagrams, TreeViewItem projectItem, List<TreeViewItem> diagramList)
        {
            // create diagrams
            foreach (var diagram in diagrams)
            {
                ParseDiagram(counter, diagram, projectItem, diagramList);
            }
        }

        private void ParseDiagram(IdCounter counter, TreeDiagram diagram, TreeViewItem projectItem, List<TreeViewItem> diagramList)
        {
            var sb = new StringBuilder();

            // create diagram model
            var lines = diagram.Reverse();
            var firstLine = lines.First()
                .Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            string diagramName = firstLine.Length >= 1 ? firstLine[1] : null;

            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            string model = sb.ToString();

            //System.Diagnostics.Debug.Print(model);

            var diagramItem = CreateDiagramItem(diagramName);
            diagramItem.Tag = new Diagram(model, null);

            projectItem.Items.Add(diagramItem);

            diagramList.Add(diagramItem);

            // update diagram count
            int diagramId = int.Parse(diagramName.Split(ModelConstants.TagNameSeparator)[1]);
            counter.DiagramCount = Math.Max(counter.DiagramCount, diagramId + 1);
        }

        private void ClearSolution()
        {
            var tree = CurrentOptions.CurrentTree;

            // clear solution tree
            ClearSolutionTree(tree);

            // reset counter
            CurrentOptions.Counter.ResetAll();

            ResetTags();

            ResetSelectedList();

            ElementThumb.SetItems(CurrentOptions.CurrentCanvas, null);
        }

        private void ClearSolutionTree(TreeView tree)
        {
            var items = tree.Items.Cast<TreeViewItem>().ToList();

            foreach (var item in items)
            {
                DeleteSolution(item);
            }
        }

        public void NewSolution()
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;

            ClearModel(canvas);

            ClearSolution();

            CreateDefaultSolution(tree);
        }

        public void CreateDefaultSolution(TreeView tree)
        {
            var solutionItem = CreateSolutionItem(null);
            tree.Items.Add(solutionItem);

            var projectItem = CreateProjectItem(null);
            solutionItem.Items.Add(projectItem);

            var diagramItem = CreateDiagramItem(null);
            projectItem.Items.Add(diagramItem);

            diagramItem.IsSelected = true;
        }

        #endregion

        #region Tags

        public void OpenTags()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Tags"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                var tags = Editor.OpenTags(tagFileName);

                CurrentOptions.TagFileName = tagFileName;
                CurrentOptions.Tags = tags;
            }
        }

        public void SaveTags()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Tags",
                FileName = CurrentOptions.TagFileName == null ? "tags" : System.IO.Path.GetFileName(CurrentOptions.TagFileName)
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                Editor.ExportTags(tagFileName, CurrentOptions.Tags);

                CurrentOptions.TagFileName = tagFileName;
            }
        }

        public void ImportTags()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Tags"
            };

            if (CurrentOptions.Tags == null)
            {
                CurrentOptions.Tags = new List<object>();
            }

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                Editor.ImportTags(tagFileName, CurrentOptions.Tags, true);
            }
        }

        public void ExportTags()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Export Tags",
                FileName = "tags"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                Editor.ExportTags(tagFileName, CurrentOptions.Tags);
            }
        }

        private void UpdateTags()
        {
            string tagFileName = CurrentOptions.TagFileName;
            var tags = CurrentOptions.Tags;

            if (tagFileName != null && tags != null)
            {
                Editor.ExportTags(tagFileName, tags);
            }
            else if (tagFileName == null && tags != null)
            {
                SaveTags();
            }
        }

        private void LoadTags(string tagFileName)
        {
            // load tags
            if (tagFileName != null)
            {
                CurrentOptions.TagFileName = tagFileName;

                try
                {
                    var tags = Editor.OpenTags(tagFileName);

                    CurrentOptions.Tags = tags;

                    ElementThumb.SetItems(CurrentOptions.CurrentCanvas, tags);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Failed to load tags from file: {0}, error: {1}", tagFileName, ex.Message);
                }
            }
        }

        private void ResetTags()
        {
            if (CurrentOptions.Tags != null)
            {
                CurrentOptions.Tags.Clear();
                CurrentOptions.Tags = null;
            }

            CurrentOptions.TagFileName = null;
        }

        #endregion
    }

    #endregion
}
