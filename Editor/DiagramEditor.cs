// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Dxf.Enums;
using CanvasDiagramEditor.Core;
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

    #region DiagramEditor

    public class DiagramEditor
    {
        #region Properties

        public Context Context { get; set; }

        #endregion

        #region Constructor

        public DiagramEditor()
        {
        }

        #endregion

        #region Model

        public void ModelClear()
        {
            var canvas = Context.CurrentCanvas;

            HistoryAdd(canvas, true);

            Model.Clear(canvas);
        }

        public string ModelGenerate()
        {
            var canvas = Context.CurrentCanvas;

            var diagram = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

            return diagram;
        }

        public string ModelGenerateFromSelected(ICanvas canvas)
        {
            return Model.Generate(Model.GetSelected(canvas));
        }

        public void ModelInsert(string diagram, double offsetX, double offsetY, bool select)
        {
            var canvas = Context.CurrentCanvas;

            HistoryAdd(canvas, true);

            SelectNone();
            Model.Parse(diagram, 
                canvas, Context.DiagramCreator, 
                offsetX, offsetY,
                true, true, select, true);
        }

        public void ModelResetThumbTags()
        {
            var canvas = Context.CurrentCanvas;

            HistoryAdd(canvas, true);

            TagsResetThumbs(canvas);
        }

        public string ModelGetCurrent(bool update)
        {
            var tree = Context.CurrentTree;
            var canvas = Context.CurrentCanvas;
            var item = tree.GetSelectedItem() as ITreeItem;

            if (item != null)
            {
                string uid = item.GetUid();
                bool isDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var model = Model.GenerateDiagram(canvas, uid, canvas.GetProperties());

                    if (update == true)
                    {
                        item.SetTag(new Diagram(model, canvas.GetTag() as UndoRedo));
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

        public Solution ModelGenerateSolution(string fileName, bool includeHistory)
        {
            var tree = Context.CurrentTree;
            var tagFileName = Context.TagFileName;

            return Model.GenerateSolution(tree, fileName, tagFileName, includeHistory);
        }

        public IEnumerable<string> ModelGetCurrentProjectDiagrams()
        {
            var tree = Context.CurrentTree;
            var selected = tree.GetSelectedItem() as ITreeItem;
            if (selected == null)
                return null;

            string uid = selected.GetUid();
            bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
            bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
            bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

            if (isSelectedDiagram == true)
            {
                var project = selected.GetParent() as ITreeItem;

                var models = new List<string>();

                Model.GenerateProject(project, models, false);

                return models;

            }
            else if (isSelectedProject == true)
            {
                var models = new List<string>();

                Model.GenerateProject(selected, models, false);

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
                        Model.GenerateProject(project, models, false);

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

        private void ConnectionCreate(ICanvas canvas, IThumb pin)
        {
            if (pin == null)
                return;

            Context.CurrentRoot = pin.GetParent() as IThumb;

            //System.Diagnostics.Debug.Print("ConnectPins, pin: {0}, {1}", pin.GetType(), pin.Name);

            var position = Model.GetPinPosition(Context.CurrentRoot, pin);
            double x = position.Item1;
            double y = position.Item2;

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            ConnectionCreate(canvas, x, y);
        }

        private ILine ConnectionCreate(ICanvas canvas, double x, double y)
        {
            ILine result = null;

            var rootTag = Context.CurrentRoot.GetTag();
            if (rootTag == null)
            {
                Context.CurrentRoot.SetTag(new Selection(false, new List<MapWire>()));
            }

            var selection = Context.CurrentRoot.GetTag() as Selection;
            var tuples = selection.Item2;

            if (Context.CurrentLine == null)
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
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();
            string rootUid = Context.CurrentRoot.GetUid();

            bool startIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) 
                || StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            var line = creator.CreateElement(ModelConstants.TagElementWire,
                new object[] 
                {
                    x, y,
                    x, y,
                    false, false,
                    startIsIO, false,
                    counter.WireCount
                },
                0.0, 0.0, false) as ILine;

            counter.WireCount += 1;
            Context.CurrentLine = line;

            // update connections
            var tuple = new MapWire(Context.CurrentLine, Context.CurrentRoot, null);
            tuples.Add(tuple);

            canvas.Add(Context.CurrentLine);

            // line Tag is start root element
            if (Context.CurrentLine != null || 
                !(Context.CurrentLine is ILine))
            {
                Context.CurrentLine.SetTag(Context.CurrentRoot);
            }

            return line;
        }

        private ILine ConnectionCreateSecond(double x, double y, List<MapWire> tuples)
        {
            var margin = Context.CurrentLine.GetMargin();

            Context.CurrentLine.SetX2(x - margin.Left);
            Context.CurrentLine.SetY2(y - margin.Top);

            // update IsEndIO flag
            string rootUid = Context.CurrentRoot.GetUid();

            bool endIsIO = StringUtil.StartsWith(rootUid, ModelConstants.TagElementInput) ||
                StringUtil.StartsWith(rootUid, ModelConstants.TagElementOutput);

            Context.CurrentLine.SetEndIO(endIsIO);

            // update connections
            var tuple = new MapWire(Context.CurrentLine, null, Context.CurrentRoot);
            tuples.Add(tuple);

            // line Tag is start root element
            var line = Context.CurrentLine;
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
                        line.SetTag(new Tuple<object, object>(start, Context.CurrentRoot));
                    }
                }
            }

            var result = Context.CurrentLine;

            // reset current line and root
            Context.CurrentLine = null;
            Context.CurrentRoot = null;

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

                Context.CurrentRoot = startRoot;
                var startLine = ConnectionCreate(canvas, x1, y1);

                Context.CurrentRoot = splitPin;
                ConnectionCreate(canvas, x, y);

                Context.CurrentRoot = splitPin;
                var endLine = ConnectionCreate(canvas, x, y);

                Context.CurrentRoot = endRoot;
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
            if (Context.CurrentLine == null)
            {
                HistoryAdd(canvas, true);
            }

            // create split pin
            var splitPin = InsertPin(canvas, point);
            Context.CurrentRoot = splitPin;

            // connect current line to split pin
            double x = Context.CurrentRoot.GetX();
            double y = Context.CurrentRoot.GetY();

            ConnectionCreate(canvas, x, y);

            // remove original hit tested line
            canvas.Remove(line);

            // remove wire connections
            var connections = Model.RemoveWireConnections(canvas, line);

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
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(ModelConstants.TagElementPin,
                new object[] { counter.PinCount },
                point.X, point.Y, Context.EnableSnap) as IThumb;

            counter.PinCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertInput(ICanvas canvas, IPoint point)
        {
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(ModelConstants.TagElementInput,
                new object[] { counter.InputCount, -1 },
                point.X, point.Y, Context.EnableSnap) as IThumb;

            counter.InputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOutput(ICanvas canvas, IPoint point)
        {
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(ModelConstants.TagElementOutput,
                new object[] { counter.OutputCount, -1 },
                point.X, point.Y, Context.EnableSnap) as IThumb;

            counter.OutputCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertAndGate(ICanvas canvas, IPoint point)
        {
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(ModelConstants.TagElementAndGate,
                new object[] { counter.AndGateCount },
                point.X, point.Y, Context.EnableSnap) as IThumb;

            counter.AndGateCount += 1;

            canvas.Add(thumb);

            return thumb;
        }

        public IElement InsertOrGate(ICanvas canvas, IPoint point)
        {
            var creator = Context.DiagramCreator;
            var counter = canvas.GetCounter();

            var thumb = creator.CreateElement(ModelConstants.TagElementOrGate,
                new object[] { counter.OrGateCount },
                point.X, point.Y, Context.EnableSnap) as IThumb;

            counter.OrGateCount += 1;

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

        #region History

        public string HistoryAdd(ICanvas canvas, bool resetSelectedList)
        {
            if (Context.EnableHistory != true)
                return null;

            var model = History.Add(canvas);

            if (resetSelectedList == true)
                SelectedListReset();

            return model;
        }

        public void HistoryUndo()
        {
            if (Context.EnableHistory == true)
            {
                var canvas = Context.CurrentCanvas;
                var creator = Context.DiagramCreator;

                History.Undo(canvas, creator, true);
            }
        }

        public void HistoryRedo()
        {
            if (Context.EnableHistory == true)
            {
                var canvas = Context.CurrentCanvas;
                var creator = Context.DiagramCreator;

                History.Redo(canvas, creator, true);
            }
        }

        #endregion

        #region Move

        private static Tuple<PointEx, PointEx> GetLineExStartAndEnd(MapWire map1, MapWire map2)
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

        public void SetPosition(IElement element, double left, double top, bool snap)
        {
            element.SetX(SnapOffsetX(left, snap));
            element.SetY(SnapOffsetY(top, snap));
        }

        private void MoveRoot(IElement element, double dX, double dY, bool snap)
        {
            double left = element.GetX() + dX;
            double top = element.GetY() + dY;

            SetPosition(element, left, top, snap);

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
            var thumbs = canvas.GetElements().OfType<IThumb>().Where(x => x.GetSelected());

            foreach (var thumb in thumbs)
            {
                MoveRoot(thumb, dX, dY, snap);
            }
        }

        public void MoveLeft(ICanvas canvas)
        {
            HistoryAdd(canvas, false);

            if (Context.EnableSnap == true)
            {
                double delta = canvas.GetProperties().GridSize;
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

            if (Context.EnableSnap == true)
            {
                double delta = canvas.GetProperties().GridSize;
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

            if (Context.EnableSnap == true)
            {
                double delta = canvas.GetProperties().GridSize;
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

            if (Context.EnableSnap == true)
            {
                double delta = canvas.GetProperties().GridSize;
                MoveSelectedElements(canvas, 0.0, delta, false);
            }
            else
            {
                MoveSelectedElements(canvas, 0.0, 1.0, false);
            }
        }

        #endregion

        #region Drag

        private bool IsSnapOnDragEnabled()
        {
            return (Context.SnapOnRelease == true &&
                Context.EnableSnap == true) ? false : Context.EnableSnap;
        }

        public void Drag(ICanvas canvas, IThumb element, double dX, double dY)
        {
            bool snap = IsSnapOnDragEnabled();

            if (Context.MoveAllSelected == true)
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
                Context.MoveAllSelected = true;
            }
            else
            {
                Context.MoveAllSelected = false;

                element.SetSelected(true);
            }
        }

        public void DragEnd(ICanvas canvas, IThumb element)
        {
            if (Context.SnapOnRelease == true && Context.EnableSnap == true)
            {
                if (Context.MoveAllSelected == true)
                {
                    MoveSelectedElements(canvas, 0.0, 0.0, Context.EnableSnap);
                }
                else
                {
                    element.SetSelected(false);

                    MoveRoot(element, 0.0, 0.0, Context.EnableSnap);
                }
            }
            else
            {
                if (Context.MoveAllSelected != true)
                    element.SetSelected(false);
            }

            Context.MoveAllSelected = false;
        }

        #endregion

        #region Delete

        public void Delete(ICanvas canvas, IPoint point)
        {
            HistoryAdd(canvas, true);

            Model.DeleteElement(canvas, point);

            Context.SkipLeftClick = false;
        }

        #endregion

        #region Wire

        public void WireToggleStart()
        {
            var canvas = Context.CurrentCanvas;
            var wires = Model.GetSelectedWires(canvas);

            if (wires.Count() > 0)
            {
                HistoryAdd(canvas, false);

                foreach (var wire in wires.Cast<ILine>())
                {
                    wire.SetStartVisible(wire.GetStartVisible() == true ? false : true);
                }
            }
        }

        public void WireToggleEnd()
        {
            var canvas = Context.CurrentCanvas;
            var wires = Model.GetSelectedWires(canvas);

            if (wires.Count() > 0)
            {
                HistoryAdd(canvas, false);

                foreach (var wire in wires.Cast<ILine>())
                {
                    wire.SetEndVisible(wire.GetEndVisible() == true ? false : true);
                }
            }
        }

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

                Context.SkipLeftClick = false;
            }
        }

        public void WireToggleEnd(ICanvas canvas, IPoint point)
        {
            var line = WireFind(canvas, point);

            if (line != null)
            {
                HistoryAdd(canvas, false);

                line.SetEndVisible(line.GetEndVisible() == true ? false : true);

                Context.SkipLeftClick = false;
            }
        }

        #endregion

        #region Open/Save Diagram

        private void OpenDiagram(string fileName, ICanvas canvas)
        {
            string diagram = Model.Open(fileName);

            HistoryAdd(canvas, true);

            Model.Clear(canvas);
            Model.Parse(diagram,
                canvas, Context.DiagramCreator, 
                0, 0, 
                false, true, false, true);
        }

        public void SaveDiagram(string fileName, ICanvas canvas)
        {
            string model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

            Model.Save(fileName, model);
        }

        #endregion

        #region Open/Save Solution

        public TreeSolution OpenSolution(string fileName)
        {
            TreeSolution solution = null;

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string diagram = reader.ReadToEnd();

                solution = Model.Parse(diagram,
                    null, Context.DiagramCreator, 
                    0, 0, 
                    false, false, false, false);
            }

            return solution;
        }

        public void SaveSolution(string fileName)
        {
            ModelUpdateSelectedDiagram();

            var model = ModelGenerateSolution(fileName, false).Item1;

            Model.Save(fileName, model);
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
                DiagramProperties = Context.CurrentCanvas.GetProperties(),
                Tags = Context.Tags
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

        public void DxfExportDiagram(string fileName, 
            ICanvas canvas, 
            bool shortenStart, 
            bool shortenEnd,
            DxfAcadVer version,
            DiagramTable table)
        {
            string model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

            string dxf = DxfGenerate(model, shortenStart, shortenEnd, version, table);

            DxfSave(fileName, dxf);
        }

        #endregion

        #region Edit

        public void EditCut()
        {
            var canvas = Context.CurrentCanvas;
            string model = ModelGenerateFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

                var elements = Model.GetAll(canvas);

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
            var canvas = Context.CurrentCanvas;
            string model = ModelGenerateFromSelected(canvas);

            if (model.Length == 0)
            {
                model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());
            }

            ClipboardSetText(model);
        }

        public void EditPaste(IPoint point, bool select)
        {
            var model = ClipboardGetText();

            if (model != null && model.Length > 0)
            {
                double offsetX = point.X != 0.0 ? SnapOffsetX(point.X, Context.EnableSnap) : 0.0;
                double offsetY = point.Y != 0.0 ? SnapOffsetY(point.Y, Context.EnableSnap) : 0.0;

                ModelInsert(model, offsetX, offsetY, select);
            }
        }

        public void EditDelete()
        {
            var canvas = Context.CurrentCanvas;
            var elements = Model.GetSelected(canvas);

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
                Model.DeleteElement(canvas, element);
            }
        }

        #endregion

        #region Selection

        public IEnumerable<IElement> GetElementsSelected()
        {
            var canvas = Context.CurrentCanvas;

            return Model.GetSelected(canvas);
        }

        public IEnumerable<IElement> GetElementsSelectedThumb()
        {
            var canvas = Context.CurrentCanvas;

            return Model.GetSelectedThumbs(canvas);
        }

        public IEnumerable<IElement> GetElementsThumb()
        {
            var canvas = Context.CurrentCanvas;

            return Model.GetThumbs(canvas);
        }

        public IEnumerable<IElement> GetElementsAll()
        {
            var canvas = Context.CurrentCanvas;

            return Model.GetAll(canvas);
        }

        public IEnumerable<IElement> GetAllInputOutputElements()
        {
            return GetElementsAll().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            });
        }

        public IEnumerable<IElement> GetSelectedInputOutputElements()
        {
            return GetElementsSelected().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            });
        }

        public void SelectAll()
        {
            var canvas = Context.CurrentCanvas;

            Model.SelectAll(canvas);
        }

        public void SelectNone()
        {
            var canvas = Context.CurrentCanvas;

            Model.SelectNone(canvas);
        }

        public void SelectPrevious(bool deselect)
        {
            if (Context.SelectedThumbList == null)
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
            var canvas = Context.CurrentCanvas;
            var elements = Model.GetAll(canvas);

            if (elements != null)
            {
                Context.SelectedThumbList = new LinkedList<IElement>(elements);

                Context.CurrentThumbNode = Context.SelectedThumbList.Last;
                if (Context.CurrentThumbNode != null)
                {
                    SelectOneElement(Context.CurrentThumbNode.Value, deselect);
                }
            }
        }

        private void SelectPreviousElement(bool deselect)
        {
            if (Context.CurrentThumbNode != null)
            {
                Context.CurrentThumbNode = Context.CurrentThumbNode.Previous;
                if (Context.CurrentThumbNode == null)
                {
                    Context.CurrentThumbNode = Context.SelectedThumbList.Last;
                }

                SelectOneElement(Context.CurrentThumbNode.Value, deselect);
            }
        }

        public void SelectNext(bool deselect)
        {
            if (Context.SelectedThumbList == null)
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
            var canvas = Context.CurrentCanvas;
            var elements = Model.GetAll(canvas);

            if (elements != null)
            {
                Context.SelectedThumbList = new LinkedList<IElement>(elements);

                Context.CurrentThumbNode = Context.SelectedThumbList.First;

                if (Context.CurrentThumbNode != null)
                {
                    SelectOneElement(Context.CurrentThumbNode.Value, deselect);
                }
            }
        }

        private void SelectNextElement(bool deselect)
        {
            if (Context.CurrentThumbNode != null)
            {
                Context.CurrentThumbNode = Context.CurrentThumbNode.Next;

                if (Context.CurrentThumbNode == null)
                {
                    Context.CurrentThumbNode = Context.SelectedThumbList.First;
                }

                SelectOneElement(Context.CurrentThumbNode.Value, deselect);
            }
        }

        public void SelectedListReset()
        {
            if (Context.SelectedThumbList != null)
            {
                Context.SelectedThumbList.Clear();
                Context.SelectedThumbList = null;
                Context.CurrentThumbNode = null;
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
            var canvas = Context.CurrentCanvas;

            Model.SelectConnected(canvas);
        }

        #endregion

        #region Mouse Conditions

        public bool IsSameUid(string uid1, string uid2)
        {
            return StringUtil.Compare(uid2, uid1) == false;
        }

        public bool IsWire(string elementUid)
        {
            return StringUtil.StartsWith(elementUid, ModelConstants.TagElementWire) == true;
        }

        public bool CanConnect()
        {
            return Context.CurrentRoot != null &&
                   Context.CurrentLine != null;
        }

        public bool CanSplitWire(IElement element)
        {
            if (element == null)
            {
                return false;
            }

            var elementUid = element.GetUid();
            var lineUid = Context.CurrentLine.GetUid();

            return element != null &&
                CanConnect() &&
                IsSameUid(elementUid, lineUid) &&
                IsWire(elementUid);
        }

        public bool CanToggleLine()
        {
            return Context.CurrentRoot == null &&
                Context.CurrentLine == null &&
                (Context.IsControlPressed != null && Context.IsControlPressed());
        }

        public bool CanConnectToPin(IThumb pin)
        {
            return pin != null &&
            (
                !StringUtil.Compare(pin.GetUid(), ResourceConstants.StandalonePinName)
                || (Context.IsControlPressed != null && Context.IsControlPressed())
            );
        }

        public bool CanMoveCurrentLine()
        {
            return Context.CurrentRoot != null &&
                Context.CurrentLine != null;
        }

        #endregion

        #region Mouse Helpers

        private void MouseCreateCanvasConnection(ICanvas canvas, IPoint point)
        {
            var root = InsertPin(canvas, point);

            Context.CurrentRoot = root;

            //System.Diagnostics.Debug.Print("Canvas_MouseLeftButtonDown, root: {0}", root.GetType());

            double x = Context.CurrentRoot.GetX();
            double y = Context.CurrentRoot.GetY();

            //System.Diagnostics.Debug.Print("x: {0}, y: {0}", x, y);

            ConnectionCreate(canvas, x, y);

            Context.CurrentRoot = root;
            ConnectionCreate(canvas, x, y);
        }

        private IElement MouseGetElementAtPoint(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0)
                .Where(x => StringUtil.Compare(Context.CurrentLine.GetUid(), x.GetUid()) == false)
                .FirstOrDefault() as IElement;

            return element;
        }

        private void MouseRemoveCurrentLine(ICanvas canvas)
        {
            var selection = Context.CurrentRoot.GetTag() as Selection;
            var tuples = selection.Item2;

            var last = tuples.LastOrDefault();
            tuples.Remove(last);

            canvas.Remove(Context.CurrentLine);
        }

        private void MouseToggleLineSelection(ICanvas canvas, IPoint point)
        {
            var element = canvas.HitTest(point, 6.0).FirstOrDefault() as IElement;

            if (element != null)
            {
                Model.SelectionToggleWire(element);
            }
            else
            {
                Model.SetLinesSelection(canvas, false);
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
            else if (Context.EnableInsertLast == true)
            {
                HistoryAdd(canvas, true);

                InsertLast(canvas, Context.LastInsert, point);
            }
        }

        public bool MouseEventPreviewLeftDown(ICanvas canvas, IPoint point, IThumb pin)
        {
            if (CanConnectToPin(pin))
            {
                if (Context.CurrentLine == null)
                {
                    HistoryAdd(canvas, true);
                }

                ConnectionCreate(canvas, pin);

                return true;
            }
            else if (Context.CurrentLine != null)
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
                var margin = Context.CurrentLine.GetMargin();
                double x = point.X - margin.Left;
                double y = point.Y - margin.Top;

                if (Context.CurrentLine.GetX2() != x)
                {
                    //CurrentOptions.CurrentLine.X2 = SnapX(x);
                    Context.CurrentLine.SetX2(x);
                }

                if (Context.CurrentLine.GetY2() != y)
                {
                    //CurrentOptions.CurrentLine.Y2 = SnapY(y);
                    Context.CurrentLine.SetY2(y);
                }
            }
        }

        public bool MouseEventRightDown(ICanvas canvas)
        {
            if (Context.CurrentRoot != null && 
                Context.CurrentLine != null)
            {
                if (Context.EnableHistory == true)
                {
                    var creator = Context.DiagramCreator;

                    History.Undo(canvas, creator, false);
                }
                else
                {
                    MouseRemoveCurrentLine(canvas);
                }

                Context.CurrentLine = null;
                Context.CurrentRoot = null;

                return true;
            }

            return false;
        }

        #endregion

        #region Tree

        public void TreeSelectPreviousItem(bool selectParent)
        {
            var tree = Context.CurrentTree;

            // get current diagram
            var selected = tree.GetSelectedItem() as ITreeItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.GetUid(), ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.GetParent() as ITreeItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    int index = parent.GetItemIndex(selected);
                    int count = parent.GetItemsCount();

                    // use '<' key for navigation in tree (project scope)
                    if (count > 0 && index > 0)
                    {
                        // select previous diagram
                        index = index - 1;

                        var item = parent.GetItem(index) as ITreeItem;
                        item.SetSelected(true);
                        item.PushIntoView();
                    }

                    // use 'Ctrl + <' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        TreeSelectPreviousParentItem(parent);
                    }
                }
            }
        }

        private static void TreeSelectPreviousParentItem(ITreeItem parent)
        {
            // get parent of current project
            var parentParent = parent.GetParent() as ITreeItem;
            int parentIndex = parentParent.GetItemIndex(parent);
            int parentCount = parentParent.GetItemsCount();

            if (parentCount > 0 && parentIndex > 0)
            {
                TreeSelectLastItemInPreviousProject(parentParent, parentIndex);
            }
        }

        private static void TreeSelectLastItemInPreviousProject(ITreeItem parentParent, int parentIndex)
        {
            // get previous project
            int index = parentIndex - 1;
            var parentProject = parentParent.GetItem(index);
            int count = parentProject.GetItemsCount();

            // select last item in previous project
            if (count > 0)
            {
                var item = parentProject.GetItem(count - 1);

                item.SetSelected(true);
                item.PushIntoView();
            }
        }

        public void TreeSelectNextItem(bool selectParent)
        {
            var tree = Context.CurrentTree;

            // get current diagram
            var selected = tree.GetSelectedItem() as ITreeItem;

            if (selected != null && 
                StringUtil.StartsWith(selected.GetUid(), ModelConstants.TagHeaderDiagram))
            {
                // get current project
                var parent = selected.GetParent() as ITreeItem;
                if (parent != null)
                {
                    // get all sibling diagrams in current project
                    int index = parent.GetItemIndex(selected);
                    int count = parent.GetItemsCount();

                    // use '>' key for navigation in tree (project scope)
                    if (count > 0 && index < count - 1)
                    {
                        // select next diagram
                        index = index + 1;

                        var item = parent.GetItem(index);
                        item.SetSelected(true);
                        item.PushIntoView();
                    }
             
                    // use 'Ctrl + >' key combination for navigation in tree (solution scope)
                    else if (selectParent == true)
                    {
                        TreeSelectNextParentItem(parent);
                    }
                }
            }
        }

        private static void TreeSelectNextParentItem(ITreeItem parent)
        {
            // get parent of current project
            var parentParent = parent.GetParent() as ITreeItem;
            int parentIndex = parentParent.GetItemIndex(parent);
            int parentCount = parentParent.GetItemsCount();

            if (parentCount > 0 && parentIndex < parentCount - 1)
            {
                TreeSelectFirstItemInNextProject(parentParent, parentIndex);
            }
        }

        private static void TreeSelectFirstItemInNextProject(ITreeItem parentParent, int parentIndex)
        {
            // get next project
            int index = parentIndex + 1;
            var parentProject = parentParent.GetItem(index);

            // select first item in next project
            if (parentProject.GetItemsCount() > 0)
            {
                var item = parentProject.GetItem(0);
                item.SetSelected(true);
                item.PushIntoView();
            }
        }

        public bool TreeSwitchItems(ICanvas canvas, 
            IDiagramCreator creator, 
            ITreeItem oldItem, ITreeItem newItem)
        {
            if (newItem == null)
                return false;

            string oldUid = oldItem == null ? null : oldItem.GetUid();
            string newUid = newItem == null ? null : newItem.GetUid();

            bool isOldItemDiagram = oldUid == null ? false : StringUtil.StartsWith(oldUid, ModelConstants.TagHeaderDiagram);
            bool isNewItemDiagram = newUid == null ? false : StringUtil.StartsWith(newUid, ModelConstants.TagHeaderDiagram);

            if (isOldItemDiagram == true)
            {
                // save current model
                Model.Store(canvas, oldItem);
            }

            if (isNewItemDiagram == true)
            {
                if (Context.UpdateProperties != null)
                {
                    Context.UpdateProperties();
                }

                // load new model
                Model.Load(canvas, creator, newItem);
            }

            //System.Diagnostics.Debug.Print("Old Uid: {0}, new Uid: {1}", oldUid, newUid);

            return isNewItemDiagram;
        }
        
        private ITreeItem TreeCreateSolutionItem(string uid)
        {
            var solution = Context.CreateTreeSolutionItem();

            if (uid == null)
            {
                var counter = Context.CurrentCanvas.GetCounter();
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
            var project = Context.CreateTreeProjectItem();

            if (uid == null)
            {
                var counter = Context.CurrentCanvas.GetCounter();
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
            var diagram = Context.CreateTreeDiagramItem();

            if (uid == null)
            {
                var counter = Context.CurrentCanvas.GetCounter();
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
            var tree= Context.CurrentTree;
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
                EditPaste(point, true);
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

            Model.Store(null, diagram);

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

        public void TreeDeleteProject(ITreeItem project)
        {
            var solution = project.GetParent() as ITreeItem;
            var diagrams = project.GetItems().ToList();

            foreach (var diagram in diagrams)
            {
                project.Remove(diagram);
            }

            solution.Remove(project);
        }

        public void TreeDeleteDiagram(ITreeItem diagram)
        {
            var project = diagram.GetParent() as ITreeItem;

            project.Remove(diagram);
        }

        private void TreeParseSolution(ITree tree, TreeSolution solution)
        {
            var counter = Context.CurrentCanvas.GetCounter();

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

        public void TreeClearSolution(ITree tree)
        {
            // clear solution tree
            TreeClear(tree);

            // reset counter
            Context.CurrentCanvas.GetCounter().ResetAll();

            TagsReset();

            SelectedListReset();

            Context.CurrentCanvas.SetTags(null);
        }

        public void TreeClear(ITree tree)
        {
            var items = tree.GetItems().ToList();

            foreach (var item in items)
            {
                TreeDeleteSolution(item);
            }
        }

        public void TreeCreateNewSolution()
        {
            var tree = Context.CurrentTree;
            var canvas = Context.CurrentCanvas;

            Model.Clear(canvas);

            TreeClearSolution(tree);

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

        private void TagsUpdate()
        {
            string tagFileName = Context.TagFileName;
            var tags = Context.Tags;

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
                Context.TagFileName = tagFileName;

                try
                {
                    var tags = Tags.Open(tagFileName);

                    Context.Tags = tags;

                    Context.CurrentCanvas.SetTags(tags);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Failed to load tags from file: {0}, error: {1}", tagFileName, ex.Message);
                }
            }
        }

        private void TagsReset()
        {
            if (Context.Tags != null)
            {
                Context.Tags.Clear();
                Context.Tags = null;
            }

            Context.TagFileName = null;
        }

        #endregion

        #region Clipboard

        public string ClipboardGetText()
        {
            try
            {
                if (Context.Clipboard.ContainsText())
                {
                    return Context.Clipboard.GetText();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);

                return Context.ClipboardText;
            }

            return Context.ClipboardText;
        }

        public void ClipboardSetText(string model)
        {
            try
            {
                Context.ClipboardText = model;

                Context.Clipboard.SetText(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        #endregion

        #region Snap

        public double SnapOffsetX(double original, bool snap)
        {
            var prop = Context.CurrentCanvas.GetProperties();

            return snap == true ?
                SnapUtil.Snap(original,
                    prop.SnapX, prop.SnapOffsetX) : 
                    original;
        }

        public double SnapOffsetY(double original, bool snap)
        {
            var prop = Context.CurrentCanvas.GetProperties();

            return snap == true ?
                SnapUtil.Snap(original,
                    prop.SnapY, prop.SnapOffsetY) : 
                    original;
        }

        public double SnapX(double original, bool snap)
        {
            var prop = Context.CurrentCanvas.GetProperties();

            return snap == true ?
                SnapUtil.Snap(original, prop.SnapX) : original;
        }

        public double SnapY(double original, bool snap)
        {
            var prop = Context.CurrentCanvas.GetProperties();

            return snap == true ?
                SnapUtil.Snap(original, prop.SnapY) : original;
        }

        #endregion

        #region File Dialogs

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
                var fileName = dlg.FileName;
                var canvas = Context.CurrentCanvas;

                this.OpenDiagram(fileName, canvas);
            }
        }

        public void OpenSolution()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Solution"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var fileName = dlg.FileName;
                var canvas = Context.CurrentCanvas;

                Model.Clear(canvas);

                TreeSolution solution = OpenSolution(fileName);

                if (solution != null)
                {
                    var tree = Context.CurrentTree;

                    TreeClearSolution(tree);
                    TreeParseSolution(tree, solution);
                }
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

                var tree = Context.CurrentTree;

                TagsUpdate();

                SaveSolution(fileName);
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
                var fileName = dlg.FileName;
                var canvas = Context.CurrentCanvas;

                SaveDiagram(fileName, canvas);
            }
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
                var filter = dlg.FilterIndex;
                var fileName = dlg.FileName;
                var canvas = Context.CurrentCanvas;

                DxfExportDiagram(fileName,
                    canvas,
                    shortenStart, shortenEnd,
                    FilterToAcadVer(filter),
                    table);
            }
        }

        private DxfAcadVer FilterToAcadVer(int filter)
        {
            DxfAcadVer version;

            switch (filter)
            {
                case 1: version = DxfAcadVer.AC1009; break;
                case 2: version = DxfAcadVer.AC1015; break;
                case 3: version = DxfAcadVer.AC1015; break;
                default: version = DxfAcadVer.AC1015; break;
            }

            return version;
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

                Context.TagFileName = tagFileName;
                Context.Tags = tags;
            }
        }

        public void TagsSave()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Tags",
                FileName = Context.TagFileName == null ? "tags" : System.IO.Path.GetFileName(Context.TagFileName)
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                Tags.Export(tagFileName, Context.Tags);

                Context.TagFileName = tagFileName;
            }
        }

        public void TagsImport()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Tags"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                if (Context.Tags == null)
                {
                    Context.Tags = new List<object>();
                }

                Tags.Import(tagFileName, Context.Tags, true);
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

                Tags.Export(tagFileName, Context.Tags);
            }
        }

        #endregion
    }

    #endregion
}
