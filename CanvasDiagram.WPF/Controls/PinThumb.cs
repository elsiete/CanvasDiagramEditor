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
using System.Windows.Controls.Primitives;

#endregion

namespace CanvasDiagram.WPF.Controls
{
    #region PinThumb

    public class PinThumb : Thumb, IThumb
    {
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
            return ((this.Parent as FrameworkElement).Parent as FrameworkElement).TemplatedParent;
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
    }

    #endregion
}
