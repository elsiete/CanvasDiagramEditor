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
    using TreeSolution = Tuple<string, string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;
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

        #region Model

        public void ModelClear()
        {
            var canvas = Context.CurrentCanvas;

            Snapshot(canvas, true);

            Model.Clear(canvas);
        }

        public void ModelInsert(string diagram, double offsetX, double offsetY, bool select)
        {
            var canvas = Context.CurrentCanvas;

            Snapshot(canvas, true);

            SelectNone();
            Model.Parse(diagram, 
                canvas, Context.DiagramCreator, 
                offsetX, offsetY,
                true, true, select, true);
        }

        public void ModelResetThumbTags()
        {
            var canvas = Context.CurrentCanvas;

            Snapshot(canvas, true);

            TagsResetThumbs(canvas);
        }

        public string ModelGetCurrent()
        {
            var tree = Context.CurrentTree;
            var canvas = Context.CurrentCanvas;
            var item = tree.GetSelectedItem() as ITreeItem;
            return Model.GenerateItemModel(canvas, item, true);
        }

        public Solution ModelGenerateSolution(string fileName, bool includeHistory)
        {
            var tree = Context.CurrentTree;
            var tagFileName = Context.TagFileName;
            var tableFileName = Context.TableFileName;
            return Model.GenerateSolution(tree, fileName, tagFileName, tableFileName, includeHistory);
        }

        public static IEnumerable<string> ModelGetCurrentProjectDiagrams(ITree tree)
        {
            var selected = tree.GetSelectedItem() as ITreeItem;
            if (selected == null)
                return null;

            string uid = selected.GetUid();
            bool isSolution = StringUtil.StartsWith(uid, Constants.TagHeaderSolution);
            bool isProject = StringUtil.StartsWith(uid, Constants.TagHeaderProject);
            bool isDiagram = StringUtil.StartsWith(uid, Constants.TagHeaderDiagram);

            if (isDiagram == true)
            {
                var project = selected.GetParent() as ITreeItem;
                var models = new List<string>();
                Model.GenerateProject(project, models, false);
                return models;
            }
            else if (isProject == true)
            {
                var models = new List<string>();
                Model.GenerateProject(selected, models, false);
                return models;
            }
            else if (isSolution == true)
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
            ITreeItem solution)
        {
            var items = new List<ITreeItem>();

            foreach (var project in projects)
            {
                string name = project.Item1;
                var diagrams = project.Item2.Reverse();
                var item = Tree.CreateProjectItem(name, Context.CreateProject, counter);
                solution.Add(item);

                int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
                counter.Set(Math.Max(counter.Count, id + 1));

                ModelParseDiagrams(counter, diagrams, item, items);
            }

            var first = items.FirstOrDefault();
            if (first != null)
                first.SetSelected(true);

            return items;
        }

        private void ModelParseDiagrams(IdCounter counter,
            IEnumerable<TreeDiagram> diagrams,
            ITreeItem project,
            List<ITreeItem> diagramList)
        {
            foreach (var diagram in diagrams)
                ModelParseDiagram(counter, diagram, project, diagramList);
        }

        private void ModelParseDiagram(IdCounter counter,
            TreeDiagram diagram,
            ITreeItem project,
            List<ITreeItem> diagrams)
        {
            var lines = diagram.Reverse();
            var first = lines.First().Split(new char[] { Constants.ArgumentSeparator, '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string name = first.Length >= 1 ? first[1] : null;
            
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine(line);

            var item = Tree.CreateDiagramItem(name, Context.CreateDiagram, counter);
            item.SetTag(new Diagram(sb.ToString(), null));

            project.Add(item);
            diagrams.Add(item);

            int id = int.Parse(name.Split(Constants.TagNameSeparator)[1]);
            counter.Set(Math.Max(counter.Count, id + 1));
        }

        #endregion

        #region Wire Connection

        private void Connect(ICanvas canvas, IThumb pin, IDiagramCreator creator)
        {
            if (pin == null)
                return;

            Context.CurrentRoot = pin.GetParent() as IThumb;

            var position = Model.GetPinPosition(Context.CurrentRoot, pin);
            double x = position.Item1;
            double y = position.Item2;

            Context.CurrentLine = Wire.Connect(canvas, Context.CurrentRoot, Context.CurrentLine, x, y, creator);
            if (Context.CurrentLine == null)
                Context.CurrentRoot = null;
        }

        #endregion

        #region Add

        public IElement Add(ICanvas canvas, string type, IPoint point)
        {
            var context = Context.DiagramCreator;
            var snap = Context.EnableSnap;
            switch (type)
            {
                case Constants.TagElementInput: return Insert.Input(canvas, point, context, snap);
                case Constants.TagElementOutput: return Insert.Output(canvas, point, context, snap);
                case Constants.TagElementAndGate: return Insert.AndGate(canvas, point, context, snap);
                case Constants.TagElementOrGate: return Insert.OrGate(canvas, point, context, snap);
                case Constants.TagElementPin: return Insert.Pin(canvas, point, context, snap);
                default: return null;
            }
        }

        #endregion

        #region History

        public string Snapshot(ICanvas canvas, bool reset)
        {
            if (reset == true)
                SelectedListReset();

            return History.Add(canvas);
        }

        public void Undo()
        {
            History.Undo(Context.CurrentCanvas, Context.DiagramCreator, true);
        }

        public void Redo()
        {
            History.Redo(Context.CurrentCanvas, Context.DiagramCreator, true);
        }

        #endregion

        #region Move

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
                    MoveLine(dX, dY, snap, tuple);
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
                MoveRoot(thumb, dX, dY, snap);
        }

        public void MoveLeft(ICanvas canvas)
        {
            Snapshot(canvas, false);

            double delta = Context.EnableSnap ? -canvas.GetProperties().GridSize : -1.0;
            MoveSelectedElements(canvas, delta, 0.0, false);
        }

        public void MoveRight(ICanvas canvas)
        {
            Snapshot(canvas, false);

            double delta = Context.EnableSnap ? canvas.GetProperties().GridSize : 1.0;
            MoveSelectedElements(canvas, delta, 0.0, false);
        }

        public void MoveUp(ICanvas canvas)
        {
            Snapshot(canvas, false);

            double delta = Context.EnableSnap ? -canvas.GetProperties().GridSize : -1.0;
            MoveSelectedElements(canvas, 0.0, delta, false);
        }

        public void MoveDown(ICanvas canvas)
        {
            Snapshot(canvas, false);

            double delta = Context.EnableSnap ? canvas.GetProperties().GridSize : 1.0;
            MoveSelectedElements(canvas, 0.0, delta, false);
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
                MoveSelectedElements(canvas, dX, dY, snap);
            else
                MoveRoot(element, dX, dY, snap);
        }

        public void DragStart(ICanvas canvas, IThumb element)
        {
            Snapshot(canvas, false);

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

        #region Wire

        public void ToggleWireStart()
        {
            var canvas = Context.CurrentCanvas;
            var wires = Model.GetSelectedWires(canvas);
            if (wires.Count() <= 0)
                return;

            Snapshot(canvas, false);

            foreach (var wire in wires.Cast<ILine>())
                wire.SetStartVisible(wire.GetStartVisible() == true ? false : true);
        }

        public void ToggleWireEnd()
        {
            var canvas = Context.CurrentCanvas;
            var wires = Model.GetSelectedWires(canvas);
            if (wires.Count() <= 0)
                return;

            Snapshot(canvas, false);

            foreach (var wire in wires.Cast<ILine>())
                wire.SetEndVisible(wire.GetEndVisible() == true ? false : true);
        }

        #endregion

        #region Open/Save Diagram

        public void OpenDiagram(string fileName, ICanvas canvas)
        {
            string diagram = Model.Open(fileName);

            Snapshot(canvas, true);

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
            ModelGetCurrent();

            var model = ModelGenerateSolution(fileName, false).Item1;

            Model.Save(fileName, model);
        }

        #endregion

        #region Edit

        public void Cut()
        {
            var canvas = Context.CurrentCanvas;
            string model = Model.Generate(Model.GetSelected(canvas));

            if (model.Length == 0)
            {
                model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

                var elements = Model.GetAll(canvas);

                Delete(canvas, elements);
            }
            else
            {
                Delete();
            }

            ClipboardSetText(model);
        }

        public void Copy()
        {
            var canvas = Context.CurrentCanvas;
            string model = Model.Generate(Model.GetSelected(canvas));

            if (model.Length == 0)
                model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

            ClipboardSetText(model);
        }

        public void Paste(IPoint point, bool select)
        {
            var model = ClipboardGetText();

            if (model != null && model.Length > 0)
            {
                double offsetX = point.X != 0.0 ? SnapOffsetX(point.X, Context.EnableSnap) : 0.0;
                double offsetY = point.Y != 0.0 ? SnapOffsetY(point.Y, Context.EnableSnap) : 0.0;

                ModelInsert(model, offsetX, offsetY, select);
            }
        }

        public void Delete()
        {
            var canvas = Context.CurrentCanvas;
            var elements = Model.GetSelected(canvas);

            Delete(canvas, elements);
        }

        public void Delete(ICanvas canvas, IPoint point)
        {
            Snapshot(canvas, true);

            Model.DeleteElement(canvas, point);

            Context.SkipLeftClick = false;
        }

        public void Delete(ICanvas canvas, IEnumerable<IElement> elements)
        {
            Snapshot(canvas, true);

            foreach (var element in elements)
                Model.DeleteElement(canvas, element);
        }

        #endregion

        #region Selection

        public IEnumerable<IElement> GetElementsSelected()
        {
            return Model.GetSelected(Context.CurrentCanvas);
        }

        public IEnumerable<IElement> GetElementsSelectedThumb()
        {
            return Model.GetSelectedThumbs(Context.CurrentCanvas);
        }

        public IEnumerable<IElement> GetElementsThumb()
        {
            return Model.GetThumbs(Context.CurrentCanvas);
        }

        public IEnumerable<IElement> GetElementsAll()
        {
            return Model.GetAll(Context.CurrentCanvas);
        }

        public IEnumerable<IElement> GetAllInputOutputElements()
        {
            return GetElementsAll().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, Constants.TagElementInput) ||
                    StringUtil.StartsWith(uid, Constants.TagElementOutput);
            });
        }

        public IEnumerable<IElement> GetSelectedInputOutputElements()
        {
            return GetElementsSelected().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, Constants.TagElementInput) ||
                    StringUtil.StartsWith(uid, Constants.TagElementOutput);
            });
        }

        public void SelectAll()
        {
            Model.SelectAll(Context.CurrentCanvas);
        }

        public void SelectNone()
        {
            Model.SelectNone(Context.CurrentCanvas);
        }

        public void SelectPrevious(bool deselect)
        {
            if (Context.SelectedThumbList == null)
                SelectPreviousInitialize(deselect);
            else
                SelectPreviousElement(deselect);
        }

        private void SelectPreviousInitialize(bool deselect)
        {
            var elements = Model.GetAll(Context.CurrentCanvas);

            if (elements != null)
            {
                Context.SelectedThumbList = new LinkedList<IElement>(elements);
                Context.CurrentThumbNode = Context.SelectedThumbList.Last;

                if (Context.CurrentThumbNode != null)
                    SelectOneElement(Context.CurrentThumbNode.Value, deselect);
            }
        }

        private void SelectPreviousElement(bool deselect)
        {
            if (Context.CurrentThumbNode != null)
            {
                Context.CurrentThumbNode = Context.CurrentThumbNode.Previous;
                if (Context.CurrentThumbNode == null)
                    Context.CurrentThumbNode = Context.SelectedThumbList.Last;

                SelectOneElement(Context.CurrentThumbNode.Value, deselect);
            }
        }

        public void SelectNext(bool deselect)
        {
            if (Context.SelectedThumbList == null)
                SelectNextInitialize(deselect);
            else
                SelectNextElement(deselect);
        }

        private void SelectNextInitialize(bool deselect)
        {
            var elements = Model.GetAll(Context.CurrentCanvas);

            if (elements != null)
            {
                Context.SelectedThumbList = new LinkedList<IElement>(elements);
                Context.CurrentThumbNode = Context.SelectedThumbList.First;

                if (Context.CurrentThumbNode != null)
                    SelectOneElement(Context.CurrentThumbNode.Value, deselect);
            }
        }

        private void SelectNextElement(bool deselect)
        {
            if (Context.CurrentThumbNode != null)
            {
                Context.CurrentThumbNode = Context.CurrentThumbNode.Next;

                if (Context.CurrentThumbNode == null)
                    Context.CurrentThumbNode = Context.SelectedThumbList.First;

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
            if (element == null)
                return;

            if (deselect == true)
            {
                SelectNone();
                element.SetSelected(true);
            }
            else
                element.SetSelected(!element.GetSelected());
        }

        public void SelectConnected()
        {
            Model.SelectConnected(Context.CurrentCanvas);
        }

        #endregion

        #region Mouse Conditions

        public bool IsSameUid(string uid1, string uid2)
        {
            return StringUtil.Compare(uid2, uid1) == false;
        }

        public bool IsWire(string elementUid)
        {
            return StringUtil.StartsWith(elementUid, Constants.TagElementWire) == true;
        }

        public bool CanConnect()
        {
            return Context.CurrentRoot != null &&
                   Context.CurrentLine != null;
        }

        public bool CanSplitWire(IElement element)
        {
            if (element == null)
                return false;

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
                !StringUtil.Compare(pin.GetUid(), Constants.PinStandalone)
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
            var root = Insert.Pin(canvas, point, Context.DiagramCreator, Context.EnableSnap);

            Context.CurrentRoot = root;

            double x = Context.CurrentRoot.GetX();
            double y = Context.CurrentRoot.GetY();

            Context.CurrentLine = Wire.Connect(canvas, Context.CurrentRoot, Context.CurrentLine, x, y, Context.DiagramCreator);
            if (Context.CurrentLine == null)
                Context.CurrentRoot = null;

            Context.CurrentRoot = root;
            Context.CurrentLine = Wire.Connect(canvas, Context.CurrentRoot, Context.CurrentLine, x, y, Context.DiagramCreator);
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
                Model.SelectionToggleWire(element);
            else
                Model.SetLinesSelection(canvas, false);
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
                Snapshot(canvas, true);
                Add(canvas, Context.LastInsert, point);
            }
        }

        public bool MouseEventPreviewLeftDown(ICanvas canvas, IPoint point, IThumb pin)
        {
            if (CanConnectToPin(pin))
            {
                if (Context.CurrentLine == null)
                    Snapshot(canvas, true);

                Connect(canvas, pin, Context.DiagramCreator);

                return true;
            }
            else if (Context.CurrentLine != null)
            {
                var element = MouseGetElementAtPoint(canvas, point);
                if (CanSplitWire(element))
                {
                    if (Context.CurrentLine == null)
                        Snapshot(canvas, true);

                    bool result = Wire.Split(canvas, element as ILine, Context.CurrentLine, point, Context.DiagramCreator, Context.EnableSnap);

                    Context.CurrentRoot = null;
                    Context.CurrentLine = null;

                    return result;
                }
            }

            if (CanToggleLine())
                MouseToggleLineSelection(canvas, point);

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
                var creator = Context.DiagramCreator;
                History.Undo(canvas, creator, false);

                Context.CurrentLine = null;
                Context.CurrentRoot = null;

                return true;
            }

            return false;
        }

        #endregion

        #region Solution

        public void SolutionParse(ITree tree,
            TreeSolution solution,
            IdCounter counter,
            Func<ITreeItem> CreateTreeSolutionItem)
        {
            // create solution
            string tagFileName = null;

            string solutionName = solution.Item1;
            tagFileName = solution.Item2;
            var projects = solution.Item4.Reverse();

            TagsLoad(tagFileName);

            var solutionItem = Tree.CreateSolutionItem(solutionName, CreateTreeSolutionItem, counter);
            tree.Add(solutionItem);

            ModelParseProjects(projects, counter, solutionItem);
        }

        public void SolutionClear(ITree tree, ICanvas canvas, IdCounter counter)
        {
            // clear solution tree
            Tree.Clear(tree);

            // reset counter
            counter.Reset();

            TagsReset();
            SelectedListReset();

            canvas.SetTags(null);
        }

        #endregion

        #region Tags

        public void TagsResetThumbs(ICanvas canvas)
        {
            var thumbs = canvas.GetElements().OfType<IThumb>().Where(x => x.GetTag() != null);
            var selectedThumbs = thumbs.Where(x => x.GetSelected());
 
            if (selectedThumbs.Count() > 0)
                ResetThumbs(selectedThumbs);
            else
                ResetThumbs(thumbs);
        }

        private static void ResetThumbs(IEnumerable<IThumb> thumbs)
        {
            foreach (var thumb in thumbs)
                thumb.SetData(null);
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
                    System.Diagnostics.Debug.Print("Failed to load tags from file: {0}, error: {1}",
                        tagFileName, ex.Message);
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
                    return Context.Clipboard.GetText();
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
    }

    #endregion
}
