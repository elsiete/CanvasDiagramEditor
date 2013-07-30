// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Controls;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Parser;
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
        #region Fields

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

        private void CreateAdorner(Canvas canvas, Point origin, Point point)
        {
            var layer = AdornerLayer.GetAdornerLayer(canvas);

            Adorner = new SelectionAdorner(canvas);
            Adorner.Zoom = GetZoomScaleTransform().ScaleX;
            Adorner.SelectionOrigin = new Point(origin.X, origin.Y);

            Adorner.SelectionRect = new Rect(origin, point);

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

            Adorner.SelectionRect = new Rect(point, origin);
            Adorner.InvalidateVisual();
        }

        #endregion

        #region Pan

        private void BeginPan(Point point)
        {
            Editor.CurrentOptions.PanStart = point;

            Editor.CurrentOptions.PreviousScrollOffsetX = -1.0;
            Editor.CurrentOptions.PreviousScrollOffsetY = -1.0;

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
            double scrollOffsetX = point.X - Editor.CurrentOptions.PanStart.X;
            double scrollOffsetY = point.Y - Editor.CurrentOptions.PanStart.Y;

            double horizontalOffset = this.PanScrollViewer.HorizontalOffset;
            double verticalOffset = this.PanScrollViewer.VerticalOffset;

            double scrollableWidth = this.PanScrollViewer.ScrollableWidth;
            double scrollableHeight = this.PanScrollViewer.ScrollableHeight;

            double zoom = ZoomSlider.Value;

            scrollOffsetX = Math.Round(horizontalOffset + (scrollOffsetX * 1.0) * Editor.CurrentOptions.ReversePanDirection, 0);
            scrollOffsetY = Math.Round(verticalOffset + (scrollOffsetY * 1.0) * Editor.CurrentOptions.ReversePanDirection, 0);

            scrollOffsetX = scrollOffsetX > scrollableWidth ? scrollableWidth : scrollOffsetX;
            scrollOffsetY = scrollOffsetY > scrollableHeight ? scrollableHeight : scrollOffsetY;

            scrollOffsetX = scrollOffsetX < 0 ? 0.0 : scrollOffsetX;
            scrollOffsetY = scrollOffsetY < 0 ? 0.0 : scrollOffsetY;

            if (scrollOffsetX != Editor.CurrentOptions.PreviousScrollOffsetX)
            {
                this.PanScrollViewer.ScrollToHorizontalOffset(scrollOffsetX);
                Editor.CurrentOptions.PreviousScrollOffsetX = scrollOffsetX;
            }

            if (scrollOffsetY != Editor.CurrentOptions.PreviousScrollOffsetY)
            {
                this.PanScrollViewer.ScrollToVerticalOffset(scrollOffsetY);
                Editor.CurrentOptions.PreviousScrollOffsetY = scrollOffsetY;
            }

            Editor.CurrentOptions.PanStart = point;
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
            double l = Math.Log(x, Editor.CurrentOptions.ZoomLogBase);
            double e = Math.Exp(l / Editor.CurrentOptions.ZoomExpFactor);
            double y = x + x * l * e;
            return y;
        }

        public void Zoom(double zoom)
        {
            if (Editor == null || Editor.CurrentOptions == null)
                return;

            double zoom_fx = CalculateZoom(zoom);

            //System.Diagnostics.Debug.Print("Zoom: {0}, zoom_fx: {1}", zoom, zoom_fx);

            var st = GetZoomScaleTransform();

            double oldZoom = st.ScaleX; // ScaleX == ScaleY

            st.ScaleX = zoom_fx;
            st.ScaleY = zoom_fx;

            UpdateStrokeThickness(zoom_fx);

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

            double oldX = Editor.CurrentOptions.ZoomPoint.X * oldZoom;
            double oldY = Editor.CurrentOptions.ZoomPoint.Y * oldZoom;

            double newX = Editor.CurrentOptions.ZoomPoint.X * zoom;
            double newY = Editor.CurrentOptions.ZoomPoint.Y * zoom;

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

            zoom += Editor.CurrentOptions.ZoomInFactor;

            if (zoom >= ZoomSlider.Minimum && zoom <= ZoomSlider.Maximum)
            {
                ZoomSlider.Value = zoom;
            }
        }

        private void ZoomOut()
        {
            double zoom = ZoomSlider.Value;

            zoom -= Editor.CurrentOptions.ZoomOutFactor;

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

            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.CurrentOptions.ZoomPoint = e.GetPosition(canvas);

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
            if (e.ChangedButton == Editor.CurrentOptions.PanButton)
            {
                var point = e.GetPosition(this.PanScrollViewer);

                BeginPan(point);
            }
        }

        private void PanScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == Editor.CurrentOptions.PanButton)
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
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var point = e.GetPosition(canvas);

            if (Editor.CurrentOptions.CurrentRoot == null && Editor.CurrentOptions.CurrentLine == null && Editor.CurrentOptions.EnableInsertLast == false)
            {
                Editor.CurrentOptions.SelectionOrigin = point;

                if (Keyboard.Modifiers != ModifierKeys.Control)
                {
                    Editor.DeselectAll();
                }

                canvas.CaptureMouse();
            }
            else
            {
                Editor.HandleLeftDown(canvas, point);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            if (canvas.IsMouseCaptured)
            {
                canvas.ReleaseMouseCapture();

                if (Adorner != null)
                {
                    var rect = Adorner.SelectionRect;
                    var elements = Editor.HitTest(canvas, ref rect);

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
            if (Editor.CurrentOptions.SkipLeftClick == true)
            {
                Editor.CurrentOptions.SkipLeftClick = false;
                e.Handled = true;
                return;
            }

            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var point = e.GetPosition(canvas);
            var pin = (e.OriginalSource as FrameworkElement).TemplatedParent as FrameworkElement;

            var result = Editor.HandlePreviewLeftDown(canvas, point, pin);
            if (result == true)
                e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            var point = e.GetPosition(canvas);

            if (canvas.IsMouseCaptured)
            {
                if (Adorner == null)
                {
                    CreateAdorner(canvas, Editor.CurrentOptions.SelectionOrigin, point);
                }

                UpdateAdorner(point);
            }
            else
            {
                Editor.HandleMove(canvas, point);
            }
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var path = Editor.CurrentOptions.CurrentPathGrid;

            Editor.CurrentOptions.RightClick = e.GetPosition(canvas);

            var result = Editor.HandleRightDown(canvas, path);
            if (result == true)
            {
                Editor.CurrentOptions.SkipContextMenu = true;
                e.Handled = true;
            }
        }

        #endregion

        #region ContextMenu Events

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Editor.CurrentOptions.SkipContextMenu == true)
            {
                Editor.CurrentOptions.SkipContextMenu = false;
                e.Handled = true;
            }
            else
            {
                Editor.CurrentOptions.SkipLeftClick = true;
            }
        }

        private void InsertPin_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.AddToHistory(canvas);

            Editor.InsertPin(canvas, Editor.CurrentOptions.RightClick);

            Editor.CurrentOptions.LastInsert = ModelConstants.TagElementPin;
            Editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertInput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.AddToHistory(canvas);

            Editor.InsertInput(canvas, Editor.CurrentOptions.RightClick);

            Editor.CurrentOptions.LastInsert = ModelConstants.TagElementInput;
            Editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.AddToHistory(canvas);

            Editor.InsertOutput(canvas, Editor.CurrentOptions.RightClick);

            Editor.CurrentOptions.LastInsert = ModelConstants.TagElementOutput;
            Editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.AddToHistory(canvas);

            Editor.InsertAndGate(canvas, Editor.CurrentOptions.RightClick);

            Editor.CurrentOptions.LastInsert = ModelConstants.TagElementAndGate;
            Editor.CurrentOptions.SkipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;

            Editor.AddToHistory(canvas);

            Editor.InsertOrGate(canvas, Editor.CurrentOptions.RightClick);

            Editor.CurrentOptions.LastInsert = ModelConstants.TagElementOrGate;
            Editor.CurrentOptions.SkipLeftClick = false;
        }

        private void DeleteElement_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var point = new Point(Editor.CurrentOptions.RightClick.X, Editor.CurrentOptions.RightClick.Y);

            Editor.Delete(canvas, point);
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var point = new Point(Editor.CurrentOptions.RightClick.X, Editor.CurrentOptions.RightClick.Y);

            Editor.ToggleStart(canvas, point);
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.CurrentOptions.CurrentCanvas;
            var point = new Point(Editor.CurrentOptions.RightClick.X, Editor.CurrentOptions.RightClick.Y);

            Editor.ToggleEnd(canvas, point);
        }

        #endregion
    } 

    #endregion
}
