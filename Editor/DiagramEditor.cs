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

    #endregion

    #region DiagramEditor

    public class DiagramEditor : IDiagramCreator
    {
        #region Constructor

        public DiagramEditor()
        {
        }

        #endregion

        #region Fields

        public DiagramEditorOptions CurrentOptions = null;
        public Action UpdateDiagramProperties { get; set; }

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
                Counter = CurrentOptions.Counter,
                Properties = CurrentOptions.CurrentProperties
            };

            parserCanvas = canvas;
            parserPath = path;

            var result = parser.Parse(model, this, parseOptions);

            CurrentOptions.Counter = parseOptions.Counter;
            CurrentOptions.CurrentProperties = parseOptions.Properties;

            parserCanvas = null;
            parserPath = null;

            return result;
        }

        public string GenerateDiagramModel(Canvas canvas, string uid)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var diagram = new StringBuilder();

            var elements = canvas != null ? canvas.Children.Cast<FrameworkElement>() : Enumerable.Empty<FrameworkElement>();
            var prop = CurrentOptions.CurrentProperties;

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

            CurrentOptions.Counter.ResetDiagram();
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
                string uid = element.Uid;

                if (StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
                {
                    var line = element as LineEx;
                    var margin = line.Margin;

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}{5}{7}{5}{8}{5}{9}{5}{10}",
                        uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.X2 + margin.Left, line.Y2 + margin.Top,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        line.IsStartVisible, line.IsEndVisible,
                        line.IsStartIO, line.IsEndIO);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else if (StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput))
                {
                    var data = ElementThumb.GetData(element);
                    Tag tag = null;

                    if (data != null && data is Tag)
                    {
                        tag = data as Tag;
                    }

                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}{3}{5}",
                        uid,
                        x, 
                        y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        tag != null ? tag.Id : -1);

                    diagram.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                        uid,
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
                    ElementThumb.SetIsSelected(element, true);
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

                int appendedId = GetUpdatedId(CurrentOptions.Counter, type);

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
                AddToHistory(canvas);
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

        public string AddToHistory(Canvas canvas)
        {
            if (CurrentOptions.EnableHistory != true)
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
            var thumbs = canvas.Children.OfType<ElementThumb>().Where(x => ElementThumb.GetIsSelected(x));

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        public void MoveLeft(Canvas canvas)
        {
            AddToHistory(canvas);

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
            AddToHistory(canvas);

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
            AddToHistory(canvas);

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
            AddToHistory(canvas);

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
            AddToHistory(canvas);

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

        #endregion

        #region Create

        private void SetThumbEvents(ElementThumb thumb)
        {
            thumb.DragDelta += this.RootElement_DragDelta;
            thumb.DragStarted += this.RootElement_DragStarted;
            thumb.DragCompleted += this.RootElement_DragCompleted;
        }

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
            var canvas = parserCanvas;
            var path = parserPath;

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

        private void CreatePinConnection(Canvas canvas, FrameworkElement pin)
        {
            if (pin == null)
                return;

            var root =
                (
                    (pin.Parent as FrameworkElement).Parent as FrameworkElement
                ).TemplatedParent as FrameworkElement;

            CurrentOptions.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            double rx = Canvas.GetLeft(CurrentOptions.CurrentRoot);
            double ry = Canvas.GetTop(CurrentOptions.CurrentRoot);
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

            if (CurrentOptions.CurrentRoot.Tag == null)
            {
                CurrentOptions.CurrentRoot.Tag = new Selection(false, new List<MapWire>());
            }

            var selection = CurrentOptions.CurrentRoot.Tag as Selection;
            var tuples = selection.Item2;

            if (CurrentOptions.CurrentLine == null)
            {
                // update IsStartIO
                string rootUid = CurrentOptions.CurrentRoot.Uid;
                bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

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

                result = line;
            }
            else
            {
                var margin = CurrentOptions.CurrentLine.Margin;

                CurrentOptions.CurrentLine.X2 = x - margin.Left;
                CurrentOptions.CurrentLine.Y2 = y - margin.Top;

                // update IsEndIO flag
                string rootUid = CurrentOptions.CurrentRoot.Uid;
                bool endIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

                CurrentOptions.CurrentLine.IsEndIO = endIsIO;

                // update connections
                var tuple = new MapWire(CurrentOptions.CurrentLine, null, CurrentOptions.CurrentRoot);
                tuples.Add(tuple);

                result = CurrentOptions.CurrentLine;

                CurrentOptions.CurrentLine = null;
                CurrentOptions.CurrentRoot = null;
            }

            return result;
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

            CurrentOptions.SkipLeftClick = false;
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

                CurrentOptions.SkipLeftClick = false;
            }
        }

        public void ToggleEnd(Canvas canvas, Point point)
        {
            var line = FindLineEx(canvas, point);

            if (line != null)
            {
                AddToHistory(canvas);

                line.IsEndVisible = line.IsEndVisible == true ? false : true;

                CurrentOptions.SkipLeftClick = false;
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

        private TreeSolution OpenTreeSolutionModel(string fileName)
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
                var canvas = CurrentOptions.CurrentCanvas;
                var path = CurrentOptions.CurrentPathGrid;

                this.OpenDiagram(dlg.FileName, canvas, path);
            }
        }

        public TreeSolution OpenTreeSolutionModel()
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

                ClearDiagramModel(canvas);

                solution = OpenTreeSolutionModel(dlg.FileName);
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
                var canvas = CurrentOptions.CurrentCanvas;

                this.SaveDiagram(dlg.FileName, canvas);
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

        private void UpdateTags()
        {
            string tagFileName = CurrentOptions.TagFileName;
            var tags = CurrentOptions.Tags;

            if (tagFileName != null && tags != null)
            {
                ExportTags(tagFileName, tags);
            }
            else if (tagFileName == null && tags != null)
            {
                SaveTags();
            }
        }

        private void SaveSolution(string fileName)
        {
            var model = GenerateSolution(fileName).Item1;
            this.SaveModel(fileName, model);
        }

        private List<string> GetDiagramHistory(Canvas canvas)
        {
            List<string> diagrams = null;

            var currentDiagram = GenerateDiagramModel(canvas, null);

            var history = GetHistory(canvas);
            var undoHistory = history.Item1;
            var redoHistory = history.Item2;

            diagrams = new List<string>(undoHistory.Reverse());

            diagrams.Add(currentDiagram);

            return diagrams;
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
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            AddToHistory(canvas);

            DeselectAll();
            ParseDiagramModel(diagram, canvas, path, offsetX, offsetY, true, true, true, true);
        }

        public void Clear()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            AddToHistory(canvas);

            ClearDiagramModel(canvas);
        }

        public string Generate()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = GenerateDiagramModel(canvas, null);

            return diagram;
        }

        public string GenerateFromSelected()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = GenerateDiagramModelFromSelected(canvas);

            return diagram;
        }

        #endregion

        #region Edit

        public void Cut()
        {
            var canvas = CurrentOptions.CurrentCanvas;

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
            var canvas = CurrentOptions.CurrentCanvas;

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
            var canvas = CurrentOptions.CurrentCanvas;
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
            var thumbs = canvas.Children.OfType<ElementThumb>();

            foreach (var thumb in thumbs)
            {
                if (ElementThumb.GetIsSelected(thumb) == true)
                {
                    elements.Add(thumb);
                }
            }

            // get selected lines
            var lines = canvas.Children.OfType<LineEx>();

            foreach (var line in lines)
            {
                if (ElementThumb.GetIsSelected(line) == true)
                {
                    elements.Add(line);
                }
            }

            return elements;
        }

        public IEnumerable<FrameworkElement> GetSelectedElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return GetSelectedElements(canvas);
        }

        public static IEnumerable<FrameworkElement> GetAllElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get all thumbs
            var thumbs = canvas.Children.OfType<ElementThumb>();

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

        public IEnumerable<FrameworkElement> GetAllElements()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return GetAllElements(canvas);
        }

        public static void SetElementThumbsSelection(Canvas canvas, bool isSelected)
        {
            var thumbs = canvas.Children.OfType<ElementThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                ElementThumb.SetIsSelected(thumb, isSelected);
            }
        }

        public static void SetLinesSelection(Canvas canvas, bool isSelected)
        {
            var lines = canvas.Children.OfType<LineEx>();

            // deselect all lines
            foreach (var line in lines)
            {
                ElementThumb.SetIsSelected(line, isSelected);
            }
        }

        public void SelectAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            SetElementThumbsSelection(canvas, true);
            SetLinesSelection(canvas, true);
        }

        public void DeselectAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            SetElementThumbsSelection(canvas, false);
            SetLinesSelection(canvas, false);
        }

        #endregion

        #region Handlers

        public void HandleLeftDown(Canvas canvas, Point point)
        {
            if (CurrentOptions.CurrentRoot != null && CurrentOptions.CurrentLine != null)
            {
                var root = InsertPin(canvas, point);

                CurrentOptions.CurrentRoot = root;

                //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

                double rx = Canvas.GetLeft(CurrentOptions.CurrentRoot);
                double ry = Canvas.GetTop(CurrentOptions.CurrentRoot);
                double px = 0;
                double py = 0;
                double x = rx + px;
                double y = ry + py;

                //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

                CreatePinConnection(canvas, x, y);

                CurrentOptions.CurrentRoot = root;

                CreatePinConnection(canvas, x, y);
            }
            else if (CurrentOptions.EnableInsertLast == true)
            {
                AddToHistory(canvas);

                InsertLast(canvas, CurrentOptions.LastInsert, point);
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
                bool isSelected = ElementThumb.GetIsSelected(line);
                ElementThumb.SetIsSelected(line, isSelected ? false : true);
            }
        }

        public bool HandlePreviewLeftDown(Canvas canvas, Point point, FrameworkElement pin)
        {
            if (CurrentOptions.CurrentRoot == null &&
                CurrentOptions.CurrentLine == null &&
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
                if (CurrentOptions.CurrentLine == null)
                    AddToHistory(canvas);

                CreatePinConnection(canvas, pin);

                return true;
            }

            return false;
        }

        public void HandleMove(Canvas canvas, Point point)
        {
            if (CurrentOptions.CurrentRoot != null && CurrentOptions.CurrentLine != null)
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

        public bool HandleRightDown(Canvas canvas, Path path)
        {
            if (CurrentOptions.CurrentRoot != null && CurrentOptions.CurrentLine != null)
            {
                if (CurrentOptions.EnableHistory == true)
                {
                    Undo(canvas, path, false);
                }
                else
                {
                    var selection = CurrentOptions.CurrentRoot.Tag as Selection;
                    var tuples = selection.Item2;

                    var last = tuples.LastOrDefault();
                    tuples.Remove(last);

                    canvas.Children.Remove(CurrentOptions.CurrentLine);
                }

                CurrentOptions.CurrentLine = null;
                CurrentOptions.CurrentRoot = null;

                return true;
            }

            return false;
        }

        #endregion

        #region TreeView Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                //item.Focus();
                item.BringIntoView();

                e.Handled = true;
            }
        }

        #endregion

        #region Solution

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

            ClearDiagramModel(canvas);

            if (tag != null)
            {
                var diagram = tag as Diagram;

                var model = diagram.Item1;
                var history = diagram.Item2;

                canvas.Tag = history;

                ParseDiagramModel(model, canvas, CurrentOptions.CurrentPathGrid, 0, 0, false, true, false, true);
            }
            else
            {
                canvas.Tag = new History(new Stack<string>(), new Stack<string>());

                GenerateGrid(false);
            }
        }

        private void StoreModel(Canvas canvas, TreeViewItem item)
        {
            var uid = item.Uid;
            var model = GenerateDiagramModel(canvas, uid);

            if (item != null)
            {
                item.Tag = new Diagram(model, canvas != null ? canvas.Tag as History : null);
            }
        }
        
        private TreeViewItem CreateSolutionItem(string uid)
        {
            var solution = new TreeViewItem();

            solution.Header = "Solution";
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

            project.Header = "Project";
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

            diagram.Header = "Diagram";
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

        private void UpdateSelectedDiagramModel()
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
                    var model = GenerateDiagramModel(canvas, uid);

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

        public Tuple<string, IEnumerable<string>> GenerateSolution(string fileName)
        {
            var models = new List<string>();

            var tree = CurrentOptions.CurrentTree;
            var solution = tree.Items.Cast<TreeViewItem>().First();
            var projects = solution.Items.Cast<TreeViewItem>();
            string line = null;

            var sb = new StringBuilder();

            // update current diagram
            UpdateSelectedDiagramModel();

            // tags file path is relative to solution file path
            var tagFileName = CurrentOptions.TagFileName;

            if (tagFileName != null && fileName != null)
            {
                string relativePath = MakeRelativePath(tagFileName, fileName);
                string onlyFileName =  System.IO.Path.GetFileName(tagFileName);

                if (relativePath != null)
                {
                    tagFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(relativePath), onlyFileName);
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
                var diagrams = project.Items.Cast<TreeViewItem>();

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
                    }
                }
            }

            var tuple = new Tuple<string, IEnumerable<string>>(sb.ToString(), models);

            return tuple;
        }

        public void OpenSolution()
        {
            var tree = CurrentOptions.CurrentTree;
            var solution = OpenTreeSolutionModel();

            if (solution != null)
            {
                TreeViewItem firstDiagram = null;
                bool haveFirstDiagram = false;

                ClearSolution();

                var counter = CurrentOptions.Counter;

                // create solution
                string name = null;
                string tagFileName = null;
                int id = -1;

                name = solution.Item1;
                tagFileName = solution.Item2;
                var projects = solution.Item3.Reverse();

                // load tags
                if (tagFileName != null)
                {
                    CurrentOptions.TagFileName = tagFileName;

                    try
                    {
                        var tags = OpenTags(tagFileName);

                        CurrentOptions.Tags = tags;

                        ElementThumb.SetItems(CurrentOptions.CurrentCanvas, tags);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print("Failed to load tags from file: {0}, error: {1}", tagFileName, ex.Message);
                    }
                }
                
                //System.Diagnostics.Debug.Print("Solution: {0}", name);

                var solutionItem = CreateSolutionItem(name);
                tree.Items.Add(solutionItem);

                // create projects
                foreach (var project in projects)
                {
                    name = project.Item1;
                    var diagrams = project.Item2.Reverse();

                    //System.Diagnostics.Debug.Print("Project: {0}", name);

                    // create project
                    var projectItem = CreateProjectItem(name);
                    solutionItem.Items.Add(projectItem);

                    // update project count
                    id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                    counter.ProjectCount = Math.Max(counter.ProjectCount, id + 1);

                    // create diagrams
                    foreach (var diagram in diagrams)
                    {
                        var lines = diagram.Reverse();
                        var sb = new StringBuilder();
                        string model = null;

                        var firstLine = lines.First().Split(new char[] { ModelConstants.ArgumentSeparator, '\t', ' ' },
                            StringSplitOptions.RemoveEmptyEntries);

                        name = firstLine.Length >= 1 ? firstLine[1] : null;

                        // create diagram
                        foreach (var line in lines)
                        {
                            sb.AppendLine(line);
                        }

                        model = sb.ToString();

                        //System.Diagnostics.Debug.Print(model);

                        var diagramItem = CreateDiagramItem(name);

                        diagramItem.Tag = new Diagram(model, null);

                        projectItem.Items.Add(diagramItem);

                        // check for first diagram
                        if (haveFirstDiagram == false)
                        {
                            firstDiagram = diagramItem;
                            haveFirstDiagram = true;
                        }

                        // update diagram count
                        id = int.Parse(name.Split(ModelConstants.TagNameSeparator)[1]);
                        counter.DiagramCount = Math.Max(counter.DiagramCount, id + 1);
                    }
                }

                // select first diagram in tree
                if (haveFirstDiagram == true)
                {
                    firstDiagram.IsSelected = true;
                }
            }
        }

        private void ClearSolution()
        {
            var tree = CurrentOptions.CurrentTree;

            // clear solution tree
            var items = tree.Items.Cast<TreeViewItem>().ToList();

            foreach (var item in items)
            {
                DeleteSolution(item);
            }

            // reset counter
            CurrentOptions.Counter.ResetAll();

            // reset tags
            if (CurrentOptions.Tags != null)
            {
                CurrentOptions.Tags.Clear();
                CurrentOptions.Tags = null;
            }

            CurrentOptions.TagFileName = null;

            ElementThumb.SetItems(CurrentOptions.CurrentCanvas, null);
        }

        public void NewSolution()
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;

            ClearDiagramModel(canvas);

            ClearSolution();

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

                var tags = OpenTags(tagFileName);

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

                ExportTags(tagFileName, CurrentOptions.Tags);

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

                ImportTags(tagFileName, CurrentOptions.Tags, true);
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

                ExportTags(tagFileName, CurrentOptions.Tags);
            }
        }

        public static List<object> OpenTags(string fileName)
        {
            var tags = new List<object>();

            ImportTags(fileName, tags, false);

            return tags;
        }

        public static void SaveTags(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        public static void ImportTags(string fileName, List<object> tags, bool appedIds)
        {
            int count = 0;
            if (appedIds == true)
            {
                count = tags.Count > 0 ? tags.Cast<Tag>().Max(x => x.Id) + 1 : 0;
            }

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string data = reader.ReadToEnd();

                var lines = data.Split(Environment.NewLine.ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var args = line.Split(new char[] { ModelConstants.ArgumentSeparator, '\t' },
                        StringSplitOptions.RemoveEmptyEntries);

                    int length = args.Length;

                    if (length == 5)
                    {
                        int id = -1;

                        if (appedIds == true)
                        {
                            id = count;
                            count = count + 1;
                        }
                        else
                        {
                            id = int.Parse(args[0]);
                        }

                        var tag = new Tag()
                        {
                            Id = id,
                            Designation = args[1],
                            Signal = args[2],
                            Condition = args[3],
                            Description = args[4]
                        };

                        tags.Add(tag);
                    }
                }
            }
        }
            
        public static void ExportTags(string fileName, List<object> tags)
        {
            var model = GenerateTags(tags);
            SaveTags(fileName, model);
        }

        public static string GenerateTags(List<object> tags)
        {
            string line = null;

            var sb = new StringBuilder();

            if (tags != null)
            {
                foreach (var tag in tags.Cast<Tag>())
                {
                    line = string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                        ModelConstants.ArgumentSeparator,
                        tag.Id,
                        tag.Designation,
                        tag.Signal,
                        tag.Condition,
                        tag.Description);

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        #endregion
    }

    #endregion
}
