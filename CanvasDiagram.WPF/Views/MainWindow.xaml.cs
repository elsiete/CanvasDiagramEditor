// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Dxf;
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
using CanvasDiagram.Core.Model;

#endregion

namespace CanvasDiagram.WPF
{
    #region MainWindow

    public partial class MainWindow : Window
    {
        #region Fields

        private string ResourcesUri = "ElementsDictionary.xaml";

        private DiagramEditor Editor { get; set; }

        private PointEx InsertPointInput = new PointEx(30, 30.0);
        private PointEx InsertPointOutput = new PointEx(930.0, 30.0);
        private PointEx InsertPointGate = new PointEx(325.0, 30.0);

        private double PageWidth = 1260.0;
        private double PageHeight = 891.0;

        private double GuideSpeedUpLevel1 = 1.0;
        private double GuideSpeedUpLevel2 = 2.0;

        private string WindowDefaultTitle = "Canvas Diagram Editor";
        private string WindowTitleDirtyString = "*";
        private string WindowTitleSeparator = " - ";

        private string SolutionNewFileName = "Solution0";
        private bool SolutionIsDirty = false;
        private string SolutionFileName = null;

        private string TagsNewFileName = "Tags0";

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

        #region Window Title

        private void UpdateWindowTitle()
        {
            if (SolutionFileName == null && SolutionIsDirty == false)
            {
                string title = string.Concat(SolutionNewFileName,
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName == null && SolutionIsDirty == true)
            {
                string title = string.Concat(SolutionNewFileName,
                    WindowTitleDirtyString,
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName != null && SolutionIsDirty == false)
            {
                string title = string.Concat(System.IO.Path.GetFileName(SolutionFileName),
                    WindowTitleSeparator,
                    WindowDefaultTitle);

                this.Title = title;
            }
            else if (SolutionFileName != null && SolutionIsDirty == true)
            {
                string title = string.Concat(System.IO.Path.GetFileName(SolutionFileName),
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

        private void UpdateSolutionState(bool isDirty, string fileName)
        {
            SolutionIsDirty = isDirty;
            SolutionFileName = fileName;

            UpdateWindowTitle();
        }

        #endregion

        #region Dxf Inspect

        private void ShowHtml(string html, string title)
        {
            var window = new HtmlWindow();

            window.Title = title;
            window.Html.NavigateToString(html);
            window.Show();
        }

        public void DxfInspect()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Dxf (*.dxf)|*.dxf|All Files (*.*)|*.*",
                Title = "Inspect Dxf"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                try
                {
                    var inspect = new DxfInspect();
                    var html = inspect.GetHtml(dlg.FileName);

                    ShowHtml(html, "Dxf Inspect");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                    System.Diagnostics.Debug.Print(ex.StackTrace);
                }
            }
        }

        #endregion

        #region Initialize

        private void InitializeFileMenuEvents()
        {
            FileNew.Click += (sender, e) => NewSolution();
            FileOpen.Click += (sender, e) => OpenSolution();
            FileSave.Click += (sender, e) => SaveSolutionAsDlg(false);
            FileSaveAs.Click += (sender, e) => SaveSolutionAsDlg(true);
            FileOpenDiagram.Click += (sender, e) => OpenDiagramDlg();
            FileSaveDiagram.Click += (sender, e) => SaveDiagramDlg();
            FileOpenTags.Click += (sender, e) => OpenTags();
            FileSaveTags.Click += (sender, e) => TagsSaveDlg();
            FileImportTags.Click += (sender, e) => ImportTags();
            FileExportTags.Click += (sender, e) => TagsExportDlg();
            FileExportToDxf.Click += (sender, e) => ExportDxf();
            FileInspectDxf.Click += (sender, e) => DxfInspect();
            FilePrint.Click += (sender, e) => Print();
            FilePrintHistory.Click += (sender, e) => PrintHistory();
            FileExit.Click += (sender, e) => Application.Current.Shutdown();
        }

        private void InitializeEditMenuEvents()
        {
            EditUndo.Click += (sender, e) => Editor.Undo();
            EditRedo.Click += (sender, e) => Editor.Redo();
            EditCut.Click += (sender, e) => Editor.Cut();
            EditCopy.Click += (sender, e) => Editor.Copy();
            EditPaste.Click += (sender, e) => Editor.Paste(new PointEx(0.0, 0.0), true);
            EditDelete.Click += (sender, e) => Delete();
            EditSelectAll.Click += (sender, e) => Editor.SelectAll();
            EditDeselectAll.Click += (sender, e) => DeselectAll();
            EditSelectPrevious.Click += (sender, e) => Editor.SelectPrevious(!(Keyboard.Modifiers == ModifierKeys.Control));
            EditSelectNext.Click += (sender, e) => Editor.SelectNext(!(Keyboard.Modifiers == ModifierKeys.Control));
            EditSelectConnected.Click += (sender, e) => Editor.SelectConnected();
            EditClear.Click += (sender, e) => Editor.ClearCanvas();
            EditResetThumbTags.Click += (sender, e) => Editor.ResetThumbTags();
            EditConnect.Click += (sender, e) => Connect();
        }

        private void InitializeViewMenuEvents()
        {
            ViewProjectDiagrams.Click += (sender, e) => ShowProjectDiagrams();
            ViewSolutionDiagrams.Click += (sender, e) => ShowSolutionDiagrams();
            ViewDiagram.Click += (sender, e) => ShowDiagram();
            ViewDiagramSelectedElements.Click += (sender, e) => ShowDiagramSelectedElements();
            ViewDiagramHistory.Click += (sender, e) => ShowDiagramHistory();
            ViewPreviousDiagramProject.Click += (sender, e) => TreeEditor.SelectPreviousItem(Editor.Context.CurrentTree, false);
            ViewNextDiagramProjcet.Click += (sender, e) => TreeEditor.SelectNextItem(Editor.Context.CurrentTree, false);
            ViewPreviousDiagramSolution.Click += (sender, e) => TreeEditor.SelectPreviousItem(Editor.Context.CurrentTree, true);
            ViewNextDiagramSolution.Click += (sender, e) => TreeEditor.SelectNextItem(Editor.Context.CurrentTree, true);
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
        }

        private void InitializeWindowEvents()
        {
            this.Loaded += (sender, e) =>
            {
                this.DiagramControl.Focus();

                SetCurrentTable();
                InitializeTagEditor();
                InitializeTableEditor();
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
            counter.Set(3);
            this.DiagramControl.DiagramCanvas.SetCounter(counter);

            var prop = DiagramProperties.Default;
            this.DiagramControl.DiagramCanvas.SetProperties(prop);
            SetProperties(prop);

            Editor.Context.IsControlPressed = () => Keyboard.Modifiers == ModifierKeys.Control;
            Editor.Context.UpdateProperties = () => UpdateProperties(Editor.Context.CurrentCanvas.GetProperties());
            Editor.Context.SetProperties = (p) => SetProperties(p);

            Editor.Context.Clipboard = new WindowsClipboard();

            // diagram creator
            Editor.Context.DiagramCreator = GetDiagramCreator();

            // set checkbox states
            EnableInsertLast.IsChecked = Editor.Context.EnableInsertLast;
            EnableSnap.IsChecked = Editor.Context.EnableSnap;
            SnapOnRelease.IsChecked = Editor.Context.SnapOnRelease;

            // explorer control
            this.ExplorerControl.Editor = Editor;
            this.ExplorerControl.DiagramView = this.DiagramControl.RootBorder;

            // tree actions
            Editor.Context.CreateSolution = () => this.ExplorerControl.CreateTreeSolutionItem();
            Editor.Context.CreateProject = () => this.ExplorerControl.CreateTreeProjectItem();
            Editor.Context.CreateDiagram = () => this.ExplorerControl.CreateTreeDiagramItem();

            // update canvas grid
            UpdateDiagramGrid(false);
        }

        private void InitializeHistory()
        {
            HistoryEditor.CanvasHistoryChanged += (sender, e) =>
            {
                var canvas = e.Canvas;
                var undo = e.Undo;
                var redo = e.Redo;
                int undoCount = undo != null ? undo.Count : 0;
                int redoCount = redo != null ? redo.Count : 0;

                UpdateSolutionState(undoCount > 0 ? true : false, SolutionFileName);
            };

            UpdateWindowTitle();
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

        private void SetProperties(DiagramProperties prop)
        {
            TextPageWidth.Text = prop.PageWidth.ToString();
            TextPageHeight.Text = prop.PageHeight.ToString();
            TextGridOriginX.Text = prop.GridOriginX.ToString();
            TextGridOriginY.Text = prop.GridOriginY.ToString();
            TextGridWidth.Text = prop.GridWidth.ToString();
            TextGridHeight.Text = prop.GridHeight.ToString();
            TextGridSize.Text = prop.GridSize.ToString();
            TextSnapX.Text = prop.SnapX.ToString();
            TextSnapY.Text = prop.SnapY.ToString();
            TextSnapOffsetX.Text = prop.SnapOffsetX.ToString();
            TextSnapOffsetY.Text = prop.SnapOffsetY.ToString();
        }

        private void UpdateDiagramGrid(bool undo)
        {
            var canvas = Editor.Context.CurrentCanvas;
            var creator = Editor.Context.DiagramCreator;

            if (undo == true)
                HistoryEditor.Add(canvas);

            Editor.Context.UpdateProperties();
            ModelEditor.SetGrid(canvas, creator);
        }

        private void OpenSolution()
        {
            OpenSolutionDlg();
            UpdateEditors();
        }

        private void NewSolution()
        {
            UpdateSolutionState(false, null);
            SetProperties(DiagramProperties.Default);
            UpdateDiagramGrid(false);

            ModelEditor.Clear(Editor.Context.CurrentCanvas);

            Editor.Clear(Editor.Context.CurrentTree,
                Editor.Context.CurrentCanvas, 
                Editor.Context.CurrentCanvas.GetCounter());

            TreeEditor.CreateDefaultSolution(Editor.Context.CurrentTree, 
                Editor.Context.CreateSolution,
                Editor.Context.CreateProject,
                Editor.Context.CreateDiagram, 
                Editor.Context.CurrentCanvas.GetCounter());

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
        }

        private double CalculateMoveSpeedUp(int delta)
        {
            return (delta > -200.0 && delta < -50.0) ? 
                GuideSpeedUpLevel1 : (delta > -50.0) ? 
                GuideSpeedUpLevel2 : 1.0;
        }

        private void MoveUp()
        {
            Editor.MoveUp(Editor.Context.CurrentCanvas);
        }

        private void MoveDown()
        {
            Editor.MoveDown(Editor.Context.CurrentCanvas);
        }

        private void MoveLeft()
        {
            Editor.MoveLeft(Editor.Context.CurrentCanvas);
        }

        private void MoveRight()
        {
            Editor.MoveRight(Editor.Context.CurrentCanvas);
        }

        private void Delete()
        {
            Editor.Delete();
            InitializeTagEditor();
        }

        #endregion

        #region Handle Key Events

        private void HandleKeyEvents(KeyEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;
            bool isControl = Keyboard.Modifiers == ModifierKeys.Control;
            bool canMove = e.OriginalSource is DiagramControl;
            var key = e.Key;

            if (isControl == true)
            {
                switch (key)
                {
                    case Key.O: OpenSolution(); break;
                    case Key.S: SaveSolutionAsDlg(false); break;
                    case Key.N: NewSolution(); break;
                    case Key.T: OpenTags(); break;
                    case Key.I: ImportTags(); break;
                    case Key.R: Editor.ResetThumbTags(); break;
                    case Key.E: ExportDxf(); break;
                    case Key.P: Print(); break;
                    case Key.Z: Editor.Undo(); break;
                    case Key.Y: Editor.Redo(); break;
                    case Key.X: Editor.Cut(); break;
                    case Key.C: Editor.Copy(); break;
                    case Key.V: Editor.Paste(new PointEx(0.0, 0.0), true); break;
                    case Key.A: Editor.SelectAll(); break;
                    case Key.OemOpenBrackets: Editor.SelectPrevious(false); break;
                    case Key.OemCloseBrackets: Editor.SelectNext(false); break;
                    case Key.J: Editor.CreateAndPaste(); break;
                    case Key.M: Editor.Create(); break;
                    case Key.OemComma: TreeEditor.SelectPreviousItem(Editor.Context.CurrentTree, true); break;
                    case Key.OemPeriod: TreeEditor.SelectNextItem(Editor.Context.CurrentTree, true); break;
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
                    case Key.Up: if (canMove == true) { MoveUp(); e.Handled = true; } break;
                    case Key.Down: if (canMove == true) { MoveDown(); e.Handled = true; } break;
                    case Key.Left: if (canMove == true) { MoveLeft(); e.Handled = true; } break;
                    case Key.Right: if (canMove == true) { MoveRight(); e.Handled = true; } break;
                    case Key.I: InsertInput(canvas, GetInsertionPoint()); break;
                    case Key.O: InsertOutput(canvas, GetInsertionPoint()); break;
                    case Key.R: InsertOrGate(canvas, GetInsertionPoint()); break;
                    case Key.A: InsertAndGate(canvas, GetInsertionPoint()); break;
                    case Key.S: Editor.ToggleWireStart(); break;
                    case Key.E: Editor.ToggleWireEnd(); break;
                    case Key.C: Connect(); break;
                    case Key.OemComma: TreeEditor.SelectPreviousItem(Editor.Context.CurrentTree, false); break;
                    case Key.OemPeriod: TreeEditor.SelectNextItem(Editor.Context.CurrentTree, false); break;
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

        private void DefaultZoom()
        {
            DiagramControl.ResetZoom();
            DiagramControl.ResetPan();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            DefaultZoom();
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            Editor.GetCurrentModel();

            var solution = Editor.GenerateSolution(System.IO.Directory.GetCurrentDirectory(), false);

            this.TextModel.Text = solution.Model;
        }

        private void GenerateModelFromSelected_Click(object sender, RoutedEventArgs e)
        {
            this.TextModel.Text = ModelEditor.Generate(ModelEditor.GetSelected(Editor.Context.CurrentCanvas));
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            Editor.Paste(diagram, offsetX, offsetY, true);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            UpdateDiagramGrid(true);
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
                Editor.MouseEventLeftDown(canvas, point);
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

            return insertionPoint;
        }

        #endregion

        #region Insert

        private void InsertInput(ICanvas canvas, PointEx point)
        {
            Editor.Snapshot(canvas, true);
            
            var element = Insert.Input(canvas,
                point != null ? point : InsertPointInput, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.SelectOneElement(element, true);
        }

        private void InsertOutput(ICanvas canvas, PointEx point)
        {
            Editor.Snapshot(canvas, true);

            var element = Insert.Output(canvas,
                point != null ? point : InsertPointOutput, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.SelectOneElement(element, true);
        }

        private void InsertOrGate(ICanvas canvas, PointEx point)
        {
            Editor.Snapshot(canvas, true);

            var element = Insert.OrGate(canvas,
                point != null ? point : InsertPointGate, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.SelectOneElement(element, true);
        }

        private void InsertAndGate(ICanvas canvas, PointEx point)
        {
            Editor.Snapshot(canvas, true);

            var element = Insert.AndGate(canvas,
                point != null ? point : InsertPointGate, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

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
            return TableGrid.GetData(this) as DiagramTable;
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

        #region Show

        public void ShowDiagram()
        {
            var model = Editor.GetCurrentModel();
            var diagrams = new List<string>();

            diagrams.Add(model);

            ShowDiagramsWindow(diagrams, "Diagram");
        }

        public void ShowDiagramSelectedElements()
        {
            var model = ModelEditor.Generate(ModelEditor.GetSelected(Editor.Context.CurrentCanvas));
            var diagrams = new List<string>();

            diagrams.Add(model);

            ShowDiagramsWindow(diagrams, "Diagram (Selected Elements)");
        }

        public void ShowProjectDiagrams()
        {
            Editor.GetCurrentModel();

            var diagrams = DiagramEditor.GetCurrentProjectDiagrams(Editor.Context.CurrentTree);
            if (diagrams != null)
                ShowDiagramsWindow(diagrams, "Project Diagrams");
        }

        public void ShowSolutionDiagrams()
        {
            Editor.GetCurrentModel();

            var diagrams = Editor.GenerateSolution(null, false).Models;

            ShowDiagramsWindow(diagrams, "Solution Diagrams");
        }

        public void ShowDiagramHistory()
        {
            Editor.GetCurrentModel();

            var diagrams = Editor.GenerateSolution(null, true).Models;

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
                WindowState = WindowState.Maximized,
                Content = new DocumentViewer() { Document = fixedDocument }
            };

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

            Editor.GetCurrentModel();

            var diagrams = Editor.GenerateSolution(null, false).Models;

            printer.Print(diagrams, "solution", table);
        }

        private void PrintHistory()
        {
            var printer = GetDefaultPrinter();
            var table = TableGrid.GetData(this) as DiagramTable;

            Editor.GetCurrentModel();

            var diagrams = Editor.GenerateSolution(null, true).Models;

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
            string model = ModelEditor.GenerateDiagram(canvas, null, canvas.GetProperties());

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

                ModelEditor.Clear(canvas);

                TreeSolution solution = Editor.OpenSolution(fileName);

                if (solution != null)
                {
                    UpdateSolutionState(false, fileName);

                    var tree = Editor.Context.CurrentTree;

                    Editor.Clear(tree, canvas, canvas.GetCounter());
                    Editor.Parse(tree, solution, canvas.GetCounter(), Editor.Context.CreateSolution);
                }
            }
        }

        private void SaveSolutionAsDlg(bool saveAs)
        {
            if (SolutionFileName == null || saveAs == true)
                SaveSolutionDlg(saveAs);
            else if (SolutionFileName != null)
                SaveSolution(SolutionFileName, false);
        }

        private void SaveSolutionDlg(bool saveAs)
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
                SaveSolution(fileName, saveAs);
            }
        }

        private void SaveSolution(string fileName, bool saveAs)
        {
            var tree = Editor.Context.CurrentTree;

            TagsUpdate(saveAs);

            Editor.SaveSolution(fileName);

            UpdateSolutionState(false, fileName);
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
            switch (filter)
            {
                case 1: return DxfAcadVer.AC1009;
                case 2: return DxfAcadVer.AC1015;
                case 3: return DxfAcadVer.AC1015;
                default: return DxfAcadVer.AC1015;
            }
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

        private void TagsUpdate(bool saveAs)
        {
            string tagFileName = Editor.Context.TagFileName;
            var tags = Editor.Context.Tags;

            if (tagFileName != null && tags != null && saveAs == false)
                Tags.Export(tagFileName, tags);
            else if ((tagFileName == null && tags != null) || saveAs == true)
                TagsSaveDlg();
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
                    Editor.Context.Tags = new List<object>();

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

        #region History

        private void GetHistory_Click(object sender, RoutedEventArgs e)
        {
            var history = Editor.Context.CurrentCanvas.GetTag() as UndoRedo;
            if (history == null)
                return;

            ListHistory.Items.Clear();
            int index = 0;

            foreach (var model in history.Undo.Reverse())
            {
                AddHistoryItem(index, model);
                index++;
            }

            var current = ModelEditor.GenerateDiagram(Editor.Context.CurrentCanvas, null, Editor.Context.CurrentCanvas.GetProperties());
            AddHistoryItem(index, current);
        }

        private void AddHistoryItem(int index, string model)
        {
            var item = new ListBoxItem();
            item.Content = index;
            item.Tag = model;
            item.Selected += Item_Selected;
            ListHistory.Items.Add(item);
        }

        void Item_Selected(object sender, RoutedEventArgs e)
        {
            var item = sender as ListBoxItem;
            var model = item.Tag as string;

            ModelEditor.Clear(Editor.Context.CurrentCanvas);
            ModelEditor.Parse(model,
                Editor.Context.CurrentCanvas,
                Editor.Context.DiagramCreator,
                0, 0,
                false, true, false, true);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            ListHistory.Items.Clear();
        } 

        #endregion
    }

    #endregion
}
