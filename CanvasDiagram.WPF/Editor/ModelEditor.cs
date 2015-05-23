// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Core.Model;
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
                string uid = element.GetUid();

                if (IsWire(uid))
                    GenerateWire(sb, element, uid);
                else if (IsInputOutput(uid))
                    GenerateInputOutput(sb, element, element.GetX(), element.GetY(), uid);
                else
                    GenerateElement(sb, element.GetX(), element.GetY(), uid);

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
            Tag tag = (data != null && data is Tag) ? data as Tag : null;

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
            var tag = element.GetTag();
            if (tag != null && !(element is ILine))
            {
                var wires = (tag as Connection).Wires;

                foreach (var wire in wires)
                {
                    var line = wire.Line as ILine;
                    var start = wire.Start;
                    var end = wire.End;

                    if (start != null)
                        GenerateWireStart(sb, line);
                    
                    if (end != null)
                        GenerateWireEnd(sb, line);
                }
            }
        }
    
        private static void GenerateWireStart(StringBuilder sb, ILine line)
        {
            sb.Append(Constants.PrefixChild);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(line.GetUid());
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(Constants.WireStartType);
            sb.Append(Environment.NewLine);
        }

        private static void GenerateWireEnd(StringBuilder sb, ILine line)
        {
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

            foreach (var project in solution.GetItems())
                sb.Append(GenerateProject(project, models, includeHistory));

            return new Solution(sb.ToString(), models);
        }

        public static string GenerateProject(ITreeItem project,
            List<string> models,
            bool includeHistory)
        {
            var sb = new StringBuilder();

            sb.Append(Constants.PrefixRoot);
            sb.Append(Constants.ArgumentSeparator);
            sb.Append(project.GetUid());
            sb.Append(Environment.NewLine);

            foreach (var diagram in project.GetItems())
            {
                if (diagram.GetTag() != null)
                {
                    var tag = diagram.GetTag() as Diagram;
                    var model = tag.Model;
                    var history = tag.History;

                    if (model == null)
                        model = GenerateItemModel(null, diagram, true);

                    models.Add(model);
                    sb.Append(model);

                    if (includeHistory == true && history != null)
                    {
                        foreach (var m in history.Undo)
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

            var temp = creator.GetCanvas();

            creator.SetCanvas(canvas);

            var result = parser.Parse(model, creator, options);

            creator.SetCanvas(temp);

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
            using (var reader = new System.IO.StreamReader(fileName))
            {
                return reader.ReadToEnd();
            }
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

            canvas.SetTag(diagram.History);

            Parse(diagram.Model,
                canvas, creator,
                0, 0,
                false, true, false, true);
        }

        #endregion

        #region Store

        public static void Store(ICanvas canvas, ITreeItem item)
        {
            string model = (canvas == null) ? 
                GenerateItemModel(null, item, true) :
                GenerateDiagram(canvas, item.GetUid(), canvas == null ? null : canvas.GetProperties());
  
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
            MoveLineStart(line, dX, dY);
            MoveLineEnd(line, dX, dY);
        }

        public static void MoveLineEnd(ILine line, double dX, double dY)
        {
            double left = line.GetX2();
            double top = line.GetY2();
            double x = dX != 0.0 ? left - dX : left;
            double y = dY != 0.0 ? top - dY : top;

            line.SetX2(x);
            line.SetY2(y);
        }

        public static void MoveLineStart(ILine line, double dX, double dY)
        {
            var margin = line.GetMargin();
            double left = margin.Left;
            double top = margin.Top;
            double x = dX != 0.0 ? left - dX : left;
            double y = dY != 0.0 ? top - dY : top;

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
                    MoveElement(element, dX, dY);
                else if (element is ILine && (dX != 0.0 || dY != 0.0))
                    MoveLine(element as ILine, dX, dY);

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
            foreach (var thumb in canvas.GetElements().OfType<IThumb>())
                thumb.SetSelected(isSelected);
        }

        public static void SetLinesSelection(ICanvas canvas, bool isSelected)
        {
            foreach (var line in canvas.GetElements().OfType<ILine>())
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

            var tag = element.GetTag();
            if (tag == null)
                return;

            visited.Add(element.GetUid());
            element.SetSelected(true);

            foreach (var wire in (tag as Connection).Wires)
                SelectConnected(wire, element, visited);
        }

        public static void SelectConnected(Wire wire, IElement root, HashSet<string> visited)
        {
            var line = wire.Line as ILine;
            var tag = line.GetTag() as Wire;

            line.SetSelected(true);
            if (tag == null)
                return;

            if (CanSelectStart(root, visited, tag.Start))
                SelectConnected(tag.Start, visited);

            if (CanSelectEnd(root, visited, tag.End))
                SelectConnected(tag.End, visited);
        }

        private static bool CanSelectStart(IElement root, HashSet<string> visited, IElement startRoot)
        {
            return startRoot != null &&
                StringUtil.Compare(startRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(startRoot.GetUid()) == false;
        }

        private static bool CanSelectEnd(IElement root, HashSet<string> visited, IElement endRoot)
        {
            return endRoot != null &&
                StringUtil.Compare(endRoot.GetUid(), root.GetUid()) == false &&
                visited.Contains(endRoot.GetUid()) == false;
        }

        #endregion

        #region IDs

        public static void IdsAppend(IEnumerable<object> elements, IdCounter counter)
        {
            foreach (var element in elements.Cast<IElement>())
                element.SetUid(GetUid(counter, element));
        }

        private static string GetUid(IdCounter counter, IElement element)
        {
            return string.Concat(element.GetUid().Split(Constants.TagNameSeparator)[0], 
                Constants.TagNameSeparator, 
                counter.Next().ToString());
        }

        public static void IdsUpdateCounter(IdCounter original, IdCounter counter)
        {
            original.Set(Math.Max(original.Count, counter.Count));
        }

        #endregion

        #region Connections

        public static void ConnectionsUpdate(IDictionary<string, Child> dict)
        {
            foreach (var item in dict)
            {
                var element = item.Value.Element as IElement;
                if (element == null)
                    continue;

                if (element.GetTag() == null)
                    element.SetTag(new Connection(element, new List<Wire>()));

                var pins = item.Value.Pins;
                if (pins.Count > 0)
                    UpdateWires(dict, element, pins);
            }
        }

        private static void UpdateWires(IDictionary<string, Child> dict, IElement element, List<Pin> pins)
        {
            var connection = element.GetTag() as Connection;
            var wires = connection.Wires;

            foreach (var pin in pins)
            {
                string name = pin.Name;
                string type = pin.Type;

                if (StringUtil.Compare(type, Constants.WireStartType))
                {
                    Child child = null;
                    if (dict.TryGetValue(name, out child) == true)
                    {
                        var line = child.Element;
                        if (line == null)
                            continue;

                        UpdateStartTag(element, wires, line);
                    }
                    else
                        System.Diagnostics.Debug.Print("Failed to map wire Start: {0}", name);
                }
                else if (StringUtil.Compare(type, Constants.WireEndType))
                {
                    Child child = null;
                    if (dict.TryGetValue(name, out child) == true)
                    {
                        var line = child.Element;
                        if (line == null)
                            continue;

                        UpdateEndTag(element, wires, line);
                    }
                    else
                        System.Diagnostics.Debug.Print("Failed to map wire End: {0}", name);
                }
            }
        }

        private static void UpdateStartTag(IElement element, List<Wire> wires, object line)
        {
            wires.Add(new Wire(line, element, null));

            var lineEx = line as ILine;
            if (lineEx.GetTag() != null)
            {
                var root = lineEx.GetTag() as IElement;
                if (root != null)
                    lineEx.SetTag(new Wire(lineEx, element, root));
            }
            else
            {
                lineEx.SetTag(element);
            }
        }

        private static void UpdateEndTag(IElement element, List<Wire> wires, object line)
        {
            wires.Add(new Wire(line, null, element));

            var lineEx = line as ILine;
            if (lineEx.GetTag() != null)
            {
                var root = lineEx.GetTag() as IElement;
                if (root != null)
                    lineEx.SetTag(new Wire(lineEx, root, element));
            }
            else
            {
                lineEx.SetTag(element);
            }
        }

        public static void GetPinPosition(IElement root, IThumb pin, out double x, out double y)
        {
            x = root.GetX() + pin.GetX();
            y = root.GetY() + pin.GetY();
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
            foreach (var pin in FindEmptyPins(canvas))
                canvas.Remove(pin);
        }

        public static List<IElement> FindEmptyPins(ICanvas canvas)
        {
            var empty = new List<IElement>();

            foreach (var element in canvas.GetElements())
            {
                if (IsElementPin(element.GetUid()))
                {
                    var tag = element.GetTag();
                    if (tag == null)
                        empty.Add(element);
                    else if ((tag as Connection).Wires.Count <= 0)
                        empty.Add(element);
                }
            }

            return empty;
        }

        public static bool IsElementPin(string uid)
        {
            return uid != null &&
                   StringUtil.StartsWith(uid, Constants.TagElementPin);
        }

        public static List<Connection> RemoveWireConnections(ICanvas canvas, ILine line)
        {
            var connections = new List<Connection>();

            foreach (var element in canvas.GetElements())
            {
                if (element.GetTag() != null && !(element is ILine))
                    RemoveWireConnections(line, connections, element);
            }

            return connections;
        }

        public static void RemoveWireConnections(ILine line, List<Connection> connections, IElement element)
        {
            var wires = (element.GetTag() as Connection).Wires;
            var maps = CreateMapWire(line, wires);

            if (maps.Count > 0)
                connections.Add(new Connection(element, maps));

            foreach (var map in maps)
                wires.Remove(map);
        }

        private static List<Wire> CreateMapWire(ILine line, List<Wire> wires)
        {
            var map = new List<Wire>();

            foreach (var wire in wires)
            {
                if (StringUtil.Compare((wire.Line as ILine).GetUid(), line.GetUid()))
                    map.Add(wire);
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

            if (element is T && uid != null && StringUtil.StartsWith(uid, Constants.TagElementWire))
                return element as T;

            return null;
        }

        #endregion
    }

    #endregion
}
