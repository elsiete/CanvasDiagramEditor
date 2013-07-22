#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Export;
using CanvasDiagramEditor.Parser;
using CanvasDiagramEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#endregion

namespace CanvasDiagramEditor
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

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private DiagramEditor editor = null;
        private SelectionAdorner adorner = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeOptions();

            GenerateGrid(false);

            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PanScrollViewer.Focus();
        }

        private void InitializeOptions()
        {
            editor = new DiagramEditor();
            editor.options = new DiagramEditorOptions();

            editor.options.Counter.ProjectCount = 1;
            editor.options.Counter.DiagramCount = 1;

            UpdateDiagramProperties();

            editor.options.CurrentCanvas = this.DiagramCanvas;
            editor.options.CurrentPathGrid = this.PathGrid;

            EnableHistory.IsChecked = editor.options.EnableHistory;
            EnableInsertLast.IsChecked = editor.options.EnableInsertLast;
            EnablePageGrid.IsChecked = editor.options.EnablePageGrid;
            EnablePageTemplate.IsChecked = editor.options.EnablePageTemplate;
            EnableSnap.IsChecked = editor.options.EnableSnap;
            SnapOnRelease.IsChecked = editor.options.SnapOnRelease;
        }

        #endregion

        #region Solution

        private void SwitchItems(Canvas canvas, TreeViewItem oldItem, TreeViewItem newItem)
        {
            if (newItem == null)
                return;

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

                EnablePage.IsChecked = true;
            }
            else
            {
                EnablePage.IsChecked = false;
            }

            System.Diagnostics.Debug.Print("Old Uid: {0}, new Uid: {1}", oldUid, newUid);
        }

        private void LoadModel(Canvas canvas, TreeViewItem item)
        {
            var tag = item.Tag;

            editor.ClearDiagramModel(canvas);

            if (tag != null)
            {
                var diagram = tag as Diagram;

                var model = diagram.Item1;
                var history = diagram.Item2;

                canvas.Tag = history;

                editor.ParseDiagramModel(model, canvas, editor.options.CurrentPathGrid, 0, 0, false, true, false, true);
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
            var model = editor.GenerateDiagramModel(canvas, uid);

            if (item != null)
            {
                item.Tag = new Diagram(model, canvas != null ? canvas.Tag as History : null);
            }
        }

        private TreeViewItem CreateSolutionItem(string uid)
        {
            var solution = new TreeViewItem();

            solution.Header = "Solution";
            solution.ContextMenu = this.Resources["SolutionContextMenuKey"] as ContextMenu;
            solution.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.Counter;
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
            project.ContextMenu = this.Resources["ProjectContextMenuKey"] as ContextMenu;
            project.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.Counter;
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
            diagram.ContextMenu = this.Resources["DiagramContextMenuKey"] as ContextMenu;
            diagram.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            if (uid == null)
            {
                var counter = editor.options.Counter;
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

        private void AddProject(TreeViewItem solution)
        {
            var project = CreateProjectItem(null);

            solution.Items.Add(project);

            System.Diagnostics.Debug.Print("Added project: {0} to solution: {1}", project.Uid, solution.Uid);
        }

        private void AddDiagram(TreeViewItem project, bool select)
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

        private void DeleteProject(TreeViewItem project)
        {
            var solution = project.Parent as TreeViewItem;

            var diagrams = project.Items.Cast<TreeViewItem>().ToList();

            foreach (var diagram in diagrams)
            {
                project.Items.Remove(diagram);
            }

            solution.Items.Remove(project);
        }

        private void DeleteDiagram(TreeViewItem diagram)
        {
            var project = diagram.Parent as TreeViewItem;

            project.Items.Remove(diagram);
        }

        private void UpdateSelectedDiagramModel()
        {
            var canvas = editor.options.CurrentCanvas;
            var item = SolutionTree.SelectedItem as TreeViewItem;

            if (item != null)
            {
                string uid = item.Uid;
                bool isDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                if (isDiagram == true)
                {
                    var model = editor.GenerateDiagramModel(canvas, uid);

                    item.Tag = new Diagram(model, canvas.Tag as History);
                }
            }
        }

        public string GenerateSolution()
        {
            var solution = SolutionTree.Items.Cast<TreeViewItem>().First();
            var projects = solution.Items.Cast<TreeViewItem>();
            string line = null;

            var sb = new StringBuilder();

            // update current diagram
            UpdateSelectedDiagramModel();

            // Solution
            line = string.Format("{0}{1}{2}",
                ModelConstants.PrefixRoot,
                ModelConstants.ArgumentSeparator,
                solution.Uid);

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

                        sb.Append(model);
                    }
                }
            }

            return sb.ToString();
        }

        public void OpenSolution()
        {
            var solution = editor.OpenSolution();

            if (solution != null)
            {
                var tree = SolutionTree;
                TreeViewItem firstDiagram = null;
                bool haveFirstDiagram = false;

                ClearSolutionTree(tree);

                var counter = editor.options.Counter;

                // create solution
                string name = null;
                int id = -1;

                name = solution.Item1;
                var projects = solution.Item2.Reverse();

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

        private void ClearSolutionTree(TreeView tree)
        {
            // clear solution tree
            var items = tree.Items.Cast<TreeViewItem>().ToList();

            foreach (var item in items)
            {
                DeleteSolution(item);
            }

            // reset counter
            editor.options.Counter.ResetAll();
        }

        public void NewSolution()
        {
            var canvas = DiagramCanvas;
            var tree = SolutionTree;

            editor.ClearDiagramModel(canvas);

            ClearSolutionTree(tree);

            var solutionItem = CreateSolutionItem(null);
            tree.Items.Add(solutionItem);

            var projectItem = CreateProjectItem(null);
            solutionItem.Items.Add(projectItem);

            var diagramItem = CreateDiagramItem(null);
            projectItem.Items.Add(diagramItem);

            diagramItem.IsSelected = true;
        }

        #endregion

        #region SelectionAdorner

        private void CreateAdorner(Canvas canvas, Point origin, Point point)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            adorner = new SelectionAdorner(canvas);
            adorner.Zoom = GetZoomScaleTransform().ScaleX;
            adorner.SelectionOrigin = new Point(origin.X, origin.Y);

            adorner.SelectionRect = new Rect(origin, point);

            adorner.SnapsToDevicePixels = false;
            RenderOptions.SetEdgeMode(adorner, EdgeMode.Aliased);

            layer.Add(adorner);
            adorner.InvalidateVisual();
        }

        private void RemoveAdorner(Canvas canvas)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            layer.Remove(adorner);

            adorner = null;
        }

        private void UpdateAdorner(Point point)
        {
            var origin = adorner.SelectionOrigin;
            double width = Math.Abs(point.X - origin.X);
            double height = Math.Abs(point.Y - origin.Y);

            adorner.SelectionRect = new Rect(point, origin);
            adorner.InvalidateVisual();
        }

        #endregion

        #region Move

        private void MoveLeft(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.EnableSnap == true)
            {
                editor.MoveSelectedElements(canvas, -editor.options.DefaultGridSize, 0.0, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, -1.0, 0.0, false);
            }
        }

        private void MoveRight(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.EnableSnap == true)
            {
                editor.MoveSelectedElements(canvas, editor.options.DefaultGridSize, 0.0, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 1.0, 0.0, false);
            }
        }

        private void MoveUp(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.EnableSnap == true)
            {
                editor.MoveSelectedElements(canvas, 0.0, -editor.options.DefaultGridSize, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 0.0, -1.0, false);
            }
        }

        private void MoveDown(Canvas canvas)
        {
            editor.AddToHistory(canvas);

            if (editor.options.EnableSnap == true)
            {
                editor.MoveSelectedElements(canvas, 0.0, editor.options.DefaultGridSize, false);
            }
            else
            {
                editor.MoveSelectedElements(canvas, 0.0, 1.0, false);
            }
        }

        #endregion

        #region Grid

        private void UpdateDiagramProperties()
        {
            var prop = editor.options.CurrentProperties;

            prop.PageWidth = int.Parse(TextPageWidth.Text);
            prop.PageHeight = int.Parse(TextPageHeight.Text);

            prop.GridOriginX = int.Parse(TextGridOriginX.Text);
            prop.GridOriginY = int.Parse(TextGridOriginY.Text);
            prop.GridWidth = int.Parse(TextGridWidth.Text);
            prop.GridHeight = int.Parse(TextGridHeight.Text);
            prop.GridSize = int.Parse(TextGridSize.Text);

            prop.SnapX = double.Parse(TextSnapX.Text);
            prop.SnapY = double.Parse(TextSnapY.Text);
            prop.SnapOffsetX = double.Parse(TextSnapOffsetX.Text);
            prop.SnapOffsetY = double.Parse(TextSnapOffsetY.Text);
        }

        private void GenerateGrid(bool undo)
        {
            var canvas = editor.options.CurrentCanvas;
            var path = this.PathGrid;

            UpdateDiagramProperties();

            if (undo == true)
            {
                editor.AddToHistory(canvas);
            }

            var prop = editor.options.CurrentProperties;

            editor.GenerateGrid(path, 
                prop.GridOriginX, prop.GridOriginY,
                prop.GridWidth, prop.GridHeight,
                prop.GridSize);

            editor.SetDiagramSize(canvas, prop.PageWidth, prop.PageHeight);
        }

        #endregion

        #region Pan

        private void BeginPan(Point point)
        {
            editor.options.PanStart = point;

            editor.options.PreviousScrollOffsetX = -1.0;
            editor.options.PreviousScrollOffsetY = -1.0;

            this.Cursor = Cursors.ScrollAll;
            this.PanScrollViewer.CaptureMouse();
        }

        private void EndPan()
        {
            if (PanScrollViewer.IsMouseCaptured == true)
            {
                this.Cursor = Cursors.Arrow;
                this.PanScrollViewer.ReleaseMouseCapture();
            }
        }

        private void PanToPoint(Point point)
        {
            double scrollOffsetX = point.X - editor.options.PanStart.X;
            double scrollOffsetY = point.Y - editor.options.PanStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * 1.0) * editor.options.ReversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * 1.0) * editor.options.ReversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != editor.options.PreviousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                editor.options.PreviousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != editor.options.PreviousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                editor.options.PreviousScrollOffsetY = scrollOffsetY;
            }

            editor.options.PanStart = point;
        }

        private void PanToOffset(double offsetX, double offsetY)
        {
            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = Math.Round(horizontalOffset + offsetX, 0);
            double scrollOffsetY = Math.Round(verticalOffset + offsetY, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
            this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
        }

        #endregion

        #region Zoom

        public double CalculateZoom(double x)
        {
            double l = Math.Log(x, editor.options.ZoomLogBase);
            double e = Math.Exp(l / editor.options.ZoomExpFactor);
            double y = x + x * l * e;
            return y;
        }

        private void Zoom(double zoom)
        {
            if (editor == null || editor.options == null)
                return;

            double zoom_fx = CalculateZoom(zoom);

            //System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            var st = GetZoomScaleTransform();

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            Application.Current.Resources[ResourceConstants.KeyStrokeThickness] = editor.options.DefaultStrokeThickness / zoom_fx;

            // zoom to point
            ZoomToPoint(zoom_fx, oldZoom);
        }

        private ScaleTransform GetZoomScaleTransform()
        {
            //var tg = RootGrid.RenderTransform as TransformGroup;
            var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is ScaleTransform) as ScaleTransform;

            return st;
        }

        private void ZoomToPoint(double zoom, double oldZoom)
        {
            double offsetX = 0;
            double offsetY = 0;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double scrollOffsetX = this.PanScrollViewer.HorizontalOffset;
            double scrollOffsetY = this.PanScrollViewer.VerticalOffset;

            double oldX = editor.options.ZoomPoint.X * oldZoom;
            double oldY = editor.options.ZoomPoint.Y * oldZoom;

            double newX = editor.options.ZoomPoint.X * zoom;
            double newY = editor.options.ZoomPoint.Y * zoom;

            offsetX = newX - oldX;
            offsetY = newY - oldY;

            //System.Diagnostics.Debug.Print("");
            //System.Diagnostics.Debug.Print("zoomPoint: {0},{1}", Math.Round(zoomPoint.X, 0), Math.Round(zoomPoint.Y, 0));
            //System.Diagnostics.Debug.Print("scrollableWidth/Height: {0},{1}", scrollableWidth, scrollableHeight);
            //System.Diagnostics.Debug.Print("scrollOffsetX/Y: {0},{1}", scrollOffsetX, scrollOffsetY);
            //System.Diagnostics.Debug.Print("oldX/Y: {0},{1}", oldX, oldY);
            //System.Diagnostics.Debug.Print("newX/Y: {0},{1}", newX, newY);
            //System.Diagnostics.Debug.Print("offsetX/Y: {0},{1}", offsetX, offsetY);

            if (scrollableWidth <= 0)
                offsetX = 0.0;

            if (scrollableHeight <= 0)
                offsetY = 0.0;

            PanToOffset(offsetX, offsetY);

            if (adorner != null)
            {
                adorner.Zoom = zoom;
            }
        }

        private void ZoomIn()
        {
            double zoom = ZoomSlider.Value;

            zoom += editor.options.ZoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= editor.options.ZoomOutFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        #endregion

        #region Zoom Events

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double zoom = ZoomSlider.Value;

            zoom = Math.Round(zoom, 1);

            if (e.OldValue != e.NewValue)
            {
                Zoom(zoom);
            }
        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
                return;

            var canvas = editor.options.CurrentCanvas;

            editor.options.ZoomPoint = e.GetPosition(canvas);

            if (e.Delta > 0)
            {
                ZoomIn();

                e.Handled = true;
            }
            else if (e.Delta < 0)
            {
                ZoomOut();

                e.Handled = true;
            }
        }

        #endregion

        #region PanScrollViewer Events

        private void PanScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.PanButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.options.PanButton)
            {
                EndPan();
            }
        }

        private void PanScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.PanScrollViewer.IsMouseCaptured == true)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                PanToPoint(point);
            }
        }

        #endregion

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;
            var point = e.GetPosition(canvas);

            if (editor.options.CurrentRoot == null && editor.options.CurrentLine == null && editor.options.EnableInsertLast == false)
            {
                editor.options.SelectionOrigin = point;

                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    editor.DeselectAll();
                }

                canvas.CaptureMouse();
            }
            else
            {
                editor.HandleLeftDown(canvas, point);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            if (canvas.IsMouseCaptured)
            {
                canvas.ReleaseMouseCapture();

                if (adorner != null)
                {
                    var rect = adorner.SelectionRect;
                    var elements = editor.HitTest(canvas, ref rect);

                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            if (SelectionThumb.GetIsSelected(element) == false)
                            {
                                SelectionThumb.SetIsSelected(element, true);
                            }
                            else
                            {
                                SelectionThumb.SetIsSelected(element, false);
                            }
                        }
                    }

                    RemoveAdorner(canvas);
                }
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editor.options.SkipLeftClick == true)
            {
                editor.options.SkipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = editor.options.CurrentCanvas;
            var point = e.GetPosition(canvas);
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = editor.HandlePreviewLeftDown(canvas, point, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            var point = e.GetPosition(canvas);

            if (canvas.IsMouseCaptured)
            {
                if (adorner == null)
                {
                    CreateAdorner(canvas, editor.options.SelectionOrigin, point);
                }

                UpdateAdorner(point);
            }
            else
            {
                editor.HandleMove(canvas, point);
            }
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;
            var path = editor.options.CurrentPathGrid;

            editor.options.RightClick = e.GetPosition(canvas);

            var result = editor.HandleRightDown(canvas, path);
            if (result == true)
            {
                editor.options.SkipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (editor.options.SkipContextMenu == true)
            {
                editor.options.SkipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                editor.options.SkipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertPin(canvas, editor.options.RightClick);

            editor.options.LastInsert = ModelConstants.TagElementPin;
            editor.options.SkipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertInput(canvas, editor.options.RightClick);

            editor.options.LastInsert = ModelConstants.TagElementInput;
            editor.options.SkipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOutput(canvas, editor.options.RightClick);

            editor.options.LastInsert = ModelConstants.TagElementOutput;
            editor.options.SkipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertAndGate(canvas, editor.options.RightClick);

            editor.options.LastInsert = ModelConstants.TagElementAndGate;
            editor.options.SkipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOrGate(canvas, editor.options.RightClick);

            editor.options.LastInsert = ModelConstants.TagElementOrGate;
            editor.options.SkipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;
            var point = new Point(editor.options.RightClick.X, editor.options.RightClick.Y);

            editor.Delete(canvas, point);
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;
            var point = new Point(editor.options.RightClick.X, editor.options.RightClick.Y);

            editor.ToggleStart(canvas, point);
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.options.CurrentCanvas;
            var point = new Point(editor.options.RightClick.X, editor.options.RightClick.Y);

            editor.ToggleEnd(canvas, point);
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.options.EnableHistory = EnableHistory.IsChecked == true ? true : false;

            if (editor.options.EnableHistory == false)
            {
                var canvas = editor.options.CurrentCanvas;

                editor.ClearHistory(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            editor.options.EnableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            editor.options.SnapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            editor.options.EnableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        }

        private void EnablePageGrid_Click(object sender, RoutedEventArgs e)
        {
            editor.options.EnablePageGrid = EnablePageGrid.IsChecked == true ? true : false;
        }

        private void EnablePageTemplate_Click(object sender, RoutedEventArgs e)
        {
            editor.options.EnablePageTemplate = EnablePageTemplate.IsChecked == true ? true : false;
        }

        #endregion

        #region Button Events

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            //var diagram = editor.Generate();
            //this.TextModel.Text = diagram;

            var solution = GenerateSolution();

            this.TextModel.Text = solution;
        }

        private void GenerateModelFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.GenerateFromSelected();

            this.TextModel.Text = diagram;
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            editor.Insert(diagram, offsetX, offsetY);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            GenerateGrid(true);
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

        private void SolutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (editor == null)
                return;

            var canvas = editor.options.CurrentCanvas;

            var oldItem = e.OldValue as TreeViewItem;
            var newItem = e.NewValue as TreeViewItem;

            SwitchItems(canvas, oldItem, newItem);
        }

        private void SolutionAddProject_Click(object sender, RoutedEventArgs e)
        {
            var solution = SolutionTree.SelectedItem as TreeViewItem;

            AddProject(solution);
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            AddDiagram(project, true);
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            DeleteProject(project);
        }

        private void ProjectDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            var diagram = SolutionTree.SelectedItem as TreeViewItem;

            DeleteDiagram(diagram);
        }

        #endregion

        #region Main Menu Events

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            NewSolution();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSolution();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            var model = GenerateSolution();

            editor.SaveSolution(model);
        }

        private void FileOpenDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.OpenDiagram();
        }

        private void FileSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.SaveDiagram();
        }

        private void FileImport_Click(object sender, RoutedEventArgs e)
        {
            var diagram = editor.Import();

            if (diagram != null)
            {
                this.TextModel.Text = diagram;
            }
        }

        private void FileExport_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportDiagram();
        }

        private void FileExportHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportDiagramHistory();
        }

        private void FilePrint_Click(object sender, RoutedEventArgs e)
        {
            editor.Print();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            editor.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            editor.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            editor.Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            editor.Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            var point = new Point(0.0, 0.0);

            editor.Paste(point);
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            editor.Delete();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.SelectAll();
        }

        private void EditDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            editor.DeselectAll();
        }

        private void EditClear_Click(object sender, RoutedEventArgs e)
        {
            editor.Clear();
        }

        private void EditOptions_Click(object sender, RoutedEventArgs e)
        {
            TabOptions.IsSelected = true;
        }

        #endregion

        #region Window Key Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //System.Diagnostics.Debug.Print("PreviewKeyDown sender: {0}, source: {1}", sender.GetType(), e.OriginalSource.GetType());

            if (!(e.OriginalSource is TextBox) &&
                Keyboard.Modifiers != ModifierKeys.Shift)
            {
                var canvas = editor.options.CurrentCanvas;
                bool isControl = Keyboard.Modifiers == ModifierKeys.Control;

                switch (e.Key)
                {
                    // add new project to selected solution
                    // add new diagram to selected project
                    // add new diagram after selected diagram and select new diagram
                    case Key.M:
                        {
                            if (isControl == true)
                            {
                                var selected = SolutionTree.SelectedItem as TreeViewItem;

                                string uid = selected.Uid;
                                bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
                                bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
                                bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

                                if (isSelectedDiagram == true)
                                {
                                    var project = selected.Parent as TreeViewItem;

                                    AddDiagram(project, true);
                                }
                                else if (isSelectedProject == true)
                                {
                                    AddDiagram(selected, false);
                                }
                                else if (isSelectedSolution == true)
                                {
                                    AddProject(selected);
                                }
                            }
                        }
                        break;

                    // open solution
                    case Key.O:
                        {
                            if (isControl == true)
                            {
                                editor.OpenDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // save solution
                    case Key.S:
                        {
                            if (isControl == true)
                            {
                                editor.SaveDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // new solution
                    case Key.N:
                        {
                            if (isControl == true)
                            {
                                NewSolution();
                            }
                        }
                        break;

                    // import
                    case Key.I:
                        {
                            if (isControl == true)
                            {
                                editor.Import();
                                e.Handled = true;
                            }
                        }
                        break;

                    // export
                    case Key.E:
                        {
                            if (isControl == true)
                            {
                                editor.ExportDiagram();
                                e.Handled = true;
                            }
                        }
                        break;

                    // export history
                    case Key.H:
                        {
                            if (isControl == true)
                            {
                                editor.ExportDiagramHistory();
                                e.Handled = true;
                            }
                        }
                        break;

                    //
                    case Key.P:
                        {
                            if (isControl == true)
                            {
                                editor.Print();
                                e.Handled = true;
                            }
                        }
                        break;

                    // undo
                    case Key.Z:
                        {
                            if (isControl == true)
                            {
                                editor.Undo();
                                e.Handled = true;
                            }
                        }
                        break;

                    // redo
                    case Key.Y:
                        {
                            if (isControl == true)
                            {
                                editor.Redo();
                                e.Handled = true;
                            }
                        }
                        break;

                    // cut
                    case Key.X:
                        {
                            if (isControl == true)
                            {
                                editor.Cut();
                                e.Handled = true;
                            }
                        }
                        break;

                    // copy
                    case Key.C:
                        {
                            if (isControl == true)
                            {
                                editor.Copy();
                                e.Handled = true;
                            }
                        }
                        break;

                    // paste
                    case Key.V:
                        {
                            if (isControl == true)
                            {
                                var point = new Point(0.0, 0.0);
                                editor.Paste(point);
                                e.Handled = true;
                            }
                        }
                        break;

                    // select all
                    case Key.A:
                        {
                            if (isControl == true)
                            {
                                editor.SelectAll();
                                e.Handled = true;
                            }
                        }
                        break;

                    // delete
                    case Key.Delete:
                        {
                            editor.Delete();
                            e.Handled = true;
                        }
                        break;

                    // move up
                    case Key.Up:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveUp(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move down
                    case Key.Down:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveDown(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move left
                    case Key.Left:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveLeft(canvas);
                                e.Handled = true;
                            }
                        }
                        break;

                    // move right
                    case Key.Right:
                        {
                            if (e.OriginalSource is ScrollViewer)
                            {
                                MoveRight(canvas);
                                e.Handled = true;
                            }
                        }
                        break;
                }
            }
        }

        #endregion
    }

    #endregion
}
