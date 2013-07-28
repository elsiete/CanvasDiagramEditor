// Copyright (C) Wiesław Šoltés 2013. 
// All Rights Reserved

#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives; 

#endregion

namespace CanvasDiagramEditor.Controls
{
    #region ElementThumb

    public class ElementThumb : Thumb
    {
        #region IsSelected Attached Property

        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(ElementThumb),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Data Attached Property

        public static object GetData(DependencyObject obj)
        {
            return (object)obj.GetValue(DataProperty);
        }

        public static void SetData(DependencyObject obj, object value)
        {
            obj.SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(object), typeof(ElementThumb),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Items Attached Property

        public static List<object> GetItems(DependencyObject obj)
        {
            return (List<object>)obj.GetValue(ItemsProperty);
        }

        public static void SetItems(DependencyObject obj, List<object> value)
        {
            obj.SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.RegisterAttached("Items", typeof(List<object>), typeof(ElementThumb),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion
    }

    #endregion
}
