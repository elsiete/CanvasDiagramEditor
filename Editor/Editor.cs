// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    #region Editor

    public static class Editor
    {
        #region Generate Model

        public static string GenerateModel(IEnumerable<IElement> elements)
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

        public static string GenerateModel(ICanvas canvas, string uid, DiagramProperties properties)
        {
            if (canvas == null)
            {
                return null;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();
            var elements = canvas.GetElements();

            string header = string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}{1}{6}{1}{7}{1}{8}{1}{9}{1}{10}{1}{11}{1}{12}{1}{13}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                uid == null ? ModelConstants.TagHeaderDiagram : uid,
                properties.PageWidth, properties.PageHeight,
                properties.GridOriginX, properties.GridOriginY,
                properties.GridWidth, properties.GridHeight,
                properties.GridSize,
                properties.SnapX, properties.SnapY,
                properties.SnapOffsetX, properties.SnapOffsetY);

            sb.AppendLine(header);
            //System.Diagnostics.Debug.Print(header);

            string model = GenerateModel(elements);

            sb.Append(model);

            var result = sb.ToString();

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModel() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return result;
        }

        #endregion

        #region Get Elements

        public static IEnumerable<IElement> GetSelectedElements(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get selected thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                if (thumb.GetSelected() == true)
                {
                    elements.Add(thumb);
                }
            }

            // get selected lines
            var lines = canvas.GetElements().OfType<ILine>();

            foreach (var line in lines)
            {
                if (line.GetSelected() == true)
                {
                    elements.Add(line);
                }
            }

            return elements;
        }

        public static IEnumerable<IElement> GetSelectedThumbElements(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get selected thumbs
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

        public static IEnumerable<IElement> GetAllElements(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get all thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {

                elements.Add(thumb);
            }

            // get all lines
            var lines = canvas.GetElements().OfType<ILine>();

            foreach (var line in lines)
            {
                elements.Add(line);
            }

            return elements;
        }

        public static IEnumerable<IElement> GetThumbElements(ICanvas canvas)
        {
            var elements = new List<IElement>();

            // get all thumbs
            var thumbs = canvas.GetElements().OfType<IThumb>();

            foreach (var thumb in thumbs)
            {
                elements.Add(thumb);
            }

            return elements;
        }

        #endregion

        #region Insert Elements

        public static void InsertElements(ICanvas canvas, IEnumerable<IElement> elements, bool select)
        {
            foreach (var element in elements)
            {
                canvas.Add(element);

                if (select == true)
                {
                    element.SetSelected(true);
                }
            }
        }

        #endregion

        #region Selection

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

        public static void ToggleLineSelection(IElement element)
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

        public static void SelectAll(ICanvas canvas)
        {
            Editor.SetThumbsSelection(canvas, true);
            Editor.SetLinesSelection(canvas, true);
        }

        public static void DeselectAll(ICanvas canvas)
        {
            Editor.SetThumbsSelection(canvas, false);
            Editor.SetLinesSelection(canvas, false);
        }

        #endregion

        #region Select Connected

        public static void SelectConnected(ICanvas canvas)
        {
            var elements = GetSelectedThumbElements(canvas);

            if (elements != null)
            {
                var element = elements.FirstOrDefault();

                if (element != null)
                {
                    DeselectAll(canvas);

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
                // MessageBox.Show("Tag is null.");
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

        public static void AppendElementIds(IEnumerable<object> elements, IdCounter counter)
        {
            // append ids to the existing elements in canvas
            //System.Diagnostics.Debug.Print("Appending Ids:");

            foreach (var element in elements.Cast<IElement>())
            {
                string[] uid = element.GetUid().Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = GetUpdatedElementId(counter, type);

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

        public static int GetUpdatedElementId(IdCounter counter, string type)
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

        public static void UpdateIdCounter(IdCounter original, IdCounter counter)
        {
            original.PinCount = Math.Max(original.PinCount, counter.PinCount);
            original.WireCount = Math.Max(original.WireCount, counter.WireCount);
            original.InputCount = Math.Max(original.InputCount, counter.InputCount);
            original.OutputCount = Math.Max(original.OutputCount, counter.OutputCount);
            original.AndGateCount = Math.Max(original.AndGateCount, counter.AndGateCount);
            original.OrGateCount = Math.Max(original.OrGateCount, counter.OrGateCount);
        }

        #endregion

        #region Connections

        public static void UpdateElementConnections(IDictionary<string, MapWires> dict)
        {
            // update wire to element connections
            foreach (var item in dict)
            {
                var element = item.Value.Item1 as IElement;
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

        #endregion

        #region Open/Save Model

        public static string OpenModel(string fileName)
        {
            string model = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                model = reader.ReadToEnd();
            }

            return model;
        }

        public static void SaveModel(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        #endregion

        #region Tags

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

        #endregion
    }

    #endregion
}
