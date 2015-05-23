// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using CanvasDiagram.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

#endregion

namespace CanvasDiagram.WPF.Controls
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

            double sx = X1;
            double sy = Y1;
            double ex = X2;
            double ey = Y2;

            double zet = LineUtil.Zet(sx, sy, ex, ey);
            double width = LineUtil.Width(radius, thickness, zet);
            double height = LineUtil.Height(radius, thickness, zet);

            bool shortenStart = GetShortenStart(this);
            bool shortenEnd = GetShortenEnd(this);
            bool isStartIO = GetStartIO();
            bool isEndIO = GetEndIO();

            // shorten start
            if (isStartIO == true && isEndIO == false && shortenStart == true)
            {
                if (Math.Round(sy, 1) == Math.Round(ey, 1))
                    sx = ex - ShortenLineSize;
            }

            // shorten end
            if (isStartIO == false && isEndIO == true && shortenEnd == true)
            {
                if (Math.Round(sy, 1) == Math.Round(ey, 1))
                    ex = sx + ShortenLineSize;
            }

            // get ellipse position
            IPoint ellipseStart = LineUtil.EllipseStart(sx, sy, width, height, isStartVisible);
            IPoint ellipseEnd = LineUtil.EllipseEnd(ex, ey, width, height, isEndVisible);

            // get line position
            IPoint lineStart = LineUtil.LineStart(sx, sy, width, height, isStartVisible);
            IPoint lineEnd = LineUtil.LineEnd(ex, ey, width, height, isEndVisible);

            var g = new GeometryGroup() { FillRule = FillRule.Nonzero };

            if (isStartVisible == true)
            {
                var startEllipse = new EllipseGeometry(
                    new Point(ellipseStart.X, ellipseStart.Y),
                    radius, radius);

                g.Children.Add(startEllipse);
            }

            if (isEndVisible == true)
            {
                var endEllipse = new EllipseGeometry(
                    new Point(ellipseEnd.X, ellipseEnd.Y), 
                    radius, radius);

                g.Children.Add(endEllipse);
            }

            var line = new LineGeometry(
                new Point(lineStart.X, lineStart.Y), 
                new Point(lineEnd.X, lineEnd.Y));

            g.Children.Add(line);

            g.Freeze();
            return g;
        }

        #endregion

        #region DefiningGeometry

        protected override Geometry DefiningGeometry
        {
            get { return GetDefiningGeometry(); }
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

        public object GetParent()
        {
            return this.Parent;
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

        public IMargin GetMargin()
        {
            var margin = this.Margin;
            return new MarginEx(margin.Bottom, margin.Left, margin.Right, margin.Top);
        }

        public void SetMargin(IMargin margin)
        {
            this.Margin = new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);
        }

        #endregion
    }

    #endregion
}
