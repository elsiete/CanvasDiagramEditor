// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media; 

#endregion

namespace CanvasDiagramEditor.Controls
{
    #region SelectionAdorner

    public class SelectionAdorner : Adorner
    {
        #region Properties

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public Point SelectionOrigin
        {
            get { return (Point)GetValue(SelectionOriginProperty); }
            set { SetValue(SelectionOriginProperty, value); }
        }

        public static readonly DependencyProperty SelectionOriginProperty =
            DependencyProperty.Register("SelectionOrigin", typeof(Point), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(new Point(),
                FrameworkPropertyMetadataOptions.None));

        public Rect SelectionRect
        {
            get { return (Rect)GetValue(SelectionRectProperty); }
            set { SetValue(SelectionRectProperty, value); }
        }

        public static readonly DependencyProperty SelectionRectProperty =
            DependencyProperty.Register("SelectionRect", typeof(Rect), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(new Rect(),
                FrameworkPropertyMetadataOptions.None));

        #endregion

        #region Fields

        private SolidColorBrush brush = null;
        private Pen pen = null;
        private double defaultThickness = 1.0;

        #endregion

        #region Constructor

        public SelectionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            brush = new SolidColorBrush(Color.FromArgb(0x90, 0xB0, 0xB0, 0xB0));
            pen = new Pen(new SolidColorBrush(Color.FromArgb(0x90, 0x70, 0x70, 0x70)), defaultThickness);
        }

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = SelectionRect;

            if (rect != null)
            {
                double zoom = Zoom;
                double thickness = defaultThickness / zoom;
                double half = thickness / 2.0;

                pen.Thickness = thickness;

                drawingContext.DrawRectangle(brush, pen, rect);
            }
        }

        #endregion
    }

    #endregion
}
