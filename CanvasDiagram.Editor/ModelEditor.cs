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
    #region ModelEditor

    public static class ModelEditor
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

                if (IsWire(uid))
                    GenerateWire(sb, element, uid);
                else if (IsInputOutput(uid))
                    GenerateInputOutput(sb, element, x, y, uid);
                else
                    GenerateElement(sb, x, y, uid);

                GenerateChildren(sb, element);
            }

            return sb.ToString();
        }

        private static bool IsWire(string uid)
        {
            return StringUtil.StartsWith(uid, Constants.TagElementWire);
        }

        private static bool IsInputOutput(string uid)
        {
            return StringUtil.StartsWith(uid, Constants.TagElementInput) ||
                StringUtil.StartsWith(uid, Constants.TagElementOutput);
        }

        private static void GenerateElement(StringBuilder sb, double x, double y, string uid)
        {
            sb.Append("    ");
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(uid);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(x);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(y);
            sb.Append(Environment.NewLine);
        }

        private static void GenerateInputOutput(StringBuilder sb, IElement element, double x, double y, string uid)
        {
            var data = element.GetData();
            Tag tag = null;

            if (data != null && data is Tag)
                tag = data as Tag;

            sb.Append("    ");
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(uid);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(x);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(y);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(tag != null ? tag.Id : -1);
            sb.Append(Environment.NewLine);
        }

        private static void GenerateWire(StringBuilder sb, IElement element, string uid)
        {
            var line = element as ILine;
            var margin = line.GetMargin();

            sb.Append("    ");
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(uid);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(margin.Left); // line.X1
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(margin.Top); // line.Y1
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetX2() + margin.Left);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetY2() + margin.Top);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetStartVisible());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetEndVisible());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetStartIO());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetEndIO());
            sb.Append(Environment.NewLine);
        }

        private static void GenerateChildren(StringBuilder sb, IElement element)
        {
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
                        GenerateWireStart(sb, line);
                    
                    if (end != null)
                        GenerateWireEnd(sb, line);
                }
            }
        }
    
        private static void GenerateWireStart(StringBuilder sb, ILine line)
        {
            sb.Append("        ");
            sb.Append(Constants.PrefixChild);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetUid());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(Constants.WireStartType);
            sb.Append(Environment.NewLine);
        }

        private static void GenerateWireEnd(StringBuilder sb, ILine line)
        {
            sb.Append("        ");
            sb.Append(Constants.PrefixChild);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetUid());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(Constants.WireEndType);
            sb.Append(Environment.NewLine);
        }

        private static string DefaultUid = Constants.TagHeaderDiagram + Constants.TagNameSeparator + (-1).ToString();

        private static void GenerateHeader(StringBuilder sb, string uid, DiagramProperties prop)
        {
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(uid == null ? DefaultUid : uid);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.PageWidth);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.PageHeight);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.GridOriginX);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.GridOriginY);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.GridWidth);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.GridHeight);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.GridSize);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.SnapX);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.SnapY);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.SnapOffsetX);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(prop.SnapOffsetY);
            sb.Append(Environment.NewLine);
        }

        public static string GenerateDiagram(ICanvas canvas, string uid, DiagramProperties properties)
        {
            var sb = new StringBuilder();
            var elements = (canvas == null) ? null : canvas.GetElements();

            GenerateHeader(sb, uid, properties);

            if (elements != null)
                sb.Append(Generate(elements));

            return sb.ToString();
        }

        public static Solution GenerateSolution(ITree tree,
            string fileName,
            string tagFileName,
            string tableFileName,
            bool includeHistory)
        {
            var models = new List<string>();
            var solution = tree.GetItems().First();
            var projects = solution.GetItems();
            string relativeTagFileName = tagFileName;
            string relativeTableFileName = tableFileName;
            var sb = new StringBuilder();

            // tags file path is relative to solution file path
            if (tagFileName != null && fileName != null)
                relativeTagFileName = PathUtil.GetRelativeFileName(fileName, tagFileName);

            // table file path is relative to solution file path
            if (tableFileName != null && fileName != null)
                relativeTableFileName = PathUtil.GetRelativeFileName(fileName, tableFileName);

            // Solution
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(solution.GetUid());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(relativeTagFileName);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(relativeTableFileName);
            sb.Append(Environment.NewLine);

            foreach (var project in projects)
                sb.Append(GenerateProject(project, models, includeHistory));

            return new Solution(sb.ToString(), models);
        }

        public static string GenerateProject(ITreeItem project,
            List<string> models,
            bool includeHistory)
        {
            var diagrams = project.GetItems();
            var sb = new StringBuilder();

            // Project
            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(project.GetUid());
            sb.Append(Environment.NewLine);

            foreach (var diagram in diagrams)
            {
                // Diagram Elements
                if (diagram.GetTag() != null)
                {
                    var _diagram = diagram.GetTag() as Diagram;
                    var model = _diagram.Item1;
                    var history = _diagram.Item2;

                    if (model == null)
                        model = GenerateItemModel(null, diagram, true);

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

        public static string GenerateItemModel(ICanvas canvas, ITreeItem item, bool update)
        {
            string model = null;

            if (item != null)
            {
                string uid = item.GetUid();
                bool isDiagram = StringUtil.StartsWith(uid, Constants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var prop = (canvas == null) ? DiagramProperties.Default : canvas.GetProperties();
                    model = GenerateDiagram(canvas, uid, prop);
                    if (update == true)
                    {
                        UndoRedo undoRedo = (canvas == null) ? 
                            new UndoRedo(new Stack<string>(), new Stack<string>()) :
                            canvas.GetTag() as UndoRedo;

                        item.SetTag(new Diagram(model, undoRedo));
                    }
                }
            }

            return model;
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

        public static void SetGrid(ICanvas canvas, IDiagramCreator creator)
        {
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

            Clear(canvas);

            if (tag != null)
                LoadFromTag(canvas, creator, tag);
        }

        public static void LoadFromTag(ICanvas canvas, IDiagramCreator creator, object tag)
        {
            var diagram = tag as Diagram;
            var model = diagram.Item1;
            var history = diagram.Item2;

            canvas.SetTag(history);

            Parse(model,
                canvas, creator,
                0, 0,
                false, true, false, true);
        }

        #endregion

        #region Store

        public static void Store(ICanvas canvas, ITreeItem item)
        {
            var uid = item.GetUid();
            string model = null;

            if (canvas == null)
                model = GenerateItemModel(null, item, true);
            else
                model = GenerateDiagram(canvas, uid, canvas == null ? null : canvas.GetProperties());
  
            item.SetTag(new Diagram(model, canvas != null ? canvas.GetTag() as UndoRedo : null));
        } 

        #endregion

        #region Elements

        public static IEnumerable<IElement> GetSelected(ICanvas canvas)
        {
             return canvas
                 .GetElements()
                 .OfType<IElement>()
                 .Where(x => x.GetSelected() == true)
                 .ToList();
        }

        public static IEnumerable<IElement> GetSelectedThumbs(ICanvas canvas)
        {
            return canvas
                .GetElements()
                .OfType<IThumb>()
                .Where(x => x.GetSelected() == true)
                .Cast<IElement>()
                .ToList();
        }

        public static IEnumerable<IElement> GetSelectedWires(ICanvas canvas)
        {
            return canvas
                .GetElements()
                .OfType<ILine>()
                .Where(x => x.GetSelected() == true)
                .Cast<IElement>()
                .ToList();
        }

        public static IEnumerable<IElement> GetAll(ICanvas canvas)
        {
            return canvas
                .GetElements()
                .OfType<IElement>()
                .ToList();
        }

        public static IEnumerable<IElement> GetThumbs(ICanvas canvas)
        {
            return canvas
                .GetElements()
                .OfType<IThumb>()
                .Cast<IElement>()
                .ToList();
        }

        public static IEnumerable<IElement> GetWires(ICanvas canvas)
        {
            return canvas
                .GetElements()
                .OfType<ILine>()
                .Cast<IElement>()
                .ToList();
        }

        #endregion

        #region Move

        private static void MoveElement(IElement element, double dX, double dY)
        {
            if (dX != 0.0)
                element.SetX(element.GetX() - dX);

            if (dY != 0.0)
                element.SetY(element.GetY() - dY);
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

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, Constants.TagElementWire))
            {
                var line = element as ILine;
                line.SetSelected(line.GetSelected() ? false : true);
            }
        }

        public static void SetThumbsSelection(ICanvas canvas, bool isSelected)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>();
            foreach (var thumb in thumbs)
                thumb.SetSelected(isSelected);
        }

        public static void SetLinesSelection(ICanvas canvas, bool isSelected)
        {
            var lines = canvas.GetElements().OfType<ILine>();
            foreach (var line in lines)
                line.SetSelected(isSelected);
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
                return;

            var elmentTag = element.GetTag();
            if (elmentTag != null)
            {
                visited.Add(element.GetUid());
                element.SetSelected(true);

                var selection = elmentTag as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                    SelectConnected(tuple, element, visited);
            }
        }

        public static void SelectConnected(MapWire tuple, IElement root, HashSet<string> visited)
        {
            var line = tuple.Item1 as ILine;
            var tag = line.GetTag() as Tuple<object, object>;

            line.SetSelected(true);

            if (tag == null)
                return;

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
            foreach (var element in elements.Cast<IElement>())
            {
                string[] uid = element.GetUid().Split(Constants.TagNameSeparator);
                string type = uid[0];
                int id = int.Parse(uid[1]);
                int appendedId = counter.Next();
                string appendedUid = string.Concat(type, Constants.TagNameSeparator, appendedId.ToString());

                element.SetUid(appendedUid);
            }
        }

        public static void IdsUpdateCounter(IdCounter original, IdCounter counter)
        {
            original.Set(Math.Max(original.Count, counter.Count));
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
                    element.SetTag(new Selection(false, new List<MapWire>()));

                if (wires.Count > 0)
                    UpdateWires(dict, element, wires);
            }
        }

        private static void UpdateWires(IDictionary<string, MapWires> dict, IElement element, List<MapPin> wires)
        {
            var selection = element.GetTag() as Selection;
            var tuples = selection.Item2;

            foreach (var wire in wires)
            {
                string _name = wire.Item1;
                string _type = wire.Item2;

                if (StringUtil.Compare(_type, Constants.WireStartType))
                {
                    MapWires mapWires = null;
                    if (dict.TryGetValue(_name, out mapWires) == true)
                    {
                        var line = mapWires.Item1;
                        if (line == null)
                            continue;

                        UpdateStartTag(element, tuples, line);
                    }
                    else
                        System.Diagnostics.Debug.Print("Failed to map wire Start: {0}", _name);
                }
                else if (StringUtil.Compare(_type, Constants.WireEndType))
                {
                    MapWires mapWires = null;
                    if (dict.TryGetValue(_name, out mapWires) == true)
                    {
                        var line = mapWires.Item1;
                        if (line == null)
                            continue;

                        UpdateEndTag(element, tuples, line);
                    }
                    else
                        System.Diagnostics.Debug.Print("Failed to map wire End: {0}", _name);
                }
            }
        }

        private static void UpdateStartTag(IElement element, List<MapWire> tuples, object line)
        {
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

        private static void UpdateEndTag(IElement element, List<MapWire> tuples, object line)
        {
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

            if (element is ILine && uid != null && StringUtil.StartsWith(uid, Constants.TagElementWire))
                DeleteWire(canvas, element as ILine);
            else
                canvas.Remove(element);
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
            foreach (var pin in pins)
                canvas.Remove(pin);
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

                        // empty pin
                        if (tuples.Count <= 0)
                            pins.Add(element);
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
                   StringUtil.StartsWith(uid, Constants.TagElementPin);
        }

        public static Connections RemoveWireConnections(ICanvas canvas, ILine line)
        {
            var connections = new Connections();

            foreach (var element in canvas.GetElements())
            {
                var elementTag = element.GetTag();
                if (elementTag != null && !(element is ILine))
                    RemoveWireConnections(line, connections, element);
            }

            return connections;
        }

        public static void RemoveWireConnections(ILine line, Connections connections, IElement element)
        {
            var selection = element.GetTag() as Selection;
            var tuples = selection.Item2;
            var map = CreateMapWire(line, tuples);

            if (map.Count > 0)
                connections.Add(new Connection(element, map));

            foreach (var tuple in map)
                tuples.Remove(tuple);
        }

        private static List<MapWire> CreateMapWire(ILine line, List<MapWire> tuples)
        {
            var map = new List<MapWire>();

            foreach (var tuple in tuples)
            {
                var _line = tuple.Item1 as ILine;
                if (StringUtil.Compare(_line.GetUid(), line.GetUid()))
                    map.Add(tuple);
            }

            return map;
        }

        #endregion

        #region Find

        public static T Find<T>(ICanvas canvas, IPoint point, double radius) where T : class
        {
            var element = canvas.HitTest(point, radius).FirstOrDefault();
            if (element == null)
                return null;

            string uid = element.GetUid();

            if (element is T && uid != null &&
                StringUtil.StartsWith(uid, Constants.TagElementWire))
            {
                return element as T;
            }

            return null;
        }

        #endregion
    }

    #endregion
}
