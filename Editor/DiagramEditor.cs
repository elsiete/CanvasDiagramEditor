// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Controls;
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

    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    
    #endregion

    #region DiagramCreator

    public class DiagramEditor
    {
        #region Fields

        public DiagramEditorOptions CurrentOptions = null;
        public Action UpdateDiagramProperties { get; set; }

        public LinkedList<IElement> SelectedThumbList = null;
        public LinkedListNode<IElement> CurrentThumbNode = null;

        public WpfDiagramCreator WpfCreator { get; set; }

        #endregion

        #region Constructor

        public DiagramEditor()
        {
            InitializeWpfCreator();
        }

        public void InitializeWpfCreator()
        {
            WpfCreator = new WpfDiagramCreator();

            WpfCreator.SetThumbEvents = (thumb) =>
            {
                this.SetThumbEvents(thumb);
            };

            WpfCreator.SetElementPosition = (element, left, top, snap) =>
            {
                this.SetElementPosition(element, left, top, snap);
            };

            WpfCreator.GetTags = () =>
            {
                return this.CurrentOptions.Tags;
            };

            WpfCreator.GetCounter = () =>
            {
                return this.CurrentOptions.Counter;
            };
        }

        #endregion

        #region Model

        public TreeSolution ParseDiagramModel(string model,
            ICanvas canvas, 
            Path path,
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

            WpfCreator.ParserCanvas = canvas;
            WpfCreator.ParserPath = path;

            var result = parser.Parse(model, WpfCreator, parseOptions);

            CurrentOptions.Counter = parseOptions.Counter;
            CurrentOptions.CurrentProperties = parseOptions.Properties;

            WpfCreator.ParserCanvas = null;
            WpfCreator.ParserPath = null;

            return result;
        }

        public void ClearModel(ICanvas canvas)
        {
            canvas.Clear();

            CurrentOptions.Counter.ResetDiagram();
        }

        public void ResetThumbTags(ICanvas canvas)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>().Where(x => x.GetTag() != null);
            var selectedThumbs = thumbs.Where(x => x.GetSelected());

            if (selectedThumbs.Count() > 0)
            {
                // reset selected tags
                foreach(var thumb in selectedThumbs)
                {
                    thumb.SetData(null);
                }
            }
            else
            {
                // reset all tags
                foreach(var thumb in thumbs)
                {
                    thumb.SetData(null);
                }
            }
        }

        public string GenerateModelFromSelected(DiagramCanvas canvas)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var elements = Editor.GetSelectedElements(canvas);

            string model = Editor.GenerateModel(elements.Cast<IElement>());

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModelFromSelected() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return model;
        }

        #endregion

        #region Wire Connections

        private static Tuple<double, double> GetPinPosition(IElement root, FrameworkElement pin)
        {
            // get root position in canvas
            double rx = root.GetX();
            double ry = root.GetY();

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

        private void CreateConnection(ICanvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            CurrentOptions.CurrentRoot = GetPinTemplatedParent(pin) as IElement;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            var position = GetPinPosition(CurrentOptions.CurrentRoot, pin);
            double x = position.Item1;
            double y = position.Item2;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreateConnection(canvas, x, y);
        }

        private ILine CreateConnection(ICanvas canvas, double x, double y)
        {
            ILine result = null;

            var rootTag = CurrentOptions.CurrentRoot.GetTag();
            if (rootTag == null)
            {
                CurrentOptions.CurrentRoot.SetTag(new Selection(false, new List<MapWire>()));
            }

            var selection = CurrentOptions.CurrentRoot.GetTag() as Selection;
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

        private LineEx CreateFirstConnection(ICanvas canvas, double x, double y, List<MapWire> tuples)
        {
            // update IsStartIO
            string rootUid = CurrentOptions.CurrentRoot.GetUid();

            bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) 
                || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            var line = WpfCreator.CreateWire(x, y, x, y,
                false, false,
                startIsIO, false,
                CurrentOptions.Counter.WireCount) as LineEx;

            CurrentOptions.Counter.WireCount += 1;
            CurrentOptions.CurrentLine = line;

            // update connections
            var tuple = new MapWire(CurrentOptions.CurrentLine, CurrentOptions.CurrentRoot, null);
            tuples.Add(tuple);

            canvas.Add(CurrentOptions.CurrentLine);

            // line Tag is start root element
            if (CurrentOptions.CurrentLine != null || 
                !(CurrentOptions.CurrentLine is LineEx))
            {
                CurrentOptions.CurrentLine.SetTag(CurrentOptions.CurrentRoot);
            }

            return line;
        }

        private ILine CreateSecondConnection(double x, double y, List<MapWire> tuples)
        {
            var margin = CurrentOptions.CurrentLine.GetMargin();

            CurrentOptions.CurrentLine.SetX2(x - margin.Left);
            CurrentOptions.CurrentLine.SetY2(y - margin.Top);

            // update IsEndIO flag
            string rootUid = CurrentOptions.CurrentRoot.GetUid();

            bool endIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) ||
                StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            CurrentOptions.CurrentLine.SetEndIO(endIsIO);

            // update connections
            var tuple = new MapWire(CurrentOptions.CurrentLine, null, CurrentOptions.CurrentRoot);
            tuples.Add(tuple);

            // line Tag is start root element
            var line = CurrentOptions.CurrentLine;
            if (line != null)
            {
                var lineTag = line.GetTag();

                if (lineTag != null)
                {
                    // line Tag is start root element
                    var start = lineTag as IElement;
                    if (start != null)
                    {
                        // line Tag is Tuple of start & end root element
                        // this Tag is used to find all connected elements
                        line.SetTag(new Tuple<object, object>(start, CurrentOptions.CurrentRoot));
                    }
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

        private bool CreateWireSplit(ICanvas canvas, ILine line, IPoint point)
        {
            if (CurrentOptions.CurrentLine == null)
            {
                AddToHistory(canvas, true);
            }

            // create split pin
            var splitPin = InsertPin(canvas, point);
            CurrentOptions.CurrentRoot = splitPin;

            // connect current line to split pin
            double x = CurrentOptions.CurrentRoot.GetX();
            double y = CurrentOptions.CurrentRoot.GetY();

            CreateConnection(canvas, x, y);

            // remove original hit tested line
            canvas.Remove(line);

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

        private void CreateSplitConnections(ICanvas canvas, 
            ILine line, IElement splitPin, 
            double x, double y, 
            Connections connections)
        {
            var c1 = connections[0];
            var c2 = connections[1];
            var map1 = c1.Item2.FirstOrDefault();
            var map2 = c2.Item2.FirstOrDefault();
            var startRoot = (map1.Item2 != null ? map1.Item2 : map2.Item2) as IElement;
            var endRoot = (map1.Item3 != null ? map1.Item3 : map2.Item3) as IElement;
            var location = GetLineExStartAndEnd(map1, map2);

            //System.Diagnostics.Debug.Print("c1: {0}", c1.Item1.Uid);
            //System.Diagnostics.Debug.Print("c2: {0}", c2.Item1.Uid);
            //System.Diagnostics.Debug.Print("startRoot: {0}", startRoot.Uid);
            //System.Diagnostics.Debug.Print("endRoot: {0}", endRoot.Uid);

            if (location.Item1 != null && location.Item2 != null)
            {
                PointEx start = location.Item1;
                PointEx end = location.Item2;
                double x1 = start.X;
                double y1 = start.Y;
                double x2 = x1 + end.X;
                double y2 = y1 + end.Y;
                bool isStartVisible = line.GetStartVisible();
                bool isEndVisible = line.GetEndVisible();
                bool isStartIO = line.GetStartIO();
                bool isEndIO = line.GetEndIO();

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

                startLine.SetStartVisible(isStartVisible);
                startLine.SetStartIO(isStartIO);
                endLine.SetEndVisible(isEndVisible);
                endLine.SetEndIO(isEndIO);
            }
            else
            {
                throw new InvalidOperationException("LineEx should have corrent location info for Start and End.");
            }
        }

        #endregion

        #region Insert

        public IElement InsertPin(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreatePin(point.X, point.Y, CurrentOptions.Counter.PinCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.PinCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertInput(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateInput(point.X, point.Y, CurrentOptions.Counter.InputCount, -1, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.InputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOutput(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateOutput(point.X, point.Y, CurrentOptions.Counter.OutputCount, -1, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.OutputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertAndGate(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateAndGate(point.X, point.Y, CurrentOptions.Counter.AndGateCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.AndGateCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOrGate(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateOrGate(point.X, point.Y, CurrentOptions.Counter.OrGateCount, CurrentOptions.EnableSnap) as ElementThumb;
            CurrentOptions.Counter.OrGateCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertLast(ICanvas canvas, string type, IPoint point)
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

        public static string GenerateGrid(double originX, double originY, double width, double height, double size)
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

        public static void GenerateGrid(Path path, double originX, double originY, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            string grid = GenerateGrid(originX, originY, width, height, size);

            path.Data = Geometry.Parse(grid);

            sw.Stop();
            //System.Diagnostics.Debug.Print("GenerateGrid() in {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public static void SetDiagramSize(ICanvas canvas, double width, double height)
        {
            canvas.SetWidth(width);
            canvas.SetHeight(height);
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

        private History GetHistory(ICanvas canvas)
        {
            var canvasTag = canvas.GetTag();
            if (canvasTag == null)
            {
                canvasTag = new History(new Stack<string>(), new Stack<string>());
                canvas.SetTag(canvasTag);
            }

            var tuple = canvasTag as History;

            return tuple;
        }

        public string AddToHistory(ICanvas canvas, bool resetSelectedList)
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

        private void RollbackUndoHistory(ICanvas canvas)
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

        private void RollbackRedoHistory(ICanvas canvas)
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

        public void ClearHistory(ICanvas canvas)
        {
            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            undoHistory.Clear();
            redoHistory.Clear();
        }

        private void Undo(ICanvas canvas, Path path, bool pushRedo)
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

        private void Redo(ICanvas canvas, Path path, bool pushUndo)
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

        public List<string> GetDiagramModelHistory(DiagramCanvas canvas)
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

        public void SetElementPosition(IElement element, double left, double top, bool snap)
        {
            element.SetX(SnapOffsetX(left, snap));
            element.SetY(SnapOffsetY(top, snap));
        }

        private void MoveRoot(IElement element, double dX, double dY, bool snap)
        {
            double left = element.GetX() + dX;
            double top = element.GetY() + dY;

            SetElementPosition(element, left, top, snap);

            MoveLines(element, dX, dY, snap);
        }

        private void MoveLines(IElement element, double dX, double dY, bool snap)
        {
            if (element != null && element.GetTag() != null)
            {
                var selection = element.GetTag() as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                {
                    MoveLine(dX, dY, snap, tuple);
                }
            }
        }

        private void MoveLine(double dX, double dY, bool snap, MapWire tuple)
        {
            var line = tuple.Item1 as ILine;
            var start = tuple.Item2;
            var end = tuple.Item3;

            if (start != null)
            {
                var margin = line.GetMargin();
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
                    line.SetX2(line.GetX2() + (left - x));
                    line.SetY2(line.GetY2() + (top - y));
                    line.SetMargin(new MarginEx(0, x, 0, y));
                }
            }

            if (end != null)
            {
                double left = line.GetX2();
                double top = line.GetY2();
                double x = 0.0;
                double y = 0.0;

                x = SnapX(left + dX, snap);
                y = SnapY(top + dY, snap);

                line.SetX2(x);
                line.SetY2(y);
            }
        }

        private Tuple<PointEx, PointEx> GetLineExStartAndEnd(MapWire map1, MapWire map2)
        {
            var line1 = map1.Item1 as ILine;
            var start1 = map1.Item2;
            var end1 = map1.Item3;

            var line2 = map2.Item1 as ILine;
            var start2 = map2.Item2;
            var end2 = map2.Item3;

            PointEx startPoint = null;
            PointEx endPoint = null;

            if (start1 != null)
            {
                var margin = line1.GetMargin();
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new PointEx(left, top);
            }

            if (end1 != null)
            {
                double left = line1.GetX2();
                double top = line1.GetY2();

                endPoint = new PointEx(left, top);
            }

            if (start2 != null)
            {
                var margin = line2.GetMargin();
                double left = margin.Left;
                double top = margin.Top;

                startPoint = new PointEx(left, top);
            }

            if (end2 != null)
            {
                double left = line2.GetX2();
                double top = line2.GetY2();

                endPoint = new PointEx(left, top);
            }

            return new Tuple<PointEx, PointEx>(startPoint, endPoint);
        }

        public void MoveSelectedElements(ICanvas canvas, double dX, double dY, bool snap)
        {
            // move all selected elements
            var thumbs = canvas.GetElements().OfType<IThumb>().Where(x => x.GetSelected());

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        public void MoveLeft(ICanvas canvas)
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

        public void MoveRight(ICanvas canvas)
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

        public void MoveUp(ICanvas canvas)
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

        public void MoveDown(ICanvas canvas)
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

        public void Drag(ICanvas canvas, IThumb element, double dX, double dY)
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

        public void DragStart(ICanvas canvas, IThumb element)
        {
            AddToHistory(canvas, false);

            if (element.GetSelected() == true)
            {
                CurrentOptions.MoveAllSelected = true;
            }
            else
            {
                CurrentOptions.MoveAllSelected = false;

                // select
                element.SetSelected(true);
            }
        }

        public void DragEnd(ICanvas canvas, IThumb element)
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
                    element.SetSelected(false);

                    MoveRoot(element, 0.0, 0.0, CurrentOptions.EnableSnap);
                }
            }
            else
            {
                if (CurrentOptions.MoveAllSelected != true)
                {
                    // de-select
                    element.SetSelected(false);
                }
            }

            CurrentOptions.MoveAllSelected = false;
        }

        #endregion

        #region Thumb Events

        private void SetThumbEvents(ElementThumb thumb)
        {
            thumb.DragDelta += (sender, e) =>
            {
                var canvas = CurrentOptions.CurrentCanvas;
                var element = sender as IThumb;

                double dX = e.HorizontalChange;
                double dY = e.VerticalChange;

                Drag(canvas, element, dX, dY);
            };

            thumb.DragStarted += (sender, e) =>
            {
                var canvas = CurrentOptions.CurrentCanvas;
                var element = sender as IThumb;

                DragStart(canvas, element);
            };

            thumb.DragCompleted += (sender, e) =>
            {
                var canvas = CurrentOptions.CurrentCanvas;
                var element = sender as IThumb;

                DragEnd(canvas, element);
            };
        }

        #endregion

        #region Delete

        private void DeleteElement(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        private void DeleteElement(ICanvas canvas, IElement element)
        {
            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                DeleteWire(canvas, line);
            }
            else
            {
                canvas.Remove(element);
            }
        }

        private static void DeleteWire(ICanvas canvas, ILine line)
        {
            canvas.Remove(line);

            RemoveWireConnections(canvas, line);

            DeleteEmptyPins(canvas);
        }

        private static void DeleteEmptyPins(ICanvas canvas)
        {
            var pins = FindEmptyPins(canvas);

            // remove empty pins
            foreach (var pin in pins)
            {
                canvas.Remove(pin);
            }
        }

        private static List<IElement> FindEmptyPins(ICanvas canvas)
        {
            var pins = new List<IElement>();

            foreach (var element in canvas.GetElements())
            {
                string uid = element.GetUid();

                if (IsElementPin(uid))
                {
                    var elementTag = element.GetTag();
                    if (elementTag != null)
                    {
                        var selection = elementTag as Selection;
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

        private static Connections RemoveWireConnections(ICanvas canvas, ILine line)
        {
            var connections = new Connections();

            foreach (var element in canvas.GetElements())
            {
                var elementTag = element.GetTag();
                if (elementTag  != null && !(element is ILine))
                {
                    RemoveWireConnections(line, connections, element);
                }
            }

            return connections;
        }

        private static void RemoveWireConnections(ILine line, Connections connections, IElement element)
        {
            var selection = element.GetTag() as Selection;
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

        private static void CreateMapWire(ILine line, List<MapWire> tuples, List<MapWire> map)
        {
            foreach (var tuple in tuples)
            {
                var _line = tuple.Item1 as ILine;

                if (StringUtil.Compare(_line.GetUid(), line.GetUid()))
                {
                    map.Add(tuple);
                }
            }
        }

        public void Delete(ICanvas canvas, IPoint point)
        {
            AddToHistory(canvas, true);

            DeleteElement(canvas, point);

            CurrentOptions.SkipLeftClick = false;
        }

        #endregion

        #region Invert Wire Start or End

        public LineEx FindLineEx(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as FrameworkElement;
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

        public void ToggleWireStart(ICanvas canvas, IPoint point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas, false);

                line.SetStartVisible(line.GetStartVisible() == true ? false : true);

                CurrentOptions.SkipLeftClick = false;
            }
        }

        public void ToggleWireEnd(ICanvas canvas, IPoint point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas, false);

                line.SetEndVisible(line.GetEndVisible() == true ? false : true);

                CurrentOptions.SkipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save

        private void OpenDiagram(string fileName, ICanvas canvas, Path path)
        {
            string diagram = Editor.OpenModel(fileName);

            AddToHistory(canvas, true);

            ClearModel(canvas);
            ParseDiagramModel(diagram, canvas, path, 0, 0, false, true, false, true);
        }

        private void SaveDiagram(string fileName, ICanvas canvas)
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

            var model = GenerateSolutionModel(fileName, false).Item1;
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

        public void ResetThumbTags()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            AddToHistory(canvas, true);

            ResetThumbTags(canvas);
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

        private void ExportDiagramToDxf(string fileName, DiagramCanvas canvas, bool shortenStart, bool shortenEnd)
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

        public void Paste(IPoint point)
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

        public void Delete(ICanvas canvas, IEnumerable<IElement> elements)
        {
            AddToHistory(canvas, true);

            DeleteThumbsAnndLines(canvas, elements);
        }

        private void DeleteThumbsAnndLines(ICanvas canvas, IEnumerable<IElement> elements)
        {
            // delete thumbs & lines
            foreach (var element in elements)
            {
                DeleteElement(canvas, element);
            }
        }

        #endregion

        #region Selection

        public IEnumerable<IElement> GetSelectedElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetSelectedElements(canvas);
        }

        public IEnumerable<IElement> GetSelectedThumbElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetSelectedThumbElements(canvas);
        }

        public IEnumerable<IElement> GetThumbElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Editor.GetThumbElements(canvas);
        }

        public IEnumerable<IElement> GetAllElements()
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
                SelectedThumbList = new LinkedList<IElement>(elements);

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
                SelectedThumbList = new LinkedList<IElement>(elements);

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

        public void SelectOneElement(IElement element, bool deselect)
        {
            if (element != null)
            {
                if (deselect == true)
                {
                    DeselectAll();
                    element.SetSelected(true);
                }
                else
                {
                    bool isSelected = element.GetSelected();
                    element.SetSelected(!isSelected);
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

        public void HandleLeftDown(ICanvas canvas, IPoint point)
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

        private void CreateCanvasPinConnection(ICanvas canvas, IPoint point)
        {
            var root = InsertPin(canvas, point);

            CurrentOptions.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

            double x = CurrentOptions.CurrentRoot.GetX();
            double y = CurrentOptions.CurrentRoot.GetY();

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

        public bool HandlePreviewLeftDown(ICanvas canvas, IPoint point, FrameworkElement pin)
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
                var element = GetElementAtPoint(canvas, point);

                System.Diagnostics.Debug.Print("Split wire: {0}", element == null ? "<null>" : element.GetUid());

                if (CanSplitWire(element))
                {
                    return CreateWireSplit(canvas, element as ILine, point);
                }
            }

            if (CanToggleLine())
            {
                ToggleLineSelection(canvas, point);
            }

            return false;
        }

        private bool CanSplitWire(IElement element)
        {
            if (element == null)
            {
                return false;
            }

            var elementUid = element.GetUid();
            var lineUid = CurrentOptions.CurrentLine.GetUid();

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

        private void ToggleLineSelection(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;

            if (element != null)
            {
                Editor.ToggleLineSelection(element);
            }
            else
            {
                Editor.SetLinesSelection(canvas, false);
            }
        }

        private bool CanToggleLine()
        {
            return CurrentOptions.CurrentRoot == null &&
                CurrentOptions.CurrentLine == null &&
                Keyboard.Modifiers != ModifierKeys.Control;
        }

        private IElement GetElementAtPoint(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0)
                .Where(x => StringUtil.Compare(CurrentOptions.CurrentLine.GetUid(), x.GetUid()) == false)
                .FirstOrDefault() as IElement;

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

        public void HandleMove(ICanvas canvas, IPoint point)
        {
            if (CanMoveCurrentLine())
            {
                var margin = CurrentOptions.CurrentLine.GetMargin();
                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (CurrentOptions.CurrentLine.GetX2() != x)
                {
                    //this._line.X2 = SnapX(x);
                    CurrentOptions.CurrentLine.SetX2(x);
                }

                if (CurrentOptions.CurrentLine.GetY2() != y)
                {
                    //this._line.Y2 = SnapY(y);
                    CurrentOptions.CurrentLine.SetY2(y);
                }
            }
        }

        private bool CanMoveCurrentLine()
        {
            return CurrentOptions.CurrentRoot != null &&
                CurrentOptions.CurrentLine != null;
        }

        public bool HandleRightDown(ICanvas canvas, Path path)
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

        private void RemoveCurrentLine(ICanvas canvas)
        {
            var selection = CurrentOptions.CurrentRoot.GetTag() as Selection;
            var tuples = selection.Item2;

            var last = tuples.LastOrDefault();
            tuples.Remove(last);

            canvas.Remove(CurrentOptions.CurrentLine);
        }

        #endregion

        #region TreeView Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as SolutionTreeViewItem;
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
            var selected = tree.SelectedItem as SolutionTreeViewItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.Uid, ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.Parent as SolutionTreeViewItem;
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

                        var item = items[index] as SolutionTreeViewItem;
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

        private static void SelectPreviousParentTreeItem(SolutionTreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as SolutionTreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex > 0)
            {
                SelectLastItemInPreviousProject(parentParent, parentIndex);
            }
        }

        private static void SelectLastItemInPreviousProject(SolutionTreeViewItem parentParent, int parentIndex)
        {
            // get previous project
            int index = parentIndex - 1;
            var parentProject = (parentParent.Items[index] as SolutionTreeViewItem);

            // select last item in previous project
            if (parentProject.Items.Count > 0)
            {
                var item = (parentProject.Items[parentProject.Items.Count - 1] as SolutionTreeViewItem);
                item.IsSelected = true;
                item.BringIntoView();
            }
        }

        public void SelectNextTreeItem(bool selectParent)
        {
            var tree = CurrentOptions.CurrentTree;

            // get current diagram
            var selected = tree.SelectedItem as SolutionTreeViewItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.Uid, ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.Parent as SolutionTreeViewItem;
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

                        var item = items[index] as SolutionTreeViewItem;
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

        private static void SelectNextParentTreeItem(SolutionTreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as SolutionTreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex < parentCount - 1)
            {
                SelectFirstItemInNextProject(parentParent, parentIndex);
            }
        }

        private static void SelectFirstItemInNextProject(SolutionTreeViewItem parentParent, int parentIndex)
        {
            // get next project
            int index = parentIndex + 1;
            var parentProject = (parentParent.Items[index] as SolutionTreeViewItem);

            // select first item in next project
            if (parentProject.Items.Count > 0)
            {
                var item = (parentProject.Items[0] as SolutionTreeViewItem);
                item.IsSelected = true;
                item.BringIntoView();
            }
        }

        public bool SwitchItems(DiagramCanvas canvas, SolutionTreeViewItem oldItem, SolutionTreeViewItem newItem)
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

        private void LoadModel(DiagramCanvas canvas, SolutionTreeViewItem item)
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

        private void LoadModelFromTag(DiagramCanvas canvas, object tag)
        {
            var diagram = tag as Diagram;

            var model = diagram.Item1;
            var history = diagram.Item2;

            canvas.Tag = history;

            ParseDiagramModel(model, canvas, CurrentOptions.CurrentPathGrid, 0, 0, false, true, false, true);
        }

        private void StoreModel(DiagramCanvas canvas, SolutionTreeViewItem item)
        {
            var uid = item.Uid;
            var model = Editor.GenerateModel(canvas, uid, CurrentOptions.CurrentProperties);

            if (item != null)
            {
                item.Tag = new Diagram(model, canvas != null ? canvas.Tag as History : null);
            }
        }
        
        private SolutionTreeViewItem CreateSolutionItem(string uid)
        {
            var solution = new SolutionTreeViewItem();

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

        private SolutionTreeViewItem CreateProjectItem(string uid)
        {
            var project = new SolutionTreeViewItem();

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

        private SolutionTreeViewItem CreateDiagramItem(string uid)
        {
            var diagram = new SolutionTreeViewItem();

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

        public void AddProject(SolutionTreeViewItem solution)
        {
            var project = CreateProjectItem(null);

            solution.Items.Add(project);

            System.Diagnostics.Debug.Print("Added project: {0} to solution: {1}", project.Uid, solution.Uid);
        }

        public void AddDiagram(SolutionTreeViewItem project, bool select)
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

        private void DeleteSolution(SolutionTreeViewItem solution)
        {
            var tree = solution.Parent as TreeView;
            var projects = solution.Items.Cast<SolutionTreeViewItem>().ToList();

            foreach (var project in projects)
            {
                var diagrams = project.Items.Cast<SolutionTreeViewItem>().ToList();

                foreach (var diagram in diagrams)
                {
                    project.Items.Remove(diagram);
                }

                solution.Items.Remove(project);
            }

            tree.Items.Remove(solution);
        }

        public void DeleteProject(SolutionTreeViewItem project)
        {
            var solution = project.Parent as SolutionTreeViewItem;
            var diagrams = project.Items.Cast<SolutionTreeViewItem>().ToList();

            foreach (var diagram in diagrams)
            {
                project.Items.Remove(diagram);
            }

            solution.Items.Remove(project);
        }

        public void DeleteDiagram(SolutionTreeViewItem diagram)
        {
            var project = diagram.Parent as SolutionTreeViewItem;

            project.Items.Remove(diagram);
        }

        public void UpdateSelectedDiagramModel()
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;
            var item = tree.SelectedItem as SolutionTreeViewItem;

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

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath, UriKind.RelativeOrAbsolute);
            Uri toUri = new Uri(toPath, UriKind.RelativeOrAbsolute);

            if (fromUri.IsAbsoluteUri == true)
            {
                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            }
            else
            {
                return null;
            }
        }

        public static Tuple<string, IEnumerable<string>> GenerateSolutionModel(TreeView tree, 
            string fileName, 
            string tagFileName,
            bool includeHistory)
        {
            var models = new List<string>();

            var solution = tree.Items.Cast<SolutionTreeViewItem>().First();
            var projects = solution.Items.Cast<SolutionTreeViewItem>();
            string line = null;

            var sb = new StringBuilder();

            // tags file path is relative to solution file path
            if (tagFileName != null && fileName != null)
            {
                string relativePath = MakeRelativePath(tagFileName, fileName);
                string onlyFileName = System.IO.Path.GetFileName(tagFileName);

                if (relativePath != null)
                {
                    tagFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(relativePath), 
                        onlyFileName);
                }
            }

            // Solution
            line = string.Format("{0}{1}{2}{1}{3}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                solution.Uid,
                tagFileName);

            sb.AppendLine(line);

            //System.Diagnostics.Debug.Print(line);

            foreach (var project in projects)
            {
                var diagrams = project.Items.Cast<SolutionTreeViewItem>();

                // Project
                line = string.Format("{0}{1}{2}",
                    ModelConstants.PrefixRoot,
                    ModelConstants.ArgumentSeparator,
                    project.Uid);

                sb.AppendLine(line);

                //System.Diagnostics.Debug.Print(line);

                foreach (var diagram in diagrams)
                {
                    // Diagram

                    //line = string.Format("{0}{1}{2}",
                    //    ModelConstants.PrefixRootElement,
                    //    ModelConstants.ArgumentSeparator,
                    //    diagram.Uid);
                    //sb.AppendLine(line);
                    //System.Diagnostics.Debug.Print(line);

                    // Diagram Elements
                    if (diagram.Tag != null)
                    {
                        var _diagram = diagram.Tag as Diagram;

                        var model = _diagram.Item1;
                        var history = _diagram.Item2;

                        models.Add(model);
                        sb.Append(model);

                        if (includeHistory == true)
                        {
                            var undoHistory = history.Item1;
                            var redoHistory = history.Item2;

                            foreach(var m in undoHistory)
                            {
                                models.Add(m);
                                sb.Append(m);
                            }
                        }     
                    }
                }
            }

            var tuple = new Tuple<string, IEnumerable<string>>(sb.ToString(), models);

            return tuple;
        }

        public Tuple<string, IEnumerable<string>> GenerateSolutionModel(string fileName, 
            bool includeHistory)
        {
            var tree = CurrentOptions.CurrentTree;
            var tagFileName = CurrentOptions.TagFileName;

            return GenerateSolutionModel(tree, fileName, tagFileName, includeHistory);
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

        private IEnumerable<SolutionTreeViewItem> ParseProjects(IEnumerable<TreeProject> projects, 
            IdCounter counter, 
            SolutionTreeViewItem solutionItem)
        {
            var diagramList = new List<SolutionTreeViewItem>();

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

        private void ParseDiagrams(IdCounter counter, IEnumerable<TreeDiagram> diagrams, 
            SolutionTreeViewItem projectItem, 
            List<SolutionTreeViewItem> diagramList)
        {
            // create diagrams
            foreach (var diagram in diagrams)
            {
                ParseDiagram(counter, diagram, projectItem, diagramList);
            }
        }

        private void ParseDiagram(IdCounter counter, 
            TreeDiagram diagram, 
            SolutionTreeViewItem projectItem, 
            List<SolutionTreeViewItem> diagramList)
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
            var items = tree.Items.Cast<SolutionTreeViewItem>().ToList();

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
