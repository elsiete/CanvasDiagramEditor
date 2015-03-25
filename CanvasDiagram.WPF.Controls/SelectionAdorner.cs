// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media; 

#endregion

namespace CanvasDiagram.WPF.Controls
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

        public RectEx SelectionRect
        {
            get { return (RectEx)GetValue(SelectionRectProperty); }
            set { SetValue(SelectionRectProperty, value); }
        }

        public static readonly DependencyProperty SelectionRectProperty =
            DependencyProperty.Register("SelectionRect", typeof(RectEx), typeof(SelectionAdorner),
            new FrameworkPropertyMetadata(new RectEx(),
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

                var r = new Rect(new Point(rect.X1, rect.Y1), new Point(rect.X2, rect.Y2));
                drawingContext.DrawRectangle(brush, pen, r);
            }
        }

        #endregion
    }

    #endregion
}
