// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
    #region DiagramControl

    public partial class DiagramControl : UserControl
    {
        #region Properties

        public Action SelectionChanged { get; set; }

        public Slider ZoomSlider { get; set; }
        public DiagramEditor Editor { get; set; }
        private SelectionAdorner Adorner { get; set; }

        #endregion

        #region Constructor

        public DiagramControl()
        {
            InitializeComponent();
        } 

        #endregion

        #region SelectionAdorner

        private void CreateAdorner(Canvas canvas, PointEx origin, PointEx point)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            Adorner = new SelectionAdorner(canvas);
            Adorner.Zoom = GetZoomScaleTransform().ScaleX;
            Adorner.SelectionOrigin = new Point(origin.X, origin.Y);

            Adorner.SelectionRect = new RectEx(origin.X, origin.Y, point.X, point.Y);

            Adorner.SnapsToDevicePixels = false;
            RenderOptions.SetEdgeMode(Adorner, EdgeMode.Aliased);

            layer.Add(Adorner);
            Adorner.InvalidateVisual();
        }

        private void RemoveAdorner(Canvas canvas)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            layer.Remove(Adorner);

            Adorner = null;
        }

        private void UpdateAdorner(Point point)
        {
            var origin = Adorner.SelectionOrigin;
            double width = Math.Abs(point.X - origin.X);
            double height = Math.Abs(point.Y - origin.Y);

            Adorner.SelectionRect = new RectEx(point.X, point.Y, origin.X, origin.Y);
            Adorner.InvalidateVisual();
        }

        private void UpdateSelectedTags()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged();
            }
        }

        #endregion

        #region Pan

        private void BeginPan(Point point)
        {
            Editor.Context.PanStart = new PointEx(point.X, point.Y);

            Editor.Context.PreviousScrollOffsetX = -1.0;
            Editor.Context.PreviousScrollOffsetY = -1.0;

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
            /*
            double dX = point.X - Editor.CurrentOptions.PanStart.X;
            double dY = point.Y - Editor.CurrentOptions.PanStart.Y;

            var st = GetZoomTranslateTransform();

            st.X += dX;
            st.Y += dY;

            Editor.CurrentOptions.PanStart = point;
            */

            double scrollOffsetX = point.X - Editor.Context.PanStart.X;
            double scrollOffsetY = point.Y - Editor.Context.PanStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * 1.0) * Editor.Context.ReversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * 1.0) * Editor.Context.ReversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != Editor.Context.PreviousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                Editor.Context.PreviousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != Editor.Context.PreviousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                Editor.Context.PreviousScrollOffsetY = scrollOffsetY;
            }

            Editor.Context.PanStart = new PointEx(point.X, point.Y);
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

        public double DefaultLogicStrokeThickness = 1.0;
        public double DefaultWireStrokeThickness = 2.0;
        public double DefaultElementStrokeThickness = 2.0;
        public double DefaultIOStrokeThickness = 2.0;
        public double DefaultPageStrokeThickness = 1.0;

        private void UpdateStrokeThickness(double zoom)
        {
            Application.Current.Resources[ResourceConstants.KeyLogicStrokeThickness] = DefaultLogicStrokeThickness / zoom;
            Application.Current.Resources[ResourceConstants.KeyWireStrokeThickness] = DefaultWireStrokeThickness / zoom;
            Application.Current.Resources[ResourceConstants.KeyElementStrokeThickness] = DefaultElementStrokeThickness / zoom;
            Application.Current.Resources[ResourceConstants.KeyIOStrokeThickness] = DefaultIOStrokeThickness / zoom;
            Application.Current.Resources[ResourceConstants.KeyPageStrokeThickness] = DefaultPageStrokeThickness / zoom;
        }

        public double CalculateZoom(double x)
        {
            double lb = Editor.Context.ZoomLogBase;
            double ef = Editor.Context.ZoomExpFactor;
            double l = (lb == 1.0 || lb == 0.0) ? 1.0 : Math.Log(x, lb);
            double e = (ef == 0.0) ? 1.0 : Math.Exp(l / ef);
            double y = x + x * l * e;
            return y;
        }

        public double Zoom(double zoom)
        {
            if (Editor == null || Editor.Context == null)
                return 1.0;

            double zoom_fx = CalculateZoom(zoom);

            //System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            var st = GetZoomScaleTransform();

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            UpdateStrokeThickness(zoom_fx);

            // zoom to point
            ZoomToPoint(zoom_fx, oldZoom);

            return zoom_fx;
        }

        private ScaleTransform GetZoomScaleTransform()
        {
            //var tg = RootGrid.RenderTransform as TransformGroup;
            var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is ScaleTransform) as ScaleTransform;

            return st;
        }

        private TranslateTransform GetZoomTranslateTransform()
        {
            var tg = RootGrid.RenderTransform as TransformGroup;
            //var tg = RootGrid.LayoutTransform as TransformGroup;
            var st = tg.Children.First(t => t is TranslateTransform) as TranslateTransform;

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

            double oldX = Editor.Context.ZoomPoint.X * oldZoom;
            double oldY = Editor.Context.ZoomPoint.Y * oldZoom;

            double newX = Editor.Context.ZoomPoint.X * zoom;
            double newY = Editor.Context.ZoomPoint.Y * zoom;

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

            if (Adorner != null)
            {
                Adorner.Zoom = zoom;
            }
        }

        private void ZoomIn()
        {
            double zoom = ZoomSlider.Value;

            zoom += Editor.Context.ZoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= Editor.Context.ZoomOutFactor;

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

            var canvas = Editor.Context.CurrentCanvas;
            var point = e.GetPosition(canvas as DiagramCanvas);
            Editor.Context.ZoomPoint = new PointEx(point.X, point.Y);

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
            if (e.ChangedButton == MouseButton.Middle)
            {
                var point = e.GetPosition(this);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                EndPan();
            }
        }

        private void PanScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.PanScrollViewer.IsMouseCaptured == true)
            {
                var point = e.GetPosition(this);

                PanToPoint(point);
            }
        }

        #endregion

        #region Canvas Events

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as DiagramCanvas;
            var point = e.GetPosition(canvas);

            if (Editor.Context.CurrentRoot == null && 
                Editor.Context.CurrentLine == null && 
                Editor.Context.EnableInsertLast == false)
            {
                Editor.Context.SelectionOrigin = new PointEx(point.X, point.Y);

                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    Editor.SelectNone();
                }

                UpdateSelectedTags();

                canvas.CaptureMouse();
            }
            else
            {
                Editor.MouseEventLeftDown(canvas as ICanvas, new PointEx(point.X, point.Y));
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as DiagramCanvas;

            if (canvas.IsMouseCaptured)
            {
                canvas.ReleaseMouseCapture();

                if (Adorner != null)
                {
                    var rect = Adorner.SelectionRect;
                    var elements = canvas.HitTest(rect);

                    if (elements != null)
                    {
                        foreach (var element in elements)
                        {
                            if (element.GetSelected() == false)
                            {
                                element.SetSelected(true);
                            }
                            else
                            {
                                element.SetSelected(false);
                            }
                        }
                    }

                    RemoveAdorner(canvas);
                    UpdateSelectedTags();
                }
            }
        }

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Editor.Context.SkipLeftClick == true)
            {
                Editor.Context.SkipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = sender as DiagramCanvas;
            var point = e.GetPosition(canvas);
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as IThumb;

            var result = Editor.MouseEventPreviewLeftDown(canvas, new PointEx(point.X, point.Y), pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = sender as DiagramCanvas;

            var point = e.GetPosition(canvas);

            if (canvas.IsMouseCaptured)
            {
                if (Adorner == null)
                {
                    CreateAdorner(canvas, 
                        Editor.Context.SelectionOrigin, 
                        new PointEx(point.X, point.Y));
                }

                UpdateAdorner(point);
            }
            else
            {
                Editor.MouseEventMove(canvas, new PointEx(point.X, point.Y));
            }
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as DiagramCanvas;

            var point = e.GetPosition(canvas);
            Editor.Context.RightClick = new PointEx(point.X, point.Y);

            var result = Editor.MouseEventRightDown(canvas);
            if (result == true)
            {
                Editor.Context.SkipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Editor.Context.SkipContextMenu == true)
            {
                Editor.Context.SkipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                Editor.Context.SkipLeftClick = true;
            }
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.HistoryAdd(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.InsertInput(canvas, point);

            Editor.Context.LastInsert = ModelConstants.TagElementInput;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.HistoryAdd(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.InsertOutput(canvas, point);

            Editor.Context.LastInsert = ModelConstants.TagElementOutput;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.HistoryAdd(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.InsertAndGate(canvas, point);

            Editor.Context.LastInsert = ModelConstants.TagElementAndGate;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.HistoryAdd(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.InsertOrGate(canvas, point);

            Editor.Context.LastInsert = ModelConstants.TagElementOrGate;
            Editor.Context.SkipLeftClick = false;
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            //var canvas = Editor.Context.CurrentCanvas;
            //var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            //Editor.WireToggleStart(canvas, point);
            Editor.WireToggleStart();
            Editor.Context.SkipLeftClick = false;
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            //var canvas = Editor.Context.CurrentCanvas;
            //var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            //Editor.WireToggleEnd(canvas, point);
            Editor.WireToggleEnd();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            Editor.EditCut();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            Editor.EditCopy();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.EditPaste(point, true);
            Editor.Context.SkipLeftClick = false;
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            Editor.EditDelete();
            Editor.Context.SkipLeftClick = false;
        }

        #endregion

        #region Drag & Drop

        private enum TagDragAndDropType
        {
            None,
            Input,
            Output
        }

        private TagDragAndDropType IsTagInputOrOutput(ICanvas canvas, IPoint point)
        {
            double x = point.X;
            var prop = canvas.GetProperties();
            double half = (double)prop.PageWidth / 2.0;

            if (x < half)
                return TagDragAndDropType.Input;
            else
                return TagDragAndDropType.Output;  
        }

        private void DiagramCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Tag") || 
                !e.Data.GetDataPresent("Table") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DiagramCanvas_Drop(object sender, DragEventArgs e)
        {
            // Tag
            if (e.Data.GetDataPresent("Tag"))
            {
                var tag = e.Data.GetData("Tag") as Tag;
                if (tag != null)
                {
                    var point = e.GetPosition(DiagramCanvas);

                    var insertPoint = new PointEx(point.X, point.Y);
                    var canvas = Editor.Context.CurrentCanvas;
                    var type = IsTagInputOrOutput(canvas, insertPoint);

                    if (type == TagDragAndDropType.Input)
                    {
                        Editor.HistoryAdd(canvas, true);

                        var element = Editor.InsertInput(DiagramCanvas, insertPoint);
                        element.SetData(tag);

                        e.Handled = true;
                    }
                    else if (type == TagDragAndDropType.Output)
                    {
                        Editor.HistoryAdd(canvas, true);

                        var element = Editor.InsertOutput(DiagramCanvas, insertPoint);
                        element.SetData(tag);

                        e.Handled = true;
                    }
                }
            }

            // Table
            else if (e.Data.GetDataPresent("Table"))
            {
                var table = e.Data.GetData("Table") as DiagramTable;
                if (table != null)
                {
                    var canvas = Editor.Context.CurrentCanvas;

                    Editor.HistoryAdd(canvas, true);

                    canvas.SetData(table);

                    // TODO:
                    TableGrid.SetData(this, table);

                    e.Handled = true;
                }
            }
        }

        #endregion
    } 

    #endregion
}
