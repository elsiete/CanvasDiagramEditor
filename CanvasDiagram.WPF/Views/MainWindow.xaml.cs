﻿// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagram.Core;
using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Editor;
using CanvasDiagram.Util;
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
using CanvasDiagram.Dxf.Enums;

#endregion

namespace CanvasDiagram.WPF
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

    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Window Title

        private string WindowDefaultTitle = "Canvas Diagram Editor";
        private string WindowTitleDirtyString = "*";
        private string WindowTitleSeparator = " - ";

        private string SolutionNewFileName = "Solution0";
        private bool SolutionIsDirty = false;
        private string SolutionFileName = null;

        private string TagsNewFileName = "Tags0";

        public void SetWindowTitle()
        {
            if (SolutionFileName == null && SolutionIsDirty == false)
            {
                string title = string.Format("{0}{1}{2}",
                    SolutionNewFileName,
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName == null && SolutionIsDirty == true)
            {
                string title = string.Format("{0}{1}{2}{3}",
                    SolutionNewFileName,
                    WindowTitleDirtyString,
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName != null && SolutionIsDirty == false)
            {
                string title = string.Format("{0}{1}{2}",
                    System.IO.Path.GetFileName(SolutionFileName),
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName != null && SolutionIsDirty == true)
            {
                string title = string.Format("{0}{1}{2}{3}",
                    System.IO.Path.GetFileName(SolutionFileName),
                    WindowTitleDirtyString,
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else
            {
                this.Title = WindowDefaultTitle;
            }
        }

        #endregion

        #region Fields

        private string ResourcesUri = "ElementsDictionary.xaml";

        private DiagramEditor Editor { get; set; }

        private PointEx InsertPointInput = new PointEx(30, 30.0);
        private PointEx InsertPointOutput = new PointEx(930.0, 30.0);
        private PointEx InsertPointGate = new PointEx(325.0, 30.0);

        private double PageWidth = 1260;
        private double PageHeight = 891;

        private LineGuidesAdorner GuidesAdorner = null;

        private double GuideSpeedUpLevel1 = 1.0;
        private double GuideSpeedUpLevel2 = 2.0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeEditor();
            InitializeHistory();
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
            FileSave.Click += (sender, e) => SaveSolutionDlg();;
            FileOpenDiagram.Click += (sender, e) => OpenDiagramDlg();
            FileSaveDiagram.Click += (sender, e) => SaveDiagramDlg();
            FileOpenTags.Click += (sender, e) => OpenTags();
            FileSaveTags.Click += (sender, e) => TagsSaveDlg();
            FileImportTags.Click += (sender, e) => ImportTags();
            FileExportTags.Click += (sender, e) => TagsExportDlg();
            FileExportToDxf.Click += (sender, e) => ExportDxf();
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
                InitializeTableEditor();
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

            Editor.Context.CurrentTree = this.ExplorerControl.SolutionTree;
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
            EnableInsertLast.IsChecked = Editor.Context.EnableInsertLast;
            EnableSnap.IsChecked = Editor.Context.EnableSnap;
            SnapOnRelease.IsChecked = Editor.Context.SnapOnRelease;

            // explorer control
            this.ExplorerControl.Editor = Editor;
            this.ExplorerControl.DiagramView = this.DiagramControl.PanScrollViewer;

            // tree actions
            Editor.Context.CreateTreeSolutionItem = () => this.ExplorerControl.CreateTreeSolutionItem();
            Editor.Context.CreateTreeProjectItem = () => this.ExplorerControl.CreateTreeProjectItem();
            Editor.Context.CreateTreeDiagramItem = () => this.ExplorerControl.CreateTreeDiagramItem();

            // update canvas grid
            Editor.Context.UpdateProperties();
            Model.SetGrid(Editor.Context.CurrentCanvas, 
                Editor.Context.DiagramCreator,
                false);
        }

        private void InitializeHistory()
        {
            // handle canvas history changes
            History.CanvasHistoryChanged += (sender, e) =>
            {
                var canvas = e.Canvas;
                var undo = e.Undo;
                var redo = e.Redo;
                int undoCount = undo != null ? undo.Count : 0;
                int redoCount = redo != null ? redo.Count : 0;

                System.Diagnostics.Debug.Print("HistoryChanged, undo: {0}, redo: {1}", undoCount, redoCount);

                if (undoCount > 0)
                    SolutionIsDirty = true;
                else
                    SolutionIsDirty = false;

                SetWindowTitle();
            };

            // update window title
            SetWindowTitle();
        }

        private void UpdateEditors()
        {
            InitializeTagEditor();
            InitializeTableEditor();
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
                //Logo1 = null,
                //Logo2 = null,
                PathLogo1 = "",
                PathLogo2 = "",
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
                SubTitle1 = "DIAGRAM",
                SubTitle2 = "",
                SubTitle3 = "",
                Rev = "0",
                Status = "-",
                Page = "-",
                Pages = "-",
                Project = "Sample",
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

        private void OpenSolution()
        {
            OpenSolutionDlg();
            UpdateEditors();
        }

        private void NewSolution()
        {
            SolutionIsDirty = false;
            SolutionFileName = null;
            SetWindowTitle();

            Editor.TreeCreateNewSolution();
            UpdateEditors();
        }

        private void OpenTags()
        {
            TagsOpenDlg();
            UpdateEditors();
        }

        private void ImportTags()
        {
            TagsImportDlg();
            InitializeTagEditor();
        }

        private void ExportDxf()
        {
            DxfExportDlg(ShortenStart.IsChecked.Value,
                ShortenEnd.IsChecked.Value,
                TableGrid.GetData(this) as DiagramTable);
        }

        private void DeselectAll()
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.SelectNone();
            Editor.MouseEventRightDown(canvas);

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
                Editor.EditDelete();
            }
            else
            {
                var canvas = Editor.Context.CurrentCanvas;
                var elements = Model.GetSelected(canvas);

                if (elements.Count() > 0)
                    Editor.EditDelete(canvas, elements);
                else
                    Editor.Delete(canvas, GetInsertionPoint());
            }

            InitializeTagEditor();
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

            if (isControl == true)
            {
                switch (key)
                {
                    case Key.O: OpenSolution(); break;
                    case Key.S: SaveSolutionDlg(); break;
                    case Key.N: NewSolution(); break;
                    case Key.T: OpenTags(); break;
                    case Key.I: ImportTags(); break;
                    case Key.R: Editor.ModelResetThumbTags(); break;
                    case Key.E: ExportDxf(); break;
                    case Key.P: Print(); break;
                    case Key.Z: Editor.HistoryUndo(); break;
                    case Key.Y: Editor.HistoryRedo(); break;
                    case Key.X: Editor.EditCut(); break;
                    case Key.C: Editor.EditCopy(); break;
                    case Key.V: Editor.EditPaste(new PointEx(0.0, 0.0), true); break;
                    case Key.A: Editor.SelectAll(); break;
                    case Key.OemOpenBrackets: Editor.SelectPrevious(false); break;
                    case Key.OemCloseBrackets: Editor.SelectNext(false); break;
                    case Key.J: Editor.TreeAddNewItemAndPaste(); break;
                    case Key.M: Editor.TreeAddNewItem(); break;
                    case Key.OemComma: Editor.TreeSelectPreviousItem(true); break;
                    case Key.OemPeriod: Editor.TreeSelectNextItem(true); break;
                    case Key.H: ShowDiagramHistory(); break;
                }
            }
            else
            {
                switch (key)
                {
                    case Key.OemOpenBrackets: Editor.SelectPrevious(true); break;
                    case Key.OemCloseBrackets: Editor.SelectNext(true); break;
                    case Key.OemPipe: Editor.SelectConnected(); break;
                    case Key.Escape: DeselectAll(); break;
                    case Key.Delete: Delete(); break;
                    case Key.Up: if (canMove == true) MoveUp(timeStamp); break;
                    case Key.Down: if (canMove == true) MoveDown(timeStamp); break;
                    case Key.Left: if (canMove == true) MoveLeft(timeStamp); break;
                    case Key.Right: if (canMove == true) MoveRight(timeStamp); break;
                    case Key.I: InsertInput(canvas, GetInsertionPoint()); break;
                    case Key.O: InsertOutput(canvas, GetInsertionPoint()); break;
                    case Key.R: InsertOrGate(canvas, GetInsertionPoint()); break;
                    case Key.A: InsertAndGate(canvas, GetInsertionPoint()); break;
                    case Key.S: Editor.WireToggleStart(); break;
                    case Key.E: Editor.WireToggleEnd(); break;
                    case Key.C: Connect(); break;
                    case Key.G: ToggleGuides(); break;
                    case Key.OemComma: Editor.TreeSelectPreviousItem(false); break;
                    case Key.OemPeriod: Editor.TreeSelectNextItem(false); break;
                    case Key.F5: TabExplorer.IsSelected = true; break;
                    case Key.F6: TabTags.IsSelected = true; InitializeTagEditor(); break;
                    case Key.F7: TabTables.IsSelected = true; InitializeTableEditor(); break;
                    case Key.F8: TabModel.IsSelected = true; break;
                    case Key.F9: TabOptions.IsSelected = true; break;
                }
            }
        }

        #endregion

        #region CheckBox Events

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.EnableSnap = 
                EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.SnapOnRelease = 
                SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            Editor.Context.EnableInsertLast = 
                EnableInsertLast.IsChecked == true ? true : false;
        }

        private void EnablePage_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.DiagramControl;
            var visibility = diagram.Visibility;
            diagram.Visibility = visibility == Visibility.Collapsed ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        private void EnablePageGrid_Click(object sender, RoutedEventArgs e)
        {
            var grid = this.DiagramControl.DiagramGrid;
            var visibility = grid.Visibility;
            grid.Visibility = visibility == Visibility.Collapsed ? 
                Visibility.Visible :Visibility.Collapsed;
        }

        private void EnablePageTemplate_Click(object sender, RoutedEventArgs e)
        {
            var template = this.DiagramControl.DiagramTemplate;
            var visibility = template.Visibility;
            template.Visibility = visibility == Visibility.Collapsed ? 
                Visibility.Visible : Visibility.Collapsed;
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

        #region Guides

        private void ToggleGuides()
        {
            var point = GetInsertionPoint();

            if (GuidesAdorner == null)
            {
                var prop = Editor.Context.CurrentCanvas.GetProperties();

                if (point == null)
                    ShowGuides(prop.SnapX + prop.SnapOffsetX, 
                        prop.SnapY + prop.SnapOffsetY);
                else
                    ShowGuides(Editor.SnapOffsetX(point.X, true), 
                        Editor.SnapOffsetY(point.Y, true));
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
            var selected = Editor.GetSelectedInputOutputElements();
            return (selected.Count() == 0) ? null : selected.ToList();
        }

        private void InitializeTagEditor()
        {
            var control = this.TagEditorControl;

            if (Editor.Context.Tags == null)
                Editor.Context.Tags = new List<object>();

            control.Selected = GetSeletedIO();
            control.Tags = Editor.Context.Tags;
            control.Initialize();

            DiagramControl.SelectionChanged = () =>
            {
                control.Selected = GetSeletedIO();
                control.UpdateSelected();
            };
        }

        #endregion

        #region Table Editor

        private void InitializeTableEditor()
        {
            var control = this.TableEditorControl;

            if (Editor.Context.Tables == null)
                Editor.Context.Tables = new List<object>();

            control.Tables = Editor.Context.Tables;
            control.Initialize();

            DiagramControl.SelectionChanged = () =>
            {
                // TODO: Updated current table.
            };
        }

        #endregion

        #region Set Table Logo

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
                    // TODO: Get image logo from PathLogo1
                    //table.Logo1 = src;

                    UpdateCurrentTable(table);
                }
                else if (logoId == 2)
                {
                    // TODO: Get image logo from PathLogo2
                    //table.Logo2 = src;

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

        #region Slider Events

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
                    GuidesAdorner.StrokeThickness = 1.0 / zoom_fx;
            }
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

            var diagrams = Editor.ModelGenerateSolution(null, true).Item2;

            ShowDiagramsWindow(diagrams, "Diagram History");
        }

        public void ShowDiagramsWindow(IEnumerable<string> diagrams, string title)
        {
            var areaExtent = new Size(PageWidth, PageHeight);
            var areaOrigin = new Size(0, 0);

            var printer = GetDefaultPrinter();
            var table = TableGrid.GetData(this) as DiagramTable;

            var fixedDocument = printer.CreateFixedDocument(diagrams,
                areaExtent, areaOrigin,
                true,
                table);

            var window = new Window()
            {
                Title = title,
                Width = PageWidth + 80,
                Height = PageHeight + 120,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };

            var viewer = new DocumentViewer() { Document = fixedDocument };

            window.Content = viewer;

            window.Show();
        }

        #endregion

        #region Print

        private WpfDiagramPrinter GetDefaultPrinter()
        {
            return new WpfDiagramPrinter()
            {
                DiagramCreator = this.Editor.Context.DiagramCreator,
                ResourcesUri = this.ResourcesUri,
                PageWidth = PageWidth,
                PageHeight = PageHeight,
                ShortenStart = this.ShortenStart.IsChecked.Value,
                ShortenEnd = this.ShortenEnd.IsChecked.Value
            };
        }

        private void Print()
        {
            var printer = GetDefaultPrinter();
            var table = TableGrid.GetData(this) as DiagramTable;

            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, false).Item2;

            printer.Print(diagrams, "solution", table);
        }

        private void PrintHistory()
        {
            var printer = GetDefaultPrinter();
            var table = TableGrid.GetData(this) as DiagramTable;

            Editor.ModelUpdateSelectedDiagram();

            var diagrams = Editor.ModelGenerateSolution(null, true).Item2;

            printer.Print(diagrams, "history", table);
        }

        #endregion

        #region Dxf

        private string DxfGenerate(string model,
            bool shortenStart,
            bool shortenEnd,
            DxfAcadVer version,
            DiagramTable table)
        {
            var dxf = new DxfDiagramCreator()
            {
                ShortenStart = shortenStart,
                ShortenEnd = shortenEnd,
                DiagramProperties = Editor.Context.CurrentCanvas.GetProperties(),
                Tags = Editor.Context.Tags
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
            string model = Model.GenerateDiagram(canvas, null, canvas.GetProperties());

            string dxf = DxfGenerate(model, shortenStart, shortenEnd, version, table);

            DxfSave(fileName, dxf);
        }

        #endregion

        #region File Dialogs

        private void OpenDiagramDlg()
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
                var canvas = Editor.Context.CurrentCanvas;

                Editor.OpenDiagram(fileName, canvas);
            }
        }

        private void OpenSolutionDlg()
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
                var canvas = Editor.Context.CurrentCanvas;

                Model.Clear(canvas);

                TreeSolution solution = Editor.OpenSolution(fileName);

                if (solution != null)
                {
                    SolutionIsDirty = false;
                    SolutionFileName = fileName;
                    SetWindowTitle();

                    var tree = Editor.Context.CurrentTree;

                    Editor.TreeClearSolution(tree);
                    Editor.TreeParseSolution(tree, solution);
                }
            }
        }

        private void SaveSolutionDlg()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Solution (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Solution",
                FileName = SolutionFileName == null ? SolutionNewFileName : System.IO.Path.GetFileName(SolutionFileName)
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var fileName = dlg.FileName;

                var tree = Editor.Context.CurrentTree;

                TagsUpdate();

                Editor.SaveSolution(fileName);

                SolutionIsDirty = false;
                SolutionFileName = fileName;
                SetWindowTitle();
            }
        }

        private void SaveDiagramDlg()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Diagram (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Diagram",
                FileName = "Diagram0"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var fileName = dlg.FileName;
                var canvas = Editor.Context.CurrentCanvas;

                Editor.SaveDiagram(fileName, canvas);
            }
        }

        private void DxfExportDlg(bool shortenStart, bool shortenEnd, DiagramTable table)
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
                var canvas = Editor.Context.CurrentCanvas;

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

        private void TagsOpenDlg()
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

                Editor.Context.TagFileName = tagFileName;
                Editor.Context.Tags = tags;
            }
        }

        private void TagsUpdate()
        {
            string tagFileName = Editor.Context.TagFileName;
            var tags = Editor.Context.Tags;

            if (tagFileName != null && tags != null)
            {
                Tags.Export(tagFileName, tags);
            }
            else if (tagFileName == null && tags != null)
            {
                TagsSaveDlg();
            }
        }

        private void TagsSaveDlg()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Tags (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Tags",
                FileName = Editor.Context.TagFileName == null ? TagsNewFileName : System.IO.Path.GetFileName(Editor.Context.TagFileName)
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                var tagFileName = dlg.FileName;

                Tags.Export(tagFileName, Editor.Context.Tags);

                Editor.Context.TagFileName = tagFileName;
            }
        }

        private void TagsImportDlg()
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

                if (Editor.Context.Tags == null)
                {
                    Editor.Context.Tags = new List<object>();
                }

                Tags.Import(tagFileName, Editor.Context.Tags, true);
            }
        }

        private void TagsExportDlg()
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

                Tags.Export(tagFileName, Editor.Context.Tags);
            }
        }

        #endregion
    }

    #endregion
}