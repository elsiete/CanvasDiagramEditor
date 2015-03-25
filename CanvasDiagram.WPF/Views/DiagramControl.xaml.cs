// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.WPF.Controls;
using CanvasDiagram.Core;
using CanvasDiagram.Editor;
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

namespace CanvasDiagram.WPF
{
    #region DiagramControl

    public partial class DiagramControl : UserControl
    {
        #region Properties

        public Action SelectionChanged { get; set; }
        public DiagramEditor Editor { get; set; }
        private SelectionAdorner Adorner { get; set; }

        #endregion

        #region Fields

        public double DefaultLogicStrokeThickness = 1.0;
        public double DefaultWireStrokeThickness = 2.0;
        public double DefaultElementStrokeThickness = 2.0;
        public double DefaultIOStrokeThickness = 2.0;
        public double DefaultPageStrokeThickness = 1.0;
        private double CurrentZoom = 1.0;

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
            Adorner = new SelectionAdorner(canvas);
            Adorner.Zoom = GetZoomScaleTransform().ScaleX;
            Adorner.SelectionOrigin = new Point(origin.X, origin.Y);
            Adorner.SelectionRect = new RectEx(origin.X, origin.Y, point.X, point.Y);
            Adorner.SnapsToDevicePixels = false;

            RenderOptions.SetEdgeMode(Adorner, EdgeMode.Aliased);
            AdornerLayer.GetAdornerLayer(canvas).Add(Adorner);
            Adorner.InvalidateVisual();
        }

        private void RemoveAdorner(Canvas canvas)
        {
            AdornerLayer.GetAdornerLayer(canvas).Remove(Adorner);
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
                SelectionChanged();
        }

        #endregion

        #region Pan

        public void ResetPan()
        {
            var st = GetZoomTranslateTransform();
            st.X = 0.0;
            st.Y = 0.0;
        }

        private void BeginPan(Point point)
        {
            Editor.Context.PanStart = new PointEx(point.X, point.Y);
            Editor.Context.PreviousScrollOffsetX = -1.0;
            Editor.Context.PreviousScrollOffsetY = -1.0;

            this.Cursor = Cursors.ScrollAll;
            this.CaptureMouse();
        }

        private void EndPan()
        {
            if (this.IsMouseCaptured == true)
            {
                this.Cursor = Cursors.Arrow;
                this.ReleaseMouseCapture();
            }
        }

        private void PanToPoint(Point point)
        {
            double dX = point.X - Editor.Context.PanStart.X;
            double dY = point.Y - Editor.Context.PanStart.Y;
            var st = GetZoomTranslateTransform();
            st.X += dX;
            st.Y += dY;
            Editor.Context.PanStart = new PointEx(point.X, point.Y);
        }

        #endregion

        #region Zoom

        public void ResetZoom()
        {
            var st = GetZoomScaleTransform();
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;
        }

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
            double lb = 1.9;
            double ef = 1.3;
            double l = (lb == 1.0 || lb == 0.0) ? 1.0 : Math.Log(x, lb);
            double e = (ef == 0.0) ? 1.0 : Math.Exp(l / ef);
            return x + x * l * e;
        }

        public void Zoom(double zoom)
        {
            if (Editor == null || Editor.Context == null)
                return;

            double czoom = CalculateZoom(zoom);
            var st = GetZoomScaleTransform();
            double old = st.ScaleX;

            st.ScaleX = czoom;
            st.ScaleY = czoom;

            UpdateStrokeThickness(czoom);
            ZoomToPoint(czoom, old);
        }

        private ScaleTransform GetZoomScaleTransform()
        {
            var tg = RootGrid.RenderTransform as TransformGroup;
            return tg.Children.First(t => t is ScaleTransform) as ScaleTransform;
        }

        private TranslateTransform GetZoomTranslateTransform()
        {
            var tg = RootGrid.RenderTransform as TransformGroup;
            return tg.Children.First(t => t is TranslateTransform) as TranslateTransform;
        }

        private void ZoomToPoint(double zoom, double oldZoom)
        {
            var relative = Editor.Context.ZoomPoint;
            var tt = GetZoomTranslateTransform();

            double absoluteX = relative.X * oldZoom + tt.X;
            double absoluteY = relative.Y * oldZoom + tt.Y;

            tt.X = absoluteX - relative.X * zoom;
            tt.Y = absoluteY - relative.Y * zoom;

            if (Adorner != null)
                Adorner.Zoom = zoom;
        }

        private void ZoomToFit(Size viewport, Size source)
        {
            var st = GetZoomScaleTransform();
            var tt = GetZoomTranslateTransform();
            double sX = viewport.Width / source.Width;
            double sY = viewport.Height / source.Height;
            double zoom = Math.Min(sX, sY);
            double dXZoomed = viewport.Width - (source.Width * zoom);
            double dYZoomed = viewport.Height - (source.Height * zoom);
            double dX = viewport.Width - source.Width;
            double dY = viewport.Height - source.Height;
            double x = 0.0;
            double y = 0.0;

            if (source.Width >= viewport.Width && source.Height >= viewport.Height)
            {
                x = sX > sY ? dXZoomed / 2.0 : 0.0;
                y = sX > sY ? 0.0 : dYZoomed / 2.0;
            }
            else
            {
                x = source.Width >= viewport.Width ? 0.0 : (dXZoomed - dX) / 2.0;
                y = source.Height >= viewport.Height ? 0.0 : (dYZoomed - dY) / 2.0;
            }

            st.ScaleX = zoom;
            st.ScaleY = zoom;
            tt.X = x;
            tt.Y = y;

            UpdateStrokeThickness(zoom);

            if (Adorner != null)
                Adorner.Zoom = zoom;

            CurrentZoom = 1.0;
        }

        public void ZoomToFit()
        {
            var viewport = new Size(this.ActualWidth + 0.0, this.ActualHeight + 0.0);
            var source = new Size(this.DiagramCanvas.Width + 6.0, this.DiagramCanvas.Height + 6.0);
            ZoomToFit(viewport, source);
        }

        private void ZoomIn()
        {
            double zoom = CurrentZoom + Editor.Context.ZoomInFactor;

            if (zoom >= 0.1 && zoom <= 5.0)
            {
                CurrentZoom = zoom;
                Zoom(zoom);
            }
        }

        private void ZoomOut()
        {
            double zoom = CurrentZoom - Editor.Context.ZoomOutFactor;

            if (zoom >= 0.1 && zoom <= 5.0)
            {
                CurrentZoom = zoom;
                Zoom(zoom);
            }
        }

        #endregion

        #region Border Events

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

        #region UserControl Events

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 1)
                BeginPan(e.GetPosition(this));
            else if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
                ZoomToFit();

            this.Focus();
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
                EndPan();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured == true)
                PanToPoint(e.GetPosition(this));
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
                    Editor.SelectNone();

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
                                element.SetSelected(true);
                            else
                                element.SetSelected(false);
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

            Editor.Snapshot(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Insert.Input(canvas, point, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.Context.LastInsert = Constants.TagElementInput;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertOutput_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.Snapshot(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Insert.Output(canvas, point, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.Context.LastInsert = Constants.TagElementOutput;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertAndGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.Snapshot(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Insert.AndGate(canvas, point, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.Context.LastInsert = Constants.TagElementAndGate;
            Editor.Context.SkipLeftClick = false;
        }

        private void InsertOrGate_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Editor.Context.CurrentCanvas;

            Editor.Snapshot(canvas, true);

            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Insert.OrGate(canvas, point, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);

            Editor.Context.LastInsert = Constants.TagElementOrGate;
            Editor.Context.SkipLeftClick = false;
        }

        private void InvertStart_Click(object sender, RoutedEventArgs e)
        {
            Editor.ToggleWireStart();
            Editor.Context.SkipLeftClick = false;
        }

        private void InvertEnd_Click(object sender, RoutedEventArgs e)
        {
            Editor.ToggleWireEnd();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            Editor.Cut();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            Editor.Copy();
            Editor.Context.SkipLeftClick = false;
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            var point = new PointEx(Editor.Context.RightClick.X, Editor.Context.RightClick.Y);
            Editor.Paste(point, true);
            Editor.Context.SkipLeftClick = false;
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            Editor.Delete();
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
            if (point.X < (double)canvas.GetProperties().PageWidth / 2.0)
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
                        Editor.Snapshot(canvas, true);

                        var element = Insert.Input(DiagramCanvas, insertPoint, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);
                        element.SetData(tag);

                        e.Handled = true;
                    }
                    else if (type == TagDragAndDropType.Output)
                    {
                        Editor.Snapshot(canvas, true);

                        var element = Insert.Output(DiagramCanvas, insertPoint, Editor.Context.DiagramCreator, Editor.Context.EnableSnap);
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

                    Editor.Snapshot(canvas, true);

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
