// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace CanvasDiagram.Editor
{
    #region Aliases

    using MapPin = Tuple<string, string>;
    using MapWire = Tuple<object, object, object>;
    using MapWires = Tuple<object, List<Tuple<string, string>>>;
    using Selection = Tuple<bool, List<Tuple<object, object, object>>>;
    using UndoRedo = Tuple<Stack<string>, Stack<string>>;
    using Diagram = Tuple<string, Tuple<Stack<string>, Stack<string>>>;
    using TreeDiagram = Stack<string>;
    using TreeDiagrams = Stack<Stack<string>>;
    using TreeProject = Tuple<string, Stack<Stack<string>>>;
    using TreeProjects = Stack<Tuple<string, Stack<Stack<string>>>>;
    using TreeSolution = Tuple<string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
    using Position = Tuple<double, double>;
    using Connection = Tuple<IElement, List<Tuple<object, object, object>>>;
    using Connections = List<Tuple<IElement, List<Tuple<object, object, object>>>>;
    using Solution = Tuple<string, IEnumerable<string>>;

    #endregion

    #region Model

    public static class Model
    {
        #region Generate

        public static string Generate(IEnumerable<IElement> elements)
        {
            var sb = new StringBuilder();

            foreach (var element in elements)
            {
                double x = element.GetX();
                double y = element.GetY();
                string uid = element.GetUid();

                if (StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
                {
                    var line = element as ILine;
                    var margin = line.GetMargin();

                    string str = string.Format("{6}{5}{0}{5}{1}{5}{2}{5}{3}{5}{4}{5}{7}{5}{8}{5}{9}{5}{10}",
                        uid,
                        margin.Left, margin.Top, //line.X1, line.Y1,
                        line.GetX2() + margin.Left, line.GetY2() + margin.Top,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot,
                        line.GetStartVisible(), line.GetEndVisible(),
                        line.GetStartIO(), line.GetEndIO());

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else if (StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput))
                {
                    var data = element.GetData();
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

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }
                else
                {
                    string str = string.Format("{4}{3}{0}{3}{1}{3}{2}",
                        uid,
                        x, y,
                        ModelConstants.ArgumentSeparator,
                        ModelConstants.PrefixRoot);

                    sb.AppendLine("".PadLeft(4, ' ') + str);

                    //System.Diagnostics.Debug.Print(str);
                }

                var elementTag = element.GetTag();
                if (elementTag != null && !(element is ILine))
                {
                    var selection = elementTag as Selection;
                    var tuples = selection.Item2;

                    foreach (var tuple in tuples)
                    {
                        var line = tuple.Item1 as ILine;
                        var start = tuple.Item2;
                        var end = tuple.Item3;

                        if (start != null)
                        {
                            // Start
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.GetUid(),
                                ModelConstants.WireStartType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            sb.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                        else if (end != null)
                        {
                            // End
                            string str = string.Format("{3}{2}{0}{2}{1}",
                                line.GetUid(),
                                ModelConstants.WireEndType,
                                ModelConstants.ArgumentSeparator,
                                ModelConstants.PrefixChild);

                            sb.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static string GenerateDiagram(ICanvas canvas, string uid, DiagramProperties properties)
        {
            if (canvas == null)
            {
                return null;
            }

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();
            var elements = canvas.GetElements();

            string defaultUid = ModelConstants.TagHeaderDiagram + ModelConstants.TagNameSeparator + (-1).ToString();

            string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                uid == null ? defaultUid : uid,
                properties.PageWidth, properties.PageHeight,
                properties.GridOriginX, properties.GridOriginY,
                properties.GridWidth, properties.GridHeight,
                properties.GridSize,
                properties.SnapX, properties.SnapY,
                properties.SnapOffsetX, properties.SnapOffsetY);

            sb.AppendLine(header);
            //System.Diagnostics.Debug.Print(header);

            string model = Generate(elements);

            sb.Append(model);

            var result = sb.ToString();

            //sw.Stop();
            //System.Diagnostics.Debug.Print("GenerateDiagram() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        public static Solution GenerateSolution(ITree tree,
            string fileName,
            string tagFileName,
            bool includeHistory)
        {
            var models = new List<string>();
            var solution = tree.GetItems().First();
            var projects = solution.GetItems();

            string line = null;
            var sb = new StringBuilder();

            // tags file path is relative to solution file path
            if (tagFileName != null && fileName != null)
            {
                tagFileName = PathUtil.GetRelativeFileName(fileName, tagFileName);
            }

            // Solution
            line = string.Format("{0}{1}{2}{1}{3}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                solution.GetUid(),
                tagFileName);

            sb.AppendLine(line);

            //System.Diagnostics.Debug.Print(line);

            foreach (var project in projects)
            {
                var model = GenerateProject(project, models, includeHistory);
                sb.Append(model);
            }

            return new Solution(sb.ToString(), models);
        }

        public static string GenerateProject(ITreeItem project,
            List<string> models,
            bool includeHistory)
        {
            var diagrams = project.GetItems();

            string line = null;
            var sb = new StringBuilder();

            // Project
            line = string.Format("{0}{1}{2}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                project.GetUid());

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
                if (diagram.GetTag() != null)
                {
                    var _diagram = diagram.GetTag() as Diagram;

                    var model = _diagram.Item1;
                    var history = _diagram.Item2;

                    models.Add(model);
                    sb.Append(model);

                    if (includeHistory == true && history != null)
                    {
                        var undoHistory = history.Item1;
                        var redoHistory = history.Item2;

                        foreach (var m in undoHistory)
                        {
                            models.Add(m);
                            sb.Append(m);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Parse

        public static TreeSolution Parse(string model,
            ICanvas canvas, IDiagramCreator creator,
            double offsetX, double offsetY,
            bool appendIds, bool updateIds,
            bool select,
            bool createElements)
        {
            var parser = new Parser();

            var options = new ParseOptions()
            {
                OffsetX = offsetX,
                OffsetY = offsetY,
                AppendIds = appendIds,
                UpdateIds = updateIds,
                Select = select,
                CreateElements = createElements,
                Counter = canvas != null ? canvas.GetCounter() : null,
                Properties = canvas != null ? canvas.GetProperties() : null
            };

            var oldCanvas = creator.GetCanvas();

            creator.SetCanvas(canvas);

            var result = parser.Parse(model, creator, options);

            creator.SetCanvas(oldCanvas);

            if (updateIds == true)
                canvas.SetCounter(options.Counter);

            if (createElements == true)
                canvas.SetProperties(options.Properties);

            return result;
        } 

        #endregion

        #region Clear

        public static void Clear(ICanvas canvas)
        {
            canvas.Clear();
            canvas.GetCounter().ResetDiagram();
        }

        #endregion

        #region Open

        public static string Open(string fileName)
        {
            string model = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                model = reader.ReadToEnd();
            }

            return model;
        }

        #endregion

        #region Save

        public static void Save(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        #endregion

        #region Grid

        public static void SetGrid(ICanvas canvas, IDiagramCreator creator, bool undo)
        {
            if (undo == true)
                History.Add(canvas);

            var prop = canvas.GetProperties();

            creator.CreateGrid(prop.GridOriginX,
                prop.GridOriginY,
                prop.GridWidth,
                prop.GridHeight,
                prop.GridSize);

            canvas.SetWidth(prop.PageWidth);
            canvas.SetHeight(prop.PageHeight);
        }

        #endregion

        #region Load

        public static void Load(ICanvas canvas, IDiagramCreator creator, ITreeItem item)
        {
            var tag = item.GetTag();

            Model.Clear(canvas);

            if (tag != null)
            {
                LoadFromTag(canvas, creator, tag);
            }
            else
            {
                canvas.SetTag(new UndoRedo(new Stack<string>(), new Stack<string>()));

                SetGrid(canvas, creator, false);
            }
        }

        public static void LoadFromTag(ICanvas canvas, IDiagramCreator creator, object tag)
        {
            var diagram = tag as Diagram;

            var model = diagram.Item1;
            var history = diagram.Item2;

            canvas.SetTag(history);

            Model.Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);
        }

        #endregion

        #region Store

        public static void Store(ICanvas canvas, ITreeItem item)
        {
            var uid = item.GetUid();
            var model = Model.GenerateDiagram(canvas, uid, canvas == null ? null : canvas.GetProperties());

            if (item != null)
            {
                item.SetTag(new Diagram(model, canvas != null ? canvas.GetTag() as UndoRedo : null));
            }
        } 

        #endregion

        #region Elements

        public static IEnumerable<IElement> GetSelected(ICanvas canvas)
        {
            var selected = new List<IElement>();

            var elements = canvas.GetElements().OfType<IElement>();

            foreach (var element in elements)
            {
                if (element.GetSelected() == true)
                {
                    selected.Add(element);
                }
            }

            return selected;
        }

        public static IEnumerable<IElement> GetSelectedThumbs(ICanvas canvas)
        {
            var elements = new List<IElement>();

            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                if (thumb.GetSelected() == true)
                {
                    elements.Add(thumb);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetSelectedWires(ICanvas canvas)
        {
            var elements = new List<IElement>();

            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                if (wire.GetSelected() == true)
                {
                    elements.Add(wire);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetAll(ICanvas canvas)
        {
            var elements = new List<IElement>();

            var all = canvas.GetElements().OfType<IElement>();

            foreach (var element in all)
            {
                elements.Add(element);
            }

            return elements;
        }

        public static IEnumerable<IElement> GetThumbs(ICanvas canvas)
        {
            var elements = new List<IElement>();

            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                elements.Add(thumb);
            }

            return elements;
        }

        public static IEnumerable<IElement> GetWires(ICanvas canvas)
        {
            var elements = new List<IElement>();

            var wires = canvas.GetElements().OfType<ILine>();

            foreach (var wire in wires)
            {
                elements.Add(wire);
            }

            return elements;
        }

        #endregion

        #region Move

        private static void MoveElement(IElement element, double dX, double dY)
        {
            if (dX != 0.0)
            {
                element.SetX(element.GetX() - dX);
            }

            if (dY != 0.0)
            {
                element.SetY(element.GetY() - dY);
            }
        }

        public static void MoveLine(ILine line, double dX, double dY)
        {
            ModeLineStart(line, dX, dY);
            MoveLineEnd(line, dX, dY);
        }

        public static void MoveLineEnd(ILine line, double dX, double dY)
        {
            double left = line.GetX2();
            double top = line.GetY2();
            double x = 0.0;
            double y = 0.0;

            x = dX != 0.0 ? left - dX : left;
            y = dY != 0.0 ? top - dY : top;

            line.SetX2(x);
            line.SetY2(y);
        }

        public static void ModeLineStart(ILine line, double dX, double dY)
        {
            var margin = line.GetMargin();
            double left = margin.Left;
            double top = margin.Top;
            double x = 0.0;
            double y = 0.0;

            x = dX != 0.0 ? left - dX : left;
            y = dY != 0.0 ? top - dY : top;

            line.SetX2(line.GetX2() + (left - x));
            line.SetY2(line.GetY2() + (top - y));
            line.SetMargin(new MarginEx(0, x, 0, y));
        }

        #endregion

        #region Insert

        public static void Insert(ICanvas canvas, 
            IEnumerable<IElement> elements, 
            bool select, 
            double offsetX, 
            double offsetY)
        {
            var thumbs = elements.Where(x => x is IThumb);
            int count = thumbs.Count();
            double minX = count == 0 ? 0.0 : thumbs.Min(x => x.GetX());
            double minY = count == 0 ? 0.0 : thumbs.Min(x => x.GetY());
            double dX = offsetX != 0.0 ? minX - offsetX : 0.0;
            double dY = offsetY != 0.0 ? minY - offsetY : 0.0;

            //System.Diagnostics.Debug.Print("minX: {0}, offsetX: {1}, dX: {2}", minX, offsetX, dX);
            //System.Diagnostics.Debug.Print("minY: {0}, offsetY: {1}, dY: {2}", minY, offsetY, dY);

            foreach (var element in elements)
            {
                canvas.Add(element);

                if (element is IThumb)
                {
                    MoveElement(element, dX, dY);
                }
                else if (element is ILine)
                {
                    if (dX != 0.0 || dY != 0.0)
                        MoveLine(element as ILine, dX, dY);
                }

                if (select == true)
                    element.SetSelected(true);
            }
        }

        #endregion

        #region Selection

        public static void SelectionToggleWire(IElement element)
        {
            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("ToggleLineSelection: {0}, uid: {1}, parent: {2}",
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as ILine;

                // select/deselect line
                bool isSelected = line.GetSelected();
                line.SetSelected(isSelected ? false : true);
            }
        }

        public static void SetThumbsSelection(ICanvas canvas, bool isSelected)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                // select
                thumb.SetSelected(isSelected);
            }
        }

        public static void SetLinesSelection(ICanvas canvas, bool isSelected)
        {
            var lines = canvas.GetElements().OfType<ILine>();

            // deselect all lines
            foreach (var line in lines)
            {
                line.SetSelected(isSelected);
            }
        }

        public static void SelectAll(ICanvas canvas)
        {
            SetThumbsSelection(canvas, true);
            SetLinesSelection(canvas, true);
        }

        public static void SelectNone(ICanvas canvas)
        {
            SetThumbsSelection(canvas, false);
            SetLinesSelection(canvas, false);
        }

        #endregion

        #region Select Connected

        public static void SelectConnected(ICanvas canvas)
        {
            var elements = GetSelectedThumbs(canvas);

            if (elements != null)
            {
                var element = elements.FirstOrDefault();

                if (element != null)
                {
                    SelectNone(canvas);

                    var visited = new HashSet<string>();

                    SelectConnected(element, visited);

                    visited = null;
                }
            }
        }

        public static void SelectConnected(IElement element, HashSet<string> visited)
        {
            if (element == null)
            {
                return;
            }

            var elmentTag = element.GetTag();
            if (elmentTag != null)
            {
                visited.Add(element.GetUid());
                element.SetSelected(true);

                var selection = elmentTag as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                {
                    SelectConnected(tuple, element, visited);
                }
            }
        }

        public static void SelectConnected(MapWire tuple, IElement root, HashSet<string> visited)
        {
            var line = tuple.Item1 as ILine;
            var tag = line.GetTag() as Tuple<object, object>;

            line.SetSelected(true);

            if (tag == null)
            {
                return;
            }

            var startRoot = tag.Item1 as IElement;
            var endRoot = tag.Item2 as IElement;

            if (startRoot != null &&
                StringUtil.Compare(startRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(startRoot.GetUid()) == false)
            {
                SelectConnected(startRoot, visited);
            }

            if (endRoot != null &&
                StringUtil.Compare(endRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(endRoot.GetUid()) == false)
            {
                SelectConnected(endRoot, visited);
            }
        }

        #endregion

        #region IDs

        public static void IdsAppend(IEnumerable<object> elements, IdCounter counter)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements.Cast<IElement>())
            {
                string[] uid = element.GetUid().Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = IdsGetUpdatedElement(counter, type);

                //System.Diagnostics.Debug.Print("+{0}, id: {1} -> {2} ", type, id, appendedId);

                string appendedUid = string.Concat(type, ModelConstants.TagNameSeparator, appendedId.ToString());
                element.SetUid(appendedUid);

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

        public static int IdsGetUpdatedElement(IdCounter counter, string type)
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

        public static void IdsUpdateCounter(IdCounter original, IdCounter counter)
        {
            original.SolutionCount = Math.Max(original.SolutionCount, counter.SolutionCount);
            original.ProjectCount = Math.Max(original.ProjectCount, counter.ProjectCount);
            original.DiagramCount = Math.Max(original.DiagramCount, counter.DiagramCount);

            original.PinCount = Math.Max(original.PinCount, counter.PinCount);
            original.WireCount = Math.Max(original.WireCount, counter.WireCount);
            original.InputCount = Math.Max(original.InputCount, counter.InputCount);
            original.OutputCount = Math.Max(original.OutputCount, counter.OutputCount);
            original.AndGateCount = Math.Max(original.AndGateCount, counter.AndGateCount);
            original.OrGateCount = Math.Max(original.OrGateCount, counter.OrGateCount);
        }

        #endregion

        #region Connections

        public static void ConnectionsUpdate(IDictionary<string, MapWires> dict)
        {
            // update wire to element connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1 as IElement;
                if (element == null)
                    continue;

                var wires = item.Value.Item2;

                if (element.GetTag() == null)
                {
                    element.SetTag(new Selection(false, new List<MapWire>()));
                }

                if (wires.Count > 0)
                {
                    var selection = element.GetTag() as Selection;
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
                                if (line == null)
                                    continue;

                                var mapWire = new MapWire(line, element, null);

                                tuples.Add(mapWire);

                                var lineEx = line as ILine;
                                if (lineEx.GetTag() != null)
                                {
                                    var endRoot = lineEx.GetTag() as IElement;
                                    if (endRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.SetTag(new Tuple<object, object>(element, endRoot));
                                    }
                                }
                                else
                                {
                                    // set line Tag as start root element
                                    lineEx.SetTag(element);
                                }
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
                                if (line == null)
                                    continue;

                                var mapWire = new MapWire(line, null, element);

                                tuples.Add(mapWire);

                                var lineEx = line as ILine;
                                if (lineEx.GetTag() != null)
                                {
                                    var startRoot = lineEx.GetTag() as IElement;
                                    if (startRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.SetTag(new Tuple<object, object>(startRoot, element));
                                    }
                                }
                                else
                                {
                                    // set line Tag as end root element
                                    lineEx.SetTag(element);
                                }
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

        public static Position GetPinPosition(IElement root, IThumb pin)
        {
            // get root position in canvas
            double rx = root.GetX();
            double ry = root.GetY();

            // get pin position in canvas (relative to root)
            double px = pin.GetX();
            double py = pin.GetY();

            // calculate real pin position
            double x = rx + px;
            double y = ry + py;

            return new Position(x, y);
        }

        #endregion

        #region Delete

        public static void DeleteElement(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        public static void DeleteElement(ICanvas canvas, IElement element)
        {
            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("DeleteElement, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.Uid, element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as ILine;

                DeleteWire(canvas, line);
            }
            else
            {
                canvas.Remove(element);
            }
        }

        public static void DeleteWire(ICanvas canvas, ILine line)
        {
            canvas.Remove(line);

            RemoveWireConnections(canvas, line);

            DeleteEmptyPins(canvas);
        }

        public static void DeleteEmptyPins(ICanvas canvas)
        {
            var pins = FindEmptyPins(canvas);

            // remove empty pins
            foreach (var pin in pins)
            {
                canvas.Remove(pin);
            }
        }

        public static List<IElement> FindEmptyPins(ICanvas canvas)
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

        public static bool IsElementPin(string uid)
        {
            return uid != null &&
                   StringUtil.StartsWith(uid, ModelConstants.TagElementPin);
        }

        public static Connections RemoveWireConnections(ICanvas canvas, ILine line)
        {
            var connections = new Connections();

            foreach (var element in canvas.GetElements())
            {
                var elementTag = element.GetTag();
                if (elementTag != null && !(element is ILine))
                {
                    RemoveWireConnections(line, connections, element);
                }
            }

            return connections;
        }

        public static void RemoveWireConnections(ILine line, Connections connections, IElement element)
        {
            var selection = element.GetTag() as Selection;
            var tuples = selection.Item2;

            var map = CreateMapWire(line, tuples);

            if (map.Count > 0)
            {
                connections.Add(new Connection(element, map));
            }

            foreach (var tuple in map)
            {
                tuples.Remove(tuple);
            }
        }

        private static List<MapWire> CreateMapWire(ILine line, List<MapWire> tuples)
        {
            var map = new List<MapWire>();

            foreach (var tuple in tuples)
            {
                var _line = tuple.Item1 as ILine;

                if (StringUtil.Compare(_line.GetUid(), line.GetUid()))
                {
                    map.Add(tuple);
                }
            }

            return map;
        }

        #endregion
    }

    #endregion
}
