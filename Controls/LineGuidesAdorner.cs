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
    #region LineGuidesAdorner

    public class LineGuidesAdorner : Adorner
    {
        #region Properties

        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(LineGuidesAdorner),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(LineGuidesAdorner),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(LineGuidesAdorner),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(LineGuidesAdorner),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(LineGuidesAdorner),
                new FrameworkPropertyMetadata(1.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Constructor

        public LineGuidesAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        #endregion

        #region Pen

        Pen PenGuides = new Pen(new SolidColorBrush(Colors.DeepPink), 1.0)
        {
            StartLineCap = PenLineCap.Flat,
            EndLineCap = PenLineCap.Flat,
            LineJoin = PenLineJoin.Miter
        };

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (IsEnabled == true)
            {
                DrawGuides(drawingContext);
            }
        }

        private void DrawGuides(DrawingContext drawingContext)
        {
            double x = X;
            double y = Y;
            double width = CanvasWidth;
            double height = CanvasHeight;
            double offsetX = 0.0;
            double offsetY = 0.0;

            PenGuides.Thickness = StrokeThickness;

            if (x >= 0 && x <= width)
            {
                var verticalPoint0 = new Point(x + offsetX, 0);
                var verticalPoint1 = new Point(x + offsetX, height);

                //double halfPenWidth = PenGuides.Thickness / 2.0;
                //GuidelineSet guidelines = new GuidelineSet();
                //guidelines.GuidelinesX.Add(verticalPoint0.X + halfPenWidth);
                //guidelines.GuidelinesX.Add(verticalPoint1.X + halfPenWidth);
                //guidelines.GuidelinesY.Add(verticalPoint0.Y + halfPenWidth);
                //guidelines.GuidelinesY.Add(verticalPoint1.Y + halfPenWidth);
                //drawingContext.PushGuidelineSet(guidelines);

                drawingContext.DrawLine(PenGuides, verticalPoint0, verticalPoint1);

                //drawingContext.Pop();
            }

            if (y >= 0 && y <= height)
            {
                var horizontalPoint0 = new Point(0, y + offsetY);
                var horizontalPoint1 = new Point(width, y + offsetY);

                //double halfPenWidth = PenGuides.Thickness / 2.0;
                //GuidelineSet guidelines = new GuidelineSet();
                //guidelines.GuidelinesX.Add(horizontalPoint0.X + halfPenWidth);
                //guidelines.GuidelinesX.Add(horizontalPoint1.X + halfPenWidth);
                //guidelines.GuidelinesY.Add(horizontalPoint0.Y + halfPenWidth);
                //guidelines.GuidelinesY.Add(horizontalPoint1.Y + halfPenWidth);
                //drawingContext.PushGuidelineSet(guidelines);

                drawingContext.DrawLine(PenGuides, horizontalPoint0, horizontalPoint1);

                //drawingContext.Pop();
            }
        }

        #endregion
    }

    #endregion
}
