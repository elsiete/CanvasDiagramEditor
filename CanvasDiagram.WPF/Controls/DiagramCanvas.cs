// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#region References

using CanvasDiagram.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

#endregion

namespace CanvasDiagram.WPF.Controls
{
    #region DiagramCanvas

    public class DiagramCanvas : Canvas, ICanvas
    {
        #region ICanvas

        private IdCounter Counter { get; set; }
        private DiagramProperties Properties { get; set; }

        public IEnumerable<IElement> GetElements()
        {
            return this.Children.Cast<FrameworkElement>().Cast<IElement>();
        }

        public void Add(IElement element)
        {
            if (element != null)
                this.Children.Add(element as FrameworkElement);
        }

        public void Remove(IElement element)
        {
            if (element != null)
                this.Children.Remove(element as FrameworkElement);
        }

        public void Clear()
        {
            this.Children.Clear();
        }

        public double GetWidth()
        {
            return this.Width;
        }

        public void SetWidth(double width)
        {
            this.Width = width;
        }

        public double GetHeight()
        {
            return this.Height;
        }

        public void SetHeight(double height)
        {
            this.Height = height;
        }

        public List<object> GetTags()
        {
            return ElementThumb.GetItems(this);
        }

        public void SetTags(List<object> tags)
        {
            ElementThumb.SetItems(this, tags);
        }

        public IdCounter GetCounter()
        {
            return this.Counter;
        }

        public void SetCounter(IdCounter counter)
        {
            this.Counter = counter;
        }

        public DiagramProperties GetProperties()
        {
            return this.Properties;
        }

        public void SetProperties(DiagramProperties properties)
        {
            this.Properties = properties;
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

        #region HitTest

        public IEnumerable<IElement> HitTest(IPoint point, double radius)
        {
            var canvas = this;
            var selectedElements = new List<DependencyObject>();
            var elippse = new EllipseGeometry() { RadiusX = radius, RadiusY = radius, Center = new Point(point.X, point.Y) };
            var hitTestParams = new GeometryHitTestParameters(elippse);
            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    if (VisualTreeHelper.GetParent(element) == canvas)
                        selectedElements.Add(element);

                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(canvas, filterCallback, resultCallback, hitTestParams);

            return selectedElements.Cast<IElement>();
        }

        public IEnumerable<IElement> HitTest(IRect rect)
        {
            var canvas = this;
            var r = new Rect(new Point(rect.X1, rect.Y1), new Point(rect.X2, rect.Y2));
            var selectedElements = new List<DependencyObject>();
            var rectangle = new RectangleGeometry(r, 0.0, 0.0);
            var hitTestParams = new GeometryHitTestParameters(rectangle);
            var resultCallback = new HitTestResultCallback(result => HitTestResultBehavior.Continue);

            var filterCallback = new HitTestFilterCallback(
                element =>
                {
                    if (VisualTreeHelper.GetParent(element) == canvas)
                        selectedElements.Add(element);

                    return HitTestFilterBehavior.Continue;
                });

            VisualTreeHelper.HitTest(canvas, filterCallback, resultCallback, hitTestParams);

            return selectedElements.Cast<IElement>();
        }

        #endregion
    } 

    #endregion
}
