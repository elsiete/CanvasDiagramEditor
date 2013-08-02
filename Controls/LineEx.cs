// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using CanvasDiagramEditor.Core;
using CanvasDiagramEditor.Editor;
using CanvasDiagramEditor.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace CanvasDiagramEditor.Controls
{
    #region LineEx

    public class LineEx : Shape, ILine
    {
        #region Properties

        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(LineEx),
            new FrameworkPropertyMetadata(3.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsStartVisible
        {
            get { return (bool)GetValue(IsStartVisibleProperty); }
            set { SetValue(IsStartVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsStartVisibleProperty =
            DependencyProperty.Register("IsStartVisible", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsEndVisible
        {
            get { return (bool)GetValue(IsEndVisibleProperty); }
            set { SetValue(IsEndVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsEndVisibleProperty =
            DependencyProperty.Register("IsEndVisible", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsStartIO
        {
            get { return (bool)GetValue(IsStartIOProperty); }
            set { SetValue(IsStartIOProperty, value); }
        }

        public static readonly DependencyProperty IsStartIOProperty =
            DependencyProperty.Register("IsStartIO", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsEndIO
        {
            get { return (bool)GetValue(IsEndIOProperty); }
            set { SetValue(IsEndIOProperty, value); }
        }

        public static readonly DependencyProperty IsEndIOProperty =
            DependencyProperty.Register("IsEndIO", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Attached Properties

        public static bool GetShortenStart(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShortenStartProperty);
        }

        public static void SetShortenStart(DependencyObject obj, bool value)
        {
            obj.SetValue(ShortenStartProperty, value);
        }

        public static readonly DependencyProperty ShortenStartProperty =
            DependencyProperty.RegisterAttached("ShortenStart", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetShortenEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShortenEndProperty);
        }

        public static void SetShortenEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(ShortenEndProperty, value);
        }

        public static readonly DependencyProperty ShortenEndProperty =
            DependencyProperty.RegisterAttached("ShortenEnd", typeof(bool), typeof(LineEx),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Get DefiningGeometry

        private const double ShortenLineSize = 15.0;

        public double GetThickness()
        {
            return StrokeThickness / 2.0;
        }

        protected virtual Geometry GetDefiningGeometry()
        {
            bool isStartVisible = IsStartVisible;
            bool isEndVisible = IsEndVisible;

            double radius = Radius;
            double thickness = GetThickness();

            double startX = X1;
            double startY = Y1;
            double endX = X2;
            double endY = Y2;

            double zet = LineCalc.CalculateZet(startX, startY, endX, endY);
            double sizeX = LineCalc.CalculateSizeX(radius, thickness, zet);
            double sizeY = LineCalc.CalculateSizeY(radius, thickness, zet);

            bool shortenStart = GetShortenStart(this);
            bool shortenEnd = GetShortenEnd(this);
            bool isStartIO = GetStartIO();
            bool isEndIO = GetEndIO();

            // shorten start
            if (isStartIO == true && isEndIO == false && shortenStart == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    startX = endX - ShortenLineSize;
                }
            }

            // shorten end
            if (isStartIO == false && isEndIO == true && shortenEnd == true)
            {
                if (Math.Round(startY, 1) == Math.Round(endY, 1))
                {
                    endX = startX + ShortenLineSize;
                }
            }

            // get start and end ellipse position
            Point ellipseStartCenter = LineCalc.GetEllipseStartCenter(startX, startY, sizeX, sizeY, isStartVisible);
            Point ellipseEndCenter = LineCalc.GetEllipseEndCenter(endX, endY, sizeX, sizeY, isEndVisible);

            // get line position
            Point lineStart = LineCalc.GetLineStart(startX, startY, sizeX, sizeY, isStartVisible);
            Point lineEnd = LineCalc.GetLineEnd(endX, endY, sizeX, sizeY, isEndVisible);

            var g = new GeometryGroup() { FillRule = FillRule.Nonzero };

            if (isStartVisible == true)
            {
                var startEllipse = new EllipseGeometry(ellipseStartCenter, radius, radius);
                g.Children.Add(startEllipse);
            }

            if (isEndVisible == true)
            {
                var endEllipse = new EllipseGeometry(ellipseEndCenter, radius, radius);
                g.Children.Add(endEllipse);
            }

            var line = new LineGeometry(lineStart, lineEnd);
            g.Children.Add(line);

            g.Freeze();

            return g;
        }

        #endregion

        #region DefiningGeometry

        protected override Geometry DefiningGeometry
        {
            get
            {
                var g = GetDefiningGeometry();
                return g;
            }
        }

        #endregion

        #region IElement

        public double GetX()
        {
            return Canvas.GetLeft(this);
        }

        public double GetY()
        {
            return Canvas.GetTop(this);
        }

        public void SetX(double x)
        {
            Canvas.SetLeft(this, x);
        }

        public void SetY(double y)
        {
            Canvas.SetTop(this, y);
        }

        #endregion

        #region IUid

        public string GetUid()
        {
            return this.Uid;
        }

        public void SetUid(string uid)
        {
            this.Uid = uid;
        }

        #endregion

        #region ITag

        public object GetTag()
        {
            return this.Tag;
        }

        public void SetTag(object tag)
        {
            this.Tag = tag;
        } 

        #endregion

        #region IData

        public object GetData()
        {
            return ElementThumb.GetData(this);
        }

        public void SetData(object data)
        {
            ElementThumb.SetData(this, data);
        } 

        #endregion

        #region ISelected

        public bool GetSelected()
        {
            return ElementThumb.GetIsSelected(this);
        }

        public void SetSelected(bool selected)
        {
            ElementThumb.SetIsSelected(this, selected);
        }

        #endregion

        #region ILine

        public bool GetStartVisible()
        {
            return this.IsStartVisible;
        }

        public void SetStartVisible(bool visible)
        {
            this.IsStartVisible = visible;
        }

        public bool GetEndVisible()
        {
            return this.IsEndVisible;
        }

        public void SetEndVisible(bool visible)
        {
            this.IsEndVisible = visible;
        }

        public bool GetStartIO()
        {
            return this.IsStartIO;
        }

        public void SetStartIO(bool flag)
        {
            this.IsStartIO = flag;
        }

        public bool GetEndIO()
        {
            return this.IsEndIO;
        }

        public void SetEndIO(bool flag)
        {
            this.IsEndIO = flag;
        }

        public double GetX1()
        {
            return this.X1;
        }

        public void SetX1(double x1)
        {
            this.X1 = x1;
        }

        public double GetY1()
        {
            return this.Y1;
        }

        public void SetY1(double y1)
        {
            this.Y1 = y1;
        }

        public double GetX2()
        {
            return this.X2;
        }

        public void SetX2(double x2)
        {
            this.X2 = x2;
        }

        public double GetY2()
        {
            return this.Y2;
        }

        public void SetY2(double y2)
        {
            this.Y2 = y2;
        }

        #endregion

        public IMargin GetMargin()
        {
            var margin = this.Margin;

            return new MarginEx()
            {
                Bottom = margin.Bottom,
                Left = margin.Left,
                Right = margin.Right,
                Top = margin.Top
            };
        }

        public void SetMargin(IMargin margin)
        {
            this.Margin = new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);
        }
    }

    #endregion
}
