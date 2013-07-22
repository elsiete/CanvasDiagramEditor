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
using CanvasDiagramEditor.Export;
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
    using TreeSolution = Tuple<string, Stack<Tuple<string, Stack<Stack<string>>>>>;

    #endregion

    #region DiagramEditor

    public class DiagramEditor : IDiagramCreator
    {
        #region Fields

        public DiagramEditorOptions options = null;
        private Canvas parserCanvas = null;
        private Path parserPath = null;

        #endregion

        #region Model

        public TreeSolution ParseDiagramModel(string model,
            Canvas canvas,
            Path path,
            double offsetX,
            double offsetY,
            bool appendIds,
            bool updateIds,
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
                Counter = options.Counter,
                Properties = options.CurrentProperties
            };

            parserCanvas = canvas;
            parserPath = path;

            var result = parser.Parse(model, this, parseOptions);

            options.Counter = parseOptions.Counter;
            options.CurrentProperties = parseOptions.Properties;

            parserCanvas = null;
            parserPath = null;

            return result;
        }

        public string GenerateDiagramModel(Canvas canvas, string uid)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            var elements = canvas != null ? canvas.Children.Cast<FrameworkElement>() : Enumerable.Empty<FrameworkElement>();
            var prop = options.CurrentProperties;

            string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                uid == null ? ModelConstants.TagHeaderDiagram : uid,
                prop.PageWidth, prop.PageHeight,
                prop.GridOriginX, prop.GridOriginY,
                prop.GridWidth, prop.GridHeight,
                prop.GridSize,
                prop.SnapX, prop.SnapY,
                prop.SnapOffsetX, prop.SnapOffsetY);

            diagram.AppendLine(header);
            //System.Diagnostics.Debug.Print(header);

            GenerateDiagramModel(diagram, elements);

            var result = diagram.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        public void ClearDiagramModel(Canvas canvas)
        {
            canvas.Children.Clear();

            options.Counter.ResetDiagram();
        }

        public string GenerateDiagramModelFromSelected(Canvas canvas)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            var elements = GetSelectedElements(canvas);

            GenerateDiagramModel(diagram, elements);

            var result = diagram.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModelFromSelected() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        private static void GenerateDiagramModel(StringBuilder diagram, IEnumerable<FrameworkElement> elements)
        {
            foreach (var element in elements)
            {
                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);

                if (element.Uid.StartsWith(ModelConstants.TagElementWire))
                {
                    var line = element as LineEx;
                    var margin = line.Margin;

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}{5}{7}{5}{8}{5}{9}{5}{10}",
                        element.Uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.X2 + margin.Left, line.Y2 + margin.Top,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        line.IsStartVisible, line.IsEndVisible,
                        line.IsStartIO, line.IsEndIO);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                        element.Uid,
                        x, y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }

                if (element.Tag != null)
                {
                    var selection = element.Tag as Selection;
                    var tuples = selection.Item2;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1 as LineEx;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        if (start != null)
                        {
                            // Start
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.Uid,
                                ModelConstants.WireStartType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            diagram.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.Uid,
                                ModelConstants.WireEndType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            diagram.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }
        }

        private static void AddElementsToCanvas(Canvas canvas, IEnumerable<FrameworkElement> elements, bool select)
        {
            foreach (var element in elements)
            {
                canvas.Children.Add(element);

                if (select == true)
                {
                    SelectionThumb.SetIsSelected(element, true);
                }
            }
        }

        public void InsertElements(IEnumerable<object> elements, bool select)
        {
            var canvas = parserCanvas;

            AddElementsToCanvas(canvas, elements.Cast<FrameworkElement>(), select);
        }

        public void UpdateCounter(IdCounter original, IdCounter counter)
        {
            original.PinCount = Math.Max(original.PinCount, counter.PinCount);
            original.WireCount = Math.Max(original.WireCount, counter.WireCount);
            original.InputCount = Math.Max(original.InputCount, counter.InputCount);
            original.OutputCount = Math.Max(original.OutputCount, counter.OutputCount);
            original.AndGateCount = Math.Max(original.AndGateCount, counter.AndGateCount);
            original.OrGateCount = Math.Max(original.OrGateCount, counter.OrGateCount);
        }

        public void UpdateConnections(IDictionary<string, MapWires> dict)
        {
            // update wire to element connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1 as FrameworkElement;
                var wires = item.Value.Item2;

                if (element.Tag == null)
                {
                    element.Tag = new Selection(false, new List<MapWire>());
                }

                if (wires.Count > 0)
                {
                    var selection = element.Tag as Selection;
                    var tuples = selection.Item2;

                    foreach (var wire in wires)
                    {
                        string _name = wire.Item1;
                        string _type = wire.Item2;

                        if (StringUtil.Compare(_type, ModelConstants.WireStartType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, element, null);

                                tuples.Add(mapWire);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire Start: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapTag(line, element, null);
                            //tuples.Add(_tuple);
                        }
                        else if (StringUtil.Compare(_type, ModelConstants.WireEndType))
                        {
                            MapWires mapWires = null;
                            if (dict.TryGetValue(_name, out mapWires) == true)
                            {
                                var line = mapWires.Item1;
                                var mapWire = new MapWire(line, null, element);

                                tuples.Add(mapWire);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Print("Failed to map wire End: {0}", _name);
                            }

                            //var line = dict[_name].Item1 as LineEx;
                            //var _tuple = new MapWire(line, null, element);
                            //tuples.Add(_tuple);
                        }
                    }
                }
            }
        }

        public void AppendIds(IEnumerable<object> elements)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements.Cast<FrameworkElement>())
            {
                string[] uid = element.Uid.Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = GetUpdatedId(options.Counter, type);

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

        private static int GetUpdatedId(IdCounter counter, string type)
        {
            int appendedId = -1;

            switch (type)
            {
                case ModelConstants.TagElementWire:
                    appendedId = counter.WireCount;
                    counter.WireCount += 1;
                    break;
                case ModelConstants.TagElementInput:
                    appendedId = counter.InputCount;
                    counter.InputCount += 1;
                    break;
                case ModelConstants.TagElementOutput:
                    appendedId = counter.OutputCount;
                    counter.OutputCount += 1;
                    break;
                case ModelConstants.TagElementAndGate:
                    appendedId = counter.AndGateCount;
                    counter.AndGateCount += 1;
                    break;
                case ModelConstants.TagElementOrGate:
                    appendedId = counter.OrGateCount;
                    counter.OrGateCount += 1;
                    break;
                case ModelConstants.TagElementPin:
                    appendedId = counter.PinCount;
                    counter.PinCount += 1;
                    break;
                default:
                    throw new Exception("Unknown element type.");
            }

            return appendedId;
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
                Snap(original, options.CurrentProperties.SnapX, options.CurrentProperties.SnapOffsetX) : original;
        }

        private double SnapOffsetY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.CurrentProperties.SnapY, options.CurrentProperties.SnapOffsetY) : original;
        }

        private double SnapX(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.CurrentProperties.SnapX) : original;
        }

        private double SnapY(double original, bool snap)
        {
            return snap == true ?
                Snap(original, options.CurrentProperties.SnapY) : original;
        }

        #endregion

        #region Grid

        public void GenerateGrid(Path path, double originX, double originY, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();

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

        public string AddToHistory(Canvas canvas)
        {
            if (options.EnableHistory != true)
                return null;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            var model = GenerateDiagramModel(canvas, null);

            undoHistory.Push(model);

            redoHistory.Clear();

            return model;
        }

        private void RollbackUndoHistory(Canvas canvas)
        {
            if (options.EnableHistory != true)
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
            if (options.EnableHistory != true)
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
            if (options.EnableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = GenerateDiagramModel(canvas, null);
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        private void Redo(Canvas canvas, Path path, bool pushUndo)
        {
            if (options.EnableHistory != true)
                return;

            var tuple = GetHistory(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = GenerateDiagramModel(canvas, null);
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();

            ClearDiagramModel(canvas);
            ParseDiagramModel(model, canvas, path, 0, 0, false, true, false, true);
        }

        public void Undo()
        {
            var canvas = options.CurrentCanvas;
            var path = options.CurrentPathGrid;

            this.Undo(canvas, path, true);
        }

        public void Redo()
        {
            var canvas = options.CurrentCanvas;
            var path = options.CurrentPathGrid;

            this.Redo(canvas, path, true);
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
            }
        }

        public void MoveSelectedElements(Canvas canvas, double dX, double dY, bool snap)
        {
            // move all selected elements
            var thumbs = canvas.Children.OfType<SelectionThumb>().Where(x => SelectionThumb.GetIsSelected(x));

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        #endregion

        #region Drag

        public void Drag(Canvas canvas, SelectionThumb element, double dX, double dY)
        {
            bool snap = (options.SnapOnRelease == true && options.EnableSnap == true) ? false : options.EnableSnap;

            if (options.MoveAllSelected == true)
            {
                MoveSelectedElements(canvas, dX, dY, snap);
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

            if (SelectionThumb.GetIsSelected(element) == true)
            {
                options.MoveAllSelected = true;
            }
            else
            {
                options.MoveAllSelected = false;

                // select
                SelectionThumb.SetIsSelected(element, true);
            }
        }

        public void DragEnd(Canvas canvas, SelectionThumb element)
        {
            if (options.SnapOnRelease == true && options.EnableSnap == true)
            {
                if (options.MoveAllSelected == true)
                {
                    MoveSelectedElements(canvas, 0.0, 0.0, options.EnableSnap);
                }
                else
                {
                    // move only selected element

                    // deselect
                    SelectionThumb.SetIsSelected(element, false);

                    MoveRoot(element, 0.0, 0.0, options.EnableSnap);
                }
            }
            else
            {
                if (options.MoveAllSelected != true)
                {
                    // de-select
                    SelectionThumb.SetIsSelected(element, false);
                }
            }

            options.MoveAllSelected = false;
        }

        #endregion

        #region Thumb Events

        private void RootElement_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = options.CurrentCanvas;
            var element = sender as SelectionThumb;

            double dX = e.HorizontalChange;
            double dY = e.VerticalChange;

            Drag(canvas, element, dX, dY);
        }

        private void RootElement_DragStarted(object sender, DragStartedEventArgs e)
        {
            var canvas = options.CurrentCanvas;
            var element = sender as SelectionThumb;

            DragStart(canvas, element);
        }

        private void RootElement_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var canvas = options.CurrentCanvas;
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

        public object CreatePin(double x, double y, int id, bool snap)
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

        public object CreateInput(double x, double y, int id, bool snap)
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

        public object CreateOutput(double x, double y, int id, bool snap)
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

        public object CreateAndGate(double x, double y, int id, bool snap)
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

        public object CreateOrGate(double x, double y, int id, bool snap)
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

        public object CreateDiagram(DiagramProperties properties)
        {
            var canvas = parserCanvas;
            var path = parserPath;

            GenerateGrid(path,
                properties.GridOriginX,
                properties.GridOriginY,
                properties.GridWidth,
                properties.GridHeight,
                properties.GridSize);

            SetDiagramSize(canvas, properties.PageWidth, properties.PageHeight);

            return null;
        }

        private void CreatePinConnection(Canvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            var root =
                (
                    (pin.Parent as FrameworkElement).Parent as FrameworkElement
                ).TemplatedParent as FrameworkElement;

            options.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(options.CurrentRoot);
            double ry = Canvas.GetTop(options.CurrentRoot);
            double px = Canvas.GetLeft(pin);
            double py = Canvas.GetTop(pin);
            double x = rx + px;
            double y = ry + py;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            CreatePinConnection(canvas, x, y);
        }

        private LineEx CreatePinConnection(Canvas canvas, double x, double y)
        {
            LineEx result = null;

            if (options.CurrentRoot.Tag == null)
            {
                options.CurrentRoot.Tag = new Selection(false, new List<MapWire>());
            }

            var selection = options.CurrentRoot.Tag as Selection;
            var tuples = selection.Item2;

            if (options.CurrentLine == null)
            {
                // update IsStartIO
                string rootUid = options.CurrentRoot.Uid;
                bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

                var line = CreateWire(x, y, x, y, 
                    false, false,
                    startIsIO, false,
                    options.Counter.WireCount) as LineEx;

                options.Counter.WireCount += 1;

                options.CurrentLine = line;

                // update connections
                var tuple = new MapWire(options.CurrentLine, options.CurrentRoot, null);
                tuples.Add(tuple);

                canvas.Children.Add(options.CurrentLine);

                result = line;
            }
            else
            {
                var margin = options.CurrentLine.Margin;

                options.CurrentLine.X2 = x - margin.Left;
                options.CurrentLine.Y2 = y - margin.Top;

                // update IsEndIO flag
                string rootUid = options.CurrentRoot.Uid;
                bool endIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

                options.CurrentLine.IsEndIO = endIsIO;

                // update connections
                var tuple = new MapWire(options.CurrentLine, null, options.CurrentRoot);
                tuples.Add(tuple);

                result = options.CurrentLine;

                options.CurrentLine = null;
                options.CurrentRoot = null;
            }

            return result;
        }

        #endregion

        #region Insert

        public FrameworkElement InsertPin(Canvas canvas, Point point)
        {
            var thumb = CreatePin(point.X, point.Y, options.Counter.PinCount, options.EnableSnap) as SelectionThumb;
            options.Counter.PinCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertInput(Canvas canvas, Point point)
        {
            var thumb = CreateInput(point.X, point.Y, options.Counter.InputCount, options.EnableSnap) as SelectionThumb;
            options.Counter.InputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOutput(Canvas canvas, Point point)
        {
            var thumb = CreateOutput(point.X, point.Y, options.Counter.OutputCount, options.EnableSnap) as SelectionThumb;
            options.Counter.OutputCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertAndGate(Canvas canvas, Point point)
        {
            var thumb = CreateAndGate(point.X, point.Y, options.Counter.AndGateCount, options.EnableSnap) as SelectionThumb;
            options.Counter.AndGateCount += 1;

            canvas.Children.Add(thumb);

            return thumb;
        }

        public FrameworkElement InsertOrGate(Canvas canvas, Point point)
        {
            var thumb = CreateOrGate(point.X, point.Y, options.Counter.OrGateCount, options.EnableSnap) as SelectionThumb;
            options.Counter.OrGateCount += 1;

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

        public FrameworkElement HitTest(Canvas canvas, ref Point point)
        {
            var selectedElements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
            {
                RadiusX = options.HitTestRadiusX,
                RadiusY = options.HitTestRadiusY,
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

        private static void DeleteWire(Canvas canvas, LineEx line)
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
                    StringUtil.StartsWith(uid, ModelConstants.TagElementPin))
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

        private static void RemoveWireConnections(Canvas canvas, LineEx line)
        {
            foreach (var child in canvas.Children)
            {
                var _element = child as FrameworkElement;

                if (_element.Tag != null)
                {
                    var selection = _element.Tag as Selection;
                    var tuples = selection.Item2;

                    var remove = new List<MapWire>();

                    foreach (var tuple in tuples)
                    {
                        var _line = tuple.Item1 as LineEx;

                        if (StringUtil.Compare(_line.Uid, line.Uid))
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

            options.SkipLeftClick = false;
        }

        #endregion

        #region Invert

        public LineEx FindLineEx(Canvas canvas, Point point)
        {
            var element = HitTest(canvas, ref point);
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

        public void ToggleStart(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas);

                line.IsStartVisible = line.IsStartVisible == true ? false : true;

                options.SkipLeftClick = false;
            }
        }

        public void ToggleEnd(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas);

                line.IsEndVisible = line.IsEndVisible == true ? false : true;

                options.SkipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save

        private void SaveModel(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        private void SaveDiagram(string fileName, Canvas canvas)
        {
            string model = GenerateDiagramModel(canvas, null);

            SaveModel(fileName, model);
        }

        private void OpenDiagram(string fileName, Canvas canvas, Path path)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                AddToHistory(canvas);

                ClearDiagramModel(canvas);
                ParseDiagramModel(diagram, canvas, path, 0, 0, false, true, false, true);
            }
        }

        private TreeSolution OpenSolution(string fileName)
        {
            TreeSolution solution = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                solution = ParseDiagramModel(diagram, null, null, 0, 0, false, false, false, false);
            }

            return solution;
        }

        public string ImportModel(string fileName)
        {
            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                return diagram;
            }
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
                var canvas = options.CurrentCanvas;
                var path = options.CurrentPathGrid;

                this.OpenDiagram(dlg.FileName, canvas, path);
            }
        }

        public TreeSolution OpenSolution()
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
                var canvas = options.CurrentCanvas;

                ClearDiagramModel(canvas);

                solution = OpenSolution(dlg.FileName);
            }

            return solution;
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
                var canvas = options.CurrentCanvas;

                this.SaveDiagram(dlg.FileName, canvas);
            }
        }

        public void SaveSolution(string model)
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
                this.SaveModel(dlg.FileName, model);
            }
        }

        public void ExportDiagram()
        {
            //Export(new MsoWordExport(), false);
            Export(new OpenXmlExport(), false);
        }

        public void ExportDiagramHistory()
        {
            //Export(new MsoWordExport(), true);
            Export(new OpenXmlExport(), true);
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
                var canvas = options.CurrentCanvas;
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

            var currentDiagram = GenerateDiagramModel(canvas, null);

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
            var model = GenerateDiagramModel(options.CurrentCanvas, null);

            var canvas = new Canvas()
            {
                Background = Brushes.Black,
                Width = options.CurrentCanvas.Width,
                Height = options.CurrentCanvas.Height
            };

            Path path = new Path();

            ParseDiagramModel(model, canvas, path, 0, 0, false, false, false, true);

            Visual visual = canvas;

            PrintDialog dlg = new PrintDialog();
            dlg.PrintVisual(visual, "diagram");
        }

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
                var diagram = this.ImportModel(dlg.FileName);

                return diagram;
            }

            return null;
        }

        public void Insert(string diagram, double offsetX, double offsetY)
        {
            var canvas = options.CurrentCanvas;
            var path = options.CurrentPathGrid;

            AddToHistory(canvas);

            DeselectAll();
            ParseDiagramModel(diagram, canvas, path, offsetX, offsetY, true, true, true, true);
        }

        public void Clear()
        {
            var canvas = options.CurrentCanvas;

            AddToHistory(canvas);

            ClearDiagramModel(canvas);
        }

        public string Generate()
        {
            var canvas = options.CurrentCanvas;

            var diagram = GenerateDiagramModel(canvas, null);

            return diagram;
        }

        public string GenerateFromSelected()
        {
            var canvas = options.CurrentCanvas;

            var diagram = GenerateDiagramModelFromSelected(canvas);

            return diagram;
        }

        #endregion

        #region Edit

        public void Cut()
        {
            var canvas = options.CurrentCanvas;

            string model = GenerateDiagramModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = GenerateDiagramModel(canvas, null);

                var elements = GetAllElements(canvas);

                Delete(canvas, elements);
            }
            else
            {
                Delete();
            }

            Clipboard.SetText(model);
        }

        public void Copy()
        {
            var canvas = options.CurrentCanvas;

            string model = GenerateDiagramModelFromSelected(canvas);

            if (model.Length == 0)
            {
                model = GenerateDiagramModel(canvas, null);
            }

            Clipboard.SetText(model);
        }

        public void Paste(Point point)
        {
            var model = Clipboard.GetText();

            if (model != null || model.Length > 0)
            {
                Insert(model, point.X, point.Y);
            }
        }

        public void Delete()
        {
            var canvas = options.CurrentCanvas;
            var elements = GetSelectedElements(canvas);

            Delete(canvas, elements);
        }

        public void Delete(Canvas canvas, IEnumerable<FrameworkElement> elements)
        {
            AddToHistory(canvas);

            // delete thumbs & lines

            foreach (var element in elements)
            {
                DeleteElement(canvas, element);
            }
        }

        public static IEnumerable<FrameworkElement> GetSelectedElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get selected thumbs
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {
                if (SelectionThumb.GetIsSelected(thumb) == true)
                {
                    elements.Add(thumb);
                }
            }

            // get selected lines
            var lines = canvas.Children.OfType<LineEx>();

            foreach (var line in lines)
            {
                if (SelectionThumb.GetIsSelected(line) == true)
                {
                    elements.Add(line);
                }
            }

            return elements;
        }

        public static IEnumerable<FrameworkElement> GetAllElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get all thumbs
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {

                elements.Add(thumb);
            }

            // get all lines
            var lines = canvas.Children.OfType<LineEx>();

            foreach (var line in lines)
            {
                elements.Add(line);
            }

            return elements;
        }

        public static void SetSelectionThumbsSelection(Canvas canvas, bool isSelected)
        {
            var thumbs = canvas.Children.OfType<SelectionThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                SelectionThumb.SetIsSelected(thumb, isSelected);
            }
        }

        public static void SetLinesSelection(Canvas canvas, bool isSelected)
        {
            var lines = canvas.Children.OfType<LineEx>();

            // deselect all lines
            foreach (var line in lines)
            {
                SelectionThumb.SetIsSelected(line, isSelected);
            }
        }

        public void SelectAll()
        {
            var canvas = options.CurrentCanvas;

            SetSelectionThumbsSelection(canvas, true);
            SetLinesSelection(canvas, true);
        }

        public void DeselectAll()
        {
            var canvas = options.CurrentCanvas;

            SetSelectionThumbsSelection(canvas, false);
            SetLinesSelection(canvas, false);
        }

        #endregion

        #region Handlers

        public void HandleLeftDown(Canvas canvas, Point point)
        {
            if (options.CurrentRoot != null && options.CurrentLine != null)
            {
                var root = InsertPin(canvas, point);

                options.CurrentRoot = root;

                //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(options.CurrentRoot);
                double ry = Canvas.GetTop(options.CurrentRoot);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                CreatePinConnection(canvas, x, y);

                options.CurrentRoot = root;

                CreatePinConnection(canvas, x, y);
            }
            else if (options.EnableInsertLast == true)
            {
                AddToHistory(canvas);

                InsertLast(canvas, options.LastInsert, point);
            }
        }

        private static void ToggleLineSelection(FrameworkElement element)
        {
            string uid = element.Uid;

            //System.Diagnostics.Debug.Print("ToggleLineSelection: {0}, uid: {1}, parent: {2}",
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is LineEx && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as LineEx;

                // select/deselect line
                bool isSelected = SelectionThumb.GetIsSelected(line);
                SelectionThumb.SetIsSelected(line, isSelected ? false : true);
            }
        }

        public bool HandlePreviewLeftDown(Canvas canvas, Point point, FrameworkElement pin)
        {
            if (options.CurrentRoot == null &&
                options.CurrentLine == null &&
                Keyboard.Modifiers != ModifierKeys.Control)
            {
                var element = HitTest(canvas, ref point);
                if (element != null)
                {
                    ToggleLineSelection(element);
                }
                else
                {
                    SetLinesSelection(canvas, false);
                }
            }

            if (pin != null &&
                (!StringUtil.Compare(pin.Name, ResourceConstants.StandalonePinName) || Keyboard.Modifiers == ModifierKeys.Control))
            {
                if (options.CurrentLine == null)
                    AddToHistory(canvas);

                CreatePinConnection(canvas, pin);

                return true;
            }

            return false;
        }

        public void HandleMove(Canvas canvas, Point point)
        {
            if (options.CurrentRoot != null && options.CurrentLine != null)
            {
                var margin = options.CurrentLine.Margin;

                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (options.CurrentLine.X2 != x)
                {
                    //this._line.X2 = SnapX(x);
                    options.CurrentLine.X2 = x;
                }

                if (options.CurrentLine.Y2 != y)
                {
                    //this._line.Y2 = SnapY(y);
                    options.CurrentLine.Y2 = y;
                }
            }
        }

        public bool HandleRightDown(Canvas canvas, Path path)
        {
            if (options.CurrentRoot != null && options.CurrentLine != null)
            {
                if (options.EnableHistory == true)
                {
                    Undo(canvas, path, false);
                }
                else
                {
                    var selection = options.CurrentRoot.Tag as Selection;
                    var tuples = selection.Item2;

                    var last = tuples.LastOrDefault();
                    tuples.Remove(last);

                    canvas.Children.Remove(options.CurrentLine);
                }

                options.CurrentLine = null;
                options.CurrentRoot = null;

                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion
}
