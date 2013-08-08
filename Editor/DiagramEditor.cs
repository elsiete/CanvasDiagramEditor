// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf;
using CanvasDiagramEditor.Dxf.Core;
using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Controls;
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
    
    using Solution = Tuple<string, IEnumerable<string>>;

    #endregion

    #region DiagramCreator

    public class DiagramEditor
    {
        #region Fields

        public DiagramEditorOptions CurrentOptions = null;
        public Action UpdateDiagramProperties { get; set; }

        public Action<IThumb> SetThumbEvents { get; set; }

        public Func<ITreeItem> CreateTreeSolutionItem { get; set; }
        public Func<ITreeItem> CreateTreeProjectItem { get; set; }
        public Func<ITreeItem> CreateTreeDiagramItem { get; set; }

        public LinkedList<IElement> SelectedThumbList = null;
        public LinkedListNode<IElement> CurrentThumbNode = null;

        public WpfDiagramCreator WpfCreator { get; set; }

        private string ClipboardText = null;

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

        public TreeSolution ModelParseDiagram(string model,
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

        public void ModelClear(ICanvas canvas)
        {
            canvas.Clear();

            CurrentOptions.Counter.ResetDiagram();
        }

        public void ModelClear()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            HistoryAdd(canvas, true);

            ModelClear(canvas);
        }

        public static string ModelGenerateFromSelected(ICanvas canvas)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var elements = Elements.GetSelected(canvas);

            string model = Model.Generate(elements.Cast<IElement>());

            sw.Stop();
            System.Diagnostics.Debug.Print("GenerateDiagramModelFromSelected() in {0}ms", sw.Elapsed.TotalMilliseconds);

            return model;
        }

        public string ModelGenerate()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

            return diagram;
        }

        public string ModelGenerateFromSelected()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            var diagram = ModelGenerateFromSelected(canvas);

            return diagram;
        }

        public string ModelImport()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var diagram = Model.Open(dlg.FileName);

                return diagram;
            }

            return null;
        }

        public void ModelInsert(string diagram, double offsetX, double offsetY)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            HistoryAdd(canvas, true);

            SelectNone();
            ModelParseDiagram(diagram, canvas, path, offsetX, offsetY, true, true, true, true);
        }

        public void ModelResetThumbTags()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            HistoryAdd(canvas, true);

            TagsResetThumbs(canvas);
        }

        private void ModelLoad(ICanvas canvas, ITreeItem item)
        {
            var tag = item.GetTag();

            ModelClear(canvas);

            if (tag != null)
            {
                ModelLoadFromTag(canvas, tag);
            }
            else
            {
                canvas.SetTag(new History(new Stack<string>(), new Stack<string>()));

                GridGenerate(false);
            }
        }

        private void ModelLoadFromTag(ICanvas canvas, object tag)
        {
            var diagram = tag as Diagram;

            var model = diagram.Item1;
            var history = diagram.Item2;

            canvas.SetTag(history);

            ModelParseDiagram(model, canvas, CurrentOptions.CurrentPathGrid, 0, 0, false, true, false, true);
        }

        private void ModelStore(ICanvas canvas, ITreeItem item)
        {
            var uid = item.GetUid();
            var model = Model.Generate(canvas, uid, CurrentOptions.CurrentProperties);

            if (item != null)
            {
                item.SetTag(new Diagram(model, canvas != null ? canvas.GetTag() as History : null));
            }
        }

        public string ModelGetCurrent(bool update)
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;
            var item = tree.GetSelectedItem() as ITreeItem;

            if (item != null)
            {
                string uid = item.GetUid();
                bool isDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var model = Model.Generate(canvas, uid, CurrentOptions.CurrentProperties);

                    if (update == true)
                    {
                        item.SetTag(new Diagram(model, canvas.Tag as History));
                    }

                    return model;
                }
            }

            return null;
        }

        public string ModelUpdateSelectedDiagram()
        {
            return ModelGetCurrent(true);
        }

        public string ModelGetSelectedDiagram()
        {
            return ModelGetCurrent(false);
        }

        public static Solution ModelGenerateSolution(ITree tree,
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
                tagFileName = PathGetRelativeFileName(fileName, tagFileName);
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
                var model = ModelGenerateProject(project, models, includeHistory);
                sb.Append(model);
            }

            return new Solution(sb.ToString(), models);
        }

        public static string ModelGenerateProject(ITreeItem project,
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

                    if (includeHistory == true)
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

        public Solution ModelGenerateSolution(string fileName,
            bool includeHistory)
        {
            var tree = CurrentOptions.CurrentTree;
            var tagFileName = CurrentOptions.TagFileName;

            return ModelGenerateSolution(tree, fileName, tagFileName, includeHistory);
        }

        public IEnumerable<string> ModelGetCurrentProjectDiagrams()
        {
            var tree = CurrentOptions.CurrentTree;
            var selected = tree.GetSelectedItem() as ITreeItem;
            if (selected == null)
            {
                return null;
            }

            string uid = selected.GetUid();
            bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
            bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
            bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

            if (isSelectedDiagram == true)
            {
                var project = selected.GetParent() as ITreeItem;

                var models = new List<string>();

                DiagramEditor.ModelGenerateProject(project, models, false);

                return models;

            }
            else if (isSelectedProject == true)
            {
                var models = new List<string>();

                DiagramEditor.ModelGenerateProject(selected, models, false);

                return models;
            }
            else if (isSelectedSolution == true)
            {
                var solution = tree.GetItems().FirstOrDefault();

                if (solution != null)
                {
                    var models = new List<string>();
                    var project = solution.GetItems().FirstOrDefault();

                    if (project != null)
                    {
                        DiagramEditor.ModelGenerateProject(project, models, false);

                        return models;
                    }
                }
            }

            return null;
        }

        private IEnumerable<ITreeItem> ModelParseProjects(IEnumerable<TreeProject> projects,
            IdCounter counter,
            ITreeItem solutionItem)
        {
            var diagramList = new List<ITreeItem>();

            // create projects
            foreach (var project in projects)
            {
                string projectName = project.Item1;
                var diagrams = project.Item2.Reverse();

                //System.Diagnostics.Debug.Print("Project: {0}", name);

                // create project
                var projectItem = TreeCreateProjectItem(projectName);
                solutionItem.Add(projectItem);

                // update project count
                int projectId = int.Parse(projectName.Split(ModelConstants.TagNameSeparator)[1]);
                counter.ProjectCount = Math.Max(counter.ProjectCount, projectId + 1);

                ModelParseDiagrams(counter, diagrams, projectItem, diagramList);
            }

            var firstDiagram = diagramList.FirstOrDefault();
            if (firstDiagram != null)
            {
                firstDiagram.SetSelected(true);
            }

            return diagramList;
        }

        private void ModelParseDiagrams(IdCounter counter,
            IEnumerable<TreeDiagram> diagrams,
            ITreeItem projectItem,
            List<ITreeItem> diagramList)
        {
            // create diagrams
            foreach (var diagram in diagrams)
            {
                ModelParseDiagram(counter, diagram, projectItem, diagramList);
            }
        }

        private void ModelParseDiagram(IdCounter counter,
            TreeDiagram diagram,
            ITreeItem projectItem,
            List<ITreeItem> diagramList)
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

            var diagramItem = TreeCreateDiagramItem(diagramName);
            diagramItem.SetTag(new Diagram(model, null));

            projectItem.Add(diagramItem);

            diagramList.Add(diagramItem);

            // update diagram count
            int diagramId = int.Parse(diagramName.Split(ModelConstants.TagNameSeparator)[1]);
            counter.DiagramCount = Math.Max(counter.DiagramCount, diagramId + 1);
        }

        #endregion

        #region Wire Connection

        private static Tuple<double, double> PinGetPosition(IElement root, IThumb pin)
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

            return new Tuple<double, double>(x, y);
        }

        private void ConnectionCreate(ICanvas canvas, IThumb pin)
        {
            if (pin == null)
                return;

            CurrentOptions.CurrentRoot = pin.GetParent() as IThumb;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            var position = PinGetPosition(CurrentOptions.CurrentRoot, pin);
            double x = position.Item1;
            double y = position.Item2;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            ConnectionCreate(canvas, x, y);
        }

        private ILine ConnectionCreate(ICanvas canvas, double x, double y)
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
                result = ConnectionCreateFirst(canvas, x, y, tuples);
            }
            else
            {
                result = ConnectionCreateSecond(x, y, tuples);
            }

            return result;
        }

        private ILine ConnectionCreateFirst(ICanvas canvas, double x, double y, List<MapWire> tuples)
        {
            // update IsStartIO
            string rootUid = CurrentOptions.CurrentRoot.GetUid();

            bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) 
                || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            var line = WpfCreator.CreateWire(x, y, x, y,
                false, false,
                startIsIO, false,
                CurrentOptions.Counter.WireCount) as ILine;

            CurrentOptions.Counter.WireCount += 1;
            CurrentOptions.CurrentLine = line;

            // update connections
            var tuple = new MapWire(CurrentOptions.CurrentLine, CurrentOptions.CurrentRoot, null);
            tuples.Add(tuple);

            canvas.Add(CurrentOptions.CurrentLine);

            // line Tag is start root element
            if (CurrentOptions.CurrentLine != null || 
                !(CurrentOptions.CurrentLine is ILine))
            {
                CurrentOptions.CurrentLine.SetTag(CurrentOptions.CurrentRoot);
            }

            return line;
        }

        private ILine ConnectionCreateSecond(double x, double y, List<MapWire> tuples)
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

        private void WireRecreateConnections(ICanvas canvas,
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
                var startLine = ConnectionCreate(canvas, x1, y1);

                CurrentOptions.CurrentRoot = splitPin;
                ConnectionCreate(canvas, x, y);

                CurrentOptions.CurrentRoot = splitPin;
                var endLine = ConnectionCreate(canvas, x, y);

                CurrentOptions.CurrentRoot = endRoot;
                ConnectionCreate(canvas, x2, y2);

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

        private bool WireSplit(ICanvas canvas, ILine line, IPoint point)
        {
            if (CurrentOptions.CurrentLine == null)
            {
                HistoryAdd(canvas, true);
            }

            // create split pin
            var splitPin = InsertPin(canvas, point);
            CurrentOptions.CurrentRoot = splitPin;

            // connect current line to split pin
            double x = CurrentOptions.CurrentRoot.GetX();
            double y = CurrentOptions.CurrentRoot.GetY();

            ConnectionCreate(canvas, x, y);

            // remove original hit tested line
            canvas.Remove(line);

            // remove wire connections
            var connections = RemoveWireConnections(canvas, line);

            // connected original root element to split pin
            if (connections != null && connections.Count == 2)
            {
                WireRecreateConnections(canvas, line, splitPin, x, y, connections);
            }
            else
            {
                throw new InvalidOperationException("LineEx should have only two connections: Start and End.");
            }

            return true;
        }

        #endregion

        #region Insert

        public IElement InsertPin(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreatePin(point.X, point.Y, 
                CurrentOptions.Counter.PinCount,
                CurrentOptions.EnableSnap) as IThumb;

            CurrentOptions.Counter.PinCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertInput(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateInput(point.X, point.Y, 
                CurrentOptions.Counter.InputCount, 
                -1, 
                CurrentOptions.EnableSnap) as IThumb;

            CurrentOptions.Counter.InputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOutput(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateOutput(point.X, point.Y, 
                CurrentOptions.Counter.OutputCount, 
                -1, 
                CurrentOptions.EnableSnap) as IThumb;

            CurrentOptions.Counter.OutputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertAndGate(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateAndGate(point.X, point.Y, 
                CurrentOptions.Counter.AndGateCount, 
                CurrentOptions.EnableSnap) as IThumb;

            CurrentOptions.Counter.AndGateCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOrGate(ICanvas canvas, IPoint point)
        {
            var thumb = WpfCreator.CreateOrGate(point.X, point.Y, 
                CurrentOptions.Counter.OrGateCount, 
                CurrentOptions.EnableSnap) as IThumb;

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

        public static void DiagramSetSize(ICanvas canvas, double width, double height)
        {
            canvas.SetWidth(width);
            canvas.SetHeight(height);
        }

        public static string GridGenerate(double originX, double originY, double width, double height, double size)
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

        public static void GridGenerate(Path path, double originX, double originY, double width, double height, double size)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            string grid = GridGenerate(originX, originY, width, height, size);

            path.Data = Geometry.Parse(grid);

            sw.Stop();
            //System.Diagnostics.Debug.Print("GenerateGrid() in {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public void GridGenerate(bool undo)
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            if (UpdateDiagramProperties != null)
            {
                UpdateDiagramProperties();
            }

            if (undo == true)
            {
                HistoryAdd(canvas, false);
            }

            var prop = CurrentOptions.CurrentProperties;

            if (path != null)
            {
                GridGenerate(path,
                    prop.GridOriginX, prop.GridOriginY,
                    prop.GridWidth, prop.GridHeight,
                    prop.GridSize);
            }

            DiagramSetSize(canvas, prop.PageWidth, prop.PageHeight);
        }

        #endregion

        #region History

        private History HistoryGet(ICanvas canvas)
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

        public string HistoryAdd(ICanvas canvas, bool resetSelectedList)
        {
            if (CurrentOptions.EnableHistory != true)
                return null;

            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            var model = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

            undoHistory.Push(model);

            redoHistory.Clear();

            if (resetSelectedList == true)
            {
                SelectedListReset();
            }

            return model;
        }

        public List<string> HistoryDiagramModel(ICanvas canvas)
        {
            List<string> diagrams = null;

            var currentDiagram = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

            var history = HistoryGet(canvas);
            var undoHistory = history.Item1;
            var redoHistory = history.Item2;

            diagrams = new List<string>(undoHistory.Reverse());

            diagrams.Add(currentDiagram);

            return diagrams;
        }

        private void HistoryRollbackUndo(ICanvas canvas)
        {
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // remove unused history
            undoHistory.Pop();
        }

        private void HistoryRollbackRedo(ICanvas canvas)
        {
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // remove unused history
            redoHistory.Pop();
        }

        public void HistoryClear(ICanvas canvas)
        {
            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            undoHistory.Clear();
            redoHistory.Clear();
        }

        private void HistoryUndo(ICanvas canvas, Path path, bool pushRedo)
        {
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (undoHistory.Count <= 0)
                return;

            // save current model
            if (pushRedo == true)
            {
                var current = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);
                redoHistory.Push(current);
            }

            // resotore previous model
            var model = undoHistory.Pop();

            ModelClear(canvas);
            ModelParseDiagram(model, canvas, path, 0, 0, false, true, false, true);
        }

        private void HistoryRedo(ICanvas canvas, Path path, bool pushUndo)
        {
            if (CurrentOptions.EnableHistory != true)
                return;

            var tuple = HistoryGet(canvas);
            var undoHistory = tuple.Item1;
            var redoHistory = tuple.Item2;

            if (redoHistory.Count <= 0)
                return;

            // save current model
            if (pushUndo == true)
            {
                var current = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);
                undoHistory.Push(current);
            }

            // resotore previous model
            var model = redoHistory.Pop();

            ModelClear(canvas);
            ModelParseDiagram(model, canvas, path, 0, 0, false, true, false, true);
        }

        public void HistoryUndo()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            this.HistoryUndo(canvas, path, true);
        }

        public void HistoryRedo()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var path = CurrentOptions.CurrentPathGrid;

            this.HistoryRedo(canvas, path, true);
        }

        #endregion

        #region Move

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
            HistoryAdd(canvas, false);

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
            HistoryAdd(canvas, false);

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
            HistoryAdd(canvas, false);

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
            HistoryAdd(canvas, false);

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
            bool snap = (CurrentOptions.SnapOnRelease == true && 
                CurrentOptions.EnableSnap == true) ? false : CurrentOptions.EnableSnap;

            if (CurrentOptions.MoveAllSelected == true)
            {
                MoveSelectedElements(canvas, dX, dY, snap);
            }
            else
            {
                MoveRoot(element, dX, dY, snap);
            }
        }

        public void DragStart(ICanvas canvas, IThumb element)
        {
            HistoryAdd(canvas, false);

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

        #region Delete

        private static bool IsElementPin(string uid)
        {
            return uid != null &&
                   StringUtil.StartsWith(uid, ModelConstants.TagElementPin);
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

        private static Connections RemoveWireConnections(ICanvas canvas, ILine line)
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

        private static void DeleteElement(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;
            if (element == null)
                return;

            DeleteElement(canvas, element);
        }

        private static void DeleteElement(ICanvas canvas, IElement element)
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

        public void Delete(ICanvas canvas, IPoint point)
        {
            HistoryAdd(canvas, true);

            DeleteElement(canvas, point);

            CurrentOptions.SkipLeftClick = false;
        }

        #endregion

        #region Wire

        public ILine WireFind(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault();
            if (element == null)
                return null;

            string uid = element.GetUid();

            //System.Diagnostics.Debug.Print("FindLineEx, element: {0}, uid: {1}, parent: {2}", 
            //    element.GetType(), element.GetUid(), element.Parent.GetType());

            if (element is ILine && uid != null &&
                StringUtil.StartsWith(uid, ModelConstants.TagElementWire))
            {
                var line = element as ILine;
                return line;
            }

            return null;
        }

        public void WireToggleStart(ICanvas canvas, IPoint point)
        {
            var line = WireFind(canvas, point);

            if (line != null)
            {
                HistoryAdd(canvas, false);

                line.SetStartVisible(line.GetStartVisible() == true ? false : true);

                CurrentOptions.SkipLeftClick = false;
            }
        }

        public void WireToggleEnd(ICanvas canvas, IPoint point)
        {
            var line = WireFind(canvas, point);

            if (line != null)
            {
                HistoryAdd(canvas, false);

                line.SetEndVisible(line.GetEndVisible() == true ? false : true);

                CurrentOptions.SkipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save

        private void OpenDiagram(string fileName, ICanvas canvas, Path path)
        {
            string diagram = Model.Open(fileName);

            HistoryAdd(canvas, true);

            ModelClear(canvas);
            ModelParseDiagram(diagram, canvas, path, 0, 0, false, true, false, true);
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

        private TreeSolution OpenSolutionModel(string fileName)
        {
            TreeSolution solution = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                solution = ModelParseDiagram(diagram, null, null, 0, 0, false, false, false, false);
            }

            return solution;
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

                ModelClear(canvas);

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
                TreeOpenSolution(tree, solution);
            }
        }

        private void SaveDiagram(string fileName, ICanvas canvas)
        {
            string model = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

            Model.Save(fileName, model);
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

        private void SaveSolution(string fileName)
        {
            ModelUpdateSelectedDiagram();

            var model = ModelGenerateSolution(fileName, false).Item1;
            Model.Save(fileName, model);
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

                TagsUpdate();

                SaveSolution(fileName);
            }
        }

        #endregion

        #region Dxf

        public string DxfGenerate(string model, 
            bool shortenStart, 
            bool shortenEnd,
            DxfAcadVer version,
            DiagramTable table)
        {
            var dxf = new DxfDiagramCreator()
            {
                ShortenStart = shortenStart,
                ShortenEnd = shortenEnd,
                DiagramProperties  = CurrentOptions.CurrentProperties,
                Tags = CurrentOptions.Tags
            };

            return dxf.GenerateDxf(model, version, table);
        }

        private void DxfSave(string fileName, string model)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(model);
            }
        }

        private void DxfExportDiagram(string fileName, 
            ICanvas canvas, 
            bool shortenStart, 
            bool shortenEnd,
            DxfAcadVer version,
            DiagramTable table)
        {
            string model = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

            string dxf = DxfGenerate(model, shortenStart, shortenEnd, version, table);

            DxfSave(fileName, dxf);
        }

        public void DxfExport(bool shortenStart, bool shortenEnd, DiagramTable table)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Dxf R12 (*.dxf)|*.dxf|Dxf AutoCAD 2000 (*.dxf)|*.dxf|All Files (*.*)|*.*",
                FilterIndex = 2,
                Title = "Export Diagram to Dxf",
                FileName = "diagram"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var canvas = CurrentOptions.CurrentCanvas;

                DxfAcadVer version;
                switch(dlg.FilterIndex)
                {
                    case 1: 
                        version = DxfAcadVer.AC1009; 
                        break;
                    case 2: 
                        version = DxfAcadVer.AC1015; 
                        break;
                    case 3:
                    default: 
                        version = DxfAcadVer.AC1015; 
                        break;
                }

                this.DxfExportDiagram(dlg.FileName, 
                    canvas, 
                    shortenStart, 
                    shortenEnd, 
                    version,
                    table);
            }
        }

        #endregion

        #region Clipboard

        public string ClipboardGetText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);

                return ClipboardText;
            }

            return ClipboardText;
        }

        public void ClipboardSetText(string model)
        {
            try
            {
                ClipboardText = model;

                Clipboard.SetText(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        #endregion

        #region Edit

        public void EditCut()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            string model = ModelGenerateFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);

                var elements = Elements.GetAll(canvas);

                EditDelete(canvas, elements);
            }
            else
            {
                EditDelete();
            }

            ClipboardSetText(model);
        }

        public void EditCopy()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            string model = ModelGenerateFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Model.Generate(canvas, null, CurrentOptions.CurrentProperties);
            }

            ClipboardSetText(model);
        }

        public void EditPaste(IPoint point)
        {
            var model = ClipboardGetText();

            if (model != null && model.Length > 0)
            {
                ModelInsert(model, point.X, point.Y);
            }
        }

        public void EditDelete()
        {
            var canvas = CurrentOptions.CurrentCanvas;
            var elements = Elements.GetSelected(canvas);

            EditDelete(canvas, elements);
        }

        public void EditDelete(ICanvas canvas, IEnumerable<IElement> elements)
        {
            HistoryAdd(canvas, true);

            EditDeleteThumbsAndLines(canvas, elements);
        }

        private void EditDeleteThumbsAndLines(ICanvas canvas, IEnumerable<IElement> elements)
        {
            foreach (var element in elements)
            {
                DeleteElement(canvas, element);
            }
        }

        #endregion

        #region Selection

        public IEnumerable<IElement> GetElementsSelected()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Elements.GetSelected(canvas);
        }

        public IEnumerable<IElement> GetElementsSelectedThumb()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Elements.GetSelectedThumbs(canvas);
        }

        public IEnumerable<IElement> GetElementsThumb()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Elements.GetThumbs(canvas);
        }

        public IEnumerable<IElement> GetElementsAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            return Elements.GetAll(canvas);
        }

        public void SelectAll()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            Editor.SelectAll(canvas);
        }

        public void SelectNone()
        {
            var canvas = CurrentOptions.CurrentCanvas;

            Editor.SelectNone(canvas);
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
            var elements = Elements.GetThumbs(canvas);

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
            var elements = Elements.GetThumbs(canvas);

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

        public void SelectedListReset()
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
                    SelectNone();
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

        #region Mouse Conditions

        private bool IsSameUid(string uid1, string uid2)
        {
            return StringUtil.Compare(uid2, uid1) == false;
        }

        private bool IsWire(string elementUid)
        {
            return StringUtil.StartsWith(elementUid, ModelConstants.TagElementWire) == true;
        }

        private bool CanConnect()
        {
            return CurrentOptions.CurrentRoot != null &&
                   CurrentOptions.CurrentLine != null;
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
                IsSameUid(elementUid, lineUid) &&
                IsWire(elementUid);
        }

        private bool CanToggleLine()
        {
            return CurrentOptions.CurrentRoot == null &&
                CurrentOptions.CurrentLine == null &&
                Keyboard.Modifiers != ModifierKeys.Control;
        }

        private bool CanConnectToPin(IThumb pin)
        {
            return pin != null &&
            (
                !StringUtil.Compare(pin.GetUid(), ResourceConstants.StandalonePinName)
                || Keyboard.Modifiers == ModifierKeys.Control
            );
        }

        private bool CanMoveCurrentLine()
        {
            return CurrentOptions.CurrentRoot != null &&
                CurrentOptions.CurrentLine != null;
        }

        #endregion

        #region Mouse Helpers

        private void MouseCreateCanvasConnection(ICanvas canvas, IPoint point)
        {
            var root = InsertPin(canvas, point);

            CurrentOptions.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

            double x = CurrentOptions.CurrentRoot.GetX();
            double y = CurrentOptions.CurrentRoot.GetY();

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            ConnectionCreate(canvas, x, y);

            CurrentOptions.CurrentRoot = root;
            ConnectionCreate(canvas, x, y);
        }

        private IElement MouseGetElementAtPoint(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0)
                .Where(x => StringUtil.Compare(CurrentOptions.CurrentLine.GetUid(), x.GetUid()) == false)
                .FirstOrDefault() as IElement;

            return element;
        }

        private void MouseRemoveCurrentLine(ICanvas canvas)
        {
            var selection = CurrentOptions.CurrentRoot.GetTag() as Selection;
            var tuples = selection.Item2;

            var last = tuples.LastOrDefault();
            tuples.Remove(last);

            canvas.Remove(CurrentOptions.CurrentLine);
        }

        private void MouseToggleLineSelection(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;

            if (element != null)
            {
                Editor.SelectionToggleWire(element);
            }
            else
            {
                Editor.SetLinesSelection(canvas, false);
            }
        }

        #endregion

        #region Mouse Event

        public void MouseEventLeftDown(ICanvas canvas, IPoint point)
        {
            if (CanConnect())
            {
                 MouseCreateCanvasConnection(canvas, point);
            }
            else if (CurrentOptions.EnableInsertLast == true)
            {
                HistoryAdd(canvas, true);

                InsertLast(canvas, CurrentOptions.LastInsert, point);
            }
        }

        public bool MouseEventPreviewLeftDown(ICanvas canvas, IPoint point, IThumb pin)
        {
            if (CanConnectToPin(pin))
            {
                if (CurrentOptions.CurrentLine == null)
                {
                    HistoryAdd(canvas, true);
                }

                ConnectionCreate(canvas, pin);

                return true;
            }
            else if (CurrentOptions.CurrentLine != null)
            {
                var element = MouseGetElementAtPoint(canvas, point);

                System.Diagnostics.Debug.Print("Split wire: {0}", element == null ? "<null>" : element.GetUid());

                if (CanSplitWire(element))
                {
                    return WireSplit(canvas, element as ILine, point);
                }
            }

            if (CanToggleLine())
            {
                MouseToggleLineSelection(canvas, point);
            }

            return false;
        }

        public void MouseEventMove(ICanvas canvas, IPoint point)
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

        public bool MouseEventRightDown(ICanvas canvas, Path path)
        {
            if (CurrentOptions.CurrentRoot != null && 
                CurrentOptions.CurrentLine != null)
            {
                if (CurrentOptions.EnableHistory == true)
                {
                    HistoryUndo(canvas, path, false);
                }
                else
                {
                    MouseRemoveCurrentLine(canvas);
                }

                CurrentOptions.CurrentLine = null;
                CurrentOptions.CurrentRoot = null;

                return true;
            }

            return false;
        }

        #endregion

        #region Path

        public static string PathMakeRelative(string fromPath, string toPath)
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

        public static string PathGetRelativeFileName(string fromPath, string toPath)
        {
            string relativePath = PathMakeRelative(toPath, fromPath);
            string onlyFileName = System.IO.Path.GetFileName(toPath);

            if (relativePath != null)
            {
                toPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(relativePath),
                    onlyFileName);
            }

            return toPath;
        }

        #endregion

        #region Tree

        public void TreeSelectPreviousItem(bool selectParent)
        {
            var tree = CurrentOptions.CurrentTree;

            // get current diagram
            var selected = tree.GetSelectedItem() as ITreeItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.GetUid(), ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.GetParent() as SolutionTreeViewItem;
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
                        TreeSelectPreviousParentItem(parent);
                    }
                }
            }
        }

        private static void TreeSelectPreviousParentItem(SolutionTreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as SolutionTreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex > 0)
            {
                TreeSelectLastItemInPreviousProject(parentParent, parentIndex);
            }
        }

        private static void TreeSelectLastItemInPreviousProject(SolutionTreeViewItem parentParent, int parentIndex)
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

        public void TreeSelectNextItem(bool selectParent)
        {
            var tree = CurrentOptions.CurrentTree;

            // get current diagram
            var selected = tree.GetSelectedItem() as SolutionTreeViewItem;

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
                        TreeSelectNextParentItem(parent);
                    }
                }
            }
        }

        private static void TreeSelectNextParentItem(SolutionTreeViewItem parent)
        {
            // get parent of current project
            var parentParent = parent.Parent as SolutionTreeViewItem;
            int parentIndex = parentParent.Items.IndexOf(parent);
            int parentCount = parentParent.Items.Count;

            if (parentCount > 0 && parentIndex < parentCount - 1)
            {
                TreeSelectFirstItemInNextProject(parentParent, parentIndex);
            }
        }

        private static void TreeSelectFirstItemInNextProject(SolutionTreeViewItem parentParent, int parentIndex)
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

        public bool TreeSwitchItems(ICanvas canvas, SolutionTreeViewItem oldItem, SolutionTreeViewItem newItem)
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
                ModelStore(canvas, oldItem);
            }

            if (isNewItemDiagram == true)
            {
                // load new model
                ModelLoad(canvas, newItem);
            }

            System.Diagnostics.Debug.Print("Old Uid: {0}, new Uid: {1}", oldUid, newUid);

            return isNewItemDiagram;
        }
        
        private ITreeItem TreeCreateSolutionItem(string uid)
        {
            var solution = CreateTreeSolutionItem();

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = 0; // there is only one solution allowed

                solution.SetUid(ModelConstants.TagHeaderSolution + ModelConstants.TagNameSeparator + id.ToString());
                counter.SolutionCount = id++;
            }
            else
            {
                solution.SetUid(uid);
            }

            return solution;
        }

        private ITreeItem TreeCreateProjectItem(string uid)
        {
            var project = CreateTreeProjectItem();

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = counter.ProjectCount;

                project.SetUid(ModelConstants.TagHeaderProject + ModelConstants.TagNameSeparator + id.ToString());
                counter.ProjectCount++;
            }
            else
            {
                project.SetUid(uid);
            }

            return project;
        }

        private ITreeItem TreeCreateDiagramItem(string uid)
        {
            var diagram = CreateTreeDiagramItem();

            if (uid == null)
            {
                var counter = CurrentOptions.Counter;
                int id = counter.DiagramCount;

                diagram.SetUid(ModelConstants.TagHeaderDiagram + ModelConstants.TagNameSeparator + id.ToString());
                counter.DiagramCount++;
            }
            else
            {
                diagram.SetUid(uid);
            }

            return diagram;
        }

        public TreeItemType TreeAddNewItem()
        {
            var tree= CurrentOptions.CurrentTree;
            var selected = tree.GetSelectedItem() as ITreeItem;

            string uid = selected.GetUid();
            bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
            bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
            bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

            if (isSelectedDiagram == true)
            {
                var project = selected.GetParent() as ITreeItem;

                TreeAddDiagram(project, true);
                return TreeItemType.Diagram;
            }
            else if (isSelectedProject == true)
            {
                TreeAddDiagram(selected, false);
                return TreeItemType.Diagram;
            }
            else if (isSelectedSolution == true)
            {
                TreeAddProject(selected);
                return TreeItemType.Project;
            }

            return TreeItemType.None;
        }

        public void TreeAddNewItemAndPaste()
        {
            var newItemType = TreeAddNewItem();
            if (newItemType == TreeItemType.Diagram)
            {
                var point = new PointEx(0.0, 0.0);
                EditPaste(point);
            }
        }

        public void TreeAddProject(ITreeItem solution)
        {
            var project = TreeCreateProjectItem(null);

            solution.Add(project);

            System.Diagnostics.Debug.Print("Added project: {0} to solution: {1}",
                project.GetUid(), 
                solution.GetUid());
        }

        public void TreeAddDiagram(ITreeItem project, bool select)
        {
            var diagram = TreeCreateDiagramItem(null);

            project.Add(diagram);

            ModelStore(null, diagram);

            if (select == true)
            {
                diagram.SetSelected(true);
            }

            System.Diagnostics.Debug.Print("Added diagram: {0} to project: {1}", 
                diagram.GetUid(), 
                project.GetUid());
        }

        private void TreeDeleteSolution(ITreeItem solution)
        {
            var tree = solution.GetParent() as ITree;

            var projects = solution.GetItems().ToList();

            foreach (var project in projects)
            {
                var diagrams = project.GetItems().ToList();

                foreach (var diagram in diagrams)
                {
                    project.Remove(diagram);
                }

                solution.Remove(project);
            }

            tree.Remove(solution as ITreeItem);
        }

        public void TreeDeleteProject(SolutionTreeViewItem project)
        {
            var solution = project.Parent as SolutionTreeViewItem;
            var diagrams = project.Items.Cast<SolutionTreeViewItem>().ToList();

            foreach (var diagram in diagrams)
            {
                project.Items.Remove(diagram);
            }

            solution.Items.Remove(project);
        }

        public void TreeDeleteDiagram(SolutionTreeViewItem diagram)
        {
            var project = diagram.Parent as SolutionTreeViewItem;

            project.Items.Remove(diagram);
        }

        private void TreeOpenSolution(ITree tree, TreeSolution solution)
        {
            TreeClearSolution();

            TreeParseSolution(tree, solution);
        }

        private void TreeParseSolution(ITree tree, TreeSolution solution)
        {
            var counter = CurrentOptions.Counter;

            // create solution
            string tagFileName = null;

            string solutionName = solution.Item1;
            tagFileName = solution.Item2;
            var projects = solution.Item3.Reverse();

            TagsLoad(tagFileName);

            //System.Diagnostics.Debug.Print("Solution: {0}", name);

            var solutionItem = TreeCreateSolutionItem(solutionName);
            tree.Add(solutionItem);

            ModelParseProjects(projects, counter, solutionItem);
        }

        private void TreeClearSolution()
        {
            var tree = CurrentOptions.CurrentTree;

            // clear solution tree
            TreeClear(tree);

            // reset counter
            CurrentOptions.Counter.ResetAll();

            TagsReset();

            SelectedListReset();

            ElementThumb.SetItems(CurrentOptions.CurrentCanvas, null);
        }

        private void TreeClear(ITree tree)
        {
            var items = tree.GetItems().ToList();

            foreach (var item in items)
            {
                TreeDeleteSolution(item);
            }
        }

        public void TreeCreateNewSolution()
        {
            var tree = CurrentOptions.CurrentTree;
            var canvas = CurrentOptions.CurrentCanvas;

            ModelClear(canvas);

            TreeClearSolution();

            TreeCreateDefaultSolution(tree);
        }

        public void TreeCreateDefaultSolution(ITree tree)
        {
            var solutionItem = TreeCreateSolutionItem(null);
            tree.Add(solutionItem);

            var projectItem = TreeCreateProjectItem(null);
            solutionItem.Add(projectItem);

            var diagramItem = TreeCreateDiagramItem(null);
            projectItem.Add(diagramItem);

            diagramItem.SetSelected(true);
        }

        #endregion

        #region Tags

        public void TagsResetThumbs(ICanvas canvas)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>().Where(x => x.GetTag() != null);
            var selectedThumbs = thumbs.Where(x => x.GetSelected());

            if (selectedThumbs.Count() > 0)
            {
                // reset selected tags
                foreach (var thumb in selectedThumbs)
                {
                    thumb.SetData(null);
                }
            }
            else
            {
                // reset all tags
                foreach (var thumb in thumbs)
                {
                    thumb.SetData(null);
                }
            }
        }

        public void TagsOpen()
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

                var tags = Tags.Open(tagFileName);

                CurrentOptions.TagFileName = tagFileName;
                CurrentOptions.Tags = tags;
            }
        }

        public void TagsSave()
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

                Tags.Export(tagFileName, CurrentOptions.Tags);

                CurrentOptions.TagFileName = tagFileName;
            }
        }

        public void TagsImport()
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

                Tags.Import(tagFileName, CurrentOptions.Tags, true);
            }
        }

        public void TagsExport()
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

                Tags.Export(tagFileName, CurrentOptions.Tags);
            }
        }

        private void TagsUpdate()
        {
            string tagFileName = CurrentOptions.TagFileName;
            var tags = CurrentOptions.Tags;

            if (tagFileName != null && tags != null)
            {
                Tags.Export(tagFileName, tags);
            }
            else if (tagFileName == null && tags != null)
            {
                TagsSave();
            }
        }

        private void TagsLoad(string tagFileName)
        {
            // load tags
            if (tagFileName != null)
            {
                CurrentOptions.TagFileName = tagFileName;

                try
                {
                    var tags = Tags.Open(tagFileName);

                    CurrentOptions.Tags = tags;

                    ElementThumb.SetItems(CurrentOptions.CurrentCanvas, tags);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Failed to load tags from file: {0}, error: {1}", tagFileName, ex.Message);
                }
            }
        }

        private void TagsReset()
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
