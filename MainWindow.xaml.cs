// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Editor;
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

        private DiagramCreator Editor { get; set; }
        private string LogicDictionaryUri = "LogicDictionary.xaml";

        private bool HaveKeyE = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitializeEditor();

            this.DiagramControl.Editor = this.Editor;
            this.DiagramControl.ZoomSlider = this.ZoomSlider;

            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DiagramControl.PanScrollViewer.Focus();

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

        private void InitializeEditor()
        {
            var options = new DiagramCreatorOptions();

            Editor = new DiagramCreator();
            Editor.CurrentOptions = options;

            Editor.CurrentOptions.Counter.ProjectCount = 1;
            Editor.CurrentOptions.Counter.DiagramCount = 1;

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

            Editor.UpdateDiagramProperties = action;

            Editor.CurrentOptions.CurrentResources = this.Resources;

            Editor.CurrentOptions.CurrentTree = this.SolutionTree;
            Editor.CurrentOptions.CurrentCanvas = this.DiagramControl.DiagramCanvas;
            Editor.CurrentOptions.CurrentPathGrid = this.DiagramControl.PathGrid;

            EnableHistory.IsChecked = options.EnableHistory;
            EnableInsertLast.IsChecked = options.EnableInsertLast;
            EnableSnap.IsChecked = options.EnableSnap;
            SnapOnRelease.IsChecked = options.SnapOnRelease;

            Editor.GenerateGrid(false);
        }

        #endregion

        #region CheckBox Events

        private void EnableHistory_Click(object sender, RoutedEventArgs e)
        {
            Editor.CurrentOptions.EnableHistory = EnableHistory.IsChecked == true ? true : false;

            if (Editor.CurrentOptions.EnableHistory == false)
            {
                var canvas = Editor.CurrentOptions.CurrentCanvas;

                Editor.ClearHistory(canvas);
            }
        }

        private void EnableSnap_Click(object sender, RoutedEventArgs e)
        {
            Editor.CurrentOptions.EnableSnap = EnableSnap.IsChecked == true ? true : false;
        }

        private void SnapOnRelease_Click(object sender, RoutedEventArgs e)
        {
            Editor.CurrentOptions.SnapOnRelease = SnapOnRelease.IsChecked == true ? true : false;
        }

        private void EnableInsertLast_Click(object sender, RoutedEventArgs e)
        {
            Editor.CurrentOptions.EnableInsertLast = EnableInsertLast.IsChecked == true ? true : false;
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
            ZoomSlider.Value = 1.0;
        }

        private void GenerateModel_Click(object sender, RoutedEventArgs e)
        {
            Editor.UpdateSelectedDiagramModel();

            var solution = Editor.GenerateSolutionModel(System.IO.Directory.GetCurrentDirectory());

            this.TextModel.Text = solution.Item1;
        }

        private void GenerateModelFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var diagram = Editor.GenerateModelFromSelected();

            this.TextModel.Text = diagram;
        }

        private void InsertModel_Click(object sender, RoutedEventArgs e)
        {
            var diagram = this.TextModel.Text;
            double offsetX = double.Parse(TextOffsetX.Text);
            double offsetY = double.Parse(TextOffsetY.Text);

            Editor.Insert(diagram, offsetX, offsetY);
        }

        private void UpdateGrid_Click(object sender, RoutedEventArgs e)
        {
            Editor.GenerateGrid(true);
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
            if (Editor == null)
                return;

            var canvas = Editor.CurrentOptions.CurrentCanvas;

            var oldItem = e.OldValue as TreeViewItem;
            var newItem = e.NewValue as TreeViewItem;

            bool isDiagram = Editor.SwitchItems(canvas, oldItem, newItem);
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
            var solution = SolutionTree.SelectedItem as TreeViewItem;

            Editor.AddProject(solution);
        }

        private void ProjectAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            AddNewItem();
        }

        private void DiagramAddDiagram_Click(object sender, RoutedEventArgs e)
        {
            AddNewItem();
        }

        private void SolutionDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var project = SolutionTree.SelectedItem as TreeViewItem;

            Editor.DeleteProject(project);
        }

        private void DiagramDeleteDiagram_Click(object sender, RoutedEventArgs e)
        {
            var diagram = SolutionTree.SelectedItem as TreeViewItem;

            Editor.DeleteDiagram(diagram);
        }

        private void DiagramAddDiagramAndPaste_Click(object sender, RoutedEventArgs e)
        {
            AddNewItemAndPaste();
        }

        #endregion

        #region File Menu Events

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            Editor.NewSolution();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            Editor.OpenSolution();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            Editor.SaveSolution();
        }

        private void FileOpenDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.OpenDiagram();
        }

        private void FileSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            Editor.SaveDiagram();
        }

        private void FileOpenTags_Click(object sender, RoutedEventArgs e)
        {
            Editor.OpenTags();
        }

        private void FileSaveTags_Click(object sender, RoutedEventArgs e)
        {
            Editor.SaveTags();
        }

        private void FileImportTags_Click(object sender, RoutedEventArgs e)
        {
            Editor.ImportTags();
        }

        private void FileExportTags_Click(object sender, RoutedEventArgs e)
        {
            Editor.ExportTags();
        }

        private void FileExportToDxf_Click(object sender, RoutedEventArgs e)
        {
            Editor.ExportToDxf(ShortenStart.IsChecked.Value, ShortenEnd.IsChecked.Value);
        }

        private void FileInspectDxf_Click(object sender, RoutedEventArgs e)
        {
            InspectDxf();
        }

        private void FileImport_Click(object sender, RoutedEventArgs e)
        {
            var diagram = Editor.Import();

            if (diagram != null)
            {
                this.TextModel.Text = diagram;
            }
        }

        private void FilePrint_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Edit Menu Events

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            Editor.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            Editor.Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            Editor.Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            var point = new Point(0.0, 0.0);

            Editor.Paste(point);
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            Editor.Delete();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectAll();
        }

        private void EditDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            Editor.DeselectAll();
        }

        private void EditSelectPrevious_Click(object sender, RoutedEventArgs e)
        {
            // use control Key to select many element
            bool deselect = !(Keyboard.Modifiers == ModifierKeys.Control);

            Editor.SelectPrevious(deselect);
        }

        private void EditSelectNext_Click(object sender, RoutedEventArgs e)
        {
            // use control Key to select many element
            bool deselect = !(Keyboard.Modifiers == ModifierKeys.Control);
            Editor.SelectNext(deselect);
        }

        private void EditSelectConnected_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectConnected();
        }

        private void EditClear_Click(object sender, RoutedEventArgs e)
        {
            Editor.Clear();
        }

        private void EditOptions_Click(object sender, RoutedEventArgs e)
        {
            TabOptions.IsSelected = true;
        }

        #endregion

        #region View Menu Events

        private void ViewPreviousDiagramProject_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectPreviousTreeItem(false);
        }

        private void ViewNextDiagramProjcet_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectNextTreeItem(false);
        }

        private void ViewPreviousDiagramSolution_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectPreviousTreeItem(true);
        }

        private void ViewNextDiagramSolution_Click(object sender, RoutedEventArgs e)
        {
            Editor.SelectNextTreeItem(true);
        }

        #endregion

        #region Tools Menu Events

        private void ToolsTagEditor_Click(object sender, RoutedEventArgs e)
        {
            ShowTagEditor();
        }

        private void ToolsTableEditor_Click(object sender, RoutedEventArgs e)
        {
            ShowTableEditor();
        }

        #endregion

        #region Print

        private void SetPrintStrokeSthickness(FrameworkElement element)
        {
            if (element != null)
            {
                element.Resources[ResourceConstants.KeyLogicStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.LogicThicknessMm);
                element.Resources[ResourceConstants.KeyWireStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.WireThicknessMm);
                element.Resources[ResourceConstants.KeyElementStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.ElementThicknessMm);
                element.Resources[ResourceConstants.KeyIOStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.IOThicknessMm);
                element.Resources[ResourceConstants.KeyPageStrokeThickness] = DipUtil.MmToDip(DxfDiagramCreator.PageThicknessMm);
            }
        }

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
            var grid = new Grid() 
            { 
                ClipToBounds = true 
            };

            // set print dictionary
            grid.Resources.Source = new Uri(LogicDictionaryUri, UriKind.Relative);

            SetPrintStrokeSthickness(grid);

            // set print colors
            SetPrintColors(grid);

            // set element template and content
            var template = new Control() 
            { 
                Template = grid.Resources["LandscapePageTemplateKey"] as ControlTemplate 
            };

            var canvas = new Canvas()
            {
                Width = Editor.CurrentOptions.CurrentCanvas.Width,
                Height = Editor.CurrentOptions.CurrentCanvas.Height
            };

            LineEx.SetShortenStart(grid, ShortenStart.IsChecked.Value);
            LineEx.SetShortenEnd(grid, ShortenEnd.IsChecked.Value);

            Editor.ParseDiagramModel(diagram, canvas, null, 0, 0, false, false, false, true);

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

            Editor.UpdateSelectedDiagramModel();

            var diagrams = Editor.GenerateSolutionModel(null).Item2;

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
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            bool isControl = HaveKeyE == true ? false : Keyboard.Modifiers == ModifierKeys.Control;
            //bool isControlShift = (Keyboard.Modifiers & ModifierKeys.Shift) > 0 && (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            switch (e.Key)
            {
                // select previous solution tree item
                case Key.OemComma:
                    {
                        Editor.SelectPreviousTreeItem(isControl);
                    }
                    break;

                // select next solution tree item
                case Key.OemPeriod:
                    {
                        Editor.SelectNextTreeItem(isControl);
                    }
                    break;

                // select previous element
                case Key.OemOpenBrackets:
                    {
                        // use control Key to select many element
                        Editor.SelectPrevious(!isControl);
                    }
                    break;

                // select next element
                case Key.OemCloseBrackets:
                    {
                        // use control Key to select many element
                        Editor.SelectNext(!isControl);
                    }
                    break;

                // select connected elements
                case Key.OemPipe:
                    {
                        Editor.SelectConnected();
                    }
                    break;

                // add new project to selected solution
                // add new diagram to selected project
                // add new diagram after selected diagram and select new diagram
                case Key.J:
                    {
                        // insert new item and paste from clipboard
                        if (isControl == true)
                        {
                            AddNewItemAndPaste();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;
                case Key.M:
                    {
                        // insert new item
                        if (isControl == true)
                        {
                            AddNewItem();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // open solution
                case Key.O:
                    {
                        if (isControl == true)
                        {
                            Editor.OpenSolution();
                            e.Handled = true;
                            break;
                        }
                        else
                        {
                            // E + O -> insert OrGate
                            if (HaveKeyE == true)
                            {
                                HaveKeyE = false;
                                InsertOrGate(canvas);
                                e.Handled = true;
                                break;
                            }

                            // O -> insert Input
                            else
                            {
                                InsertOutput(canvas);
                                e.Handled = true;
                                break;
                            }
                        }
                    }

                // save solution
                case Key.S:
                    {
                        if (isControl == true)
                        {
                            Editor.SaveSolution();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // new solution
                case Key.N:
                    {
                        if (isControl == true)
                        {
                            Editor.NewSolution();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // open tags
                case Key.T:
                    {
                        if (isControl == true)
                        {
                            Editor.OpenTags();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // import
                case Key.I:
                    {
                        if (isControl == true)
                        {
                            Editor.Import();
                            e.Handled = true;
                            break;
                        }
                        else if (HaveKeyE == false)
                        {
                            InsertInput(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // export to dxf
                case Key.E:
                    {
                        if (isControl == true)
                        {
                            Editor.ExportToDxf(ShortenStart.IsChecked.Value, ShortenEnd.IsChecked.Value);
                            e.Handled = true;
                            break;
                        }
                        else
                        {
                            // enable element insert shortcuts -> E + some Key
                            HaveKeyE = true;
                            e.Handled = true;
                            break;
                        }
                    }

                // print
                case Key.P:
                    {
                        if (isControl == true)
                        {
                            Print();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // undo
                case Key.Z:
                    {
                        if (isControl == true)
                        {
                            Editor.Undo();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // redo
                case Key.Y:
                    {
                        if (isControl == true)
                        {
                            Editor.Redo();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // cut
                case Key.X:
                    {
                        if (isControl == true)
                        {
                            Editor.Cut();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // copy
                case Key.C:
                    {
                        if (isControl == true)
                        {
                            Editor.Copy();
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // paste
                case Key.V:
                    {
                        // paste from clipboard
                        if (isControl == true)
                        {
                            var point = new Point(0.0, 0.0);
                            Editor.Paste(point);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // select all
                case Key.A:
                    {
                        if (isControl == true)
                        {
                            Editor.SelectAll();
                            e.Handled = true;
                            break;
                        }

                        // E + A -> insert AndGate
                        if (HaveKeyE == true)
                        {
                            HaveKeyE = false;
                            InsertAndGate(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // delete
                case Key.Delete:
                    {
                        Editor.Delete();
                        e.Handled = true;
                    }
                    break;

                // move up
                case Key.Up:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            Editor.MoveUp(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // move down
                case Key.Down:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            Editor.MoveDown(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // move left
                case Key.Left:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            Editor.MoveLeft(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // move right
                case Key.Right:
                    {
                        if (e.OriginalSource is ScrollViewer)
                        {
                            Editor.MoveRight(canvas);
                            e.Handled = true;
                            break;
                        }
                    }
                    break;

                // tag editor
                case Key.F5:
                    {
                        ShowTagEditor();
                        e.Handled = true;
                    }
                    break;

                    // table editor
                case Key.F6:
                    {
                        ShowTableEditor();
                        e.Handled = true;
                    }
                    break;

                // deselect all & reset have E key flah
                case Key.Escape:
                    {
                        Editor.DeselectAll();
                        HaveKeyE = false;
                    }
                    break;
            }
        }

        #endregion

        #region New Item

        public enum NewItemType
        {
            None,
            Solution,
            Project,
            Diagram,
            Element
        }

        private NewItemType AddNewItem()
        {
            var selected = SolutionTree.SelectedItem as TreeViewItem;

            string uid = selected.Uid;
            bool isSelectedSolution = StringUtil.StartsWith(uid, ModelConstants.TagHeaderSolution);
            bool isSelectedProject = StringUtil.StartsWith(uid, ModelConstants.TagHeaderProject);
            bool isSelectedDiagram = StringUtil.StartsWith(uid, ModelConstants.TagHeaderDiagram);

            if (isSelectedDiagram == true)
            {
                var project = selected.Parent as TreeViewItem;

                Editor.AddDiagram(project, true);
                return NewItemType.Diagram;
            }
            else if (isSelectedProject == true)
            {
                Editor.AddDiagram(selected, false);
                return NewItemType.Diagram;
            }
            else if (isSelectedSolution == true)
            {
                Editor.AddProject(selected);
                return NewItemType.Project;
            }

            return NewItemType.None;
        }

        private void AddNewItemAndPaste()
        {
            var newItemType = AddNewItem();
            if (newItemType == NewItemType.Diagram)
            {
                var point = new Point(0.0, 0.0);
                Editor.Paste(point);
            }
        }

        #endregion

        #region Insert

        private Point InsertPointInput = new Point(45.0, 30.0);
        private Point InsertPointOutput = new Point(930.0, 30.0);
        private Point InsertPointGate = new Point(325.0, 30.0);

        private void InsertInput(Canvas canvas)
        {
            Editor.AddToHistory(canvas, true);
            
            var element = Editor.InsertInput(canvas, InsertPointInput);
            Editor.SelectOneElement(element, true);
        }

        private void InsertOutput(Canvas canvas)
        {
            Editor.AddToHistory(canvas, true);

            var element = Editor.InsertOutput(canvas, InsertPointOutput);
            Editor.SelectOneElement(element, true);
        }

        private void InsertOrGate(Canvas canvas)
        {
            Editor.AddToHistory(canvas, true);

            var element = Editor.InsertOrGate(canvas, InsertPointGate);
            Editor.SelectOneElement(element, true);
        }

        private void InsertAndGate(Canvas canvas)
        {
            Editor.AddToHistory(canvas, true);

            var element = Editor.InsertAndGate(canvas, InsertPointGate);
            Editor.SelectOneElement(element, true);
        }

        #endregion

        #region Tag Editor

        private string DesignationFilter = "";
        private string SignalFilter = "";
        private string ConditionFilter = "";
        private string DescriptionFilter = "";

        private void ShowTagEditor()
        {
            var window = new TagEditorWindow();
            var control = window.TagEditorControl;

            if (Editor.CurrentOptions.Tags == null)
            {
                Editor.CurrentOptions.Tags = new List<object>();
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

            control.Tags = Editor.CurrentOptions.Tags;

            window.Closing += (sender, e) =>
            {
                // save filters
                DesignationFilter = control.FilterByDesignation.Text;
                SignalFilter = control.FilterBySignal.Text;
                ConditionFilter = control.FilterByCondition.Text;
                DescriptionFilter = control.FilterByDescription.Text;
            };

            // load filters
            control.FilterByDesignation.Text = DesignationFilter;
            control.FilterBySignal.Text = SignalFilter;
            control.FilterByCondition.Text = ConditionFilter;
            control.FilterByDescription.Text = DescriptionFilter;

            // display tag editor window
            window.ShowDialog();
        }

        private List<FrameworkElement> GetAllInputOutputElements()
        {
            var all = Editor.GetAllElements().Where(x =>
            {
                string uid = x.Uid;
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return all;
        }

        private List<FrameworkElement> GetSelectedInputOutputElements()
        {
            var selected = Editor.GetSelectedElements().Where(x =>
            {
                string uid = x.Uid;
                return StringUtil.StartsWith(uid, ModelConstants.TagElementInput) ||
                    StringUtil.StartsWith(uid, ModelConstants.TagElementOutput);
            }).ToList();

            return selected;
        }

        #endregion

        #region Table Editor

        public void ShowTableEditor()
        {
            SetLogo(1);
            SetLogo(2);
        }

        public void SetLogo(int logoId)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Png (*.png)|*.png|Jpg (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*",
                Title = "Open Image (115x60 @ 96dpi)"
            };

            var res = dlg.ShowDialog();
            if (res == true)
            {
                try
                {
                    var fileName = dlg.FileName;

                    var table = TableGrid.GetData(this) as DiagramTable;
                    if (table != null)
                    {
                        BitmapImage src = new BitmapImage();
                        src.BeginInit();
                        src.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
                        src.CacheOption = BitmapCacheOption.OnLoad;
                        src.EndInit();

                        if (logoId == 1)
                        {
                            table.Logo1 = src;
                            TableGrid.SetData(this, null);
                            TableGrid.SetData(this, table);
                        }
                        else if (logoId == 2)
                        {
                            table.Logo2 = src;
                            TableGrid.SetData(this, null);
                            TableGrid.SetData(this, table);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                }
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
                this.DiagramControl.Zoom(zoom);
            }
        }

        #endregion

        #region Dxf Inspect

        private void ShowHtmlWindow(string html, string title)
        {
            var window = new HtmlWindow();

            window.Title = title;

            window.Html.NavigateToString(html);

            window.ShowDialog();
        }

        public void InspectDxf()
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
                    var html = ParseDxf(dlg.FileName);

                    ShowHtmlWindow(html, "Dxf Inspect");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        private const string CodeEntityType = "0";
        private const string CodeName = "2";

        private const string HeaderSection = "SECTION";

        public class DxfTag
        {
            public string Code { get; set; }
            public string Data { get; set; }
        }

        public string ParseDxf(string fileName)
        {
            var sb = new StringBuilder();
            var tags = new List<DxfTag>();
            DxfTag tag = null;
            bool previousName = false;
            //bool haveCodeEntityType = false;

            sb.AppendLine(@"<html><head>");
            sb.AppendLine(@"<style>");
            sb.AppendLine(@"body { background-color:#DDDDDD; }");
            sb.AppendLine(@"dl,dt,dd { font-family: Arial; font-size:10pt; width:100%; }");
            sb.AppendLine(@"dl { font-weight:normal; margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:#DDDDDD; }");
            sb.AppendLine(@"dt { font-weight:bold; }");
            sb.AppendLine(@"dt.section { margin:0.0cm 0.0cm 0.0cm 0.0cm; background-color:rgb(255,242,102); }");
            sb.AppendLine(@"dt.other { margin:0.0cm 0.0cm 0.0cm 1.5cm; background-color:rgb(191,191,191); }");
            sb.AppendLine(@"dd { font-weight:normal; margin:0.0cm 0.0cm 0.0cm 1.5cm; background-color:#DDDDDD; }");
            sb.AppendLine(@"code.code{ width:1.2cm; text-align:right; color:#747474; }");
            sb.AppendLine(@"code.data{ margin:0.0cm 0.0cm 0.0cm 0.3cm; text-align:left; color:#000000; }");
            sb.AppendLine(@"</style>");
            sb.AppendLine(@"</head><body>");
            sb.AppendLine(@"<dl>");

            using (var reader = new System.IO.StreamReader(fileName))
            {
                string data = reader.ReadToEnd();

                //var lines = data.Split(Environment.NewLine.ToCharArray(),
                //    StringSplitOptions.RemoveEmptyEntries);

                var lines = data.Split("\n".ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);

                string[] entity = new string[2] { null, null };

                foreach (var line in lines)
                {
                    var str = line.Trim();

                    // CodeEntityType data
                    if (tag != null)
                    {
                        tag.Data = str;
                        tags.Add(tag);

                        string entityClass = str == HeaderSection ? "section" : "other";

                        sb.AppendFormat("<dt class=\"{3}\"><code class=\"code\">{0}:</code><code class=\"data\">{1}</code></dt>{2}",
                            tag.Code,
                            tag.Data,
                            Environment.NewLine,
                            entityClass);

                        //haveCodeEntityType = true;

                        tag = null;
                    }
                    else
                    {
                        if (str == CodeEntityType && entity[0] == null)
                        {
                            tag = new DxfTag();
                            tag.Code = str;
                        }
                        else
                        {
                            if (entity[0] == null)
                            {
                                entity[0] = str;
                                entity[1] = null;
                            }
                            else
                            {
                                entity[1] = str;

                                sb.AppendFormat("<dd><code class=\"code\">{0}:</code><code class=\"data\">{1}</code></dd>{2}",
                                    entity[0],
                                    entity[1],
                                    Environment.NewLine);

                                // entity Name
                                previousName = entity[0] == CodeName;

                                entity[0] = null;
                                entity[1] = null;

                                //haveCodeEntityType = false;
                            }
                        }
                    }
                }
            }

            sb.AppendLine(@"</dl></body></html>");

            return sb.ToString();
        } 

        #endregion
    }

    #endregion
}
