// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Util;
using CanvasDiagramEditor.Dxf.Core;
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
    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private DiagramEditor Editor { get; set; }

        private string LogicDictionaryUri = "Views/LogicDictionary.xaml";

        private PointEx InsertPointInput = new PointEx(30, 30.0);
        private PointEx InsertPointOutput = new PointEx(930.0, 30.0);
        private PointEx InsertPointGate = new PointEx(325.0, 30.0);

        private const double PageWidth = 1260;
        private const double PageHeight = 891;

        private LineGuidesAdorner GuidesAdorner = null;

        private const double GuideSpeedUpLevel1 = 2.0;
        private const double GuideSpeedUpLevel2 = 2.0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeEditor();
            InitializeDiagramControl();
            InitializeWindowEvents();
            InitializeFileMenuEvents();
            InitializeEditMenuEvents();
            InitializeViewMenuEvents();
            InitializeHelpMenuEvents();
        }

        #endregion

        #region Initialize

        private void InitializeFileMenuEvents()
        {
            FileNew.Click += (sender, e) => NewSolution();
            FileOpen.Click += (sender, e) => OpenSolution();
            FileSave.Click += (sender, e) => Editor.SaveSolution();;
            FileOpenDiagram.Click += (sender, e) => Editor.OpenDiagram();
            FileSaveDiagram.Click += (sender, e) => Editor.SaveDiagram();
            FileOpenTags.Click += (sender, e) =>OpenTags();
            FileSaveTags.Click += (sender, e) => Editor.TagsSave();
            FileImportTags.Click += (sender, e) => ImportTags();
            FileExportTags.Click += (sender, e) => Editor.TagsExport();

            FileExportToDxf.Click += (sender, e) =>
            {
                Editor.DxfExport(ShortenStart.IsChecked.Value, 
                    ShortenEnd.IsChecked.Value, 
                    TableGrid.GetData(this) as DiagramTable);
            };

            FileInspectDxf.Click += (sender, e) => (new DxfInspect()).Inspect();
            FilePrint.Click += (sender, e) => Print();
            FilePrintHistory.Click += (sender, e) => PrintHistory();
            FileExit.Click += (sender, e) => Application.Current.Shutdown();
        }

        private void InitializeEditMenuEvents()
        {
            EditUndo.Click += (sender, e) => Editor.HistoryUndo();
            EditRedo.Click += (sender, e) => Editor.HistoryRedo();
            EditCut.Click += (sender, e) => Editor.EditCut();
            EditCopy.Click += (sender, e) => Editor.EditCopy();
            EditPaste.Click += (sender, e) => Editor.EditPaste(new PointEx(0.0, 0.0), true);
            EditDelete.Click += (sender, e) => Delete();
            EditSelectAll.Click += (sender, e) => Editor.SelectAll();
            EditDeselectAll.Click += (sender, e) => DeselectAll();
            EditSelectPrevious.Click += (sender, e) => Editor.SelectPrevious(!(Keyboard.Modifiers == ModifierKeys.Control));
            EditSelectNext.Click += (sender, e) => Editor.SelectNext(!(Keyboard.Modifiers == ModifierKeys.Control));
            EditSelectConnected.Click += (sender, e) => Editor.SelectConnected();
            EditClear.Click += (sender, e) => Editor.ModelClear();
            EditResetThumbTags.Click += (sender, e) => Editor.ModelResetThumbTags();
            EditConnect.Click += (sender, e) => Connect();
            EditOptions.Click += (sender, e) => TabOptions.IsSelected = true;
        }

        private void InitializeViewMenuEvents()
        {
            ViewProjectDiagrams.Click += (sender, e) => ShowProjectDiagrams();
            ViewSolutionDiagrams.Click += (sender, e) => ShowSolutionDiagrams();
            ViewDiagram.Click += (sender, e) => ShowDiagram();
            ViewDiagramSelectedElements.Click += (sender, e) => ShowDiagramSelectedElements();
            ViewDiagramHistory.Click += (sender, e) => ShowDiagramHistory();
            ViewPreviousDiagramProject.Click += (sender, e) => Editor.TreeSelectPreviousItem(false);
            ViewNextDiagramProjcet.Click += (sender, e) => Editor.TreeSelectNextItem(false);
            ViewPreviousDiagramSolution.Click += (sender, e) => Editor.TreeSelectPreviousItem(true);
            ViewNextDiagramSolution.Click += (sender, e) => Editor.TreeSelectNextItem(true);
            ViewToggleGuides.Click += (sender, e) => ToggleGuides();
        }

        private void InitializeHelpMenuEvents()
        {
            HelpAbout.Click += (sender, e) =>
            {
                MessageBox.Show("Canvas Diagram Editor\n\n" +
                    "Copyright (C) Wiesław Šoltés 2013.\n" +
                    "All Rights Reserved",
                    "About Canvas Diagram Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };
        }

        private void InitializeDiagramControl()
        {
            this.DiagramControl.Editor = this.Editor;
            this.DiagramControl.ZoomSlider = this.ZoomSlider;
        }

        private void InitializeWindowEvents()
        {
            this.Loaded += (sender, e) =>
            {
                this.DiagramControl.PanScrollViewer.Focus();
                SetCurrentTable();
                InitializeTagEditor();
            };

            this.MouseMove += (sender, e) =>
            {
                if (GuidesAdorner != null)
                {
                    var canvas = this.DiagramControl.DiagramCanvas;
                    var point = e.GetPosition(canvas);
                    double x = Editor.SnapOffsetX(point.X, true);
                    double y = Editor.SnapOffsetY(point.Y, true);

                    GuidesAdorner.X = x;
                    GuidesAdorner.Y = y;
                }
            };

            this.PreviewKeyDown += (sender, e) =>
            {
                if (!(e.OriginalSource is TextBox) &&
                    Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    HandleKeyEvents(e);
                }
            };
        }

        private void InitializeEditor()
        {
            Editor = new DiagramEditor();
            Editor.Context = new Context();

            Editor.Context.CurrentTree = this.SolutionTree;
            Editor.Context.CurrentCanvas = this.DiagramControl.DiagramCanvas;

            var counter = new IdCounter();
            counter.ProjectCount = 1;
            counter.DiagramCount = 1;
            this.DiagramControl.DiagramCanvas.SetCounter(counter);

            var properties = new DiagramProperties();
            this.DiagramControl.DiagramCanvas.SetProperties(properties);

            Editor.Context.IsControlPressed = () => Keyboard.Modifiers == ModifierKeys.Control;
            Editor.Context.UpdateProperties = () => UpdateProperties(Editor.Context.CurrentCanvas.GetProperties());

            Editor.Context.Clipboard = new WindowsClipboard();

            // diagram creator
            var creator = GetDiagramCreator();

            Editor.Context.DiagramCreator = creator;

            // set checkbox states
            EnableHistory.IsChecked = Editor.Context.EnableHistory;
            EnableInsertLast.IsChecked = Editor.Context.EnableInsertLast;
            EnableSnap.IsChecked = Editor.Context.EnableSnap;
            SnapOnRelease.IsChecked = Editor.Context.SnapOnRelease;

            // tree actions
            Editor.Context.CreateTreeSolutionItem = () => CreateTreeSolutionItem();
            Editor.Context.CreateTreeProjectItem = () => CreateTreeProjectItem();
            Editor.Context.CreateTreeDiagramItem = () => CreateTreeDiagramItem();

            // update canvas grid
            Editor.Context.UpdateProperties();
            Model.SetGrid(Editor.Context.CurrentCanvas, 
                Editor.Context.DiagramCreator,
                false);
        }

        private void SetCurrentTable()
        {
            var table = new DiagramTable()
            {
                Id = 0,
                Revision1 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                Revision2 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                Revision3 = new Revision()
                {
                    Version = "",
                    Date = "",
                    Remarks = "",
                },
                Logo1 = null,
                Logo2 = null,
                Drawn = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Checked = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Approved = new Person()
                {
                    Name = "user",
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                },
                Title = "LOGIC DIAGRAM",
                SubTitle1 = "DIAGRAM TITLE",
                SubTitle2 = "",
                SubTitle3 = "",
                Rev = "0",
                Status = "-",
                Page = "-",
                Pages = "-",
                Project = "sample",
                OrderNo = "",
                DocumentNo = "",
                ArchiveNo = ""
            };

            TableGrid.SetData(this, table);
        }

        private IDiagramCreator GetDiagramCreator()
        {
            var creator = new WpfDiagramCreator();

            creator.SetThumbEvents = (thumb) => SetThumbEvents(thumb);
            creator.SetPosition = (element, left, top, snap) => Editor.SetPosition(element, left, top, snap);
            
            creator.GetTags = () => Editor.Context.Tags;
            creator.GetCounter = () => Editor.Context.CurrentCanvas.GetCounter();

            creator.SetCanvas(this.DiagramControl.DiagramCanvas);
            creator.ParserPath = this.DiagramControl.PathGrid;

            return creator;
        }

        private void SetThumbEvents(ElementThumb thumb)
        {
            thumb.DragDelta += (sender, e) =>
            {
                var canvas = Editor.Context.CurrentCanvas;
                var element = sender as IThumb;

                double dX = e.HorizontalChange;
                double dY = e.VerticalChange;

                Editor.Drag(canvas, element, dX, dY);
            };

            thumb.DragStarted += (sender, e) =>
            {
                var canvas = Editor.Context.CurrentCanvas;
                var element = sender as IThumb;

                Editor.DragStart(canvas, element);
            };

            thumb.DragCompleted += (sender, e) =>
            {
                var canvas = Editor.Context.CurrentCanvas;
                var element = sender as IThumb;

                Editor.DragEnd(canvas, element);
            };
        }

        private void UpdateProperties(DiagramProperties prop)
        {
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

        private ITreeItem CreateTreeDiagramItem()
        {
            var diagram = new SolutionTreeViewItem();

            diagram.Header = ModelConstants.TagHeaderDiagram;
            diagram.ContextMenu = this.Resources["DiagramContextMenuKey"] as ContextMenu;
            diagram.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;

            return diagram as ITreeItem;
        }

        private ITreeItem CreateTreeProjectItem()
        {
            var project = new SolutionTreeViewItem();

            project.Header = ModelConstants.TagHeaderProject;
            project.ContextMenu = this.Resources["ProjectContextMenuKey"] as ContextMenu;
            project.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;
            project.IsExpanded = true;

            return project as ITreeItem;
        }

        private ITreeItem CreateTreeSolutionItem()
        {
            var solution = new SolutionTreeViewItem();

            solution.Header = ModelConstants.TagHeaderSolution;
            solution.ContextMenu = this.Resources["SolutionContextMenuKey"] as ContextMenu;
            solution.MouseRightButtonDown += TreeViewItem_MouseRightButtonDown;
            solution.IsExpanded = true;

            return solution as ITreeItem;
        }

        private void OpenSolution()
        {
            Editor.OpenSolution();
            InitializeTagEditor();
        }

        private void NewSolution()
        {
            Editor.TreeCreateNewSolution();
            InitializeTagEditor();
        }

        private void OpenTags()
        {
            Editor.TagsOpen();
            InitializeTagEditor();
        }

        private void ImportTags()
        {
            Editor.TagsImport();
            InitializeTagEditor();
        }

        private void DeselectAll()
        {
            var canvas = Editor.Context.CurrentCanvas;

            // deselect all
            Editor.SelectNone();

            // cancel connection
            Editor.MouseEventRightDown(canvas);

            // hide guides
            if (GuidesAdorner != null)
                HideGuides();
        }

        private double CalculateMoveSpeedUp(int delta)
        {
            return (delta > -200.0 && delta < -50.0) ? 
                GuideSpeedUpLevel1 : (delta > -50.0) ? 
                GuideSpeedUpLevel2 : 1.0;
        }

        private void MoveUp(int timeStamp)
        {
            var canvas = Editor.Context.CurrentCanvas;

            if (GuidesAdorner == null)
            {
                Editor.MoveUp(canvas);
            }
            else
            {
                double speedUp = CalculateMoveSpeedUp(timeStamp - Environment.TickCount);
                var prop = Editor.Context.CurrentCanvas.GetProperties();
                double y = GuidesAdorner.Y;

                y -= prop.SnapY * speedUp;
                if (y >= (prop.SnapOffsetY + prop.SnapY))
                    GuidesAdorner.Y = y;
            }
        }

        private void MoveDown(int timeStamp)
        {
            var canvas = Editor.Context.CurrentCanvas;

            if (GuidesAdorner == null)
            {
                Editor.MoveDown(canvas);
            }
            else
            {
                double speedUp = CalculateMoveSpeedUp(timeStamp - Environment.TickCount);
                var prop = Editor.Context.CurrentCanvas.GetProperties();
                double y = GuidesAdorner.Y;

                y += prop.SnapY * speedUp;
                if (y <= canvas.GetHeight() - prop.SnapY - prop.SnapOffsetY)
                    GuidesAdorner.Y = y;
            }
        }

        private void MoveLeft(int timeStamp)
        {
            var canvas = Editor.Context.CurrentCanvas;

            if (GuidesAdorner == null)
            {
                Editor.MoveLeft(canvas);
            }
            else
            {
                double speedUp = CalculateMoveSpeedUp(timeStamp - Environment.TickCount);
                var prop = Editor.Context.CurrentCanvas.GetProperties();
                double x = GuidesAdorner.X;

                x -= prop.SnapX * speedUp;
                if (x >= (prop.SnapOffsetX + prop.SnapX))
                    GuidesAdorner.X = x;
            }
        }

        private void MoveRight(int timeStamp)
        {
            var canvas = Editor.Context.CurrentCanvas;

            if (GuidesAdorner == null)
            {
                Editor.MoveRight(canvas);
            }
            else
            {
                double speedUp = CalculateMoveSpeedUp(timeStamp - Environment.TickCount);
                var prop = Editor.Context.CurrentCanvas.GetProperties();
                double x = GuidesAdorner.X;

                x += prop.SnapX * speedUp;
                if (x <= canvas.GetWidth() - prop.SnapX - prop.SnapOffsetX)
                    GuidesAdorner.X = x;
            }
        }

        private void Delete()
        {
            if (GuidesAdorner == null)
            {
                // delete selected elements
                Editor.EditDelete();
            }
            else
            {
                var canvas = Editor.Context.CurrentCanvas;
                var elements = Model.GetSelected(canvas);

                // delete selected elements
                if (elements.Count() > 0)
                {
                    Editor.EditDelete(canvas, elements);
                }
                // delete single element using guides
                else
                {
                    Editor.Delete(canvas, GetInsertionPoint());
                }
            }
        }

        #endregion

        #region Handle Key Events

        private void HandleKeyEvents(KeyEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;
            bool isControl = Keyboard.Modifiers == ModifierKeys.Control;
            bool canMove = e.OriginalSource is ScrollViewer;
            int timeStamp = e.Timestamp;
            var key = e.Key;

            switch (key)
            {
                // '<' -> select previous solution tree item
                case Key.OemComma:
                    Editor.TreeSelectPreviousItem(isControl);
                    break;

                // '>' -> select next solution tree item
                case Key.OemPeriod:
                    Editor.TreeSelectNextItem(isControl);
                    break;

                // '[' -> select previous element
                // - use control Key to select many element
                case Key.OemOpenBrackets:
                    Editor.SelectPrevious(!isControl);
                    break;

                // ']' -> select next element
                // - use control Key to select many element
                case Key.OemCloseBrackets:
                    Editor.SelectNext(!isControl);
                    break;

                // '|' -> select connected elements
                case Key.OemPipe:
                    Editor.SelectConnected();
                    break;

                // Ctrl+J -> // insert new item and paste from clipboard
                // - add new project to selected solution
                // - add new diagram to selected project
                // - add new diagram after selected diagram and select new diagram
                case Key.J:
                    {
                        if (isControl == true)
                            Editor.TreeAddNewItemAndPaste();
                    }
                    break;

                // Ctrl+M -> insert new item
                case Key.M:
                    {
                        if (isControl == true)
                            Editor.TreeAddNewItem();
                    }
                    break;

                // Ctrl+O -> open solution
                // O -> insert Output
                case Key.O:
                    {
                        if (isControl == true)
                            OpenSolution();
                        else
                            InsertOutput(canvas, GetInsertionPoint());
                    }
                    break;

                // Ctrl+S -> save solution
                // S -> invert wire start
                case Key.S:
                    {
                        if (isControl == true)
                            Editor.SaveSolution();
                        else
                            Editor.WireToggleStart();
                    }
                    break;

                // Ctrl+N -> new solution
                case Key.N:
                    {
                        if (isControl == true)
                            NewSolution();
                    }
                    break;

                // Ctrl+T -> open tags
                case Key.T:
                    {
                        if (isControl == true)
                            OpenTags();
                    }
                    break;

                // Ctrl+I -> import Tags
                // I -> insert Input
                case Key.I:
                    {
                        if (isControl == true)
                            ImportTags();
                        else
                            InsertInput(canvas, GetInsertionPoint());
                    }
                    break;

                // Ctrl+E -> export to dxf
                // E -> invert wire end
                case Key.E:
                    {
                        if (isControl == true)
                        {
                            Editor.DxfExport(ShortenStart.IsChecked.Value,
                                ShortenEnd.IsChecked.Value,
                                TableGrid.GetData(this) as DiagramTable);
                        }
                        else
                        {
                            Editor.WireToggleEnd();
                        }
                    }
                    break;

                // Ctrl+P -> print
                case Key.P:
                    {
                        if (isControl == true)
                            Print();
                    }
                    break;

                // Ctrl+R -> reset tags
                // R -> insert OrGate
                case Key.R:
                    {
                        if (isControl == true)
                            Editor.ModelResetThumbTags();
                        else
                            InsertOrGate(canvas, GetInsertionPoint());
                    }
                    break;

                // Ctrl+Z -> undo
                case Key.Z:
                    {
                        if (isControl == true)
                            Editor.HistoryUndo();
                    }
                    break;

                // Ctrl+Y -> redo
                case Key.Y:
                    {
                        if (isControl == true)
                            Editor.HistoryRedo();
                    }
                    break;

                // Ctrl+X -> cut
                case Key.X:
                    {
                        if (isControl == true)
                            Editor.EditCut();
                    }
                    break;

                // Ctrl+C -> copy
                // C -> connect
                case Key.C:
                    {
                        if (isControl == true)
                            Editor.EditCopy();
                        else
                            Connect();
                    }
                    break;

                // Ctrl+V -> paste from clipboard
                case Key.V:
                    {
                        if (isControl == true)
                            Editor.EditPaste(new PointEx(0.0, 0.0), true);
                    }
                    break;

                // Ctrl+A -> select all
                // A -> insert AndGate
                case Key.A:
                    {
                        if (isControl == true)
                            Editor.SelectAll();
                        else
                            InsertAndGate(canvas, GetInsertionPoint());
                    }
                    break;

                // Del -> delete
                case Key.Delete:
                    Delete();
                    break;

                // Up Arrow -> move selected elements/line guides up
                case Key.Up:
                    {
                        if (canMove == true)
                            MoveUp(timeStamp);
                    }
                    break;

                // Down Arrow -> move selected elements/line guides down
                case Key.Down:
                    {
                        if (canMove == true)
                            MoveDown(timeStamp);
                    }
                    break;

                // Left Arrow -> move selected elements/line guides left
                case Key.Left:
                    {
                        if (canMove == true)
                            MoveLeft(timeStamp);
                    }
                    break;

                // Right Arrow -> move selected elements/line guides right
                case Key.Right:
                    {
                        if (canMove == true)
                            MoveRight(timeStamp);
                    }
                    break;

                // F5 -> tag editor
                case Key.F5:
                    InitializeTagEditor();
                    break;

                // F6 -> table editor
                case Key.F6:
                    ShowTableEditor();
                    break;

                // Ctrl+H -> show diagram history
                case Key.H:
                    {
                        if (isControl == true)
                            ShowDiagramHistory();
                    }
                    break;

                // F7 -> show project diagrams
                case Key.F7:
                    ShowProjectDiagrams();
                    break;

                // F8 -> show solution diagrams
                case Key.F8:
                    ShowSolutionDiagrams();
                    break;

                // G -> show/hide guides
                case Key.G:
                    ToggleGuides();
                    break;

                // Esc -> deselect all/cancel connection/hide guides
                case Key.Escape:
                    DeselectAll();
                    break;
            }
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.EnableHistory = EnableHistory.IsChecked == true ? true : false;

            if (Editor.Context.EnableHistory == false)
            {
                var canvas = Editor.Context.CurrentCanvas;

                History.Clear(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.EnableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.SnapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.EnableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
        }

        private void EnablePage_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.DiagramControl;
            var visibility = diagram.Visibility;
            diagram.Visibility = visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EnablePageGrid_Click(object sender, RoutedEventArgs e)
        {
            var grid = this.DiagramControl.DiagramGrid;
            var visibility = grid.Visibility;
            grid.Visibility = visibility == Visibility.Collapsed ? Visibility.Visible :Visibility.Collapsed;
        }

        private void EnablePageTemplate_Click(object sender, RoutedEventArgs e)
        {
            var template = this.DiagramControl.DiagramTemplate;
            var visibility = template.Visibility;
            template.Visibility = visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Button Events

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            LogBaseSlider.Value = 1.9;
            ExpFactorSlider.Value = 1.3;
            ZoomSlider.Value = 1.0;
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            Editor.ModelUpdateSelectedDiagram();

            var solution = Editor.ModelGenerateSolution(System.IO.Directory.GetCurrentDirectory(), false);

            this.TextModel.Text = solution.Item1;
        }

        private void GenerateModelFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var diagram = Editor.ModelGenerateFromSelected(Editor.Context.CurrentCanvas);

            this.TextModel.Text = diagram;
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            Editor.ModelInsert(diagram, offsetX, offsetY, true);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.UpdateProperties();

            Model.SetGrid(Editor.Context.CurrentCanvas,
                Editor.Context.DiagramCreator,
                true);
        }

        #endregion

        #region TreeView Events

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as SolutionTreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
                item.BringIntoView();

                e.Handled = true;
            }
        }

        private void SolutionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Editor == null)
                return;

            var canvas = Editor.Context.CurrentCanvas;
            var creator = Editor.Context.DiagramCreator;

            var oldItem = e.OldValue as SolutionTreeViewItem;
            var newItem = e.NewValue as SolutionTreeViewItem;

            bool isDiagram = Editor.TreeSwitchItems(canvas, creator, oldItem, newItem);
            if (isDiagram == true)
            {
                this.DiagramControl.PanScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                this.DiagramControl.PanScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void SolutionAddProject_Click(object sender, RoutedEventArgs e)
        {
            var solution = SolutionTree.SelectedItem as SolutionTreeViewItem;

            Editor.TreeAddProject(solution);
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.TreeAddNewItem();
        }

        private void DiagramAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.TreeAddNewItem();
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as SolutionTreeViewItem;

            Editor.TreeDeleteProject(project);
        }

        private void DiagramDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            var diagram = SolutionTree.SelectedItem as SolutionTreeViewItem;

            Editor.TreeDeleteDiagram(diagram);
        }

        private void DiagramAddDiagramAndPaste_Click(object sender, RoutedEventArgs e)
        {
            Editor.TreeAddNewItemAndPaste();
        }

        #endregion

        #region Guides

        private void ToggleGuides()
        {
            var point = GetInsertionPoint();

            if (GuidesAdorner == null)
            {
                var prop = Editor.Context.CurrentCanvas.GetProperties();

                if (point == null)
                {
                    ShowGuides(prop.SnapX + prop.SnapOffsetX,
                        prop.SnapY + prop.SnapOffsetY);
                }
                else
                {
                    ShowGuides(Editor.SnapOffsetX(point.X, true),
                        Editor.SnapOffsetY(point.Y, true));
                }
            }
            else
            {
                HideGuides();
            }
        }

        private void ShowGuides(double x, double y)
        {
            var canvas = DiagramControl.DiagramCanvas;
            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            GuidesAdorner = new LineGuidesAdorner(canvas);

            RenderOptions.SetEdgeMode(GuidesAdorner, EdgeMode.Aliased);
            GuidesAdorner.SnapsToDevicePixels = false;
            GuidesAdorner.IsHitTestVisible = false;

            double zoom = ZoomSlider.Value;
            double zoom_fx = DiagramControl.CalculateZoom(zoom);

            GuidesAdorner.StrokeThickness = 1.0 / zoom_fx;
            GuidesAdorner.CanvasWidth = canvas.Width;
            GuidesAdorner.CanvasHeight = canvas.Height;
            GuidesAdorner.X = x;
            GuidesAdorner.Y = y;

            adornerLayer.Add(GuidesAdorner);

            GuidesAdorner.Cursor = Cursors.None;
            canvas.Cursor = Cursors.None;
        }

        private void HideGuides()
        {
            var canvas = DiagramControl.DiagramCanvas;
            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            adornerLayer.Remove(GuidesAdorner);
            GuidesAdorner = null;

            canvas.Cursor = Cursors.Arrow;
        }

        #endregion

        #region Connect

        private void Connect()
        {
            var canvas = DiagramControl.DiagramCanvas;

            var point = GetInsertionPoint();
            if (point == null)
                return;

            var elements = this.HitTest(canvas, point, 6.0);
            var pin = elements.Where(x => x is PinThumb).FirstOrDefault();

            bool result = Editor.MouseEventPreviewLeftDown(canvas, point, pin as IThumb);
            if (result == false)
            {
                Editor.MouseEventLeftDown(canvas, point);
            }
        }

        public List<DependencyObject> HitTest(Visual visual, IPoint point, double radius)
        {
            var elements = new List<DependencyObject>();

            var elippse = new EllipseGeometry()
            {
                RadiusX = radius,
                RadiusY = radius,
                Center = new Point(point.X, point.Y),
            };

            var hitTestParams = new GeometryHitTestParameters(elippse);
            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    elements.Add(element);
                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(visual, filterCallback, resultCallback, hitTestParams);

            return elements;
        }

        private PointEx GetInsertionPoint()
        {
            PointEx insertionPoint = null;

            if (GuidesAdorner != null)
            {
                double x = GuidesAdorner.X;
                double y = GuidesAdorner.Y;

                insertionPoint = new PointEx(x, y);
            }
            else
            {
                var relativeTo = DiagramControl.DiagramCanvas;
                var point = Mouse.GetPosition(relativeTo);
                double x = point.X;
                double y = point.Y;
                double width = relativeTo.Width;
                double height = relativeTo.Height;

                if (x >= 0.0 && x <= width &&
                    y >= 0.0 && y <= height)
                {
                    insertionPoint = new PointEx(x, y);
                }
            }

            return insertionPoint;
        }

        #endregion

        #region Insert

        private void InsertInput(ICanvas canvas, PointEx point)
        {
            Editor.HistoryAdd(canvas, true);
            
            var element = Editor.InsertInput(canvas, 
                point != null ? point : InsertPointInput);

            if (GuidesAdorner == null)
                Editor.SelectOneElement(element, true);
        }

        private void InsertOutput(ICanvas canvas, PointEx point)
        {
            Editor.HistoryAdd(canvas, true);

            var element = Editor.InsertOutput(canvas, 
                point != null ? point : InsertPointOutput);

            if (GuidesAdorner == null)
                Editor.SelectOneElement(element, true);
        }

        private void InsertOrGate(ICanvas canvas, PointEx point)
        {
            Editor.HistoryAdd(canvas, true);

            var element = Editor.InsertOrGate(canvas, 
                point != null ? point : InsertPointGate);

            if (GuidesAdorner == null)
                Editor.SelectOneElement(element, true);
        }

        private void InsertAndGate(ICanvas canvas, PointEx point)
        {
            Editor.HistoryAdd(canvas, true);

            var element = Editor.InsertAndGate(canvas, 
                point != null ? point : InsertPointGate);

            if (GuidesAdorner == null)
                Editor.SelectOneElement(element, true);
        }

        #endregion

        #region Tag Editor

        public List<IElement> GetSeletedIO()
        {
            var selected = GetSelectedInputOutputElements();

            if (selected.Count == 0)
            {
                var all = GetAllInputOutputElements();
                return all;
            }
            else
            {
                return selected;
            }
        }

        private void InitializeTagEditor()
        {
            var control = this.TagEditorControl;

            if (Editor.Context.Tags == null)
            {
                Editor.Context.Tags = new List<object>();
            }

            control.Selected = GetSeletedIO();
            control.Tags = Editor.Context.Tags;
            control.Initialize();

            DiagramControl.SelectionChanged = () =>
            {
                control.Selected = GetSeletedIO();
                control.UpdateSelected();
            };
        }

        private List<IElement> GetAllInputOutputElements()
        {
            var all = Editor.GetElementsAll().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return all;
        }

        private List<IElement> GetSelectedInputOutputElements()
        {
            var selected = Editor.GetElementsSelected().Where(x =>
            {
                string uid = x.GetUid();
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return selected;
        }

        #endregion

        #region Table Logo

        public void SetLogo(int logoId)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Supported Images|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.bmp|" +
                        "Png (*.png)|*.png|" +
                        "Jpg (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                        "Tif (*.tif;*.tiff)|*.tif;*.tiff|" +
                        "Bmp (*.bmp)|*.bmp|" +
                        "All Files (*.*)|*.*",
                Title = "Open Logo Image (115x80 @ 96dpi)"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                try
                {
                    var fileName = dlg.FileName;
                    SetTableLogo(logoId, fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        private void SetTableLogo(int logoId, string fileName)
        {
            var table = GetCurrentTable();
            if (table != null)
            {
                BitmapImage src = CreateBitmapImage(fileName);

                if (logoId == 1)
                {
                    table.Logo1 = src;

                    UpdateCurrentTable(table);
                }
                else if (logoId == 2)
                {
                    table.Logo2 = src;

                    UpdateCurrentTable(table);
                }
            }
        }

        private void UpdateCurrentTable(DiagramTable table)
        {
            TableGrid.SetData(this, null);
            TableGrid.SetData(this, table);
        }

        private DiagramTable GetCurrentTable()
        {
            var table = TableGrid.GetData(this) as DiagramTable;

            return table;
        }

        private BitmapImage CreateBitmapImage(string fileName)
        {
            BitmapImage src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();

            return src;
        }

        #endregion

        #region Table Editor

        public void ShowTableEditor()
        {
            // SetLogo(1);
            // SetLogo(2);
        }

        #endregion

        #region Zoom Events

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.IsLoaded == false)
                return;

            Editor.Context.ZoomLogBase = LogBaseSlider.Value;
            Editor.Context.ZoomExpFactor = ExpFactorSlider.Value;

            double zoom = ZoomSlider.Value;

            zoom = Math.Round(zoom, 1);

            if (e.OldValue != e.NewValue)
            {
                double zoom_fx = this.DiagramControl.Zoom(zoom);

                if (GuidesAdorner != null)
                {
                    GuidesAdorner.StrokeThickness = 1.0 / zoom_fx;
                }
            }
        }

        #endregion

        #region Fixed Document

        private void SetPrintStrokeSthickness(ResourceDictionary resources)
        {
            if (resources == null)
                return;

            resources[ResourceConstants.KeyLogicStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.LogicThicknessMm);
            resources[ResourceConstants.KeyWireStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.WireThicknessMm);
            resources[ResourceConstants.KeyElementStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.ElementThicknessMm);
            resources[ResourceConstants.KeyIOStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.IOThicknessMm);
            resources[ResourceConstants.KeyPageStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.PageThicknessMm);
        }

        private void SetPrintColors(ResourceDictionary resources)
        {
            if (resources == null)
                return;

            var backgroundColor = resources["LogicBackgroundColorKey"] as SolidColorBrush;
            backgroundColor.Color = Colors.White;

            var gridColor = resources["LogicGridColorKey"] as SolidColorBrush;
            gridColor.Color = Colors.Transparent;

            var pageColor = resources["LogicTemplateColorKey"] as SolidColorBrush;
            pageColor.Color = Colors.Black;

            var logicColor = resources["LogicColorKey"] as SolidColorBrush;
            logicColor.Color = Colors.Black;

            var logicSelectedColor = resources["LogicSelectedColorKey"] as SolidColorBrush;
            logicSelectedColor.Color = Colors.Black;

            var helperColor = resources["LogicTransparentColorKey"] as SolidColorBrush;
            helperColor.Color = Colors.Transparent;
        }

        private void SetElementResources(ResourceDictionary resources, bool fixedStrokeThickness)
        {
            // set print dictionary
            resources.Source = new Uri(LogicDictionaryUri, UriKind.Relative);

            if (fixedStrokeThickness == false)
            {
                SetPrintStrokeSthickness(resources);
            }

            // set print colors
            SetPrintColors(resources);
        }

        public FrameworkElement CreateDiagramElement(string diagram, 
            Size areaExtent, 
            Point origin, 
            Rect area,
            bool fixedStrokeThickness,
            ResourceDictionary resources)
        {
            var grid = new Grid()
            {
                ClipToBounds = true,
                Resources = resources
            };

            //SetElementResources(grid.Resources, fixedStrokeThickness);

            // set element template and content
            var template = new Control()
            {
                Template = grid.Resources["LandscapePageTemplateKey"] as ControlTemplate
            };

            var canvas = new DiagramCanvas()
            {
                Width = Editor.Context.CurrentCanvas.GetWidth(),
                Height = Editor.Context.CurrentCanvas.GetHeight()
            };

            Model.Parse(diagram,
                canvas, Editor.Context.DiagramCreator, 
                0, 0, 
                false, false, false, true);

            grid.Children.Add(template);
            grid.Children.Add(canvas);

            // set diagram table
            var table = TableGrid.GetData(this);
            TableGrid.SetData(grid, table);

            // set shorten flags
            LineEx.SetShortenStart(grid, ShortenStart.IsChecked.Value);
            LineEx.SetShortenEnd(grid, ShortenEnd.IsChecked.Value);

            return grid;
        }

        public FixedDocument CreateFixedDocument(IEnumerable<string> diagrams, 
            Size areaExtent, 
            Size areaOrigin, 
            bool fixedStrokeThickness)
        {
            var origin = new Point(areaOrigin.Width, areaOrigin.Height);
            var area = new Rect(origin, areaExtent);
            var scale = Math.Min(areaExtent.Width / PageWidth, areaExtent.Height / PageHeight);

            // create fixed document
            var fixedDocument = new FixedDocument() { Name = "diagrams" };

            SetElementResources(fixedDocument.Resources, fixedStrokeThickness);

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

                var element = CreateDiagramElement(diagram, 
                    areaExtent, 
                    origin, 
                    area, 
                    fixedStrokeThickness,
                    fixedDocument.Resources);

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

        public FixedDocumentSequence CreateFixedDocumentSequence(IEnumerable<IEnumerable<string>> projects, 
            Size areaExtent, 
            Size areaOrigin,
            bool fixedStrokeThickness)
        {
            var fixedDocumentSeq = new FixedDocumentSequence() { Name = "diagrams" };

            foreach (var diagrams in projects)
            {
                var fixedDocument = CreateFixedDocument(diagrams, 
                    areaExtent, 
                    areaOrigin, 
                    fixedStrokeThickness);

                var documentRef = new DocumentReference();
                documentRef.BeginInit();
                documentRef.SetDocument(fixedDocument);
                documentRef.EndInit();

                (fixedDocumentSeq as IAddChild).AddChild(documentRef);
            }

            return fixedDocumentSeq;
        }

        #endregion

        #region Print

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
            var dlg = new PrintDialog();

            ShowPrintDialog(dlg);

            // print capabilities
            var caps = dlg.PrintQueue.GetPrintCapabilities(dlg.PrintTicket);
            var areaExtent = new Size(caps.PageImageableArea.ExtentWidth, caps.PageImageableArea.ExtentHeight);
            var areaOrigin = new Size(caps.PageImageableArea.OriginWidth, caps.PageImageableArea.OriginHeight);

            // create document
            var s = System.Diagnostics.Stopwatch.StartNew();

            var fixedDocument = CreateFixedDocument(diagrams, 
                areaExtent, 
                areaOrigin,
                false);

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

            var fixedDocumentSeq = CreateFixedDocumentSequence(projects, 
                areaExtent, 
                areaOrigin,
                false);

            s.Stop();
            System.Diagnostics.Debug.Print("CreateFixedDocumentSequence in {0}ms", s.Elapsed.TotalMilliseconds);

            // print document
            dlg.PrintDocument(fixedDocumentSeq.DocumentPaginator, name);
        }

        public void Print()
        {
            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, false).Item2;

            Print(diagrams, "solution");
        }

        public void PrintHistory()
        {
            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, true).Item2;

            Print(diagrams, "history");
        }

        #endregion

        #region Show

        public void ShowDiagram()
        {
            var model = Editor.ModelUpdateSelectedDiagram();

            var diagrams = new List<string>();
            diagrams.Add(model);

            ShowDiagramsWindow(diagrams, "Diagram");
        }

        public void ShowDiagramSelectedElements()
        {
            var model = Editor.ModelGenerateFromSelected(Editor.Context.CurrentCanvas);

            var diagrams = new List<string>();
            diagrams.Add(model);

            ShowDiagramsWindow(diagrams, "Diagram (Selected Elements)");
        }

        public void ShowProjectDiagrams()
        {
            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGetCurrentProjectDiagrams();

            if (diagrams != null)
            {
                ShowDiagramsWindow(diagrams, "Project Diagrams");
            }
        }

        public void ShowSolutionDiagrams()
        {
            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, false).Item2;

            ShowDiagramsWindow(diagrams, "Solution Diagrams");
        }

        public void ShowDiagramHistory()
        {
            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, false).Item2;

            ShowDiagramsWindow(diagrams, "Diagram History");
        }

        public void ShowDiagramsWindow(IEnumerable<string> diagrams, string title)
        {
            var areaExtent = new Size(PageWidth, PageHeight);
            var areaOrigin = new Size(0, 0);

            var fixedDocument = CreateFixedDocument(diagrams,
                areaExtent,
                areaOrigin,
                true);

            var window = new Window()
            {
                Title = title,
                Width = PageWidth + 80,
                Height = PageHeight + 120,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };

            var viewer = new DocumentViewer();

            viewer.Document = fixedDocument;

            window.Content = viewer;

            window.Show();
        }

        #endregion
    }

    #endregion
}
