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
using System.Printing;
using System.Windows.Markup;

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
    using TreeSolution = Tuple<string, string, Stack<Tuple<string, Stack<Stack<string>>>>>;

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

            InitializeEditor();

            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PanScrollViewer.Focus();
        }

        private void InitializeEditor()
        {
            var options = new DiagramEditorOptions();

            editor = new DiagramEditor();
            editor.CurrentOptions = options;

            editor.CurrentOptions.Counter.ProjectCount = 1;
            editor.CurrentOptions.Counter.DiagramCount = 1;

            Action action = () =>
            {
                var prop = options.CurrentProperties;

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
            };

            editor.UpdateDiagramProperties = action;

            editor.CurrentOptions.CurrentResources = this.Resources;

            editor.CurrentOptions.CurrentTree = this.SolutionTree;
            editor.CurrentOptions.CurrentCanvas = this.DiagramCanvas;
            editor.CurrentOptions.CurrentPathGrid = this.PathGrid;

            EnableHistory.IsChecked = options.EnableHistory;
            EnableInsertLast.IsChecked = options.EnableInsertLast;
            EnablePageGrid.IsChecked = options.EnablePageGrid;
            EnablePageTemplate.IsChecked = options.EnablePageTemplate;
            EnableSnap.IsChecked = options.EnableSnap;
            SnapOnRelease.IsChecked = options.SnapOnRelease;

            editor.GenerateGrid(false);
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

        #region Pan

        private void BeginPan(Point point)
        {
            editor.CurrentOptions.PanStart = point;

            editor.CurrentOptions.PreviousScrollOffsetX = -1.0;
            editor.CurrentOptions.PreviousScrollOffsetY = -1.0;

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
            double scrollOffsetX = point.X - editor.CurrentOptions.PanStart.X;
            double scrollOffsetY = point.Y - editor.CurrentOptions.PanStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * 1.0) * editor.CurrentOptions.ReversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * 1.0) * editor.CurrentOptions.ReversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != editor.CurrentOptions.PreviousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                editor.CurrentOptions.PreviousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != editor.CurrentOptions.PreviousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                editor.CurrentOptions.PreviousScrollOffsetY = scrollOffsetY;
            }

            editor.CurrentOptions.PanStart = point;
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
            double l = Math.Log(x, editor.CurrentOptions.ZoomLogBase);
            double e = Math.Exp(l / editor.CurrentOptions.ZoomExpFactor);
            double y = x + x * l * e;
            return y;
        }

        private void Zoom(double zoom)
        {
            if (editor == null || editor.CurrentOptions == null)
                return;

            double zoom_fx = CalculateZoom(zoom);

            //System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            var st = GetZoomScaleTransform();

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            Application.Current.Resources[ResourceConstants.KeyStrokeThickness] = editor.CurrentOptions.DefaultStrokeThickness / zoom_fx;

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

            double oldX = editor.CurrentOptions.ZoomPoint.X * oldZoom;
            double oldY = editor.CurrentOptions.ZoomPoint.Y * oldZoom;

            double newX = editor.CurrentOptions.ZoomPoint.X * zoom;
            double newY = editor.CurrentOptions.ZoomPoint.Y * zoom;

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

            zoom += editor.CurrentOptions.ZoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= editor.CurrentOptions.ZoomOutFactor;

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

            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.CurrentOptions.ZoomPoint = e.GetPosition(canvas);

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
            if (e.ChangedButton == editor.CurrentOptions.PanButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == editor.CurrentOptions.PanButton)
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
            var canvas = editor.CurrentOptions.CurrentCanvas;
            var point = e.GetPosition(canvas);

            if (editor.CurrentOptions.CurrentRoot == null && editor.CurrentOptions.CurrentLine == null && editor.CurrentOptions.EnableInsertLast == false)
            {
                editor.CurrentOptions.SelectionOrigin = point;

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
            var canvas = editor.CurrentOptions.CurrentCanvas;

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
                            if (ElementThumb.GetIsSelected(element) == false)
                            {
                                ElementThumb.SetIsSelected(element, true);
                            }
                            else
                            {
                                ElementThumb.SetIsSelected(element, false);
                            }
                        }
                    }

                    RemoveAdorner(canvas);
                }
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editor.CurrentOptions.SkipLeftClick == true)
            {
                editor.CurrentOptions.SkipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = editor.CurrentOptions.CurrentCanvas;
            var point = e.GetPosition(canvas);
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = editor.HandlePreviewLeftDown(canvas, point, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            var point = e.GetPosition(canvas);

            if (canvas.IsMouseCaptured)
            {
                if (adorner == null)
                {
                    CreateAdorner(canvas, editor.CurrentOptions.SelectionOrigin, point);
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
            var canvas = editor.CurrentOptions.CurrentCanvas;
            var path = editor.CurrentOptions.CurrentPathGrid;

            editor.CurrentOptions.RightClick = e.GetPosition(canvas);

            var result = editor.HandleRightDown(canvas, path);
            if (result == true)
            {
                editor.CurrentOptions.SkipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (editor.CurrentOptions.SkipContextMenu == true)
            {
                editor.CurrentOptions.SkipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                editor.CurrentOptions.SkipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertPin(canvas, editor.CurrentOptions.RightClick);

            editor.CurrentOptions.LastInsert = ModelConstants.TagElementPin;
            editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertInput(canvas, editor.CurrentOptions.RightClick);

            editor.CurrentOptions.LastInsert = ModelConstants.TagElementInput;
            editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOutput(canvas, editor.CurrentOptions.RightClick);

            editor.CurrentOptions.LastInsert = ModelConstants.TagElementOutput;
            editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertAndGate(canvas, editor.CurrentOptions.RightClick);

            editor.CurrentOptions.LastInsert = ModelConstants.TagElementAndGate;
            editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;

            editor.AddToHistory(canvas);

            editor.InsertOrGate(canvas, editor.CurrentOptions.RightClick);

            editor.CurrentOptions.LastInsert = ModelConstants.TagElementOrGate;
            editor.CurrentOptions.SkipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;
            var point = new Point(editor.CurrentOptions.RightClick.X, editor.CurrentOptions.RightClick.Y);

            editor.Delete(canvas, point);
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;
            var point = new Point(editor.CurrentOptions.RightClick.X, editor.CurrentOptions.RightClick.Y);

            editor.ToggleStart(canvas, point);
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;
            var point = new Point(editor.CurrentOptions.RightClick.X, editor.CurrentOptions.RightClick.Y);

            editor.ToggleEnd(canvas, point);
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.EnableHistory = EnableHistory.IsChecked == true ? true : false;

            if (editor.CurrentOptions.EnableHistory == false)
            {
                var canvas = editor.CurrentOptions.CurrentCanvas;

                editor.ClearHistory(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.EnableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.SnapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.EnableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        }

        private void EnablePageGrid_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.EnablePageGrid = EnablePageGrid.IsChecked == true ? true : false;
        }

        private void EnablePageTemplate_Click(object sender, RoutedEventArgs e)
        {
            editor.CurrentOptions.EnablePageTemplate = EnablePageTemplate.IsChecked == true ? true : false;
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

            var solution = editor.GenerateSolution(System.IO.Directory.GetCurrentDirectory());

            this.TextModel.Text = solution.Item1;
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
            editor.GenerateGrid(true);
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

            var canvas = editor.CurrentOptions.CurrentCanvas;

            var oldItem = e.OldValue as TreeViewItem;
            var newItem = e.NewValue as TreeViewItem;

            bool isDiagram = editor.SwitchItems(canvas, oldItem, newItem);
            if (isDiagram == true)
            {
                EnablePage.IsChecked = true;
            }
            else
            {
                EnablePage.IsChecked = false;
            }
        }

        private void SolutionAddProject_Click(object sender, RoutedEventArgs e)
        {
            var solution = SolutionTree.SelectedItem as TreeViewItem;

            editor.AddProject(solution);
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            editor.AddDiagram(project, true);
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            editor.DeleteProject(project);
        }

        private void ProjectDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            var diagram = SolutionTree.SelectedItem as TreeViewItem;

            editor.DeleteDiagram(diagram);
        }

        #endregion

        #region Main Menu Events

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            editor.NewSolution();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            editor.OpenSolution();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            editor.SaveSolution();
        }

        private void FileOpenDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.OpenDiagram();
        }

        private void FileSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            editor.SaveDiagram();
        }

        private void FileOpenTags_Click(object sender, RoutedEventArgs e)
        {
            editor.OpenTags();
        }

        private void FileSaveTags_Click(object sender, RoutedEventArgs e)
        {
            editor.SaveTags();
        }

        private void FileImportTags_Click(object sender, RoutedEventArgs e)
        {
            editor.ImportTags();
        }

        private void FileExportTags_Click(object sender, RoutedEventArgs e)
        {
            editor.ExportTags();
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
            Print();
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

        private void ToolsTagEditor_Click(object sender, RoutedEventArgs e)
        {
            ShowTagEditor();
        }

        #endregion

        #region Print

        public string LogicDictionaryUri = "LogicDictionary.xaml";

        private void SetPrintColors(FrameworkElement element)
        {
            if (element == null)
                throw new ArgumentNullException();

            var backgroundColor = element.Resources["LogicBackgroundColorKey"] as SolidColorBrush;
            backgroundColor.Color = Colors.White;

            var gridColor = element.Resources["LogicGridColorKey"] as SolidColorBrush;
            gridColor.Color = Colors.Transparent;

            var pageColor = element.Resources["LogicTemplateColorKey"] as SolidColorBrush;
            pageColor.Color = Colors.Black;

            var logicColor = element.Resources["LogicColorKey"] as SolidColorBrush;
            logicColor.Color = Colors.Black;

            var logicSelectedColor = element.Resources["LogicSelectedColorKey"] as SolidColorBrush;
            logicSelectedColor.Color = Colors.Black;

            var helperColor = element.Resources["LogicTransparentColorKey"] as SolidColorBrush;
            helperColor.Color = Colors.Transparent;
        }

        public FrameworkElement CreateContextElement(string diagram, Size areaExtent, Point origin, Rect area)
        {
            var grid = new Grid() { ClipToBounds = true };

            // set print dictionary
            grid.Resources.Source = new Uri(LogicDictionaryUri, UriKind.Relative);

            // set print colors
            SetPrintColors(grid);

            // set element template and content
            var template = new Control() { Template = grid.Resources["LandscapePageTemplateKey"] as ControlTemplate };

            var canvas = new Canvas()
            {
                Width = editor.CurrentOptions.CurrentCanvas.Width,
                Height = editor.CurrentOptions.CurrentCanvas.Height
            };

            LineEx.SetShortenStart(grid, ShortenStart.IsChecked.Value);
            LineEx.SetShortenEnd(grid, ShortenEnd.IsChecked.Value);

            editor.ParseDiagramModel(diagram, canvas, null, 0, 0, false, false, false, true);

            grid.Children.Add(template);
            grid.Children.Add(canvas);

            return grid;
        }

        public FixedDocument CreateFixedDocument(IEnumerable<string> diagrams, Size areaExtent, Size areaOrigin)
        {
            if (diagrams == null)
                throw new ArgumentNullException();

            var origin = new Point(areaOrigin.Width, areaOrigin.Height);
            var area = new Rect(origin, areaExtent);
            var scale = Math.Min(areaExtent.Width / 1260, areaExtent.Height / 891);

            // create fixed document
            var fixedDocument = new FixedDocument();

            //fixedDocument.DocumentPaginator.PageSize = new Size(areaExtent.Width, areaExtent.Height);

            foreach (var diagram in diagrams)
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage();

                //pageContent.Child = fixedPage;
                ((IAddChild)pageContent).AddChild(fixedPage);

                fixedDocument.Pages.Add(pageContent);

                fixedPage.Width = areaExtent.Width;
                fixedPage.Height = areaExtent.Height;

                var element = CreateContextElement(diagram, areaExtent, origin, area);

                // transform and scale for print
                element.LayoutTransform = new ScaleTransform(scale, scale);

                // set element position
                FixedPage.SetLeft(element, areaOrigin.Width);
                FixedPage.SetTop(element, areaOrigin.Height);

                // add element to page
                fixedPage.Children.Add(element);

                // update fixed page layout
                //fixedPage.Measure(areaExtent);
                //fixedPage.Arrange(area);
            }

            return fixedDocument;
        }

        public FixedDocumentSequence CreateFixedDocumentSequence(IEnumerable<IEnumerable<string>> projects, Size areaExtent, Size areaOrigin)
        {
            if (projects == null)
                throw new ArgumentNullException();

            var fixedDocumentSeq = new FixedDocumentSequence();

            foreach (var diagrams in projects)
            {
                var fixedDocument = CreateFixedDocument(diagrams, areaExtent, areaOrigin);

                var documentRef = new DocumentReference();
                documentRef.BeginInit();
                documentRef.SetDocument(fixedDocument);
                documentRef.EndInit();

                (fixedDocumentSeq as IAddChild).AddChild(documentRef);
            }

            return fixedDocumentSeq;
        }

        private void SetPrintDialogOptions(PrintDialog dlg)
        {
            if (dlg == null)
                throw new ArgumentNullException();

            dlg.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();

            dlg.PrintTicket = dlg.PrintQueue.DefaultPrintTicket;
            dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
            dlg.PrintTicket.OutputQuality = OutputQuality.High;
            dlg.PrintTicket.TrueTypeFontMode = TrueTypeFontMode.DownloadAsNativeTrueTypeFont;
        }

        private bool ShowPrintDialog(PrintDialog dlg)
        {
            if (dlg == null)
                throw new ArgumentNullException();

            // configure printer
            SetPrintDialogOptions(dlg);

            // show print dialog
            if (dlg.ShowDialog() == true)
                return true;
            else
                return false;
        }

        public void Print(IEnumerable<string> diagrams, string name)
        {
            if (diagrams == null)
                throw new ArgumentNullException();

            var dlg = new PrintDialog();

            ShowPrintDialog(dlg);

            // print capabilities
            var caps = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            var areaExtent = new Size(caps.PageImageableArea.ExtentWidth, caps.PageImageableArea.ExtentHeight);
            var areaOrigin = new Size(caps.PageImageableArea.OriginWidth, caps.PageImageableArea.OriginHeight);

            // create document
            var s = System.Diagnostics.Stopwatch.StartNew();

            var fixedDocument = CreateFixedDocument(diagrams, areaExtent, areaOrigin);

            s.Stop();
            System.Diagnostics.Debug.Print("CreateFixedDocument in {0}ms", s.Elapsed.TotalMilliseconds);

            // print document
            dlg.PrintDocument(fixedDocument.DocumentPaginator, name);
        }

        public void PrintSequence(IEnumerable<IEnumerable<string>> projects, string name)
        {
            if (projects == null)
                throw new ArgumentNullException();

            var dlg = new PrintDialog();

            ShowPrintDialog(dlg);

            // print capabilities
            var caps = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            var areaExtent = new Size(caps.PageImageableArea.ExtentWidth, caps.PageImageableArea.ExtentHeight);
            var areaOrigin = new Size(caps.PageImageableArea.OriginWidth, caps.PageImageableArea.OriginHeight);

            // create document
            var s = System.Diagnostics.Stopwatch.StartNew();

            var fixedDocumentSeq = CreateFixedDocumentSequence(projects, areaExtent, areaOrigin);

            s.Stop();
            System.Diagnostics.Debug.Print("CreateFixedDocumentSequence in {0}ms", s.Elapsed.TotalMilliseconds);

            // print document
            dlg.PrintDocument(fixedDocumentSeq.DocumentPaginator, name);
        }

        public void Print()
        {
            //var model = editor.GenerateDiagramModel(editor.CurrentOptions.CurrentCanvas, null);
            //var diagrams = new List<string>();
            //diagrams.Add(model);

            var diagrams = editor.GenerateSolution(null).Item2;

            Print(diagrams, "solution");
        }

        #endregion

        #region Window Key Events

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //System.Diagnostics.Debug.Print("PreviewKeyDown sender: {0}, source: {1}", sender.GetType(), e.OriginalSource.GetType());

            if (!(e.OriginalSource is TextBox) &&
                Keyboard.Modifiers != ModifierKeys.Shift)
            {
                HandleKey(e);
            }
        }

        private void HandleKey(KeyEventArgs e)
        {
            var canvas = editor.CurrentOptions.CurrentCanvas;
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

                                editor.AddDiagram(project, true);
                            }
                            else if (isSelectedProject == true)
                            {
                                editor.AddDiagram(selected, false);
                            }
                            else if (isSelectedSolution == true)
                            {
                                editor.AddProject(selected);
                            }
                        }
                    }
                    break;

                // open solution
                case Key.O:
                    {
                        if (isControl == true)
                        {
                            editor.OpenSolution();
                            e.Handled = true;
                        }
                    }
                    break;

                // save solution
                case Key.S:
                    {
                        if (isControl == true)
                        {
                            editor.SaveSolution();
                            e.Handled = true;
                        }
                    }
                    break;

                // new solution
                case Key.N:
                    {
                        if (isControl == true)
                        {
                            editor.NewSolution();
                        }
                    }
                    break;

                // open tags
                case Key.T:
                    {
                        if (isControl == true)
                        {
                            editor.OpenTags();
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

                // print
                case Key.P:
                    {
                        if (isControl == true)
                        {
                            Print();
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
                            editor.MoveUp(canvas);
                            e.Handled = true;
                        }
                    }
                    break;

                // move down
                case Key.Down:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            editor.MoveDown(canvas);
                            e.Handled = true;
                        }
                    }
                    break;

                // move left
                case Key.Left:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            editor.MoveLeft(canvas);
                            e.Handled = true;
                        }
                    }
                    break;

                // move right
                case Key.Right:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            editor.MoveRight(canvas);
                            e.Handled = true;
                        }
                    }
                    break;

                // tag editor
                case Key.F5:
                    {
                        ShowTagEditor();
                    }
                    break;
            }
        }

        #endregion

        #region Tag Editor

        private void ShowTagEditor()
        {
            var window = new TagEditorWindow();
            var control = window.TagEditorControl;

            if (editor.CurrentOptions.Tags == null)
            {
                editor.CurrentOptions.Tags = new List<object>();
            }

            var selected = GetSelectedInputOutputElements();

            if (selected.Count == 0)
            {
                var all = GetAllInputOutputElements();

                control.Selected = all;
            }
            else
            {
                control.Selected = selected;
            }

            control.Tags = editor.CurrentOptions.Tags;

            window.ShowDialog();
        }

        private List<FrameworkElement> GetAllInputOutputElements()
        {
            var all = editor.GetAllElements().Where(x =>
            {
                string uid = x.Uid;
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return all;
        }

        private List<FrameworkElement> GetSelectedInputOutputElements()
        {
            var selected = editor.GetSelectedElements().Where(x =>
            {
                string uid = x.Uid;
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return selected;
        }

        #endregion
    }

    #endregion
}
