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

    #region Editor

    public static class Editor
    {
        #region Generate Model

        public static string GenerateModel(IEnumerable<FrameworkElement> elements)
        {
            var sb = new StringBuilder();

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

                    sb.AppendLine("".PadLeft(4, ' ') + str);

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

                if (element.Tag != null && !(element is LineEx))
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

                            sb.AppendLine("".PadLeft(8, ' ') + str);

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

                            sb.AppendLine("".PadLeft(8, ' ') + str);

                            //System.Diagnostics.Debug.Print(str);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static string GenerateModel(Canvas canvas, string uid, DiagramProperties properties)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var sb = new StringBuilder();
            var elements = canvas != null ? canvas.Children.Cast<FrameworkElement>() : Enumerable.Empty<FrameworkElement>();

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

        public static Tuple<string, IEnumerable<string>> GenerateSolutionModel(TreeView tree, string fileName, string tagFileName)
        {
            var models = new List<string>();

            var solution = tree.Items.Cast<TreeViewItem>().First();
            var projects = solution.Items.Cast<TreeViewItem>();
            string line = null;

            var sb = new StringBuilder();

            // tags file path is relative to solution file path
            if (tagFileName != null && fileName != null)
            {
                string relativePath = MakeRelativePath(tagFileName, fileName);
                string onlyFileName = System.IO.Path.GetFileName(tagFileName);

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

        #endregion

        #region Relative Path

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

        #endregion

        #region Get Elements

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

        public static IEnumerable<FrameworkElement> GetSelectedThumbElements(Canvas canvas)
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

            return elements;
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

        public static IEnumerable<FrameworkElement> GetThumbElements(Canvas canvas)
        {
            var elements = new List<FrameworkElement>();

            // get all thumbs
            var thumbs = canvas.Children.OfType<ElementThumb>();

            foreach (var thumb in thumbs)
            {

                elements.Add(thumb);
            }

            return elements;
        }

        #endregion

        #region Insert Elements

        public static void InsertElements(Canvas canvas, IEnumerable<FrameworkElement> elements, bool select)
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

        #endregion

        #region Selection

        public static void SetThumbsSelection(Canvas canvas, bool isSelected)
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

        public static void ToggleLineSelection(FrameworkElement element)
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

        public static void SelectAll(Canvas canvas)
        {
            Editor.SetThumbsSelection(canvas, true);
            Editor.SetLinesSelection(canvas, true);
        }

        public static void DeselectAll(Canvas canvas)
        {
            Editor.SetThumbsSelection(canvas, false);
            Editor.SetLinesSelection(canvas, false);
        }

        #endregion

        #region Select Connected

        public static void SelectConnected(Canvas canvas)
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

        public static void SelectConnected(FrameworkElement element, HashSet<string> visited)
        {
            if (element != null && element.Tag != null)
            {
                visited.Add(element.Uid);
                ElementThumb.SetIsSelected(element, true);

                var selection = element.Tag as Selection;
                var tuples = selection.Item2;

                foreach (var tuple in tuples)
                {
                    SelectConnected(tuple, element, visited);
                }
            }
        }

        public static void SelectConnected(MapWire tuple, FrameworkElement root, HashSet<string> visited)
        {
            var line = tuple.Item1 as LineEx;
            var tag = line.Tag as Tuple<object, object>;

            ElementThumb.SetIsSelected(line, true);

            if (tag == null)
            {
                // MessageBox.Show("Tag is null.");
                return;
            }

            var startRoot = tag.Item1 as FrameworkElement;
            var endRoot = tag.Item2 as FrameworkElement;

            if (startRoot != null &&
                StringUtil.Compare(startRoot.Uid, root.Uid) == false &&
                visited.Contains(startRoot.Uid) == false)
            {
                SelectConnected(startRoot, visited);
            }

            if (endRoot != null &&
                StringUtil.Compare(endRoot.Uid, root.Uid) == false &&
                visited.Contains(endRoot.Uid) == false)
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

            foreach (var element in elements.Cast<FrameworkElement>())
            {
                string[] uid = element.Uid.Split(ModelConstants.TagNameSeparator);

                string type = uid[0];
                int id = int.Parse(uid[1]);

                int appendedId = GetUpdatedElementId(counter, type);

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

                                var lineEx = line as LineEx;
                                if (lineEx.Tag != null)
                                {
                                    var endRoot = lineEx.Tag as FrameworkElement;
                                    if (endRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.Tag = new Tuple<object, object>(element, endRoot);
                                    }
                                }
                                else
                                {
                                    // set line Tag as start root element
                                    lineEx.Tag = element;
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

                                var lineEx = line as LineEx;
                                if (lineEx.Tag != null)
                                {
                                    var startRoot = lineEx.Tag as FrameworkElement;
                                    if (startRoot != null)
                                    {
                                        // set line Tag as Tuple of start & end root element
                                        lineEx.Tag = new Tuple<object, object>(startRoot, element);
                                    }
                                }
                                else
                                {
                                    // set line Tag as end root element
                                    lineEx.Tag = element;
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
